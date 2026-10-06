using UnityEngine;
using UnityEngine.UI;

namespace ArtLake.UI
{
    public class HudUI : BaseUI
    {
        [Header("HUD Configuration")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private bool listenToEscapeKey = true;

        private void Reset()
        {
            panelType = PanelType.Screen;
        }

        private void OnEnable()
        {
            if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
        }

        private void OnDisable()
        {
            if (pauseButton != null) pauseButton.onClick.RemoveListener(TogglePause);
        }

        private void Update()
        {
            if (listenToEscapeKey && Input.GetKeyUp(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            if (UIManager.Instance == null) return;

            if (UIManager.Instance.IsOpen<PauseUI>())
            {
                UIManager.Instance.Hide<PauseUI>();
            }
            else
            {
                UIManager.Instance.Show<PauseUI>();
            }
        }
    }
}
