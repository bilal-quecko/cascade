using UnityEngine;

namespace Cascade.Core
{
    public sealed class DamageablePiece : MonoBehaviour
    {
        [SerializeField] private float impulseMultiplier = 1f;

        private DamageableStructure _structure;

        private void Awake()
        {
            _structure = GetComponentInParent<DamageableStructure>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_structure == null)
                return;

            float impulse = collision.impulse.magnitude * impulseMultiplier;
            Vector3 point = collision.contactCount > 0
                ? collision.GetContact(0).point
                : transform.position;

            _structure.ApplyImpact(impulse, collision.gameObject, point);
        }
    }
}
