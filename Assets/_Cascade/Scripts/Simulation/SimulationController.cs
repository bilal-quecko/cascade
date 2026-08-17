using Cascade.Core;
using Cascade.Levels;
using Cascade.State;
using UnityEngine;

namespace Cascade.Simulation
{
    public sealed class SimulationController : MonoBehaviour
    {
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private float minimumSimulationTime = 2f;
        [SerializeField] private float settleHoldTime = 1.25f;
        [SerializeField] private float hardTimeout = 18f;
        [SerializeField] private float linearSettleSpeed = 0.12f;
        [SerializeField] private float angularSettleSpeed = 0.2f;

        private Rigidbody[] _rigidbodies = System.Array.Empty<Rigidbody>();
        private float _simulationStartedAt;
        private float _settledSince = -1f;
        private PlacementController _placementController;
        private ObjectiveManager _objectiveManager;

        private void Awake()
        {
            if (gameStateManager == null) gameStateManager = FindFirstObjectByType<GameStateManager>();
            if (levelManager == null) levelManager = FindFirstObjectByType<LevelManager>();
            _placementController = FindFirstObjectByType<PlacementController>();
            _objectiveManager = FindFirstObjectByType<ObjectiveManager>();
        }

        private void OnEnable()
        {
            if (levelManager != null) levelManager.LevelLoaded += OnLevelLoaded;
        }

        private void OnDisable()
        {
            if (levelManager != null) levelManager.LevelLoaded -= OnLevelLoaded;
        }

        private void Update()
        {
            if (gameStateManager == null || gameStateManager.CurrentState != GameState.Simulation) return;

            float elapsed = Time.time - _simulationStartedAt;
            if (elapsed >= hardTimeout)
            {
                FinishSimulation();
                return;
            }

            if (elapsed < minimumSimulationTime) return;

            bool settled = AreBodiesSettled();
            if (settled)
            {
                if (_settledSince < 0f) _settledSince = Time.time;
                if (Time.time - _settledSince >= settleHoldTime) FinishSimulation();
            }
            else
            {
                _settledSince = -1f;
            }
        }

        private void OnLevelLoaded(LevelRuntimeBinder binder)
        {
            _rigidbodies = binder.GetRuntimeRigidbodies();
            SetSimulationArmed(false);
            EnterPreparation();
        }

        public void EnterPreparation()
        {
            if (gameStateManager == null) return;
            if (gameStateManager.CurrentState == GameState.Observation)
                gameStateManager.TrySetState(GameState.Preparation);
        }

        public bool StartCascade()
        {
            if (gameStateManager == null || gameStateManager.CurrentState != GameState.Preparation) return false;
            if (_placementController != null && !_placementController.HasValidPlacement)
            {
                Debug.LogWarning("[SimulationController] Start blocked: active tool placement is invalid.");
                return false;
            }

            _simulationStartedAt = Time.time;
            _settledSince = -1f;
            SetSimulationArmed(true);
            gameStateManager.TrySetState(GameState.Simulation);
            return true;
        }

        public void FinishSimulation()
        {
            if (gameStateManager != null && gameStateManager.CurrentState == GameState.Simulation)
                gameStateManager.TrySetState(GameState.Result);
        }

        private bool AreBodiesSettled()
        {
            foreach (Rigidbody body in _rigidbodies)
            {
                if (body == null || body.isKinematic || body.IsSleeping()) continue;
                if (body.linearVelocity.sqrMagnitude > linearSettleSpeed * linearSettleSpeed) return false;
                if (body.angularVelocity.sqrMagnitude > angularSettleSpeed * angularSettleSpeed) return false;
            }
            return true;
        }

        private void SetSimulationArmed(bool armed)
        {
            foreach (Rigidbody body in _rigidbodies)
            {
                if (body == null) continue;

                // Player-placeable tools and structural pieces manage their own kinematic state.
                if (body.GetComponentInParent<PlaceableTool>() != null) continue;
                if (body.GetComponentInParent<DamageableStructure>() != null)
                {
                    if (!armed) body.isKinematic = true;
                    continue;
                }

                body.isKinematic = !armed;
                if (!armed)
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                    body.Sleep();
                }
                else body.WakeUp();
            }
        }
    }
}
