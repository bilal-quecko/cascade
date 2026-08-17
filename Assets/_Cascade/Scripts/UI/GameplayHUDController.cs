using Cascade.Core;
using Cascade.Levels;
using Cascade.Simulation;
using Cascade.State;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.UI
{
    public sealed class GameplayHUDController : MonoBehaviour
    {
        [SerializeField] private TMP_Text objectiveText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private GameObject preparationControls;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Button startCascadeButton;
        [SerializeField] private Button replayButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private SimulationController simulationController;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private GameStateManager gameStateManager;

        private void Awake()
        {
            if (startCascadeButton != null) startCascadeButton.onClick.AddListener(StartCascade);
            if (replayButton != null) replayButton.onClick.AddListener(() => levelManager?.ReplayCurrent());
            if (menuButton != null) menuButton.onClick.AddListener(SceneNavigator.LoadMainMenu);
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

        private void OnLevelLoaded(LevelRuntimeBinder _)
        {
            var definition = levelManager.CurrentDefinition;
            if (definition == null) return;
            if (levelText != null) levelText.text = $"{definition.sequenceIndex:00}  {definition.displayName}";
            if (objectiveText != null) objectiveText.text = definition.primaryObjective;
            simulationController?.EnterPreparation();
        }

        private void OnStateChanged(GameState _, GameState next)
        {
            if (preparationControls != null) preparationControls.SetActive(next == GameState.Preparation);
            if (resultPanel != null) resultPanel.SetActive(next == GameState.Result);
        }

        private void StartCascade() => simulationController?.StartCascade();
    }
}
