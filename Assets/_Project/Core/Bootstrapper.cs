using ArtLake.UI;
using Project.Audio;
using Project.Core;
using Project.GameState;
using Project.Input;
using Project.Save;
using Project.UI;
using UnityEngine;

namespace Project
{
    /// <summary>
    /// Punto di avvio unico del progetto. Montato sulla scena Bootstrap.
    ///
    /// Responsabilità:
    ///   1. Assicura DontDestroyOnLoad per se stesso (e indirettamente per l'_UI).
    ///   2. Garantisce l'ordine corretto di inizializzazione dei servizi.
    ///   3. Avvia il flusso verso la scena MainMenu.
    ///
    /// Ordine di esecuzione:
    ///   Bootstrapper (Awake) -> SaveService (Awake) -> AudioService (Awake)
    ///   -> GameStateManager (Awake) -> SceneLoader (Awake)
    ///   -> UIManager (Awake registra _UI) -> Bootstrapper (Start) avvia flow.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        [Header("InputReader (ScriptableObject Asset)")]
        [Tooltip("Trascina qui l'asset InputReader.asset dalla cartella Input/.")]
        [SerializeField] private InputReaderSO inputReader;

        [Header("Scena Iniziale")]
        [Tooltip("Nome esatto della scena da caricare dopo il boot (es. 'MainMenu').")]
        [SerializeField] private string initialSceneName = "MainMenu";

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            if (inputReader == null)
            {
                Debug.LogError("[Bootstrapper] InputReader non assegnato nell'Inspector!");
                return;
            }

            // Registra l'InputReader nel ServiceLocator come IInputReader.
            // Gli altri servizi (GameStateManager) lo recupereranno tramite TryGet in Start.
            ServiceLocator.Register<IInputReader>(inputReader);
        }

        private async void Start()
        {
            // Attende un frame per permettere a tutti gli Awake di completarsi
            // (UIManager, AudioService, SaveService, GameStateManager si registrano in Awake).
            await Awaitable.NextFrameAsync();

            // Sincronizza volumi audio con i settings salvati
            SyncAudioWithSettings();

            // Transizione alla prima scena
            if (ServiceLocator.TryGet<ISceneLoader>(out var sceneLoader))
            {
                await sceneLoader.SwitchSceneAsync(initialSceneName);

                if (ServiceLocator.TryGet<IGameStateManager>(out var gsm))
                {
                    gsm.ChangeState(Project.GameState.GameState.MainMenu);
                }

                // Mostra la UI principale
                if (ServiceLocator.TryGet<IUIManager>(out var ui))
                {
                    ui.Show<MainMenuUI>();
                }
            }
            else
            {
                Debug.LogError("[Bootstrapper] ISceneLoader non trovato nel ServiceLocator.");
            }
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private static void SyncAudioWithSettings()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;
            if (!ServiceLocator.TryGet<IAudioService>(out var audio)) return;

            audio.SetMasterVolume(save.Settings.MasterVolume);
            audio.SetMusicVolume(save.Settings.MusicVolume);
            audio.SetSfxVolume(save.Settings.SfxVolume);
        }
    }
}
