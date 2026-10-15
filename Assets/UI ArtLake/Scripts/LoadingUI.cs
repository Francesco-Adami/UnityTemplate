using Project.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ArtLake.UI
{
    /// <summary>
    /// Schermata di caricamento animata e moderna (LoadingUI).
    /// Si aggancia automaticamente a ISceneLoader per mostrare barra di progresso,
    /// spinner rotante e micro-animazioni fluide indipendenti dal Time.timeScale.
    /// </summary>
    public class LoadingUI : BaseUI
    {
        [Header("UI References")]
        [SerializeField] private Image progressBarFill;
        [SerializeField] private Text progressText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text tipText;
        [SerializeField] private RectTransform spinnerTransform;

        [Header("Animation Settings")]
        [SerializeField] private float spinnerSpeed = 260f;
        [SerializeField] private float smoothSpeed = 6f;
        [SerializeField] private float fadeDuration = 0.22f;
        [SerializeField] private float minDisplayDuration = 0.5f;

        [Header("Tips")]
        [SerializeField] private string[] tips = new string[]
        {
            "Preparing game world...",
            "Loading assets & shaders...",
            "Initializing environment...",
            "Almost ready..."
        };

        public override bool DismissOnCancel => false;

        private ISceneLoader _sceneLoader;
        private float _currentProgress;
        private float _targetProgress;
        private float _displayTimer;
        private float _fadeTimer;
        private float _dotTimer;
        private float _tipTimer;
        private int _dotCount;
        private int _tipIndex;
        private bool _isSceneLoading;
        private bool _isFadingIn;
        private bool _isFadingOut;

        private void Reset()
        {
            panelType = PanelType.Popup;
        }

        protected override void Awake()
        {
            base.Awake();
            panelType = PanelType.Popup;
        }

        private void Start()
        {
            if (ServiceLocator.TryGet<ISceneLoader>(out var sceneLoader))
            {
                _sceneLoader = sceneLoader;
                _sceneLoader.OnLoadStarted     += HandleLoadStarted;
                _sceneLoader.OnProgressUpdated += HandleProgressUpdated;
                _sceneLoader.OnLoadCompleted   += HandleLoadCompleted;
            }
        }

        private void OnDestroy()
        {
            if (_sceneLoader != null)
            {
                _sceneLoader.OnLoadStarted     -= HandleLoadStarted;
                _sceneLoader.OnProgressUpdated -= HandleProgressUpdated;
                _sceneLoader.OnLoadCompleted   -= HandleLoadCompleted;
            }
        }

        private void Update()
        {
            if (!IsOpen && !_isFadingOut) return;

            float dt = Time.unscaledDeltaTime;

            // 1. Spinner rotazione + pulsazione scala
            if (spinnerTransform != null)
            {
                spinnerTransform.Rotate(0f, 0f, -spinnerSpeed * dt);
                float pulse = 1f + 0.05f * Mathf.Sin(Time.unscaledTime * 4f);
                spinnerTransform.localScale = new Vector3(pulse, pulse, 1f);
            }

            // 2. Progressione fluida barra
            _currentProgress = Mathf.MoveTowards(_currentProgress, _targetProgress, smoothSpeed * dt);
            if (progressBarFill != null)
            {
                progressBarFill.fillAmount = _currentProgress;
            }
            if (progressText != null)
            {
                progressText.text = $"{Mathf.RoundToInt(_currentProgress * 100f)}%";
            }

            // 3. Testo di stato ciclico animato (LOADING, LOADING., LOADING.., LOADING...)
            _dotTimer += dt;
            if (_dotTimer >= 0.32f)
            {
                _dotTimer = 0f;
                _dotCount = (_dotCount + 1) % 4;
                if (statusText != null)
                {
                    statusText.text = "LOADING" + new string('.', _dotCount);
                }
            }

            // 4. Ciclo rotazione tips
            if (tips != null && tips.Length > 0 && tipText != null)
            {
                _tipTimer += dt;
                if (_tipTimer >= 3f)
                {
                    _tipTimer = 0f;
                    _tipIndex = (_tipIndex + 1) % tips.Length;
                    tipText.text = tips[_tipIndex];
                }
            }

            // 5. Gestione dissolvenza Fade-In
            if (_isFadingIn)
            {
                _fadeTimer += dt;
                canvasGroup.alpha = Mathf.Clamp01(_fadeTimer / fadeDuration);
                if (_fadeTimer >= fadeDuration)
                {
                    canvasGroup.alpha = 1f;
                    _isFadingIn = false;
                }
            }

            // 6. Monitoraggio durata minima e transizione verso Fade-Out
            if (!_isSceneLoading && !_isFadingIn && !_isFadingOut)
            {
                _displayTimer += dt;
                if (_displayTimer >= minDisplayDuration && _currentProgress >= 0.99f)
                {
                    StartFadeOut();
                }
            }

            // 7. Gestione dissolvenza Fade-Out
            if (_isFadingOut)
            {
                _fadeTimer += dt;
                canvasGroup.alpha = Mathf.Clamp01(1f - (_fadeTimer / fadeDuration));
                if (_fadeTimer >= fadeDuration)
                {
                    canvasGroup.alpha = 0f;
                    if (UIManager.Instance != null)
                        UIManager.Instance.Hide(this);
                    else
                        Close();
                }
            }
        }

        // ── Gestione eventi ISceneLoader ───────────────────────────────────────

        private void HandleLoadStarted()
        {
            ShowLoading();
        }

        private void HandleProgressUpdated(float progress)
        {
            // Unity AsyncOperation progredisce fino a 0.9 prima dell'attivazione
            _targetProgress = Mathf.Clamp01(progress / 0.9f);
        }

        private void HandleLoadCompleted()
        {
            _targetProgress = 1f;
            _isSceneLoading = false;
        }

        // ── API Pubblica ──────────────────────────────────────────────────────

        public void ShowLoading(string customStatus = null, string customTip = null)
        {
            _currentProgress = 0f;
            _targetProgress  = 0.05f;
            _displayTimer    = 0f;
            _fadeTimer       = 0f;
            _isSceneLoading  = true;
            _isFadingIn      = true;
            _isFadingOut     = false;

            if (statusText != null)
                statusText.text = !string.IsNullOrEmpty(customStatus) ? customStatus : "LOADING...";

            if (tipText != null && tips != null && tips.Length > 0)
            {
                _tipIndex = Random.Range(0, tips.Length);
                tipText.text = !string.IsNullOrEmpty(customTip) ? customTip : tips[_tipIndex];
            }

            if (progressBarFill != null) progressBarFill.fillAmount = 0f;
            if (progressText != null) progressText.text = "0%";

            if (UIManager.Instance != null)
            {
                UIManager.Instance.Show(this);
            }
            else
            {
                Open();
            }

            // Inizializza con alpha zero per il fade-in morbido
            canvasGroup.alpha = 0f;
        }

        public void HideLoading()
        {
            _targetProgress = 1f;
            _isSceneLoading = false;
        }

        public void SetProgress(float value)
        {
            _targetProgress = Mathf.Clamp01(value);
        }

        private void StartFadeOut()
        {
            _isFadingOut = true;
            _fadeTimer   = 0f;
        }
    }
}
