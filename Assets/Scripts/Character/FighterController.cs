using UnityEngine;
using FightingGame.Core;

namespace FightingGame.Character
{
    [RequireComponent(typeof(CharacterController))]
    public class FighterController : MonoBehaviour
    {
        [Header("2.5D Plane & Auto-Facing")]
        [Tooltip("Posición Z en la que el peleador estará siempre bloqueado")]
        [SerializeField] private float lockedZ = 0f;
        [SerializeField] private bool autoLockZ = true;
        [SerializeField] private FighterController opponent;
        [SerializeField] private bool autoFaceOpponent = true;

        [Header("Movement Stats")]
        [SerializeField] private float walkForwardSpeed = 4.5f;
        [SerializeField] private float walkBackwardSpeed = 3.5f;
        [SerializeField] private float dashSpeed = 10f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float jumpForce = 8.5f;
        [SerializeField] private float jumpHorizontalSpeed = 3.5f;
        [SerializeField] private float gravity = 22f;

        [Header("Attack Stats")]
        [SerializeField] private float defaultAttackDuration = 0.35f;

        [Header("State (Debug / Read Only)")]
        [SerializeField] private FighterState currentState = FighterState.Idle;
        [SerializeField] private AttackType currentAttack = AttackType.None;
        [SerializeField] private bool isFacingRight = true;
        [SerializeField] private bool isGrounded = true;

        // Referencias de componentes
        private CharacterController characterController;
        private FighterCombat fighterCombat;

        // Variables de velocidad y movimiento
        private Vector3 velocity;
        private float dashTimer = 0f;
        private int dashDirection = 0; // -1 (izq) o 1 (der)
        private float attackTimer = 0f;
        private float hitstunTimer = 0f;

        // Propiedades públicas
        public FighterState CurrentState => currentState;
        public AttackType CurrentAttack => currentAttack;
        public bool IsFacingRight => isFacingRight;
        public bool IsGrounded => isGrounded;
        public bool IsCrouching => currentState == FighterState.Crouch;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            fighterCombat = GetComponent<FighterCombat>();
            
            // Forzar posición inicial en el plano Z
            if (autoLockZ)
            {
                Vector3 startPos = transform.position;
                startPos.z = lockedZ;
                transform.position = startPos;
            }
        }

        private void Start()
        {
            // Si no se asignó un oponente manualmente, buscar automáticamente otro FighterController en la escena
            if (opponent == null)
            {
                FighterController[] allFighters = FindObjectsByType<FighterController>(FindObjectsSortMode.None);
                foreach (var f in allFighters)
                {
                    if (f != this)
                    {
                        opponent = f;
                        break;
                    }
                }
            }
        }

        private void Update()
        {
            CheckGrounded();
            ApplyGravity();
            UpdateFacingDirection();
            UpdateStateMachine();
            ApplyMovement();
            Enforce2DPlane();
        }

        private void UpdateFacingDirection()
        {
            // Solo cambiar de frente si estamos en el suelo y no estamos en medio de un ataque, dash o hitstun
            if (autoFaceOpponent && opponent != null && isGrounded)
            {
                if (currentState != FighterState.Attack && 
                    currentState != FighterState.DashForward && 
                    currentState != FighterState.DashBackward &&
                    currentState != FighterState.Hitstun &&
                    currentState != FighterState.Dead)
                {
                    bool shouldFaceRight = opponent.transform.position.x > transform.position.x;
                    SetFacingDirection(shouldFaceRight);
                }
            }
        }

        private void CheckGrounded()
        {
            isGrounded = characterController.isGrounded;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Mantener pegado al suelo suavemente
            }
        }

        private void ApplyGravity()
        {
            if (!isGrounded)
            {
                velocity.y -= gravity * Time.deltaTime;
            }
        }

        private void UpdateStateMachine()
        {
            switch (currentState)
            {
                case FighterState.DashForward:
                case FighterState.DashBackward:
                    dashTimer -= Time.deltaTime;
                    if (dashTimer <= 0f)
                    {
                        SetState(FighterState.Idle);
                    }
                    break;

                case FighterState.JumpNeutral:
                case FighterState.JumpForward:
                case FighterState.JumpBackward:
                    if (isGrounded && velocity.y <= 0)
                    {
                        velocity.x = 0;
                        SetState(FighterState.Idle);
                    }
                    break;

                case FighterState.Attack:
                    attackTimer -= Time.deltaTime;
                    if (attackTimer <= 0f)
                    {
                        currentAttack = AttackType.None;
                        if (fighterCombat != null) fighterCombat.DeactivateAllHitboxes();
                        SetState(isGrounded ? (IsCrouching ? FighterState.Crouch : FighterState.Idle) : FighterState.JumpNeutral);
                    }
                    break;

                case FighterState.Hitstun:
                    hitstunTimer -= Time.deltaTime;
                    // Desacelerar knockback gradualmente
                    velocity.x = Mathf.Lerp(velocity.x, 0, Time.deltaTime * 10f);
                    if (hitstunTimer <= 0f)
                    {
                        velocity.x = 0;
                        SetState(isGrounded ? FighterState.Idle : FighterState.JumpNeutral);
                    }
                    break;
            }
        }

        private void ApplyMovement()
        {
            characterController.Move(velocity * Time.deltaTime);
        }

        private void Enforce2DPlane()
        {
            // Bloquear rigurosamente el movimiento en el eje Z
            if (autoLockZ)
            {
                Vector3 currentPos = transform.position;
                if (Mathf.Abs(currentPos.z - lockedZ) > 0.001f)
                {
                    currentPos.z = lockedZ;
                    transform.position = currentPos;
                }
            }

            // Bloquear rotación en X y Z (solo permitimos rotar en Y para cambiar de lado)
            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = 0f;
            euler.z = 0f;
            euler.y = isFacingRight ? 90f : -90f; // 90° para mirar a la derecha en 2.5D
            transform.rotation = Quaternion.Euler(euler);
        }

        public void SetState(FighterState newState)
        {
            if (currentState == newState) return;
            currentState = newState;
        }

        public void SetFacingDirection(bool faceRight)
        {
            isFacingRight = faceRight;
        }

        // Métodos públicos para acciones que serán llamados por el InputHandler
        public void MoveHorizontal(float inputX)
        {
            if (currentState == FighterState.Crouch || 
                currentState == FighterState.Attack || 
                currentState == FighterState.DashForward || 
                currentState == FighterState.DashBackward ||
                !isGrounded)
            {
                return;
            }

            if (Mathf.Abs(inputX) > 0.1f)
            {
                bool movingForward = (inputX > 0 && isFacingRight) || (inputX < 0 && !isFacingRight);
                float speed = movingForward ? walkForwardSpeed : walkBackwardSpeed;
                velocity.x = inputX * speed;

                SetState(movingForward ? FighterState.WalkForward : FighterState.WalkBackward);
            }
            else
            {
                velocity.x = 0;
                if (currentState == FighterState.WalkForward || currentState == FighterState.WalkBackward)
                {
                    SetState(FighterState.Idle);
                }
            }
        }

        public void Crouch(bool isCrouchingInput)
        {
            if (!isGrounded || currentState == FighterState.Attack) return;

            if (isCrouchingInput)
            {
                velocity.x = 0;
                SetState(FighterState.Crouch);
            }
            else if (currentState == FighterState.Crouch)
            {
                SetState(FighterState.Idle);
            }
        }

        public void Jump(float horizontalDirection = 0f)
        {
            if (!isGrounded || currentState == FighterState.Attack || currentState == FighterState.Crouch) return;

            velocity.y = jumpForce;

            if (Mathf.Abs(horizontalDirection) > 0.1f)
            {
                velocity.x = Mathf.Sign(horizontalDirection) * jumpHorizontalSpeed;
                bool isForward = (horizontalDirection > 0 && isFacingRight) || (horizontalDirection < 0 && !isFacingRight);
                SetState(isForward ? FighterState.JumpForward : FighterState.JumpBackward);
            }
            else
            {
                velocity.x = 0;
                SetState(FighterState.JumpNeutral);
            }
        }

        public void Dash(bool forward)
        {
            if (!isGrounded || currentState == FighterState.Crouch || currentState == FighterState.Attack) return;

            dashTimer = dashDuration;
            dashDirection = forward ? (isFacingRight ? 1 : -1) : (isFacingRight ? -1 : 1);
            velocity.x = dashDirection * dashSpeed;

            SetState(forward ? FighterState.DashForward : FighterState.DashBackward);
        }

        public void ExecuteAttack(AttackType attackType, float customDuration = -1f)
        {
            // No permitir atacar durante otro ataque (a menos que haya combo-cancel que añadiremos luego) o durante dash
            if (currentState == FighterState.Attack || 
                currentState == FighterState.DashForward || 
                currentState == FighterState.DashBackward ||
                currentState == FighterState.Hitstun ||
                currentState == FighterState.Dead)
            {
                return;
            }

            currentAttack = attackType;
            attackTimer = customDuration > 0f ? customDuration : defaultAttackDuration;

            if (isGrounded)
            {
                velocity.x = 0; // Frenar al atacar en tierra
            }

            SetState(FighterState.Attack);
            
            // Activar hitbox correspondiente
            if (fighterCombat != null)
            {
                fighterCombat.ActivateHitboxForAttack(attackType);
            }

            Debug.Log($"[FighterController] Ataque Ejecutado: {attackType} | En Suelo: {isGrounded} | Agachado: {IsCrouching}");
        }

        public void TakeHit(float duration, Vector2 knockback, FighterController attacker)
        {
            currentAttack = AttackType.None;
            if (fighterCombat != null) fighterCombat.DeactivateAllHitboxes();

            hitstunTimer = duration;

            // Calcular dirección del empuje (lejos del atacante)
            float pushDir = 1f;
            if (attacker != null)
            {
                pushDir = transform.position.x >= attacker.transform.position.x ? 1f : -1f;
            }
            else
            {
                pushDir = isFacingRight ? -1f : 1f;
            }

            velocity.x = pushDir * knockback.x;
            if (knockback.y > 0)
            {
                velocity.y = knockback.y;
            }

            SetState(FighterState.Hitstun);
        }

        public void EndAttackEarly()
        {
            if (currentState == FighterState.Attack)
            {
                currentAttack = AttackType.None;
                attackTimer = 0f;
                if (fighterCombat != null) fighterCombat.DeactivateAllHitboxes();
                SetState(isGrounded ? (IsCrouching ? FighterState.Crouch : FighterState.Idle) : FighterState.JumpNeutral);
            }
        }
    }
}
