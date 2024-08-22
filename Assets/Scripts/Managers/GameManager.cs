using Cysharp.Threading.Tasks;
using Isekai.Managers;
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

        m_Enemies = transform.GetComponentsInChildren<BaseEnemy>();

        RecordInitialPos();
        StartCoroutine(WaitUntilStart());
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
        }
    }
    // Update is called once per frame
    void Update()
    {
        
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

    void ResetGameObjects()
    {
        for (int i = 0; i < m_Enemies.Length; i++)
        {
            //Need to update the position of enemy motors, otherwise go wrong 
            m_Enemies[i].GetComponent<GridBasedMovement>().Initialize();
            m_Enemies[i].Initalize();
        }
    }
    IEnumerator WaitUntilStart()
    {
        RestorePos();
        yield return new WaitForSecondsRealtime(2);
        ResumeGame();
        ResetGameObjects();
    }
}
