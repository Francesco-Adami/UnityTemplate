using Project.Core;
using Project.GameState;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Project.UI;

namespace ArtLake.UI
{
    public class PauseUI : BaseUI
    {
        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button quitToLobbyButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Scena Target")]
        [SerializeField] private string gamePlaySceneName = "GamePlay";
        [SerializeField] private string lobbySceneName    = "Lobby";
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Events (Optional)")]
        [SerializeField] private UnityEvent onQuitToLobbyClicked;
        [SerializeField] private UnityEvent onMainMenuClicked;

        [Header("Settings")]
        [SerializeField] private bool requireConfirmation = false;

        private void Reset()
        {
            panelType = PanelType.Popup;
        }

        private void OnEnable()
        {
            if (resumeButton      != null) resumeButton.onClick.AddListener(Resume);
            if (optionsButton     != null) optionsButton.onClick.AddListener(OpenOptions);
            if (startGameButton   != null) startGameButton.onClick.AddListener(StartGame);
            if (quitToLobbyButton != null) quitToLobbyButton.onClick.AddListener(OnQuitToLobbyClicked);
            if (mainMenuButton    != null) mainMenuButton.onClick.AddListener(OnQuitToMainMenuClicked);
        }

        private void OnDisable()
        {
            if (resumeButton      != null) resumeButton.onClick.RemoveListener(Resume);
            if (optionsButton     != null) optionsButton.onClick.RemoveListener(OpenOptions);
            if (startGameButton   != null) startGameButton.onClick.RemoveListener(StartGame);
            if (quitToLobbyButton != null) quitToLobbyButton.onClick.RemoveListener(OnQuitToLobbyClicked);
            if (mainMenuButton    != null) mainMenuButton.onClick.RemoveListener(OnQuitToMainMenuClicked);
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            RefreshContextualButtons();
        }

        protected override void OnClose()
        {
            base.OnClose();

            if (ServiceLocator.TryGet<IGameStateManager>(out var gsm))
            {
                if (gsm.CurrentState == GameState.Paused)
                {
                    gsm.ResumeGame();
                }
            }
        }

        // ── Buttons interactable contestuali ─────────────────────────────────

        private void RefreshContextualButtons()
        {
            if (!ServiceLocator.TryGet<IGameStateManager>(out var gsm)) return;

            bool pausedFromLobby    = gsm.StateBeforePause == GameState.Lobby;
            bool pausedFromGameplay = gsm.StateBeforePause == GameState.Gameplay;

            if (startGameButton != null)
            {
                startGameButton.gameObject.SetActive(pausedFromLobby);
                startGameButton.interactable = true;
            }
            if (quitToLobbyButton != null)
            {
                quitToLobbyButton.gameObject.SetActive(pausedFromGameplay);
                quitToLobbyButton.interactable = true;
            }
        }

        // ── Azioni pulsanti ───────────────────────────────────────────────────

        public void Resume()
        {
            UIManager.Instance.Hide(this);
        }

        public void OpenOptions()
        {
            UIManager.Instance.Show<OptionUI>();
        }

        public async void StartGame()
        {
            if (!ServiceLocator.TryGet<ISceneLoader>(out var sceneLoader)) return;
            bool success = await sceneLoader.SwitchSceneAsync(gamePlaySceneName);

            if (!success) return;

            if (ServiceLocator.TryGet<IGameStateManager>(out var gsm))
                gsm.ChangeState(GameState.Gameplay);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.Hide<PauseUI>();
                UIManager.Instance.Show<HudUI>();
            }
        }

        private void OnQuitToLobbyClicked()
        {
            if (requireConfirmation) ConfirmGoToLobby();
            else GoToLobby();
        }

        private void OnQuitToMainMenuClicked()
        {
            if (requireConfirmation) ConfirmGoToMainMenu();
            else GoToMainMenu();
        }

        public void ConfirmGoToLobby()
        {
            var popup = UIManager.Instance.Show<ConfirmPopupUI>();
            if (popup != null)
            {
                popup.ShowPopup("Are you sure you want to return to Lobby?", GoToLobby);
            }
        }

        public void ConfirmGoToMainMenu()
        {
            var popup = UIManager.Instance.Show<ConfirmPopupUI>();
            if (popup != null)
            {
                popup.ShowPopup("Are you sure you want to return to Main Menu?", GoToMainMenu);
            }
        }

        public async void GoToLobby()
        {
            onQuitToLobbyClicked?.Invoke();

            if (UIManager.Instance != null)
                UIManager.Instance.Hide<PauseUI>();

            if (ServiceLocator.TryGet<IGameStateManager>(out var gsm))
                gsm.ChangeState(GameState.Lobby);

            if (!ServiceLocator.TryGet<ISceneLoader>(out var sceneLoader)) return;
            bool success = await sceneLoader.SwitchSceneAsync(lobbySceneName);

            if (success && UIManager.Instance != null)
                UIManager.Instance.Show<HudUI>();
        }

        public async void GoToMainMenu()
        {
            onMainMenuClicked?.Invoke();

            if (UIManager.Instance != null)
                UIManager.Instance.Hide<PauseUI>();

            if (ServiceLocator.TryGet<IGameStateManager>(out var gsm))
                gsm.ChangeState(GameState.MainMenu);

            if (!ServiceLocator.TryGet<ISceneLoader>(out var sceneLoader)) return;
            bool success = await sceneLoader.SwitchSceneAsync(mainMenuSceneName);

            if (success && UIManager.Instance != null)
                UIManager.Instance.Show<MainMenuUI>();
        }
    }
}
