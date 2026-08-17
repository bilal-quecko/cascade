using Cascade.Levels;
using Cascade.State;
using UnityEngine;

namespace Cascade.CameraSystem
{
    public sealed class CameraDirector : MonoBehaviour
    {
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotateSpeed = 6f;

        private Transform _target;
        private LevelRuntimeBinder _binder;

        private void Awake()
        {
            if (gameplayCamera == null) gameplayCamera = Camera.main;
            if (levelManager == null) levelManager = FindFirstObjectByType<LevelManager>();
            if (gameStateManager == null) gameStateManager = FindFirstObjectByType<GameStateManager>();
        }

        private void OnEnable()
        {
            if (levelManager != null) levelManager.LevelLoaded += OnLevelLoaded;
            if (gameStateManager != null) gameStateManager.StateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            if (levelManager != null) levelManager.LevelLoaded -= OnLevelLoaded;
            if (gameStateManager != null) gameStateManager.StateChanged -= OnStateChanged;
        }

        private void OnLevelLoaded(LevelRuntimeBinder binder)
        {
            _binder = binder;
            _target = binder.observationAnchor != null ? binder.observationAnchor : binder.transform;
            SnapToTarget();
        }

        private void OnStateChanged(GameState _, GameState next)
        {
            if (_binder == null) return;
            _target = next switch
            {
                GameState.Result => _binder.resultAnchor != null ? _binder.resultAnchor : _binder.observationAnchor,
                _ => _binder.observationAnchor != null ? _binder.observationAnchor : _binder.transform
            };
        }

        private void LateUpdate()
        {
            if (_target == null || gameplayCamera == null) return;
            Transform t = gameplayCamera.transform;
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
