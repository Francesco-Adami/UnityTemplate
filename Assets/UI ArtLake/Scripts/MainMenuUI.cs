using Project.Core;
using Project.GameState;
using Project.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ArtLake.UI
{
    public class MainMenuUI : BaseUI
    {
        [Header("Main Menu UI")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button exitButton;

        [Header("Scena Target")]
        [SerializeField] private string lobbySceneName = "Lobby";

        [Header("Events (Optional)")]
        [SerializeField] private UnityEvent onPlay;

        private void Reset()
        {
            panelType = PanelType.Screen;
        }

        private void OnEnable()
        {
            if (playButton != null) playButton.onClick.AddListener(GoToLobby);
            if (optionsButton != null) optionsButton.onClick.AddListener(GoToOptions);
            if (exitButton != null) exitButton.onClick.AddListener(ExitGame);
        }

        private void OnDisable()
        {
            if (playButton != null) playButton.onClick.RemoveListener(GoToLobby);
            if (optionsButton != null) optionsButton.onClick.RemoveListener(GoToOptions);
            if (exitButton != null) exitButton.onClick.RemoveListener(ExitGame);
        }

        public async void GoToLobby()
        {
            onPlay?.Invoke();

            if (!ServiceLocator.TryGet<ISceneLoader>(out var sceneLoader)) return;
            bool success = await sceneLoader.SwitchSceneAsync(lobbySceneName);

            if (!success) return;

            if (ServiceLocator.TryGet<IGameStateManager>(out var gsm))
                gsm.ChangeState(GameState.Lobby);

            if (UIManager.Instance != null)
                UIManager.Instance.Show<HudUI>();
        }

        public void GoToOptions()
        {
            UIManager.Instance.Show<OptionUI>();
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
