using UnityEngine;
using FightingGame.Core;
using FightingGame.Character;

namespace FightingGame.Combat
{
    [RequireComponent(typeof(Collider))]
    public class Hitbox : MonoBehaviour
    {
        [Header("Hitbox Info")]
        [SerializeField] private AttackType attackType = AttackType.None;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float hitstunDuration = 0.4f;
        [SerializeField] private Vector2 knockback = new Vector2(2f, 0f);

        private FighterController ownerFighter;
        private Collider hitboxCollider;
        private bool hasHitThisActivation = false;

        public AttackType AttackType => attackType;

        private void Awake()
        {
            hitboxCollider = GetComponent<Collider>();
            hitboxCollider.isTrigger = true;
            hitboxCollider.enabled = false; // Desactivado por defecto
            
            ownerFighter = GetComponentInParent<FighterController>();
        }

        public void Activate(AttackType type, float dmg = 10f, float hitstun = 0.4f, Vector2 kb = default)
        {
            attackType = type;
            damage = dmg;
            hitstunDuration = hitstun;
            knockback = kb == default ? new Vector2(2f, 0f) : kb;
            hasHitThisActivation = false;

            if (hitboxCollider != null)
            {
                hitboxCollider.enabled = true;
            }
        }

        public void Deactivate()
        {
            if (hitboxCollider != null)
            {
                hitboxCollider.enabled = false;
            }
            hasHitThisActivation = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasHitThisActivation) return; // Evitar múltiples golpes en el mismo ataque

            Hurtbox hurtbox = other.GetComponent<Hurtbox>();
            if (hurtbox != null)
            {
                // Asegurarse de no golpearse a sí mismo
                if (hurtbox.OwnerFighter != ownerFighter)
                {
                    hasHitThisActivation = true;
                    hurtbox.TakeHit(damage, hitstunDuration, knockback, ownerFighter);
                    Debug.Log($"[Hitbox] ¡Golpe conectado! {attackType} conectó en {hurtbox.name} por {damage} de daño.");
                }
            }
        }

        private void OnDrawGizmos()
        {
            Collider col = GetComponent<Collider>();
            if (col != null && col.enabled)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                Gizmos.matrix = transform.localToWorldMatrix;
                if (col is BoxCollider box)
                {
                    Gizmos.DrawCube(box.center, box.size);
                }
                else if (col is SphereCollider sphere)
                {
                    Gizmos.DrawSphere(sphere.center, sphere.radius);
                }
            }
        }
    }
}
