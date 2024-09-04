using Isekai.UI.Views.Screens;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameoverPopup : MonoBehaviour, IPopup
{
    public PopupData Data { get; set; }
    public Text ScoreText;
    private void Start()
    {
        GameModel model = ModelManager.Instance.GetModel<GameModel>(typeof(GameModel));
        ScoreText.text = $"Score: {model.Score}";
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Z))
        {
            if(Data.OnConfirmClicked == null)
            {
                GameManager.Instance.RestartGame();
                Destroy(gameObject);
            }
            else
            {
                GameManager.Instance.ResetGame();
                Data.OnConfirmClicked.Invoke();
                Destroy(gameObject);
            }
        }
    }
    public void OnCancelClicked()
    {

    }

    public void OnConfirmClicked()
    {

    }
}
