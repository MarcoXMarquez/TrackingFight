using UnityEngine;
using FightingGame.Core;
using FightingGame.Combat;

namespace FightingGame.Character
{
    [RequireComponent(typeof(FighterController))]
    public class FighterCombat : MonoBehaviour
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 1000f;
        [SerializeField] private float currentHealth;

        [Header("Hitboxes (Asignar colisionadores de extremidades)")]
        [SerializeField] private Hitbox leftHandHitbox;
        [SerializeField] private Hitbox rightHandHitbox;
        [SerializeField] private Hitbox leftFootHitbox;
        [SerializeField] private Hitbox rightFootHitbox;

        [Header("Attack Damage & Timing Defaults")]
        [SerializeField] private float lpDamage = 30f;
        [SerializeField] private float rpDamage = 50f;
        [SerializeField] private float lkDamage = 40f;
        [SerializeField] private float rkDamage = 70f;

        private FighterController fighter;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => currentHealth <= 0;

        private void Awake()
        {
            fighter = GetComponent<FighterController>();
            currentHealth = maxHealth;
        }

        public void ActivateHitboxForAttack(AttackType attackType)
        {
            DeactivateAllHitboxes();

            switch (attackType)
            {
                case AttackType.LeftPunch:
                    if (leftHandHitbox != null) leftHandHitbox.Activate(attackType, lpDamage, 0.35f, new Vector2(2f, 0f));
                    break;
                case AttackType.RightPunch:
                    if (rightHandHitbox != null) rightHandHitbox.Activate(attackType, rpDamage, 0.45f, new Vector2(3f, 0f));
                    break;
                case AttackType.LeftKick:
                    if (leftFootHitbox != null) leftFootHitbox.Activate(attackType, lkDamage, 0.4f, new Vector2(2.5f, 0f));
                    break;
                case AttackType.RightKick:
                    if (rightFootHitbox != null) rightFootHitbox.Activate(attackType, rkDamage, 0.55f, new Vector2(4.5f, 0f));
                    break;
            }
        }

        public void DeactivateAllHitboxes()
        {
            if (leftHandHitbox != null) leftHandHitbox.Deactivate();
            if (rightHandHitbox != null) rightHandHitbox.Deactivate();
            if (leftFootHitbox != null) leftFootHitbox.Deactivate();
            if (rightFootHitbox != null) rightFootHitbox.Deactivate();
        }

        public void TakeDamage(float damage, float hitstunDuration, Vector2 knockback, FighterController attacker)
        {
            if (IsDead) return;

            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);

            DeactivateAllHitboxes();

            Debug.Log($"[FighterCombat] {gameObject.name} recibió {damage} de daño. Vida restante: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                fighter.SetState(FighterState.Dead);
                Debug.Log($"[FighterCombat] ¡K.O.! {gameObject.name} ha sido derrotado.");
            }
            else
            {
                fighter.TakeHit(hitstunDuration, knockback, attacker);
            }
        }
    }
}
