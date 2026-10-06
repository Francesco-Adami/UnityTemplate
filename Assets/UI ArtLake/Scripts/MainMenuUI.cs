using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ArtLake.UI
{
    public class MainMenuUI : BaseUI
    {
        [Header("Main Menu UI")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button OptionsButton;
        [SerializeField] private Button ExitButton;

        [Header("Events (Optional)")]
        [SerializeField] private UnityEvent onPlay;

        private void Reset()
        {
            panelType = PanelType.Screen;
        }

        private void OnEnable()
        {
            if (playButton != null) playButton.onClick.AddListener(GoToHud);
            if (OptionsButton != null) OptionsButton.onClick.AddListener(GoToOptions);
            if (ExitButton != null) ExitButton.onClick.AddListener(ExitGame);
        }

        private void OnDisable()
        {
            if (playButton != null) playButton.onClick.RemoveListener(GoToHud);
            if (OptionsButton != null) OptionsButton.onClick.RemoveListener(GoToOptions);
            if (ExitButton != null) ExitButton.onClick.RemoveListener(ExitGame);
        }

        public void GoToHud()
        {
            onPlay?.Invoke();
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
