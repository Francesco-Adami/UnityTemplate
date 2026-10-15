using Project.Core;
using Project.GameState;
using Project.Input;
using UnityEngine;
using UnityEngine.UI;

namespace ArtLake.UI
{
    public class HudUI : BaseUI
    {
        [Header("HUD Configuration")]
        [SerializeField] private Button pauseButton;

        private IGameStateManager _gsm;

        private void Reset()
        {
            panelType = PanelType.Screen;
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            ResolveServices();
        }

        private void OnEnable()
        {
            if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
        }

        private void OnDisable()
        {
            if (pauseButton != null) pauseButton.onClick.RemoveListener(TogglePause);
        }

        private void ResolveServices()
        {
            ServiceLocator.TryGet<IGameStateManager>(out _gsm);
        }

        public void TogglePause()
        {
            if (_gsm == null)
            {
                if (!ServiceLocator.TryGet<IGameStateManager>(out _gsm)) return;
            }

            if (_gsm.CurrentState == GameState.Gameplay || _gsm.CurrentState == GameState.Lobby)
            {
                _gsm.PauseGame();
                UIManager.Instance.Show<PauseUI>();
            }
            else if (_gsm.CurrentState == GameState.Paused)
            {
                UIManager.Instance.Hide<PauseUI>();
            }
        }
    }
}
