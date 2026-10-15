using ArtLake.UI;
using Project.Core;
using Project.GameState;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    /// <summary>
    /// Schermata Lobby: staging area pre-partita.
    /// Pulsante "Avvia Partita" -> carica GamePlay via ISceneLoader.
    /// Pulsante "Indietro"      -> torna al MainMenu.
    /// </summary>
    public class LobbyUI : BaseUI
    {
        [Header("Buttons")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button backButton;

        [Header("Scene Names")]
        [SerializeField] private string gamePlaySceneName  = "GamePlay";
        [SerializeField] private string mainMenuSceneName  = "MainMenu";

        private void Reset()
        {
            panelType = PanelType.Screen;
        }

        private void OnEnable()
        {
            if (startGameButton != null) startGameButton.onClick.AddListener(StartGame);
            if (backButton      != null) backButton.onClick.AddListener(BackToMainMenu);
        }

        private void OnDisable()
        {
            if (startGameButton != null) startGameButton.onClick.RemoveListener(StartGame);
            if (backButton      != null) backButton.onClick.RemoveListener(BackToMainMenu);
        }

        public async void StartGame()
        {
            if (!ServiceLocator.TryGet<ISceneLoader>(out var sceneLoader)) return;
            bool success = await sceneLoader.SwitchSceneAsync(gamePlaySceneName);

            if (!success) return;

            if (ServiceLocator.TryGet<IGameStateManager>(out var gsm))
                gsm.ChangeState(Project.GameState.GameState.Gameplay);

            if (UIManager.Instance != null)
                UIManager.Instance.Show<HudUI>();
        }

        public async void BackToMainMenu()
        {
            if (!ServiceLocator.TryGet<ISceneLoader>(out var sceneLoader)) return;
            bool success = await sceneLoader.SwitchSceneAsync(mainMenuSceneName);

            if (!success) return;

            if (ServiceLocator.TryGet<IGameStateManager>(out var gsm))
                gsm.ChangeState(Project.GameState.GameState.MainMenu);

            if (UIManager.Instance != null)
                UIManager.Instance.Show<MainMenuUI>();
        }
    }
}
