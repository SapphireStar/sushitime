using Isekai.Managers;
using Isekai.UI.ViewModels.Screens;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms.Impl;
using System.IO;

public class WinPopup : MonoBehaviour, IPopup
{
    public PopupData Data { get;set; }
    public Text ScoreText;
    public GameObject PositionPrefab;
    public Transform LeaderBoardParent;
    public int MaxPositions = 10;

    GameModel model;
#if UNITY_EDITOR
        private string jsonPath = Application.streamingAssetsPath + "/JsonTest.json";
#else
    private string jsonPath = Path.Combine(Application.streamingAssetsPath, "/JsonTest.json");
#endif    
    private LeaderBoard leaderBoard;
    private void Start()
    {
        model = ModelManager.Instance.GetModel<GameModel>(typeof(GameModel));
        ScoreText.text = $"Score: {model.Score}";
        RefreshLeaderBoard();
    }
    public void OnCancelClicked()
    {
    }

    public void OnConfirmClicked()
    {
        Time.timeScale = 1;
        LevelManager.Instance.TransitionToScene("BattleScene", () =>
        {
            ScreenManager.Instance.TransitionToInstant(Isekai.UI.EScreenType.HUDScreen, ELayerType.HUDLayer, new HUDScreenViewModel()).Forget();
        }).Forget();
    }
    public void RefreshLeaderBoard()
    {
        ReadJson();
        int lastScore = PlayerPrefs.GetInt(model.PlayerName);

        if(model.Score > lastScore)
        {
            PlayerPrefs.SetInt(model.PlayerName, model.Score);
        }
        
        
        leaderBoard.Players.Sort((string a, string b) =>
        {
            return PlayerPrefs.GetInt(b)- PlayerPrefs.GetInt(a);
        });

        int maxPositions = Mathf.Min(MaxPositions, leaderBoard.TotalPlayers);
        if (leaderBoard.TotalPlayers>3)
        {
            
            for(int i = 3; i < maxPositions; i++)
            {
                Instantiate(PositionPrefab, LeaderBoardParent).SetActive(true);
            }
            
        }

        for(int i = 0; i < maxPositions; i++)
        {
            LeaderBoardParent.GetChild(i).GetComponent<LeaderBoardWidget>().Initialize(leaderBoard.Players[i],PlayerPrefs.GetInt(leaderBoard.Players[i]), i + 1);
        }
        WriteJson();
        PlayerPrefs.Save();
    }

    void ReadJson()
    {
        
        if (!File.Exists(jsonPath))
        {
            Debug.Log("JSON not exists");
        }
        string json = File.ReadAllText(jsonPath);
        leaderBoard = JsonUtility.FromJson<LeaderBoard>(json);
    }
    void WriteJson()
    {
        if (!File.Exists(jsonPath))
        {
            File.Create(jsonPath);
        }
        string json = JsonUtility.ToJson(leaderBoard, true);
        File.WriteAllText(jsonPath, json);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            OnConfirmClicked();
            Destroy(gameObject);
        }
            
    }
}
