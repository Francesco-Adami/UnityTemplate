using System.IO;
using Project.Core;
using UnityEngine;

namespace Project.Save
{
    /// <summary>
    /// Implementazione di ISaveService tramite serializzazione JSON su file.
    /// File prodotti:
    ///   - {persistentDataPath}/settings.json
    ///   - {persistentDataPath}/savegame.json
    ///
    /// Montato sul GameObject "SaveService" in Bootstrap (o Bootstrapper).
    /// </summary>
    public class SaveService : MonoBehaviour, ISaveService
    {
        private const string SettingsFileName = "settings.json";
        private const string GameDataFileName = "savegame.json";

        public SettingsData Settings { get; private set; } = new SettingsData();
        public GameData     GameData { get; private set; } = new GameData();

        private string SettingsPath => Path.Combine(Application.persistentDataPath, SettingsFileName);
        private string GameDataPath => Path.Combine(Application.persistentDataPath, GameDataFileName);

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            LoadSettings();
            LoadGame();
            ServiceLocator.Register<ISaveService>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<ISaveService>();
        }

        // ── Settings ─────────────────────────────────────────────────────────

        public void SaveSettings()
        {
            WriteJson(SettingsPath, Settings);
            Debug.Log($"[SaveService] Settings salvate in: {SettingsPath}");
        }

        public void LoadSettings()
        {
            Settings = ReadJson<SettingsData>(SettingsPath) ?? new SettingsData();
        }

        // ── GameData ─────────────────────────────────────────────────────────

        public void SaveGame()
        {
            GameData.LastSavedUtc = System.DateTime.UtcNow.ToString("o");
            WriteJson(GameDataPath, GameData);
            Debug.Log($"[SaveService] GameData salvato in: {GameDataPath}");
        }

        public void LoadGame()
        {
            GameData = ReadJson<GameData>(GameDataPath) ?? new GameData();
        }

        public void ResetGameData()
        {
            GameData = new GameData();
            SaveGame();
        }

        // ── IO Helpers ────────────────────────────────────────────────────────

        private static void WriteJson<T>(string path, T data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(path, json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveService] Errore durante il salvataggio in '{path}': {ex.Message}");
            }
        }

        private static T ReadJson<T>(string path) where T : class
        {
            if (!File.Exists(path)) return null;

            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<T>(json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveService] Errore durante il caricamento da '{path}': {ex.Message}");
                return null;
            }
        }
    }
}
