using UnityEngine;

namespace Cascade.Core
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class RestrictedFreePlacementZone : MonoBehaviour
    {
        private BoxCollider _box;

        private void Awake()
        {
            _box = GetComponent<BoxCollider>();
            _box.isTrigger = true;
        }

        public Vector3 ClampWorldPoint(Vector3 worldPoint)
        {
            if (_box == null) _box = GetComponent<BoxCollider>();
            Vector3 local = transform.InverseTransformPoint(worldPoint) - _box.center;
            Vector3 half = _box.size * 0.5f;
            local.x = Mathf.Clamp(local.x, -half.x, half.x);
            local.z = Mathf.Clamp(local.z, -half.z, half.z);
            local.y = 0f;
            return transform.TransformPoint(local + _box.center);
        }
    }
}
