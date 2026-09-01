using UnityEngine;
using FightingGame.Character;

namespace FightingGame.Combat
{
    [RequireComponent(typeof(Collider))]
    public class Hurtbox : MonoBehaviour
    {
        [SerializeField] private FighterController ownerFighter;
        [SerializeField] private FighterCombat combatManager;

        public FighterController OwnerFighter => ownerFighter;

        private void Awake()
        {
            if (ownerFighter == null)
            {
                ownerFighter = GetComponentInParent<FighterController>();
            }
            if (combatManager == null)
            {
                combatManager = GetComponentInParent<FighterCombat>();
            }

            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true; // Las hurtboxes deben ser triggers para no interferir con la física
            }
        }

        public void TakeHit(float damage, float hitstunDuration, Vector2 knockback, FighterController attacker)
        {
            if (combatManager != null)
            {
                combatManager.TakeDamage(damage, hitstunDuration, knockback, attacker);
            }
            else if (ownerFighter != null)
            {
                ownerFighter.TakeHit(hitstunDuration, knockback, attacker);
            }
        }

        private void OnDrawGizmos()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.35f);
                Gizmos.matrix = transform.localToWorldMatrix;
                if (col is BoxCollider box)
                {
                    Gizmos.DrawCube(box.center, box.size);
                }
                else if (col is SphereCollider sphere)
                {
                    Gizmos.DrawSphere(sphere.center, sphere.radius);
                }
                else if (col is CapsuleCollider cap)
                {
                    Gizmos.DrawWireSphere(cap.center, cap.radius);
                }
            }
        }
    }
}
