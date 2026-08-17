using System;
using Cascade.Levels;
using Cascade.State;
using UnityEngine;

namespace Cascade.CameraSystem
{
    public enum CameraViewMode
    {
        Overview2D,
        Cinematic
    }

    public sealed class CameraDirector : MonoBehaviour
    {
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private GameStateManager gameStateManager;

        [Header("Overview 2D")]
        [SerializeField] private float orthographicSize = 9.5f;
        [SerializeField] private float overviewMoveSpeed = 7f;
        [SerializeField] private float overviewRotateSpeed = 7f;

        [Header("Cinematic")]
        [SerializeField] private float cinematicFieldOfView = 52f;
        [SerializeField] private float cinematicDistance = 7.5f;
        [SerializeField] private float cinematicHeight = 3.2f;
        [SerializeField] private float cinematicSideOffset = 1.5f;
        [SerializeField] private float cinematicPositionDamping = 6f;
        [SerializeField] private float cinematicRotationDamping = 8f;
        [SerializeField] private float velocityDirectionThreshold = 0.4f;

        private LevelRuntimeBinder _binder;
        private Transform _overviewTarget;
        private Transform _cinematicTarget;
        private Rigidbody _cinematicBody;
        private Vector3 _lastTravelDirection = Vector3.forward;
        private CameraViewMode _viewMode = CameraViewMode.Overview2D;

        public CameraViewMode ViewMode => _viewMode;
        public bool IsCinematic => _viewMode == CameraViewMode.Cinematic;
        public event Action<CameraViewMode> ViewModeChanged;

        private void Awake()
        {
            if (gameplayCamera == null) gameplayCamera = Camera.main;
            if (levelManager == null) levelManager = FindFirstObjectByType<LevelManager>();
            if (gameStateManager == null) gameStateManager = FindFirstObjectByType<GameStateManager>();

            ApplyProjection(false);
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
            _overviewTarget = binder.observationAnchor != null ? binder.observationAnchor : binder.transform;
            _cinematicTarget = ResolveCinematicTarget(binder);
            _cinematicBody = _cinematicTarget != null ? _cinematicTarget.GetComponentInParent<Rigidbody>() : null;
            _lastTravelDirection = GetInitialTravelDirection();

            SetViewMode(CameraViewMode.Overview2D, true);
        }

        private void OnStateChanged(GameState _, GameState next)
        {
            if (_binder == null) return;

            if (next == GameState.Result)
            {
                // Results are always presented clearly in the stable authored view.
                _overviewTarget = _binder.resultAnchor != null ? _binder.resultAnchor : _binder.observationAnchor;
                SetViewMode(CameraViewMode.Overview2D);
                return;
            }

            if (next is GameState.Observation or GameState.Preparation)
                _overviewTarget = _binder.observationAnchor != null ? _binder.observationAnchor : _binder.transform;
        }

        private void LateUpdate()
        {
            if (gameplayCamera == null || _binder == null) return;

            if (_viewMode == CameraViewMode.Cinematic &&
                gameStateManager != null &&
                gameStateManager.CurrentState == GameState.Simulation &&
                _cinematicTarget != null)
            {
                UpdateCinematicFollow();
            }
            else
            {
                UpdateOverview();
            }
        }

        public void ToggleViewMode()
        {
            SetViewMode(IsCinematic ? CameraViewMode.Overview2D : CameraViewMode.Cinematic);
        }

        public void SetViewMode(CameraViewMode mode, bool snap = false)
        {
            _viewMode = mode;
            ApplyProjection(mode == CameraViewMode.Cinematic);

            if (snap)
            {
                if (mode == CameraViewMode.Cinematic)
                    SnapCinematic();
                else
                    SnapOverview();
            }

            ViewModeChanged?.Invoke(_viewMode);
        }

        public void FocusObservation(LevelRuntimeBinder binder)
        {
            if (binder == null) return;
            _overviewTarget = binder.observationAnchor != null ? binder.observationAnchor : binder.transform;
            SetViewMode(CameraViewMode.Overview2D);
        }

        public void FocusResult(LevelRuntimeBinder binder)
        {
            if (binder == null) return;
            _overviewTarget = binder.resultAnchor != null ? binder.resultAnchor : binder.observationAnchor;
            SetViewMode(CameraViewMode.Overview2D);
        }

        private void ApplyProjection(bool cinematic)
        {
            if (gameplayCamera == null) return;
            gameplayCamera.orthographic = !cinematic;
            if (cinematic)
                gameplayCamera.fieldOfView = cinematicFieldOfView;
            else
                gameplayCamera.orthographicSize = orthographicSize;
        }

        private void UpdateOverview()
        {
            if (_overviewTarget == null) return;
            Transform cameraTransform = gameplayCamera.transform;
            float dt = Time.unscaledDeltaTime;
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, _overviewTarget.position, dt * overviewMoveSpeed);
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, _overviewTarget.rotation, dt * overviewRotateSpeed);
        }

        private void UpdateCinematicFollow()
        {
            Vector3 travelDirection = GetTravelDirection();
            Vector3 right = Vector3.Cross(Vector3.up, travelDirection).normalized;
            if (right.sqrMagnitude < 0.001f) right = Vector3.right;

            Vector3 targetPosition = _cinematicTarget.position;
            Vector3 desiredPosition = targetPosition
                                      - travelDirection * cinematicDistance
                                      + Vector3.up * cinematicHeight
                                      + right * cinematicSideOffset;

            Quaternion desiredRotation = Quaternion.LookRotation(
                (targetPosition + Vector3.up * 0.45f - desiredPosition).normalized,
                Vector3.up);

            Transform cameraTransform = gameplayCamera.transform;
            float dt = Time.unscaledDeltaTime;
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPosition, dt * cinematicPositionDamping);
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, desiredRotation, dt * cinematicRotationDamping);
        }

        private void SnapOverview()
        {
            if (_overviewTarget == null || gameplayCamera == null) return;
            gameplayCamera.transform.SetPositionAndRotation(_overviewTarget.position, _overviewTarget.rotation);
        }

        private void SnapCinematic()
        {
            if (_cinematicTarget == null || gameplayCamera == null) return;
            Vector3 direction = GetTravelDirection();
            Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
            if (right.sqrMagnitude < 0.001f) right = Vector3.right;
            Vector3 position = _cinematicTarget.position - direction * cinematicDistance + Vector3.up * cinematicHeight + right * cinematicSideOffset;
            Quaternion rotation = Quaternion.LookRotation((_cinematicTarget.position + Vector3.up * 0.45f - position).normalized, Vector3.up);
            gameplayCamera.transform.SetPositionAndRotation(position, rotation);
        }

        private Vector3 GetTravelDirection()
        {
            if (_cinematicBody != null)
            {
                Vector3 velocity = _cinematicBody.linearVelocity;
                velocity.y = 0f;
                if (velocity.magnitude >= velocityDirectionThreshold)
                    _lastTravelDirection = velocity.normalized;
            }

            if (_lastTravelDirection.sqrMagnitude < 0.001f)
                _lastTravelDirection = Vector3.forward;

            return _lastTravelDirection.normalized;
        }

        private Vector3 GetInitialTravelDirection()
        {
            if (_binder != null && _binder.simulationInterestRoot != null && _cinematicTarget != null)
            {
                Vector3 direction = _binder.simulationInterestRoot.position - _cinematicTarget.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.01f)
                    return direction.normalized;
            }

            return _cinematicTarget != null ? _cinematicTarget.forward : Vector3.forward;
        }

        private static Transform ResolveCinematicTarget(LevelRuntimeBinder binder)
        {
            if (binder == null) return null;
            if (binder.cinematicFollowTarget != null) return binder.cinematicFollowTarget;

            Rigidbody[] bodies = binder.GetRuntimeRigidbodies();
            string[] preferredNames = { "boulder", "ball", "cart", "weight" };

            foreach (string preferred in preferredNames)
            {
                foreach (Rigidbody body in bodies)
                {
                    if (body != null && body.name.ToLowerInvariant().Contains(preferred))
                        return body.transform;
                }
            }

            return bodies.Length > 0 && bodies[0] != null ? bodies[0].transform : binder.transform;
        }
    }
}
