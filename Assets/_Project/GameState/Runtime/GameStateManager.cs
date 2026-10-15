using System;
using Project.Core;
using Project.Input;
using UnityEngine;

namespace Project.GameState
{
    /// <summary>
    /// Implementazione della macchina a stati di gioco globale.
    /// Coordina: Time.timeScale, visibilità cursore e input context (Player vs UI).
    ///
    /// Montato sul GameObject "GameStateManager" in Bootstrap.
    /// Dipende da IInputReader (iniettato tramite ServiceLocator).
    /// </summary>
    public class GameStateManager : MonoBehaviour, IGameStateManager
    {
        public GameState CurrentState { get; private set; } = GameState.Boot;
        public GameState StateBeforePause { get; private set; } = GameState.Lobby;
        public event Action<GameState, GameState> OnStateChanged;

        private IInputReader _inputReader;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            ServiceLocator.Register<IGameStateManager>(this);
        }

        private void Start()
        {
            // InputReader disponibile dopo la registrazione del Bootstrapper
            if (ServiceLocator.TryGet<IInputReader>(out var ir))
            {
                _inputReader = ir;
            }
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<IGameStateManager>();
        }

        // ── API Pubblica ──────────────────────────────────────────────────────

        public void ChangeState(GameState newState)
        {
            if (newState == CurrentState) return;

            GameState previousState = CurrentState;
            CurrentState = newState;

            ApplySideEffects(newState);

            OnStateChanged?.Invoke(previousState, newState);
            Debug.Log($"[GameStateManager] Stato: {previousState} → {newState}");
        }

        public void PauseGame()
        {
            if (CurrentState == GameState.Gameplay || CurrentState == GameState.Lobby)
            {
                StateBeforePause = CurrentState;
                ChangeState(GameState.Paused);
            }
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
                ChangeState(StateBeforePause);
        }

        // ── Side Effects per Stato ────────────────────────────────────────────

        private void ApplySideEffects(GameState state)
        {
            switch (state)
            {
                case GameState.Boot:
                    SetCursor(visible: false, locked: false);
                    Time.timeScale = 1f;
                    _inputReader?.DisableAllActions();
                    break;

                case GameState.MainMenu:
                case GameState.Lobby:
                    SetCursor(visible: true, locked: false);
                    Time.timeScale = 1f;
                    _inputReader?.EnableUIActions();
                    break;

                case GameState.Gameplay:
                    SetCursor(visible: true, locked: false);
                    Time.timeScale = 1f;
                    _inputReader?.EnableUIActions();
                    break;

                case GameState.Paused:
                    SetCursor(visible: true, locked: false);
                    Time.timeScale = 0f;
                    _inputReader?.EnableUIActions();
                    break;

                case GameState.GameOver:
                    SetCursor(visible: true, locked: false);
                    Time.timeScale = 0f;
                    _inputReader?.EnableUIActions();
                    break;
            }
        }



        // ── Utility ──────────────────────────────────────────────────────────

        private static void SetCursor(bool visible, bool locked)
        {
            Cursor.visible   = visible;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}
