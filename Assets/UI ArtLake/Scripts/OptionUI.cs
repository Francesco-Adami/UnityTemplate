using Project.Audio;
using Project.Core;
using Project.Save;
using UnityEngine;
using UnityEngine.UI;

namespace ArtLake.UI
{
    public class OptionUI : BaseUI
    {
        [Header("Navigazione")]
        [SerializeField] private Button backButton;

        [Header("Volume Sliders")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Grafica")]
        [SerializeField] private Toggle fullscreenToggle;

        private void Reset()
        {
            panelType = PanelType.Popup;
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            SyncUIFromSettings();
        }

        protected override void OnClose()
        {
            base.OnClose();
            SaveSettings();
        }

        private void OnEnable()
        {
            if (backButton != null) backButton.onClick.AddListener(Back);

            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (musicVolumeSlider  != null) musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            if (sfxVolumeSlider    != null) sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            if (fullscreenToggle   != null) fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }

        private void OnDisable()
        {
            if (backButton != null) backButton.onClick.RemoveListener(Back);

            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            if (musicVolumeSlider  != null) musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            if (sfxVolumeSlider    != null) sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            if (fullscreenToggle   != null) fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        }

        public void Back()
        {
            SaveSettings();
            UIManager.Instance.Back();
        }

        public void GoToMainMenu()
        {
            SaveSettings();
            UIManager.Instance.Show<MainMenuUI>();
        }

        // ── Sincronizzazione ──────────────────────────────────────────────────

        private void SyncUIFromSettings()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;

            if (masterVolumeSlider != null) masterVolumeSlider.SetValueWithoutNotify(save.Settings.MasterVolume);
            if (musicVolumeSlider  != null) musicVolumeSlider.SetValueWithoutNotify(save.Settings.MusicVolume);
            if (sfxVolumeSlider    != null) sfxVolumeSlider.SetValueWithoutNotify(save.Settings.SfxVolume);
            if (fullscreenToggle   != null) fullscreenToggle.SetIsOnWithoutNotify(save.Settings.IsFullscreen);
        }

        private void SaveSettings()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;
            save.SaveSettings();
        }

        // ── Callbacks ─────────────────────────────────────────────────────────

        private void OnMasterVolumeChanged(float v)
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.SetMasterVolume(v);
            if (ServiceLocator.TryGet<ISaveService>(out var save))   save.Settings.MasterVolume = v;
        }

        private void OnMusicVolumeChanged(float v)
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.SetMusicVolume(v);
            if (ServiceLocator.TryGet<ISaveService>(out var save))   save.Settings.MusicVolume = v;
        }

        private void OnSfxVolumeChanged(float v)
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.SetSfxVolume(v);
            if (ServiceLocator.TryGet<ISaveService>(out var save))   save.Settings.SfxVolume = v;
        }

        private void OnFullscreenChanged(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            if (ServiceLocator.TryGet<ISaveService>(out var save)) save.Settings.IsFullscreen = isFullscreen;
        }
    }
}
