using UnityEngine;
using UnityEngine.InputSystem;
using FightingGame.Core;
using FightingGame.Character;

namespace FightingGame.PlayerInput
{
    [RequireComponent(typeof(FighterController))]
    public class FighterInputHandler : MonoBehaviour
    {
        [Header("Double Tap Settings (Dash)")]
        [SerializeField] private float doubleTapThreshold = 0.25f;

        [Header("Input Buffer Settings")]
        [SerializeField] private float inputBufferWindow = 0.2f;

        private FighterController fighter;

        // Double Tap Tracking
        private float lastTapTimeLeft = -1f;
        private float lastTapTimeRight = -1f;

        // Input Buffer Tracking
        private AttackType bufferedAttack = AttackType.None;
        private float bufferTimer = 0f;

        private void Awake()
        {
            fighter = GetComponent<FighterController>();
        }

        private void Update()
        {
            HandleMovementInput();
            HandleAttackInput();
            ProcessInputBuffer();
        }

        private void HandleMovementInput()
        {
            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;

            float moveX = 0f;
            float moveY = 0f;

            // 1. Lectura de Teclado
            if (keyboard != null)
            {
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveX += 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveX -= 1f;

                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed || keyboard.spaceKey.isPressed) moveY += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveY -= 1f;

                // Detección de doble tap en teclado (Dashes)
                if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
                {
                    CheckDoubleTap(1);
                }
                if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
                {
                    CheckDoubleTap(-1);
                }

                // Teclas de Dash directo opcionales (Q = Dash Izq, E = Dash Der)
                if (keyboard.eKey.wasPressedThisFrame) TriggerDash(1);
                if (keyboard.qKey.wasPressedThisFrame) TriggerDash(-1);
            }

            // 2. Lectura de Gamepad
            if (gamepad != null)
            {
                Vector2 stick = gamepad.leftStick.ReadValue();
                Vector2 dpad = gamepad.dpad.ReadValue();

                if (Mathf.Abs(stick.x) > 0.3f) moveX = Mathf.Sign(stick.x);
                else if (Mathf.Abs(dpad.x) > 0.3f) moveX = Mathf.Sign(dpad.x);

                if (stick.y > 0.5f || dpad.y > 0.5f) moveY = 1f;
                else if (stick.y < -0.5f || dpad.y < -0.5f) moveY = -1f;

                // Detección de doble tap en D-Pad
                if (gamepad.dpad.right.wasPressedThisFrame) CheckDoubleTap(1);
                if (gamepad.dpad.left.wasPressedThisFrame) CheckDoubleTap(-1);

                // Bumpers para Dash rápido
                if (gamepad.rightShoulder.wasPressedThisFrame) TriggerDash(1);
                if (gamepad.leftShoulder.wasPressedThisFrame) TriggerDash(-1);
            }

            // Enviar movimiento horizontal y agacharse
            if (moveY < -0.3f)
            {
                fighter.Crouch(true);
            }
            else
            {
                fighter.Crouch(false);

                if (moveY > 0.3f && fighter.IsGrounded)
                {
                    fighter.Jump(moveX);
                }
                else
                {
                    fighter.MoveHorizontal(moveX);
                }
            }
        }

        private void CheckDoubleTap(int direction)
        {
            float currentTime = Time.time;

            if (direction > 0) // Derecha
            {
                if (currentTime - lastTapTimeRight <= doubleTapThreshold)
                {
                    TriggerDash(1);
                    lastTapTimeRight = -1f;
                    return;
                }
                lastTapTimeRight = currentTime;
            }
            else if (direction < 0) // Izquierda
            {
                if (currentTime - lastTapTimeLeft <= doubleTapThreshold)
                {
                    TriggerDash(-1);
                    lastTapTimeLeft = -1f;
                    return;
                }
                lastTapTimeLeft = currentTime;
            }
        }

        private void TriggerDash(int direction)
        {
            bool isForward = (direction > 0 && fighter.IsFacingRight) || (direction < 0 && !fighter.IsFacingRight);
            fighter.Dash(isForward);
        }

        private void HandleAttackInput()
        {
            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;

            // Left Punch (1 / LP): Teclado 'J' o 'U' | Gamepad X (Xbox) / Cuadrado (PS)
            bool lpPressed = (keyboard != null && (keyboard.jKey.wasPressedThisFrame || keyboard.uKey.wasPressedThisFrame)) ||
                             (gamepad != null && gamepad.buttonWest.wasPressedThisFrame);

            // Right Punch (2 / RP): Teclado 'I' | Gamepad Y (Xbox) / Triángulo (PS)
            bool rpPressed = (keyboard != null && keyboard.iKey.wasPressedThisFrame) ||
                             (gamepad != null && gamepad.buttonNorth.wasPressedThisFrame);

            // Left Kick (3 / LK): Teclado 'K' | Gamepad A (Xbox) / Cruz (PS)
            bool lkPressed = (keyboard != null && keyboard.kKey.wasPressedThisFrame) ||
                             (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame);

            // Right Kick (4 / RK): Teclado 'L' o 'O' | Gamepad B (Xbox) / Círculo (PS)
            bool rkPressed = (keyboard != null && (keyboard.lKey.wasPressedThisFrame || keyboard.oKey.wasPressedThisFrame)) ||
                             (gamepad != null && gamepad.buttonEast.wasPressedThisFrame);

            if (lpPressed) QueueAttack(AttackType.LeftPunch);
            else if (rpPressed) QueueAttack(AttackType.RightPunch);
            else if (lkPressed) QueueAttack(AttackType.LeftKick);
            else if (rkPressed) QueueAttack(AttackType.RightKick);
        }

        private void QueueAttack(AttackType attack)
        {
            if (fighter.CurrentState != FighterState.Attack)
            {
                fighter.ExecuteAttack(attack);
            }
            else
            {
                // Buffer de input para combos fluidos
                bufferedAttack = attack;
                bufferTimer = inputBufferWindow;
            }
        }

        private void ProcessInputBuffer()
        {
            if (bufferedAttack != AttackType.None)
            {
                bufferTimer -= Time.deltaTime;
                if (bufferTimer <= 0f)
                {
                    bufferedAttack = AttackType.None;
                }
                else if (fighter.CurrentState != FighterState.Attack)
                {
                    fighter.ExecuteAttack(bufferedAttack);
                    bufferedAttack = AttackType.None;
                }
            }
        }
    }
}
