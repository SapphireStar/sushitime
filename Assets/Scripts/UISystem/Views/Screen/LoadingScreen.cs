using Cysharp.Threading.Tasks;
using Isekai.Managers;
using Isekai.UI.ViewModels.Screens;
using Isekai.UI.Views.Widgets;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai.UI.Views.Screens
{
    public class LoadingScreen : Screen<LoadingViewModel>
    {
        [SerializeField]
        private Transform m_loadingAnim;
        [SerializeField]
        private TextMeshProUGUI m_loadingFinishNotify;
        [SerializeField]
        private float m_minimumLoadingTime;
        [SerializeField]
        private InputField m_inputField;

        private bool m_continue;
        private GameModel gamemodel;
#if UNITY_EDITOR
        private string jsonPath = Application.streamingAssetsPath + "/JsonTest.json";
#else
        private string jsonPath = "./StreamingAssets/JsonTest.json";
#endif
        private LeaderBoard leaderBoard;
        public override void OnEnterScreen()
        {
            curValue = 0;
            gamemodel = ModelManager.Instance.GetModel<GameModel>(typeof(GameModel));
            if (gamemodel.IsInputName)
            {
                m_continue = true;
            }
            m_inputField.gameObject.SetActive(false);
            Loading().Forget();
        }

        
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Return))
            {
                m_continue = true;
                if (m_inputField.text.Length>0)
                {
                    gamemodel.PlayerName = m_inputField.text;
                }
                else
                {
                    int num = PlayerPrefs.GetInt("totalplayers", 0);
                    gamemodel.PlayerName = $"Player{num}";
                }

                gamemodel.IsInputName = true;
                m_continue = true;

                ReadJson();

                if(!leaderBoard.Players.Contains(gamemodel.PlayerName))
                {
                    leaderBoard.TotalPlayers++;
                    leaderBoard.Players.Add(gamemodel.PlayerName);
                }

                WriteJson();

            }
        }
        float curValue = 0;
        void ReadJson()
        {

            if (PlayerPrefs.GetString("json").Length == 0)
            {
                Debug.Log("JSON not exists");
                leaderBoard = new LeaderBoard();
                leaderBoard.Players = new List<string>();
            }
            else
            {
                string json = PlayerPrefs.GetString("json");
                leaderBoard = JsonUtility.FromJson<LeaderBoard>(json);
            }

        }
        void WriteJson()
        {
            string json = JsonUtility.ToJson(leaderBoard, true);
            PlayerPrefs.SetString("json", json);
            PlayerPrefs.Save();
        }
        async UniTaskVoid Loading()
        {
            while (curValue < 1 )
            {
                //m_progressBar.SetFillValue(curValue);
                //curValue += Time.deltaTime * 0.4f;
                await UniTask.Yield(this.GetCancellationTokenOnDestroy());
            }

            while (m_minimumLoadingTime > 0)
            {
                m_minimumLoadingTime -= Time.deltaTime;
                Debug.Log(m_minimumLoadingTime);
                await UniTask.Yield(this.GetCancellationTokenOnDestroy());
            }

            m_loadingAnim.gameObject.SetActive(false);

            m_inputField.gameObject.SetActive(true);

            m_inputField.ActivateInputField();

            while (!m_continue)
            {
                await UniTask.Yield(this.GetCancellationTokenOnDestroy());
            }
            ViewModel.LoadingComplete();
            GameManager.Instance.StartGame();
        }

        public override void HandleViewModelPropertyChanged(object sender, PropertyValueChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.LoadingProgress):
                    curValue = (float)e.Value;
                    break;
                default:
                    break;
            }
        }

        public override void OnExitScreen()
        {
            Debug.Log("Exit Loading Screen");
        }

        public void OnAnyKeyDown()
        {

        }

    }
}

