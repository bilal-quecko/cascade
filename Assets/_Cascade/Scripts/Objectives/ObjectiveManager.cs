using System;
using Cascade.Levels;
using UnityEngine;

namespace Cascade.Core
{
    public sealed class ObjectiveManager : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;
        private ReactionEventBus _bus;

        public bool PrimaryComplete { get; private set; }
        public event Action<bool> ObjectiveCompleted;

        private void Awake()
        {
            if (levelManager == null) levelManager = FindFirstObjectByType<LevelManager>();
            _bus = FindFirstObjectByType<ReactionEventBus>();
        }

        private void OnEnable()
        {
            if (_bus == null) _bus = FindFirstObjectByType<ReactionEventBus>();
            if (_bus != null) _bus.EventRaised += OnReaction;
            if (levelManager != null) levelManager.LevelLoaded += OnLevelLoaded;
        }

        private void OnDisable()
        {
            if (_bus != null) _bus.EventRaised -= OnReaction;
            if (levelManager != null) levelManager.LevelLoaded -= OnLevelLoaded;
        }

        private void OnLevelLoaded(LevelRuntimeBinder _) => PrimaryComplete = false;

        private void OnReaction(ReactionEvent evt)
        {
            if (PrimaryComplete) return;
            if (evt.eventId != "tower.destroyed") return;
            PrimaryComplete = true;
            ObjectiveCompleted?.Invoke(true);
        }
    }
}
