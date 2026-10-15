using System;

namespace Project.Save
{
    /// <summary>
    /// Dati di impostazione persistenti.
    /// Serializzati in Application.persistentDataPath/settings.json
    /// </summary>
    [Serializable]
    public class SettingsData
    {
        public float MasterVolume = 1f;
        public float MusicVolume  = 0.8f;
        public float SfxVolume    = 0.8f;
        public bool  IsFullscreen = true;
        public int   QualityLevel = 2;
        public int   TargetFps    = 60;
    }

    /// <summary>
    /// Dati di gioco persistenti, estendibile per ogni progetto.
    /// Serializzati in Application.persistentDataPath/savegame.json
    /// </summary>
    [Serializable]
    public class GameData
    {
        public int    CurrentLevel  = 1;
        public int    HighScore     = 0;
        public string LastSavedUtc  = string.Empty;
    }

    /// <summary>
    /// Contratto per la persistenza di Settings e GameData.
    /// </summary>
    public interface ISaveService
    {
        SettingsData Settings { get; }
        GameData     GameData { get; }

        // ── Impostazioni ──────────────────────────────────────────────────────
        void SaveSettings();
        void LoadSettings();

        // ── Dati di gioco ─────────────────────────────────────────────────────
        void SaveGame();
        void LoadGame();
        void ResetGameData();
    }
}
