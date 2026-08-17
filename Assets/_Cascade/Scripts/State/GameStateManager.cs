using System;
using UnityEngine;

namespace Cascade.State
{
    public enum GameState
    {
        Loading,
        Observation,
        Preparation,
        Simulation,
        Result
    }

    public sealed class GameStateManager : MonoBehaviour
    {
        public GameState CurrentState { get; private set; } = GameState.Loading;
        public event Action<GameState, GameState> StateChanged;

        public bool TrySetState(GameState next)
        {
            if (!IsValidTransition(CurrentState, next))
            {
                Debug.LogWarning($"[GameStateManager] Invalid transition {CurrentState} -> {next}.");
                return false;
            }

            var previous = CurrentState;
            CurrentState = next;
            StateChanged?.Invoke(previous, next);
            return true;
        }

        public void ForceState(GameState next)
        {
            var previous = CurrentState;
            CurrentState = next;
            StateChanged?.Invoke(previous, next);
        }

        private static bool IsValidTransition(GameState current, GameState next)
        {
            if (current == next) return true;
            return current switch
            {
                GameState.Loading => next == GameState.Observation,
                GameState.Observation => next == GameState.Preparation || next == GameState.Loading,
                GameState.Preparation => next == GameState.Simulation || next == GameState.Observation || next == GameState.Loading,
                GameState.Simulation => next == GameState.Result || next == GameState.Loading,
                GameState.Result => next == GameState.Loading,
                _ => false
            };
        }
    }
}
