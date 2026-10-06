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

        public void Restart()
        {
            onRestartClicked?.Invoke();
            UIManager.Instance.Hide(this);
        }

        public void NextLevel()
        {
            onNextLevelClicked?.Invoke();
            UIManager.Instance.Hide(this);
        }

        public void GoToMainMenu()
        {
            onMainMenuClicked?.Invoke();
            UIManager.Instance.Show<MainMenuUI>();
        }
    }
}
