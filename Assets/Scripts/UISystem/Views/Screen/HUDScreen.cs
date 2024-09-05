using Isekai.UI.ViewModels.Screens;
using Isekai.UI.Views.Widgets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai.UI.Views.Screens
{
    public class HUDScreen : Screen<HUDScreenViewModel>
    {
        GameModel m_gameModel;
        public Image PatienceBar;
        public Image FullBar;
        public Text ScoreText;
        public Animator anim;
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
                    float percent = m_gameModel.PatienceBar / m_gameModel.MaxPatienceBar;
                    PatienceBar.fillAmount = percent;
                    if (percent > 0.4f)
                    {
                        anim.SetTrigger("satisfied");
                    }
                    if (percent <= 0.4f&&percent>0.2f)
                    {
                        anim.SetTrigger("gettingangry");
                    }
                    if (percent <=0.2f)
                    {
                        anim.SetTrigger("angry");
                    }
                    break;
/*                case "FullBar":
                    FullBar.fillAmount = m_gameModel.FullBar / m_gameModel.MaxFullBar;
                    break;*/
                case "Score":
                    ScoreText.text = $"SCORE: {m_gameModel.Score}";
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
            m_gameModel.PropertyValueChanged -= handleGameModelChanged;
        }
    }
}

