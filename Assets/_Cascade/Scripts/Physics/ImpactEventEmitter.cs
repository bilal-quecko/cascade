using UnityEngine;

namespace Cascade.Core
{
    public enum ImpactEventKind { Generic, Crate, Barrel, Boulder }

    public sealed class ImpactEventEmitter : MonoBehaviour
    {
        [SerializeField] private ImpactEventKind kind = ImpactEventKind.Generic;
        [SerializeField] private float minimumImpulse = 0.5f;
        private ReactionEventBus _bus;

        public void Configure(ImpactEventKind value, float minImpulse = 0.5f)
        {
            kind = value;
            minimumImpulse = minImpulse;
        }

        private void Awake() => _bus = FindFirstObjectByType<ReactionEventBus>();

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.name == "Ground" || collision.gameObject.name.Contains("Boundary")) return;

            float impulse = collision.impulse.magnitude;
            if (impulse < minimumImpulse) return;

            string id = kind switch
            {
                ImpactEventKind.Crate => "crate.hit",
                ImpactEventKind.Barrel => "barrel.hit",
                ImpactEventKind.Boulder => "boulder.hit",
                _ => "impact"
            };

            Vector3 point = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            _bus?.Publish(id, gameObject, collision.gameObject, point, impulse);
        }
    }
}
