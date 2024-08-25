using Isekai.UI.ViewModels.Screens;
using Isekai.UI.Views.Widgets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai.UI.Views.Screens
{
    public class HUDScreen : Screen<HUDScreenViewModel>
    {
        GameModel m_gameModel;
        public Image PatienceBar;
        public override void OnEnterScreen()
        {
            m_gameModel = ModelManager.Instance.GetModel<GameModel>(typeof(GameModel));
            m_gameModel.PropertyValueChanged += handleGameModelChanged;
        }
        void handleGameModelChanged(object sender, PropertyValueChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "PatienceBar":
                    PatienceBar.fillAmount = m_gameModel.PatienceBar / m_gameModel.MaxPatienceBar;
                    break;
                default:
                    break;
            }
        }
        private void OnDestroy()
        {
            m_gameModel.PropertyValueChanged -= handleGameModelChanged;
        }
        public override void OnExitScreen()
        {
            
        }
    }
}

