using Cysharp.Threading.Tasks;
using Isekai.Managers;
using Isekai.UI.ViewModels.Screens;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Isekai.UI.Views.Screens
{
    public class MainMenuScreen : Screen<MainMenuViewModel>
    {
        public override void OnEnterScreen()
        {
            
        }

        public override void OnExitScreen()
        {
            
        }
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Z))
            {
                OnNewGameClicked();
            }
        }
        public void OnNewGameClicked()
        {
            LevelManager.Instance.TransitionToScene("BattleScene",()=> 
            {
                ScreenManager.Instance.TransitionToInstant(Isekai.UI.EScreenType.HUDScreen, ELayerType.HUDLayer, new HUDScreenViewModel()).Forget();
            }).Forget();
        }
        public void OnSettingsClicked()
        {
            SettingsViewModel viewmodel = new SettingsViewModel();
            ScreenManager.Instance.TransitionTo(EScreenType.SettingsScreen, ELayerType.DefaultLayer, viewmodel).Forget();
        }

    }

}
