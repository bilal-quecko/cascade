using UnityEngine;

namespace Cascade.Core
{
    public sealed class PlaceableTool : MonoBehaviour
    {
        [SerializeField] private RestrictedFreePlacementZone placementZone;
        [SerializeField] private Vector3 overlapHalfExtents = new(1.5f, 0.25f, 1f);
        [SerializeField] private LayerMask blockingMask = ~0;

        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private Vector3 _lastValidPosition;
        private Quaternion _lastValidRotation;

        public bool IsValidPlacement { get; private set; } = true;

        public void Configure(RestrictedFreePlacementZone zone) => placementZone = zone;

        public void CaptureInitialPose()
        {
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            _lastValidPosition = _initialPosition;
            _lastValidRotation = _initialRotation;
            IsValidPlacement = ValidateCurrentPose();
        }

        public void MoveTo(Vector3 worldPoint)
        {
            if (placementZone != null) worldPoint = placementZone.ClampWorldPoint(worldPoint);
            transform.position = worldPoint;
            IsValidPlacement = ValidateCurrentPose();
        }

        public void RotateYaw(float degrees)
        {
            transform.Rotate(Vector3.up, degrees, Space.World);
            IsValidPlacement = ValidateCurrentPose();
        }

        public void CommitOrRollback()
        {
            IsValidPlacement = ValidateCurrentPose();
            if (IsValidPlacement)
            {
                _lastValidPosition = transform.position;
                _lastValidRotation = transform.rotation;
            }
            else
            {
                transform.SetPositionAndRotation(_lastValidPosition, _lastValidRotation);
                IsValidPlacement = true;
            }
        }

        public void ResetToInitial()
        {
            transform.SetPositionAndRotation(_initialPosition, _initialRotation);
            _lastValidPosition = _initialPosition;
            _lastValidRotation = _initialRotation;
            IsValidPlacement = true;
        }

        private bool ValidateCurrentPose()
        {
            Collider[] hits = Physics.OverlapBox(transform.position, overlapHalfExtents, transform.rotation, blockingMask, QueryTriggerInteraction.Ignore);
            foreach (Collider hit in hits)
            {
                if (hit == null || hit.transform.IsChildOf(transform) || transform.IsChildOf(hit.transform)) continue;
                if (placementZone != null && hit.transform.IsChildOf(placementZone.transform)) continue;
                return false;
            }
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, overlapHalfExtents * 2f);
        }
    }
}
