using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ArtLake.UI
{
    public class PauseUI : BaseUI
    {
        [Header("Pause UI Configuration")]
        [SerializeField] private bool pauseTimeScale = true;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Events (Optional)")]
        [SerializeField] private UnityEvent onMainMenuClicked;

        private void Reset()
        {
            panelType = PanelType.Popup;
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            if (pauseTimeScale)
            {
                Time.timeScale = 0f;
            }
        }

        protected override void OnClose()
        {
            base.OnClose();
            if (pauseTimeScale)
            {
                Time.timeScale = 1f;
            }
        }

        private void OnEnable()
        {
            if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
            if (optionsButton != null) optionsButton.onClick.AddListener(OpenOptions);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        private void OnDisable()
        {
            if (resumeButton != null) resumeButton.onClick.RemoveListener(Resume);
            if (optionsButton != null) optionsButton.onClick.RemoveListener(OpenOptions);
            if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(GoToMainMenu);
        }

        public void Resume()
        {
            UIManager.Instance.Hide(this);
        }

        public void OpenOptions()
        {
            UIManager.Instance.Show<OptionUI>();
        }

        public void GoToMainMenu()
        {
            if (pauseTimeScale)
            {
                Time.timeScale = 1f;
            }

            onMainMenuClicked?.Invoke();
            UIManager.Instance.Show<MainMenuUI>();
        }

        public void GoToHud()
        {
            Resume();
        }
    }
}
