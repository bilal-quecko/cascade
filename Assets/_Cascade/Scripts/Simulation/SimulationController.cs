using Cascade.Levels;
using Cascade.State;
using UnityEngine;

namespace Cascade.Simulation
{
    public sealed class SimulationController : MonoBehaviour
    {
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private LevelManager levelManager;

        private Rigidbody[] _rigidbodies = System.Array.Empty<Rigidbody>();

        private void OnEnable()
        {
            if (levelManager != null)
                levelManager.LevelLoaded += OnLevelLoaded;
        }

        private void OnDisable()
        {
            if (levelManager != null)
                levelManager.LevelLoaded -= OnLevelLoaded;
        }

        private void OnLevelLoaded(LevelRuntimeBinder binder)
        {
            _rigidbodies = binder.GetRuntimeRigidbodies();
            SetSimulationArmed(false);
        }

        public void EnterPreparation()
        {
            gameStateManager?.TrySetState(GameState.Preparation);
        }

        public void StartCascade()
        {
            if (gameStateManager == null || gameStateManager.CurrentState != GameState.Preparation)
                return;

            SetSimulationArmed(true);
            gameStateManager.TrySetState(GameState.Simulation);
        }

        public void FinishSimulation()
        {
            if (gameStateManager != null && gameStateManager.CurrentState == GameState.Simulation)
                gameStateManager.TrySetState(GameState.Result);
        }

        private void SetSimulationArmed(bool armed)
        {
            foreach (var body in _rigidbodies)
            {
                if (body == null) continue;
                body.isKinematic = !armed;
                if (!armed)
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                    body.Sleep();
                }
                else
                {
                    body.WakeUp();
                }
            }
        }
    }
}
