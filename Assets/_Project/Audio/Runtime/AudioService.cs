using System.Collections;
using Project.Core;
using UnityEngine;

namespace Project.Audio
{
    /// <summary>
    /// Implementazione concreta di IAudioService.
    /// Montato sul GameObject "AudioService" nella scena Bootstrap (DontDestroyOnLoad).
    /// Gestisce due AudioSource interni:
    ///   - _musicSourceA / _musicSourceB: crossfade BGM
    ///   - _sfxSource: SFX one-shot
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioService : MonoBehaviour, IAudioService
    {
        [Header("Impostazioni Iniziali")]
        [Range(0f, 1f)] [SerializeField] private float initialMasterVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float initialMusicVolume  = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float initialSfxVolume    = 0.8f;

        // ── Canali Audio ─────────────────────────────────────────────────────
        private AudioSource _musicSourceA;
        private AudioSource _musicSourceB;
        private AudioSource _activeMusicSource;
        private AudioSource _sfxSource;

        // ── Volumi ───────────────────────────────────────────────────────────
        public float MasterVolume { get; private set; }
        public float MusicVolume  { get; private set; }
        public float SfxVolume    { get; private set; }

        private Coroutine _fadeCoroutine;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            // Riusa il primo AudioSource aggiunto dal RequireComponent
            _musicSourceA = gameObject.GetComponent<AudioSource>();
            ConfigureMusicSource(_musicSourceA);

            _musicSourceB = gameObject.AddComponent<AudioSource>();
            ConfigureMusicSource(_musicSourceB);

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.loop        = false;

            _activeMusicSource = _musicSourceA;

            MasterVolume = initialMasterVolume;
            MusicVolume  = initialMusicVolume;
            SfxVolume    = initialSfxVolume;

            ApplyMusicVolume();

            ServiceLocator.Register<IAudioService>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<IAudioService>();
        }

        // ── Controllo Volumi ─────────────────────────────────────────────────

        public void SetMasterVolume(float v)
        {
            MasterVolume = Mathf.Clamp01(v);
            ApplyMusicVolume();
        }

        public void SetMusicVolume(float v)
        {
            MusicVolume = Mathf.Clamp01(v);
            ApplyMusicVolume();
        }

        public void SetSfxVolume(float v)
        {
            SfxVolume = Mathf.Clamp01(v);
        }

        private void ApplyMusicVolume()
        {
            float effective = MasterVolume * MusicVolume;
            if (_activeMusicSource != null) _activeMusicSource.volume = effective;
        }

        // ── Musica ───────────────────────────────────────────────────────────

        public void PlayMusic(AudioClip clip, bool loop = true, float fadeDuration = 0.5f)
        {
            if (clip == null) return;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);

            AudioSource incoming = (_activeMusicSource == _musicSourceA) ? _musicSourceB : _musicSourceA;
            float targetVolume = MasterVolume * MusicVolume;

            incoming.clip   = clip;
            incoming.loop   = loop;
            incoming.volume = 0f;
            incoming.Play();

            _fadeCoroutine = StartCoroutine(CrossfadeRoutine(_activeMusicSource, incoming, targetVolume, fadeDuration));
            _activeMusicSource = incoming;
        }

        public void StopMusic(float fadeDuration = 0.5f)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeOutRoutine(_activeMusicSource, fadeDuration));
        }

        public void PauseMusic()  => _activeMusicSource?.Pause();
        public void ResumeMusic() => _activeMusicSource?.UnPause();

        // ── SFX ──────────────────────────────────────────────────────────────

        public void PlaySfx(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return;
            _sfxSource.PlayOneShot(clip, MasterVolume * SfxVolume * volumeScale);
        }

        public void PlaySfxAtPosition(AudioClip clip, Vector3 position, float volumeScale = 1f)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position, MasterVolume * SfxVolume * volumeScale);
        }

        // ── Coroutine Crossfade ───────────────────────────────────────────────

        private IEnumerator CrossfadeRoutine(AudioSource outgoing, AudioSource incoming, float targetVolume, float duration)
        {
            float elapsed = 0f;
            float startOutVolume = outgoing.volume;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                outgoing.volume = Mathf.Lerp(startOutVolume, 0f, t);
                incoming.volume = Mathf.Lerp(0f, targetVolume, t);
                yield return null;
            }

            outgoing.Stop();
            outgoing.clip   = null;
            outgoing.volume = 0f;
            incoming.volume = targetVolume;
        }

        private IEnumerator FadeOutRoutine(AudioSource source, float duration)
        {
            float elapsed     = 0f;
            float startVolume = source.volume;

            while (elapsed < duration)
            {
                elapsed      += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            source.Stop();
            source.volume = 0f;
        }

        // ── Utility ──────────────────────────────────────────────────────────

        private static void ConfigureMusicSource(AudioSource src)
        {
            src.playOnAwake = false;
            src.loop        = true;
            src.volume      = 0f;
        }
    }
}
