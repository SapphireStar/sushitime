using Isekai.UI.Views.Screens;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameoverPopup : MonoBehaviour, IPopup
{
    public PopupData Data { get; set; }
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
