using System;
using System.Threading.Tasks;
using Cascade.State;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Cascade.Levels
{
    public sealed class LevelManager : MonoBehaviour
    {
        [SerializeField] private LevelCatalogSO catalog;
        [SerializeField] private Transform levelContainer;
        [SerializeField] private GameStateManager gameStateManager;

        private AsyncOperationHandle<GameObject>? _levelHandle;

        public LevelDefinitionSO CurrentDefinition { get; private set; }
        public LevelRuntimeBinder CurrentBinder { get; private set; }
        public event Action<LevelRuntimeBinder> LevelLoaded;

        private async void Start()
        {
            await LoadSelectedLevelAsync();
        }

        public async Task LoadSelectedLevelAsync()
        {
            await LoadLevelAsync(LevelSelection.SelectedLevelId);
        }

        public async Task LoadLevelAsync(string levelId)
        {
            await UnloadCurrentAsync();

            CurrentDefinition = catalog != null ? catalog.Find(levelId) : null;
            if (CurrentDefinition == null)
            {
                Debug.LogError($"[LevelManager] Level definition '{levelId}' was not found in the catalog.");
                return;
            }

            if (CurrentDefinition.levelPrefab == null || !CurrentDefinition.levelPrefab.RuntimeKeyIsValid())
            {
                Debug.LogError($"[LevelManager] Level '{levelId}' has no valid Addressable prefab reference.");
                return;
            }

            gameStateManager?.ForceState(GameState.Loading);

            var handle = CurrentDefinition.levelPrefab.InstantiateAsync(levelContainer);
            _levelHandle = handle;
            GameObject instance = await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded || instance == null)
            {
                Debug.LogError($"[LevelManager] Failed to load '{levelId}'.");
                return;
            }

            CurrentBinder = instance.GetComponent<LevelRuntimeBinder>();
            if (CurrentBinder == null)
            {
                Debug.LogError($"[LevelManager] '{levelId}' prefab is missing LevelRuntimeBinder.");
                return;
            }

            // A loaded level is first considered observable. Subscribers may then
            // advance it into Preparation (for example after an intro camera pass).
            // This ordering is important: Loading -> Preparation is intentionally
            // invalid in GameStateManager.
            gameStateManager?.TrySetState(GameState.Observation);
            LevelLoaded?.Invoke(CurrentBinder);
        }

        public async void ReplayCurrent()
        {
            if (CurrentDefinition != null)
                await LoadLevelAsync(CurrentDefinition.levelId);
        }

        public async Task UnloadCurrentAsync()
        {
            CurrentBinder = null;
            if (_levelHandle.HasValue && _levelHandle.Value.IsValid())
            {
                Addressables.ReleaseInstance(_levelHandle.Value);
                _levelHandle = null;
                await Task.Yield();
            }
        }

        private async void OnDestroy()
        {
            await UnloadCurrentAsync();
        }
    }
}
