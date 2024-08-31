using Cysharp.Threading.Tasks;
using Isekai.Managers;
using Isekai.UI.ViewModels.Screens;
using Isekai.UI.Views.Widgets;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

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

        private bool m_continue;
        public override void OnEnterScreen()
        {
            curValue = 0;
            Loading().Forget();
        }

        
        private void Update()
        {

        }
        float curValue = 0;
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
            ViewModel.LoadingComplete();
            
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

