using System;

namespace Project.GameState
{
    /// <summary>
    /// Stati di gioco globali gestiti dalla state machine centrale.
    /// </summary>
    public enum GameState
    {
        Boot,       // Inizializzazione dei servizi durante Bootstrap
        MainMenu,   // Schermata principale
        Lobby,      // Staging area pre-partita
        Gameplay,   // Partita in corso
        Paused,     // Gioco in pausa (Time.timeScale == 0)
        GameOver    // Fine partita (vittoria o sconfitta)
    }

    /// <summary>
    /// Contratto per la macchina a stati di gioco globale.
    /// Coordina Time.timeScale, cursore mouse e input context.
    /// </summary>
    public interface IGameStateManager
    {
        GameState CurrentState { get; }

        /// <summary>
        /// Stato prima dell'ultima pausa (Lobby o Gameplay).
        /// Usato dalla PauseUI per abilitare/disabilitare i pulsanti contestuali.
        /// </summary>
        GameState StateBeforePause { get; }

        /// <summary>
        /// Scatta ogni volta che lo stato cambia.
        /// Args: (statoVecchio, statoNuovo)
        /// </summary>
        event Action<GameState, GameState> OnStateChanged;

        void ChangeState(GameState newState);
        void PauseGame();
        void ResumeGame();
    }
}
