using System;
using UnityEngine;

namespace Project.Audio
{
    /// <summary>
    /// Contratto per la gestione audio del progetto.
    /// Supporta musica di sottofondo (BGM) con crossfade e SFX one-shot.
    /// </summary>
    public interface IAudioService
    {
        float MasterVolume { get; }
        float MusicVolume  { get; }
        float SfxVolume    { get; }

        // ── Controllo Volumi ──────────────────────────────────────────────────
        void SetMasterVolume(float normalizedVolume);
        void SetMusicVolume(float normalizedVolume);
        void SetSfxVolume(float normalizedVolume);

        // ── Musica ───────────────────────────────────────────────────────────
        void PlayMusic(AudioClip clip, bool loop = true, float fadeDuration = 0.5f);
        void StopMusic(float fadeDuration = 0.5f);
        void PauseMusic();
        void ResumeMusic();

        // ── Effetti Sonori ───────────────────────────────────────────────────
        void PlaySfx(AudioClip clip, float volumeScale = 1f);
        void PlaySfxAtPosition(AudioClip clip, Vector3 position, float volumeScale = 1f);
    }
}
