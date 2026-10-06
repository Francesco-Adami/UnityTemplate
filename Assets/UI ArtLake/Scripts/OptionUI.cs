using UnityEngine;
using UnityEngine.UI;

namespace ArtLake.UI
{
    public class OptionUI : BaseUI
    {
        [Header("Option UI")]
        [SerializeField] private Button backButton;

        private void Reset()
        {
            panelType = PanelType.Popup;
        }

        private void OnEnable()
        {
            if (backButton != null) backButton.onClick.AddListener(Back);
        }

        private void OnDisable()
        {
            if (backButton != null) backButton.onClick.RemoveListener(Back);
        }

        public void Back()
        {
            UIManager.Instance.Back();
        }

        public void GoToMainMenu()
        {
            UIManager.Instance.Show<MainMenuUI>();
        }
    }
}
