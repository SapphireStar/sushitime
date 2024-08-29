using Cysharp.Threading.Tasks;
using Isekai.Managers;
using Isekai.UI.ViewModels.Screens;
using MyPackage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    private GameModel m_GameModel;
    [SerializeField]
    private PlayerController m_Player;
    [SerializeField]
    private BaseEnemy[] m_Enemies;

    private Vector3 m_PlayerStartPos;
    private Vector3[] m_EnemyStartPos;
    // Start is called before the first frame update
    private void Awake()
    {
        m_GameModel = ModelManager.Instance.GetModel<GameModel>(typeof(GameModel));
    }
    void Start()
    {
        m_GameModel.PropertyValueChanged += onGameModelChanged;
        ScreenManager.Instance.TransitionToInstant(Isekai.UI.EScreenType.HUDScreen, ELayerType.HUDLayer, new HUDScreenViewModel()).Forget();

        m_Enemies = transform.GetComponentsInChildren<BaseEnemy>();

        RecordInitialPos();
        StartCoroutine(GameStart());
    }
    //Record Characters' initial positions for restoring
    void RecordInitialPos()
    {
        m_PlayerStartPos = m_Player.transform.position;
        m_EnemyStartPos = new Vector3[m_Enemies.Length];
        for (int i = 0; i < m_EnemyStartPos.Length; i++)
        {
            m_EnemyStartPos[i] = m_Enemies[i].transform.position;
        }
    }
    
    void RestorePos()
    {
        m_Player.transform.position = m_PlayerStartPos;
        for (int i = 0; i < m_EnemyStartPos.Length; i++)
        {
            m_Enemies[i].transform.position = m_EnemyStartPos[i];
        }
    }
    private void OnDestroy()
    {
        m_GameModel.PropertyValueChanged -= onGameModelChanged;
    }
    void onGameModelChanged(object sender, PropertyValueChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "PlayerDead":
                if (m_GameModel.PlayerDead)
                {
                    PauseGame();
                    PopupManager.Instance.ShowPopup<GameoverPopup>(PopupType.GameoverPopup, new PopupData()).Forget();
                }
                break;
            case "PatienceBar":
                if(m_GameModel.PatienceBar<=0)
                {
                    PauseGame();
                    m_GameModel.IsGameOver = true;
                    PopupManager.Instance.ShowPopup<GameoverPopup>(PopupType.GameoverPopup, (new PopupData(null,()=> { Time.timeScale = 1; LevelManager.Instance.TransitionToScene("BattleScene").Forget(); }))).Forget();
                }
                break;
            case "FullBar":
                if(m_GameModel.FullBar>=100)
                {
                    PauseGame();
                    PopupManager.Instance.ShowPopup<WinPopup>(PopupType.WinPopup, new PopupData()).Forget();
                }
                break;
            case "IsGameOver":
                PauseGame();
                break;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (m_GameModel.IsPaused)
            return;
        decreasePatienceBar();
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        m_GameModel.IsPaused = true; 
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        m_GameModel.IsPaused = false;
    }

    public void RestartGame()
    {
        m_GameModel.Reset();
        StartCoroutine(WaitUntilStart());
    }
    public void ResetGame()
    {
        m_GameModel.RestartGame();
        StartCoroutine(GameStart());
    }
    public void PrepareSushi()
    {

    }

    void ResetGameObjects()
    {
        for (int i = 0; i < m_Enemies.Length; i++)
        {
            //Need to update the position of enemy motors, otherwise go wrong 
            m_Enemies[i].GetComponent<GridBasedMovement>().Initialize();
            m_Enemies[i].Initalize();
        }
    }
    void decreasePatienceBar()
    {
        m_GameModel.PatienceBar -= Time.deltaTime;
    }
    //Increase PatienceBar when sushi delivered
    public void IncreasePatienceBar()
    {
        m_GameModel.PatienceBar += 30;
        if(m_GameModel.PatienceBar>100)
        {
            m_GameModel.PatienceBar = 100;
        }
    }
    public void IncreaseFullBar(SushiResultModel result)
    {
        m_GameModel.FullBar += 20;
    }
    public void CheckSushiResult(SushiResultModel result)
    {
        m_GameModel.Score += result.FinalScore;
    }
    IEnumerator WaitUntilStart()
    {
        RestorePos();
        yield return new WaitForSecondsRealtime(2);
        ResumeGame();
        ResetGameObjects();
    }
    IEnumerator GameStart()
    {
        RestorePos();
        m_GameModel.RestartGame();
        yield return new WaitForSeconds(0.1f);
        EventSystem.Instance.SendEvent(typeof(GameStartEvent), new GameStartEvent());
        yield return new WaitForSecondsRealtime(2);
        ResumeGame();
        ResetGameObjects();
    }
}
public class GameStartEvent:IEventHandler
{

}
//Send this event to remind SushiGenerationController
public class SushiCheckedEvent : IEventHandler
{

}
public class SliceSetEvent :IEventHandler
{
    public Vector3 pos;
    public SliceType sliceType;
    public SliceSetEvent(Vector3 pos, SliceType slicetype, bool isHitByOther)
    {
        this.pos = pos;
        sliceType = slicetype;
    }
}
public class SliceDropEvent : IEventHandler
{
    public Vector3 lastPos;
    public Vector3 nowPos;
    public SliceType sliceType;
    public SliceDropEvent(Vector3 lastpos ,Vector3 nowpos, SliceType slicetype)
    {
        lastPos = lastpos;
        nowPos = nowpos;
        sliceType = slicetype;
    }
}
public class SliceFallEvent : IEventHandler
{
    public Vector3 LastPos;
    public Vector3 CurPos;
    public SliceType sliceType;
    public bool IsHitByOther;
    public SliceFallEvent(Vector3 lastPos, Vector3 curPos, SliceType slicetype, bool isHitByOther)
    {
        this.LastPos = lastPos;
        this.CurPos = curPos;
        sliceType = slicetype;
        this.IsHitByOther = isHitByOther;
    }
}