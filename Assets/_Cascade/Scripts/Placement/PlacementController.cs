using Cascade.Levels;
using Cascade.State;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cascade.Core
{
    public sealed class PlacementController : MonoBehaviour
    {
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private float rotationStep = 15f;

        private PlaceableTool _activeTool;
        private Plane _dragPlane;
        private bool _dragging;

        public bool HasValidPlacement => _activeTool != null && _activeTool.IsValidPlacement;

        private void Awake()
        {
            if (gameplayCamera == null) gameplayCamera = Camera.main;
            if (levelManager == null) levelManager = FindFirstObjectByType<LevelManager>();
            if (gameStateManager == null) gameStateManager = FindFirstObjectByType<GameStateManager>();
        }

        private void OnEnable()
        {
            if (levelManager != null) levelManager.LevelLoaded += OnLevelLoaded;
        }

        private void OnDisable()
        {
            if (levelManager != null) levelManager.LevelLoaded -= OnLevelLoaded;
        }

        private void OnLevelLoaded(LevelRuntimeBinder binder)
        {
            _activeTool = binder.GetComponentInChildren<PlaceableTool>(true);
            if (_activeTool != null)
            {
                _activeTool.CaptureInitialPose();
                _dragPlane = new Plane(Vector3.up, _activeTool.transform.position);
            }
        }

        private void Update()
        {
            if (_activeTool == null || gameStateManager == null || gameStateManager.CurrentState != GameState.Preparation)
                return;

            if (Input.GetMouseButtonDown(0)) BeginDrag(Input.mousePosition);
            if (_dragging && Input.GetMouseButton(0)) Drag(Input.mousePosition);
            if (_dragging && Input.GetMouseButtonUp(0)) EndDrag();
        }

        private void BeginDrag(Vector2 screenPosition)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            if (gameplayCamera == null) return;

            Ray ray = gameplayCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, ~0, QueryTriggerInteraction.Ignore))
            {
                var tool = hit.collider.GetComponentInParent<PlaceableTool>();
                if (tool == _activeTool) _dragging = true;
            }
        }

        private void Drag(Vector2 screenPosition)
        {
            Ray ray = gameplayCamera.ScreenPointToRay(screenPosition);
            if (!_dragPlane.Raycast(ray, out float enter)) return;

            Vector3 point = ray.GetPoint(enter);
            _activeTool.MoveTo(point);
        }

        private void EndDrag()
        {
            _dragging = false;
            _activeTool.CommitOrRollback();
        }

        public void RotateActiveTool()
        {
            if (_activeTool == null || gameStateManager == null || gameStateManager.CurrentState != GameState.Preparation) return;
            _activeTool.RotateYaw(rotationStep);
            _activeTool.CommitOrRollback();
        }

        public void ResetPlacement()
        {
            if (_activeTool == null || gameStateManager == null || gameStateManager.CurrentState != GameState.Preparation) return;
            _activeTool.ResetToInitial();
        }
    }
}
