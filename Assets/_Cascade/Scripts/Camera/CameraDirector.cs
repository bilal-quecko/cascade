using Cascade.Levels;
using UnityEngine;

namespace Cascade.CameraSystem
{
    public sealed class CameraDirector : MonoBehaviour
    {
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotateSpeed = 6f;

        private Transform _target;

        private void OnEnable()
        {
            if (levelManager != null) levelManager.LevelLoaded += OnLevelLoaded;
        }

        private void OnDisable()
        {
            if (levelManager != null) levelManager.LevelLoaded -= OnLevelLoaded;
        }

        private void OnLevelLoaded(LevelRuntimeBinder binder)
        {
            _target = binder.observationAnchor != null ? binder.observationAnchor : binder.transform;
            SnapToTarget();
        }

        private void LateUpdate()
        {
            if (_target == null || gameplayCamera == null) return;
            var t = gameplayCamera.transform;
            t.position = Vector3.Lerp(t.position, _target.position, Time.unscaledDeltaTime * moveSpeed);
            t.rotation = Quaternion.Slerp(t.rotation, _target.rotation, Time.unscaledDeltaTime * rotateSpeed);
        }

        public void FocusObservation(LevelRuntimeBinder binder) => SetTarget(binder != null ? binder.observationAnchor : null);
        public void FocusResult(LevelRuntimeBinder binder) => SetTarget(binder != null ? binder.resultAnchor : null);
        public void SetTarget(Transform target) => _target = target;

        private void SnapToTarget()
        {
            if (_target == null || gameplayCamera == null) return;
            gameplayCamera.transform.SetPositionAndRotation(_target.position, _target.rotation);
        }
    }
}
