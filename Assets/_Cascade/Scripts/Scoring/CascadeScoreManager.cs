using System;
using Cascade.Levels;
using UnityEngine;

namespace Cascade.Core
{
    public sealed class CascadeScoreManager : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;
        private ReactionEventBus _bus;
        private bool _crateHit;
        private bool _barrelHit;

        public int Score { get; private set; }
        public event Action<int> ScoreChanged;

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

        private void OnLevelLoaded(LevelRuntimeBinder _)
        {
            _crateHit = false;
            _barrelHit = false;
            SetScore(0);
        }

        private void OnReaction(ReactionEvent evt)
        {
            if (evt.eventId == "crate.hit") _crateHit = true;
            if (evt.eventId == "barrel.hit") _barrelHit = true;

            if (evt.eventId == "tower.damage" && Score == 0)
                SetScore(25);

            if (evt.eventId == "tower.destroyed")
            {
                if (_crateHit && _barrelHit) SetScore(100);
                else if (_barrelHit) SetScore(91);
                else SetScore(72);
            }
        }

        private void SetScore(int value)
        {
            value = Mathf.Clamp(value, 0, 100);
            if (Score == value) return;
            Score = value;
            ScoreChanged?.Invoke(Score);
        }
    }
}
