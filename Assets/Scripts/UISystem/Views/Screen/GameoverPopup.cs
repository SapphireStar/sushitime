using Isekai.UI.Views.Screens;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameoverPopup : MonoBehaviour, IPopup
{
    public PopupData Data { get; set; }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            GameManager.Instance.RestartGame();
            Destroy(gameObject);
        }
    }
    public void OnCancelClicked()
    {

    }

    public void OnConfirmClicked()
    {

    }
}
