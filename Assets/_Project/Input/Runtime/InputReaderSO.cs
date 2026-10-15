using System;
using Project.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Input
{
    /// <summary>
    /// ScriptableObject che implementa IInputReader avvolgendo la classe generata InputSystem_Actions.
    /// Crea l'asset tramite: Assets > Create > Project > Input > InputReader
    ///
    /// Assegna il riferimento all'asset nell'Inspector del Bootstrapper nella scena Bootstrap.
    /// NON è un MonoBehaviour: persiste automaticamente per tutta la sessione come asset.
    ///
    /// Naming convention eventi: suffisso 'Performed'/'Canceled' per evitare
    /// conflitti con i metodi callback OnMove/OnLook dell'interfaccia IPlayerActions.
    /// </summary>
    [CreateAssetMenu(fileName = "InputReader", menuName = "Project/Input/InputReader")]
    public class InputReaderSO : ScriptableObject, IInputReader,
        @InputSystem_Actions.IPlayerActions,
        @InputSystem_Actions.IUIActions
    {
        // ── Istanza Actions ──────────────────────────────────────────────────
        private @InputSystem_Actions _inputActions;

        // ── Stato polling ────────────────────────────────────────────────────
        public Vector2 MoveComposite { get; private set; }
        public Vector2 LookComposite { get; private set; }
        public bool IsSprinting { get; private set; }

        // ── Player Events (nuovi nomi: Performed/Canceled suffix) ────────────
        public event Action<Vector2> MovePerformed;
        public event Action<Vector2> LookPerformed;
        public event Action JumpPerformed;
        public event Action JumpCanceled;
        public event Action SprintPerformed;
        public event Action SprintCanceled;
        public event Action InteractPerformed;
        public event Action AttackPerformed;
        public event Action CrouchPerformed;
        public event Action PausePerformed;

        // ── Lifecycle ────────────────────────────────────────────────────────

        private void OnEnable()
        {
            if (_inputActions == null)
            {
                _inputActions = new @InputSystem_Actions();
                _inputActions.Player.SetCallbacks(this);
                _inputActions.UI.SetCallbacks(this);
            }

            EnablePlayerActions();
        }

        private void OnDisable()
        {
            DisableAllActions();
        }

        // ── IInputReader: Context Switching ──────────────────────────────────

        public void EnablePlayerActions()
        {
            _inputActions.UI.Disable();
            _inputActions.Player.Enable();
        }

        public void EnableUIActions()
        {
            _inputActions.Player.Disable();
            _inputActions.UI.Enable();
        }

        public void DisableAllActions()
        {
            _inputActions?.Player.Disable();
            _inputActions?.UI.Disable();
        }

        // ── IPlayerActions Callbacks (nomi imposti dall'interfaccia generata) ─

        public void OnMove(InputAction.CallbackContext context)
        {
            MoveComposite = context.ReadValue<Vector2>();
            MovePerformed?.Invoke(MoveComposite);
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            LookComposite = context.ReadValue<Vector2>();
            LookPerformed?.Invoke(LookComposite);
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)   JumpPerformed?.Invoke();
            else if (context.phase == InputActionPhase.Canceled) JumpCanceled?.Invoke();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
            {
                IsSprinting = true;
                SprintPerformed?.Invoke();
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                IsSprinting = false;
                SprintCanceled?.Invoke();
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed) InteractPerformed?.Invoke();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started) AttackPerformed?.Invoke();
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started) CrouchPerformed?.Invoke();
        }

        public void OnPrevious(InputAction.CallbackContext context) { }
        public void OnNext(InputAction.CallbackContext context) { }

        // ── IUIActions Callbacks ─────────────────────────────────────────────

        public void OnNavigate(InputAction.CallbackContext context) { }
        public void OnSubmit(InputAction.CallbackContext context) { }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started) PausePerformed?.Invoke();
        }

        public void OnPoint(InputAction.CallbackContext context) { }
        public void OnClick(InputAction.CallbackContext context) { }
        public void OnScrollWheel(InputAction.CallbackContext context) { }
        public void OnMiddleClick(InputAction.CallbackContext context) { }
        public void OnRightClick(InputAction.CallbackContext context) { }
        public void OnTrackedDevicePosition(InputAction.CallbackContext context) { }
        public void OnTrackedDeviceOrientation(InputAction.CallbackContext context) { }
    }
}
