using System;
using UnityEngine;

namespace Cascade.Core
{
    public sealed class DamageableStructure : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float damagePerImpulse = 4f;
        [SerializeField] private float minimumDamageImpulse = 1.25f;
        [SerializeField] private float collapseThreshold01 = 0.45f;
        [SerializeField] private string damagedEventId = "tower.damage";
        [SerializeField] private string destroyedEventId = "tower.destroyed";

        private ReactionEventBus _bus;
        private Rigidbody[] _pieces = Array.Empty<Rigidbody>();
        private float _health;
        private bool _collapsed;

        public float Health01 => maxHealth <= 0f ? 0f : Mathf.Clamp01(_health / maxHealth);
        public bool IsCollapsed => _collapsed;

        private void Awake()
        {
            _bus = FindFirstObjectByType<ReactionEventBus>();
            _pieces = GetComponentsInChildren<Rigidbody>(true);
            ResetStructure();
        }

        public void Configure(float health, float impulseScale, float collapseAt01)
        {
            maxHealth = Mathf.Max(1f, health);
            damagePerImpulse = Mathf.Max(0.1f, impulseScale);
            collapseThreshold01 = Mathf.Clamp01(collapseAt01);
        }

        public void ResetStructure()
        {
            _health = maxHealth;
            _collapsed = false;

            foreach (Rigidbody rb in _pieces)
            {
                if (rb == null)
                    continue;

                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();
            }
        }

        public void ApplyImpact(float impulse, GameObject source, Vector3 point)
        {
            if (_collapsed || impulse < minimumDamageImpulse)
                return;

            float damage = impulse * damagePerImpulse;
            _health = Mathf.Max(0f, _health - damage);
            _bus?.Publish(damagedEventId, source, gameObject, point, damage);

            if (Health01 <= collapseThreshold01)
                Collapse(source, point);
        }

        public void Collapse(GameObject source, Vector3 point)
        {
            if (_collapsed)
                return;

            _collapsed = true;

            foreach (Rigidbody rb in _pieces)
            {
                if (rb == null)
                    continue;

                rb.isKinematic = false;
                rb.WakeUp();
            }

            _bus?.Publish(destroyedEventId, source, gameObject, point, 1f);
        }
    }
}
