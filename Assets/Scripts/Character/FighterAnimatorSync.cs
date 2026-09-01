using UnityEngine;
using FightingGame.Core;

namespace FightingGame.Character
{
    [RequireComponent(typeof(FighterController))]
    public class FighterAnimatorSync : MonoBehaviour
    {
        [Header("Animator Reference")]
        [SerializeField] private Animator animator;

        private FighterController fighter;
        private FighterCombat fighterCombat;

        // Hash IDs para optimizar rendimiento de parámetros del Animator
        private static readonly int ParamIsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int ParamIsCrouching = Animator.StringToHash("IsCrouching");
        private static readonly int ParamMoveX = Animator.StringToHash("MoveX");
        private static readonly int ParamJump = Animator.StringToHash("Jump");
        private static readonly int ParamDashForward = Animator.StringToHash("DashForward");
        private static readonly int ParamDashBackward = Animator.StringToHash("DashBackward");
        private static readonly int ParamAttackLP = Animator.StringToHash("Attack_LP");
        private static readonly int ParamAttackRP = Animator.StringToHash("Attack_RP");
        private static readonly int ParamAttackLK = Animator.StringToHash("Attack_LK");
        private static readonly int ParamAttackRK = Animator.StringToHash("Attack_RK");
        private static readonly int ParamHit = Animator.StringToHash("Hit");

        private FighterState previousState = FighterState.Idle;

        private void Awake()
        {
            fighter = GetComponent<FighterController>();
            fighterCombat = GetComponent<FighterCombat>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        private void Update()
        {
            if (animator == null) return;

            UpdateContinuousParameters();
            CheckStateTransitions();
        }

        private void UpdateContinuousParameters()
        {
            animator.SetBool(ParamIsGrounded, fighter.IsGrounded);
            animator.SetBool(ParamIsCrouching, fighter.IsCrouching);

            // Determinar velocidad relativa para caminar adelante o atrás
            float moveX = 0f;
            if (fighter.CurrentState == FighterState.WalkForward) moveX = 1f;
            else if (fighter.CurrentState == FighterState.WalkBackward) moveX = -1f;

            animator.SetFloat(ParamMoveX, moveX, 0.05f, Time.deltaTime);
        }

        private void CheckStateTransitions()
        {
            FighterState current = fighter.CurrentState;
            if (current == previousState) return;

            switch (current)
            {
                case FighterState.JumpNeutral:
                case FighterState.JumpForward:
                case FighterState.JumpBackward:
                    animator.SetTrigger(ParamJump);
                    break;

                case FighterState.DashForward:
                    animator.SetTrigger(ParamDashForward);
                    break;

                case FighterState.DashBackward:
                    animator.SetTrigger(ParamDashBackward);
                    break;

                case FighterState.Attack:
                    TriggerAttackAnimation(fighter.CurrentAttack);
                    break;

                case FighterState.Hitstun:
                    animator.SetTrigger(ParamHit);
                    break;
            }

            previousState = current;
        }

        private void TriggerAttackAnimation(AttackType attack)
        {
            switch (attack)
            {
                case AttackType.LeftPunch:
                    animator.SetTrigger(ParamAttackLP);
                    break;
                case AttackType.RightPunch:
                    animator.SetTrigger(ParamAttackRP);
                    break;
                case AttackType.LeftKick:
                    animator.SetTrigger(ParamAttackLK);
                    break;
                case AttackType.RightKick:
                    animator.SetTrigger(ParamAttackRK);
                    break;
            }
        }

        // ==========================================
        // ANIMATION EVENTS (Llamados desde los clips)
        // ==========================================
        
        /// <summary>
        /// Llamar en el frame donde el golpe se vuelve activo (activa hitbox)
        /// </summary>
        public void AnimEvent_OnHitboxStart()
        {
            if (fighterCombat != null)
            {
                fighterCombat.ActivateHitboxForAttack(fighter.CurrentAttack);
            }
        }

        /// <summary>
        /// Llamar en el frame donde el golpe termina su daño (desactiva hitbox)
        /// </summary>
        public void AnimEvent_OnHitboxEnd()
        {
            if (fighterCombat != null)
            {
                fighterCombat.DeactivateAllHitboxes();
            }
        }

        /// <summary>
        /// Llamar al finalizar la animación para retornar inmediatamente al Idle/Crouch
        /// </summary>
        public void AnimEvent_OnAttackFinished()
        {
            fighter.EndAttackEarly();
        }
    }
}
