using UnityEngine;
using FightingGame.Core;
using FightingGame.Character;

namespace FightingGame.Kinect
{
    /// <summary>
    /// Puente que conecta los eventos reconocidos por KinectGestureDetector directamente
    /// con el FighterController de tu personaje en escena.
    /// </summary>
    [RequireComponent(typeof(FighterController))]
    public class KinectFighterBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private KinectGestureDetector gestureDetector;

        private FighterController fighter;

        private void Awake()
        {
            fighter = GetComponent<FighterController>();

            if (gestureDetector == null)
            {
                gestureDetector = FindFirstObjectByType<KinectGestureDetector>();
            }
        }

        private void OnEnable()
        {
            if (gestureDetector != null)
            {
                gestureDetector.OnAttackDetected += HandleAttack;
                gestureDetector.OnCrouchStateChanged += HandleCrouch;
                gestureDetector.OnJumpDetected += HandleJump;
                gestureDetector.OnHorizontalMove += HandleMove;
            }
        }

        private void OnDisable()
        {
            if (gestureDetector != null)
            {
                gestureDetector.OnAttackDetected -= HandleAttack;
                gestureDetector.OnCrouchStateChanged -= HandleCrouch;
                gestureDetector.OnJumpDetected -= HandleJump;
                gestureDetector.OnHorizontalMove -= HandleMove;
            }
        }

        private void HandleAttack(AttackType attackType)
        {
            if (fighter != null)
            {
                fighter.ExecuteAttack(attackType);
            }
        }

        private void HandleCrouch(bool isCrouching)
        {
            if (fighter != null)
            {
                fighter.Crouch(isCrouching);
            }
        }

        private void HandleJump()
        {
            if (fighter != null)
            {
                fighter.Jump(0f);
            }
        }

        private void HandleMove(float inputX)
        {
            if (fighter != null)
            {
                fighter.MoveHorizontal(inputX);
            }
        }
    }
}
