using Cascade.CameraSystem;
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
        [SerializeField] private TMP_Text resultTitle;
        [SerializeField] private GameObject preparationControls;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private Button startCascadeButton;
        [SerializeField] private Button rotateButton;
        [SerializeField] private Button resetPlacementButton;
        [SerializeField] private Button cinematicButton;
        [SerializeField] private TMP_Text cinematicButtonLabel;
        [SerializeField] private Button replayButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private SimulationController simulationController;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private PlacementController placementController;
        [SerializeField] private CascadeScoreManager scoreManager;
        [SerializeField] private ObjectiveManager objectiveManager;
        [SerializeField] private CameraDirector cameraDirector;

        private void Awake()
        {
            if (simulationController == null) simulationController = FindFirstObjectByType<SimulationController>();
            if (levelManager == null) levelManager = FindFirstObjectByType<LevelManager>();
            if (gameStateManager == null) gameStateManager = FindFirstObjectByType<GameStateManager>();
            if (placementController == null) placementController = FindFirstObjectByType<PlacementController>();
            if (scoreManager == null) scoreManager = FindFirstObjectByType<CascadeScoreManager>();
            if (objectiveManager == null) objectiveManager = FindFirstObjectByType<ObjectiveManager>();
            if (cameraDirector == null) cameraDirector = FindFirstObjectByType<CameraDirector>();

            rotateButton ??= FindButton("RotateButton");
            resetPlacementButton ??= FindButton("ResetPlacementButton");
            cinematicButton ??= FindButton("CinematicButton");
            resultTitle ??= FindText("ResultTitle");

            if (cinematicButton != null && cinematicButtonLabel == null)
                cinematicButtonLabel = cinematicButton.GetComponentInChildren<TMP_Text>(true);

            if (startCascadeButton != null) startCascadeButton.onClick.AddListener(StartCascade);
            if (rotateButton != null) rotateButton.onClick.AddListener(() => placementController?.RotateActiveTool());
            if (resetPlacementButton != null) resetPlacementButton.onClick.AddListener(() => placementController?.ResetPlacement());
            if (cinematicButton != null) cinematicButton.onClick.AddListener(() => cameraDirector?.ToggleViewMode());
            if (replayButton != null) replayButton.onClick.AddListener(() => levelManager?.ReplayCurrent());
            if (menuButton != null) menuButton.onClick.AddListener(SceneNavigator.LoadMainMenu);

            RefreshCameraButton();
        }

        private void OnEnable()
        {
            if (levelManager != null) levelManager.LevelLoaded += OnLevelLoaded;
            if (gameStateManager != null)
            {
                gameStateManager.StateChanged += OnStateChanged;
                ApplyState(gameStateManager.CurrentState);
            }
            if (scoreManager != null) scoreManager.ScoreChanged += OnScoreChanged;
            if (cameraDirector != null) cameraDirector.ViewModeChanged += OnCameraModeChanged;
        }

        private void OnDisable()
        {
            if (levelManager != null) levelManager.LevelLoaded -= OnLevelLoaded;
            if (gameStateManager != null) gameStateManager.StateChanged -= OnStateChanged;
            if (scoreManager != null) scoreManager.ScoreChanged -= OnScoreChanged;
            if (cameraDirector != null) cameraDirector.ViewModeChanged -= OnCameraModeChanged;
        }

        private void OnLevelLoaded(LevelRuntimeBinder _)
        {
            var definition = levelManager != null ? levelManager.CurrentDefinition : null;
            if (definition == null) return;
            if (levelText != null) levelText.text = $"{definition.sequenceIndex:00}  {definition.displayName}";
            if (objectiveText != null) objectiveText.text = definition.primaryObjective;
            if (gameStateManager != null) ApplyState(gameStateManager.CurrentState);
            RefreshCameraButton();
        }

        private void OnStateChanged(GameState _, GameState next) => ApplyState(next);

        private void ApplyState(GameState state)
        {
            if (preparationControls != null) preparationControls.SetActive(state == GameState.Preparation);
            if (resultPanel != null) resultPanel.SetActive(state == GameState.Result);

            // The view switch remains available during preparation and simulation,
            // but is hidden on the result panel to keep the end-state presentation stable.
            if (cinematicButton != null)
                cinematicButton.gameObject.SetActive(state == GameState.Preparation || state == GameState.Simulation);

            if (state == GameState.Result && resultTitle != null)
            {
                int score = scoreManager != null ? scoreManager.Score : 0;
                bool complete = objectiveManager != null && objectiveManager.PrimaryComplete;
                resultTitle.text = complete ? $"CASCADE COMPLETE\n{score}%" : $"TRY ANOTHER ROUTE\n{score}%";
            }
        }

        private void OnScoreChanged(int score)
        {
            if (gameStateManager != null && gameStateManager.CurrentState == GameState.Result && resultTitle != null)
                resultTitle.text = (objectiveManager != null && objectiveManager.PrimaryComplete ? "CASCADE COMPLETE" : "TRY ANOTHER ROUTE") + $"\n{score}%";
        }

        private void OnCameraModeChanged(CameraViewMode _) => RefreshCameraButton();

        private void RefreshCameraButton()
        {
            if (cinematicButtonLabel == null || cameraDirector == null) return;
            cinematicButtonLabel.text = cameraDirector.IsCinematic ? "2D VIEW" : "CINEMATIC";
        }

        private void StartCascade() => simulationController?.StartCascade();

        private Button FindButton(string objectName)
        {
            foreach (Button button in GetComponentsInChildren<Button>(true))
                if (button.name == objectName) return button;
            return null;
        }

        private TMP_Text FindText(string objectName)
        {
            foreach (TMP_Text text in GetComponentsInChildren<TMP_Text>(true))
                if (text.name == objectName) return text;
            return null;
        }
    }
}
