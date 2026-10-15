# Master Technical Design Document (TDD) — UnityTemplate

Documento di architettura tecnica generale del progetto. Pensato per essere chiaro, modulare e spiegato nel modo più semplice possibile ("Monkey-Proof"). Basato sui principi della Clean Architecture.

---

## 🗺️ Mappa dei Sistemi (Architettura Generale)

L'architettura si basa su un **Service Locator Pattern** accessibile globalmente.
Nessun manager si cerca usando `FindObjectOfType`. Tutti implementano un'interfaccia e si registrano in fase di Awake.

| Modulo | Stato | Responsabile Principale | Descrizione Veloce |
|---|---|---|---|
| **00. Core Architecture** | 🟩 **Completato** | `ServiceLocator`, `Bootstrapper` | Inversione di dipendenza globale. Avvio sequenziale e sincronizzazione manager. |
| **01. Scene Management** | 🟩 **Completato** | `SceneLoader` | Gestione asincrona (Unity 6 Awaitable) delle scene, transizioni e memoria. |
| **02. Audio Service** | 🟩 **Completato** | `AudioService` | Gestione Crossfade BGM, pool di SFX, controllo volumi e persistenza. |
| **03. UI Framework** | 🟩 **Completato** | `UIManager` (ArtLake) | Stack di viste, modali, navigazione e commutazione input. |
| **04. Save System** | 🟩 **Completato** | `SaveService` | Salvataggio persistente su JSON (SettingsData e GameData separati). |
| **05. Game State** | 🟩 **Completato** | `GameStateManager` | Macchina a stati globale (Boot, MainMenu, Lobby, Gameplay, Paused, GameOver). |
| **06. Input Handling** | 🟩 **Completato** | `InputReaderSO` | Lettura centralizzata di InputSystem, interfacce disaccoppiate e gestione mapping contestuali (UI vs Player). |
| **07. Player & Camera**| 🟩 **Completato** | `PlayerController`, `CameraController` | Movimento 3D, gravità e terza persona. Rigorosamente disaccoppiati, usano `IInputReader`. |

---

## 📦 00. Core Architecture

### Service Locator (`ServiceLocator`)
- Fornisce un container centrale per i servizi (es. `ISceneLoader`, `IAudioService`).
- Registrazione: `ServiceLocator.Register<IService>(this)` nell'`Awake`.
- Risoluzione: `if (ServiceLocator.TryGet<IService>(out var service)) { ... }`.
- Pulisce automaticamente i dizionari allo scaricamento (anche se sono su `DontDestroyOnLoad`).

### Bootstrapper
Il `Bootstrapper` vive nella scena fissa **`Bootstrap`**.
Assicura che tutti i servizi si registrino correttamente e sincronizza dati prima di passare il controllo al `SceneLoader` per caricare `MainMenu`.

---

## 📦 01. Scene Management (`SceneLoader`)

Gestisce le scene in modo additivo su `Bootstrap`.
Usa `Awaitable` (Unity 6). Offre:
- `SwitchSceneAsync("NomeScena")`: scarica il livello precedente, pulisce memoria, carica il nuovo.
- `LoadAdditiveAsync` e `UnloadSceneAsync`.
Emette eventi `OnLoadStarted`, `OnProgressUpdated`, `OnSceneLoaded`.

---

## 📦 02. Audio Service (`AudioService`)

Implementa `IAudioService`. Responsabile per riprodurre BGM con Fade (crossfade o stop) e SFX.
- `PlayBGM(AudioClip)`: sfuma automaticamente tra le tracce.
- `PlaySFX(AudioClip)`: gestito con un pool base (AudioSource in array).
- Sincronizzato con `ISaveService` per volumi Master/Music/SFX.

---

## 📦 03. UI Framework (`UIManager` - ArtLake)

Implementa `IUIManager` registrato nel `ServiceLocator`.
- Gestisce viste a cascata tramite stack `Show<T>` e `Back()`.
- Distingue `PanelType.Screen` (sostituiscono la vista precedente) e `PanelType.Popup` (sovrapposti con blocco opzionale interazione).
- Sincronizzato con `IInputReader` e `IGameStateManager` per il flow Pause/Lobby/Gameplay.

---

## 📦 04. Save System (`SaveService`)

Implementa `ISaveService`. Usa la classe utility `JsonFileHandler` per scrivere in `Application.persistentDataPath`.
File separati:
- `SettingsData`: audio (Master/Music/SFX) e video (Fullscreen). Caricato durante il Boot.
- `GameData`: progresso di gioco base.

---

## 📦 05. Game State (`GameStateManager`)

Implementa `IGameStateManager`. Enum: `Boot`, `MainMenu`, `Lobby`, `Gameplay`, `Paused`, `GameOver`.
- Risolve l'interazione tra cursor (LockMode) e Time.timeScale (1 o 0).
- Nel momento di switch in stato UI (MainMenu, Paused), comunica all'`IInputReader` di attivare la mappa `UI`. Nello stato `Gameplay`, attiva `Player`.

---

## 📦 06. Input Handling (`InputReaderSO`)

Interfaccia `IInputReader`. Disaccoppia i controlli Unity Input System dalla logica.
- Implementato con lo `ScriptableObject` **`InputReaderSO`** generato da un file `.inputactions`.
- Inietta l'input in giro tramite eventi (`MovePerformed`, `JumpPerformed`, `PausePerformed`) ed espone polling diretto (`MoveComposite`).
- Offre `EnablePlayerActions()` e `EnableUIActions()` per non incrociare gli input quando si è in menu.

---

## 📦 07. Player & Camera (`PlayerController`, `CameraController`)

- `PlayerController`: usa il `CharacterController` base di Unity. Movimento 3D direzionato rispetto alla telecamera, salto e sprint. Usa l'interfaccia `IInputReader`. Configurazione esposta tramite `PlayerMovementConfig.asset`.
- `CameraController`: orbita di terza persona con mouse e stick, offset, clamping e smoothing. Assolutamente indipendente e testato senza cinemachine.

---

## Flusso di Lavoro (Monkey-Proof)
1. **Scene flow**: Si apre solo e sempre `Bootstrap` nell'editor. Da lì premi Play.
2. `Bootstrapper` attende un tick e sincronizza audio e opzioni utente da JSON.
3. Lo `SceneLoader` carica `MainMenu`.
4. L'utente sceglie "Gioca". Lo `SceneLoader` va in `Lobby`.
5. Dalla Lobby a `GamePlay`. L'UI passa a `HudUI`, il GameState abilita `Player` e nasconde il cursore.
6. Alla pressione di ESC/Start, il `GameStateManager` va in Pausa e cambia l'input map, mentre la `HudUI` apre la `PauseUI`.
