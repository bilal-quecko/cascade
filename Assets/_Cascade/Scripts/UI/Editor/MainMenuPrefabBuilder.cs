#if UNITY_EDITOR
using System.IO;
using Cascade.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cascade.EditorTools
{
    /// <summary>
    /// One-time/bootstrap editor utility for creating the editable Main Menu prefab
    /// and installing it into SCN_MainMenu.
    ///
    /// The prefab is NEVER regenerated automatically. Once designers begin editing
    /// PF_MainMenu.prefab, those manual changes remain safe unless a developer
    /// explicitly chooses the destructive rebuild command and confirms it.
    /// </summary>
    public static class MainMenuPrefabBuilder
    {
        private const string PrefabPath = "Assets/_Cascade/Prefabs/Core/UI/PF_MainMenu.prefab";
        private const string ScenePath = "Assets/_Cascade/Scenes/SCN_MainMenu.unity";
        private const string GeneratedRootName = "PF_MainMenu";

        [MenuItem("Cascade/UI/Main Menu/Create & Install Prefab", priority = 100)]
        public static void CreateAndInstall()
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existingPrefab != null)
            {
                bool installExisting = EditorUtility.DisplayDialog(
                    "Cascade Main Menu",
                    "PF_MainMenu.prefab already exists. Its visual design will NOT be rebuilt.\n\nInstall the existing prefab into SCN_MainMenu?",
                    "Install Existing",
                    "Cancel");

                if (!installExisting)
                    return;

                InstallPrefabInScene();
                Debug.Log($"[Cascade] Existing Main Menu prefab installed in SCN_MainMenu: '{PrefabPath}'.");
                return;
            }

            BuildPrefabAsset();
            InstallPrefabInScene();
            Debug.Log($"[Cascade] Main Menu prefab created at '{PrefabPath}' and installed in SCN_MainMenu.");
        }

        [MenuItem("Cascade/UI/Main Menu/Install Existing Prefab In Scene", priority = 110)]
        public static void InstallExistingPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null)
            {
                EditorUtility.DisplayDialog(
                    "Cascade Main Menu",
                    "PF_MainMenu.prefab does not exist yet. Run 'Create & Install Prefab' first.",
                    "OK");
                return;
            }

            InstallPrefabInScene();
            Debug.Log("[Cascade] Main Menu scene refreshed from the existing editable prefab.");
        }

        [MenuItem("Cascade/UI/Main Menu/DESTRUCTIVE - Rebuild Prefab From Template", priority = 200)]
        public static void RebuildFromTemplate()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Rebuild Main Menu Prefab?",
                "WARNING: This replaces PF_MainMenu.prefab with the generated template and will overwrite manual UI layout/style changes made to that prefab.\n\nContinue only if you intentionally want to reset the Main Menu UI.",
                "Rebuild & Overwrite",
                "Cancel");

            if (!confirmed)
                return;

            BuildPrefabAsset();
            InstallPrefabInScene();
            Debug.LogWarning("[Cascade] PF_MainMenu.prefab was intentionally rebuilt from the template.");
        }

        [MenuItem("Cascade/UI/Main Menu/Open Prefab", priority = 120)]
        public static void OpenPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog(
                    "Cascade Main Menu",
                    "PF_MainMenu.prefab does not exist yet. Run 'Create & Install Prefab' first.",
                    "OK");
                return;
            }

            AssetDatabase.OpenAsset(prefab);
        }

        private static void BuildPrefabAsset()
        {
            EnsureDirectory(Path.GetDirectoryName(PrefabPath));

            GameObject root = BuildPrefabHierarchy();
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static GameObject BuildPrefabHierarchy()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var root = new GameObject(GeneratedRootName,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(MainMenuController));

            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            Stretch(root.GetComponent<RectTransform>());

            CreateImage(root.transform, "Background", new Color(0.055f, 0.075f, 0.09f, 1f), true);

            GameObject mainPanel = CreatePanel(root.transform, "MainPanel");
            GameObject worldPanel = CreatePanel(root.transform, "WorldMapPanel");
            GameObject settingsPanel = CreatePanel(root.transform, "SettingsPanel");
            GameObject collectionPanel = CreatePanel(root.transform, "CollectionPanel");

            CreateText(mainPanel.transform, font, "Title", "CASCADE", 84, new Vector2(0f, 520f), FontStyle.Bold);
            CreateText(mainPanel.transform, font, "Subtitle", "Small action. Massive reaction.", 30, new Vector2(0f, 430f), FontStyle.Normal);

            Button playButton = CreateButton(mainPanel.transform, font, "PlayButton", "PLAY", new Vector2(0f, 180f), new Vector2(620f, 150f), true, true);
            Button sanctuaryButton = CreateButton(mainPanel.transform, font, "SanctuaryButton", "SANCTUARY", new Vector2(0f, -20f), new Vector2(520f, 110f), false, true);
            Button collectionButton = CreateButton(mainPanel.transform, font, "CollectionButton", "COLLECTION", new Vector2(0f, -165f), new Vector2(520f, 110f), false, true);
            Button settingsButton = CreateButton(mainPanel.transform, font, "SettingsButton", "SETTINGS", new Vector2(0f, -310f), new Vector2(520f, 110f), false, true);

            CreateText(worldPanel.transform, font, "Title", "WORLD PROGRESS", 58, new Vector2(0f, 600f), FontStyle.Bold);
            CreateText(worldPanel.transform, font, "WorldName", "Opening World", 36, new Vector2(0f, 500f), FontStyle.Normal);
            CreateText(worldPanel.transform, font, "Description", "Levels unlock sequentially. The first implementation starts with Level 1: The First Push.", 26, new Vector2(0f, 390f), FontStyle.Normal, new Vector2(820f, 170f));

            Button levelOneButton = CreateButton(worldPanel.transform, font, "LevelOneButton", "LEVEL 1  •  THE FIRST PUSH", new Vector2(0f, 120f), new Vector2(760f, 130f), true, true);
            CreateButton(worldPanel.transform, font, "LevelTwoButton", "LEVEL 2  •  LOCKED", new Vector2(0f, -40f), new Vector2(760f, 110f), false, false);
            CreateButton(worldPanel.transform, font, "LevelThreeButton", "LEVEL 3  •  LOCKED", new Vector2(0f, -180f), new Vector2(760f, 110f), false, false);
            Button worldBackButton = CreateButton(worldPanel.transform, font, "BackButton", "BACK", new Vector2(0f, -520f), new Vector2(380f, 100f), false, true);

            CreateText(settingsPanel.transform, font, "Title", "SETTINGS", 58, new Vector2(0f, 600f), FontStyle.Bold);
            CreateText(settingsPanel.transform, font, "Description", "Settings UI is connected to navigation. Audio, haptics and quality controls will be bound to SettingsService next.", 28, new Vector2(0f, 250f), FontStyle.Normal, new Vector2(820f, 260f));
            Button settingsBackButton = CreateButton(settingsPanel.transform, font, "BackButton", "BACK", new Vector2(0f, -520f), new Vector2(380f, 100f), false, true);

            CreateText(collectionPanel.transform, font, "Title", "COLLECTION", 58, new Vector2(0f, 600f), FontStyle.Bold);
            CreateText(collectionPanel.transform, font, "Description", "Cosmetic collection placeholder. This screen is reserved for visual-only unlocks such as skins, trails, particles and sanctuary decorations.", 28, new Vector2(0f, 250f), FontStyle.Normal, new Vector2(820f, 300f));
            Button collectionBackButton = CreateButton(collectionPanel.transform, font, "BackButton", "BACK", new Vector2(0f, -520f), new Vector2(380f, 100f), false, true);

            worldPanel.SetActive(false);
            settingsPanel.SetActive(false);
            collectionPanel.SetActive(false);

            var controller = root.GetComponent<MainMenuController>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("mainPanel").objectReferenceValue = mainPanel;
            serialized.FindProperty("worldMapPanel").objectReferenceValue = worldPanel;
            serialized.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
            serialized.FindProperty("collectionPanel").objectReferenceValue = collectionPanel;
            serialized.FindProperty("playButton").objectReferenceValue = playButton;
            serialized.FindProperty("sanctuaryButton").objectReferenceValue = sanctuaryButton;
            serialized.FindProperty("collectionButton").objectReferenceValue = collectionButton;
            serialized.FindProperty("settingsButton").objectReferenceValue = settingsButton;
            serialized.FindProperty("levelOneButton").objectReferenceValue = levelOneButton;
            serialized.FindProperty("worldBackButton").objectReferenceValue = worldBackButton;
            serialized.FindProperty("settingsBackButton").objectReferenceValue = settingsBackButton;
            serialized.FindProperty("collectionBackButton").objectReferenceValue = collectionBackButton;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return root;
        }

        private static void InstallPrefabInScene()
        {
            Scene previousScene = SceneManager.GetActiveScene();
            string previousScenePath = previousScene.path;

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            foreach (GameObject go in scene.GetRootGameObjects())
            {
                if (go.name == GeneratedRootName || go.name == "MainMenuRuntimeUI" || go.name == "MainMenuCanvas")
                    Object.DestroyImmediate(go);
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[Cascade] Cannot install Main Menu. Prefab not found at '{PrefabPath}'.");
                return;
            }

            PrefabUtility.InstantiatePrefab(prefab, scene);

            if (Object.FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            if (!string.IsNullOrEmpty(previousScenePath) && previousScenePath != ScenePath && File.Exists(previousScenePath))
                EditorSceneManager.OpenScene(previousScenePath, OpenSceneMode.Single);
        }

        private static GameObject CreatePanel(Transform parent, string name)
        {
            var panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            Stretch(panel.GetComponent<RectTransform>());
            return panel;
        }

        private static Image CreateImage(Transform parent, string name, Color color, bool stretch)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            if (stretch)
                Stretch(go.GetComponent<RectTransform>());

            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Button CreateButton(Transform parent, Font font, string objectName, string label, Vector2 position, Vector2 size, bool primary, bool interactable)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var image = go.GetComponent<Image>();
            image.color = interactable
                ? (primary ? new Color(0.95f, 0.58f, 0.14f, 1f) : new Color(0.13f, 0.18f, 0.21f, 0.96f))
                : new Color(0.1f, 0.12f, 0.13f, 0.65f);

            var button = go.GetComponent<Button>();
            button.interactable = interactable;

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(go.transform, false);
            Stretch(labelObject.GetComponent<RectTransform>());

            var text = labelObject.GetComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = font;
            text.fontSize = primary ? 38 : 30;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;

            return button;
        }

        private static Text CreateText(Transform parent, Font font, string objectName, string value, int fontSize, Vector2 position, FontStyle style, Vector2? size = null)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size ?? new Vector2(900f, 120f);
            rect.anchoredPosition = position;

            var text = go.GetComponent<Text>();
            text.text = value;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
#endif
