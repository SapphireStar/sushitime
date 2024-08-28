using Isekai.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinPopup : MonoBehaviour, IPopup
{
    public PopupData Data { get;set; }

    public void OnCancelClicked()
    {
    }

    public void OnConfirmClicked()
    {
        Time.timeScale = 1;
        LevelManager.Instance.TransitionToScene("BattleScene").Forget();
    }

    // Start is called before the first frame update
    void Start()
    {
        
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
