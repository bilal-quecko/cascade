#if UNITY_EDITOR
using Cascade.CameraSystem;
using Cascade.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cascade.EditorTools
{
    /// <summary>
    /// Non-destructive scene upgrade that enables the default orthographic gameplay view
    /// and adds an editable CINEMATIC / 2D VIEW toggle to the existing Gameplay HUD.
    /// It does not rebuild level prefabs.
    /// </summary>
    public static class GameplayCameraModeInstaller
    {
        private const string GameplayScenePath = "Assets/_Cascade/Scenes/SCN_Gameplay.unity";

        [MenuItem("Cascade/Gameplay/Camera/Install 2D + Cinematic Camera", priority = 340)]
        public static void Install()
        {
            Scene previousScene = SceneManager.GetActiveScene();
            string previousPath = previousScene.path;

            Scene scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);

            Camera camera = Object.FindFirstObjectByType<Camera>();
            CameraDirector director = Object.FindFirstObjectByType<CameraDirector>();
            GameplayHUDController hud = Object.FindFirstObjectByType<GameplayHUDController>();

            if (camera == null || director == null || hud == null)
            {
                EditorUtility.DisplayDialog(
                    "Cascade Gameplay Camera",
                    "SCN_Gameplay is missing Camera, CameraDirector, or GameplayHUDController. Run the Gameplay foundation builder first.",
                    "OK");
                RestorePrevious(previousPath);
                return;
            }

            camera.orthographic = true;
            camera.orthographicSize = 9.5f;
            EditorUtility.SetDirty(camera);

            Button cinematicButton = FindButton(hud.transform, "CinematicButton");
            TMP_Text label;

            if (cinematicButton == null)
            {
                cinematicButton = CreateButton(
                    hud.transform,
                    "CinematicButton",
                    "CINEMATIC",
                    new Vector2(0f, -835f),
                    new Vector2(360f, 82f));
            }

            label = cinematicButton.GetComponentInChildren<TMP_Text>(true);

            SerializedObject hudSO = new SerializedObject(hud);
            SetReference(hudSO, "cinematicButton", cinematicButton);
            SetReference(hudSO, "cinematicButtonLabel", label);
            SetReference(hudSO, "cameraDirector", director);
            hudSO.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject directorSO = new SerializedObject(director);
            SetReference(directorSO, "gameplayCamera", camera);
            directorSO.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(hud);
            EditorUtility.SetDirty(director);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[Cascade] Gameplay camera upgraded. Default view is orthographic; CINEMATIC toggles perspective follow mode.");
            RestorePrevious(previousPath);
        }

        private static Button FindButton(Transform root, string name)
        {
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
                if (button.name == name)
                    return button;
            return null;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image image = go.GetComponent<Image>();
            image.color = new Color(0.09f, 0.13f, 0.16f, 0.92f);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            RectTransform labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = labelGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 27f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;

            return go.GetComponent<Button>();
        }

        private static void SetReference(SerializedObject so, string propertyName, Object value)
        {
            SerializedProperty property = so.FindProperty(propertyName);
            if (property != null)
                property.objectReferenceValue = value;
        }

        private static void RestorePrevious(string path)
        {
            if (!string.IsNullOrEmpty(path) && path != GameplayScenePath)
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }
    }
}
#endif
