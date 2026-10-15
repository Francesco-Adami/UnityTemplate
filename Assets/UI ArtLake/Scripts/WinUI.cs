using Project.Core;
using Project.GameState;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ArtLake.UI
{
    public class WinUI : BaseUI
    {
        [Header("Win UI Buttons")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Events (Optional)")]
        [SerializeField] private UnityEvent onRestartClicked;
        [SerializeField] private UnityEvent onNextLevelClicked;
        [SerializeField] private UnityEvent onMainMenuClicked;

        public override bool DismissOnCancel => false;

        private void Reset()
        {
            panelType = PanelType.Popup;
        }

        private void OnEnable()
        {
            if (restartButton != null) restartButton.onClick.AddListener(Restart);
            if (nextLevelButton != null) nextLevelButton.onClick.AddListener(NextLevel);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        private void OnDisable()
        {
            if (restartButton != null) restartButton.onClick.RemoveListener(Restart);
            if (nextLevelButton != null) nextLevelButton.onClick.RemoveListener(NextLevel);
            if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(GoToMainMenu);
        }

        public async void Restart()
        {
            onRestartClicked?.Invoke();
            UIManager.Instance.Hide(this);

            if (ServiceLocator.TryGet<IGameStateManager>(out var gsm))
                gsm.ChangeState(GameState.Gameplay);

            if (ServiceLocator.TryGet<ISceneLoader>(out var sceneLoader))
                await sceneLoader.SwitchSceneAsync("GamePlay");
        }

        public void NextLevel()
        {
            onNextLevelClicked?.Invoke();
            UIManager.Instance.Hide(this);
        }

        public async void GoToMainMenu()
        {
            onMainMenuClicked?.Invoke();

            if (ServiceLocator.TryGet<IGameStateManager>(out var gsm))
                gsm.ChangeState(GameState.MainMenu);

            if (!ServiceLocator.TryGet<ISceneLoader>(out var sceneLoader)) return;
            bool success = await sceneLoader.SwitchSceneAsync("MainMenu");

            if (success && UIManager.Instance != null)
                UIManager.Instance.Show<MainMenuUI>();
        }
    }
}
