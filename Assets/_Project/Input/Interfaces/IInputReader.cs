using System;
using UnityEngine;

namespace Project.Input
{
    /// <summary>
    /// Contratto disaccoppiato per la lettura dell'input.
    /// I sistemi di gameplay dipendono SOLO da questa interfaccia, mai da InputReaderSO direttamente.
    ///
    /// Naming convention: eventi con 'Performed' / 'Canceled' suffix per evitare
    /// conflitti con i metodi callback OnXxx dell'interfaccia IPlayerActions di Input System.
    /// </summary>
    public interface IInputReader
    {
        // ── Stato corrente (polling) ─────────────────────────────────────────
        Vector2 MoveComposite { get; }
        Vector2 LookComposite { get; }
        bool IsSprinting { get; }

        // ── Movimento & Camera ───────────────────────────────────────────────
        event Action<Vector2> MovePerformed;
        event Action<Vector2> LookPerformed;

        // ── Azioni di gioco ──────────────────────────────────────────────────
        event Action JumpPerformed;
        event Action JumpCanceled;
        event Action SprintPerformed;
        event Action SprintCanceled;
        event Action InteractPerformed;
        event Action AttackPerformed;
        event Action CrouchPerformed;

        // ── Meta / UI ────────────────────────────────────────────────────────
        event Action PausePerformed;   // Escape / Start gamepad

        // ── Commutazione Context ─────────────────────────────────────────────
        /// <summary>Attiva la mappa Player (gameplay), disabilita la mappa UI.</summary>
        void EnablePlayerActions();

        /// <summary>Attiva la mappa UI, disabilita la mappa Player.</summary>
        void EnableUIActions();

        /// <summary>Disabilita tutte le action map.</summary>
        void DisableAllActions();
    }
}
