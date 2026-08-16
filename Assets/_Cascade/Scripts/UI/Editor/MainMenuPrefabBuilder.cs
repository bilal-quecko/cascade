#if UNITY_EDITOR
using System.IO;
using Cascade.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cascade.EditorTools
{
    /// <summary>
    /// One-time/bootstrap editor utility for the editable Cascade Main Menu prefab.
    /// All visible text is TextMeshProUGUI. Unity UI Button remains the clickable
    /// control; its child label is TextMeshProUGUI.
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
                return;
            }

            BuildPrefabAsset();
            InstallPrefabInScene();
        }

        [MenuItem("Cascade/UI/Main Menu/Migrate Existing Menu To TextMeshPro", priority = 105)]
        public static void MigrateExistingMenuToTMP()
        {
            int prefabCount = MigratePrefabToTMP();
            int sceneCount = MigrateSceneToTMP();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Cascade Main Menu - TMP Migration",
                $"Migration complete.\n\nPrefab text components converted: {prefabCount}\nScene text components converted: {sceneCount}\n\nButton components remain Unity UI Buttons; their labels are now TextMeshProUGUI.",
                "OK");
        }

        [MenuItem("Cascade/UI/Main Menu/Install Existing Prefab In Scene", priority = 110)]
        public static void InstallExistingPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null)
            {
                EditorUtility.DisplayDialog("Cascade Main Menu", "PF_MainMenu.prefab does not exist yet.", "OK");
                return;
            }

            InstallPrefabInScene();
        }

        [MenuItem("Cascade/UI/Main Menu/Open Prefab", priority = 120)]
        public static void OpenPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("Cascade Main Menu", "PF_MainMenu.prefab does not exist yet.", "OK");
                return;
            }

            AssetDatabase.OpenAsset(prefab);
        }

        [MenuItem("Cascade/UI/Main Menu/DESTRUCTIVE - Rebuild Prefab From Template", priority = 200)]
        public static void RebuildFromTemplate()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Rebuild Main Menu Prefab?",
                "WARNING: This replaces PF_MainMenu.prefab and overwrites manual UI layout/style changes.",
                "Rebuild & Overwrite",
                "Cancel");

            if (!confirmed)
                return;

            BuildPrefabAsset();
            InstallPrefabInScene();
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
            var root = new GameObject(GeneratedRootName,
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster), typeof(MainMenuController));

            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
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

            CreateText(mainPanel.transform, "Title", "CASCADE", 84, new Vector2(0f, 520f), FontStyles.Bold);
            CreateText(mainPanel.transform, "Subtitle", "Small action. Massive reaction.", 30, new Vector2(0f, 430f), FontStyles.Normal);

            Button playButton = CreateButton(mainPanel.transform, "PlayButton", "PLAY", new Vector2(0f, 180f), new Vector2(620f, 150f), true, true);
            Button sanctuaryButton = CreateButton(mainPanel.transform, "SanctuaryButton", "SANCTUARY", new Vector2(0f, -20f), new Vector2(520f, 110f), false, true);
            Button collectionButton = CreateButton(mainPanel.transform, "CollectionButton", "COLLECTION", new Vector2(0f, -165f), new Vector2(520f, 110f), false, true);
            Button settingsButton = CreateButton(mainPanel.transform, "SettingsButton", "SETTINGS", new Vector2(0f, -310f), new Vector2(520f, 110f), false, true);

            CreateText(worldPanel.transform, "Title", "WORLD PROGRESS", 58, new Vector2(0f, 600f), FontStyles.Bold);
            CreateText(worldPanel.transform, "WorldName", "Opening World", 36, new Vector2(0f, 500f), FontStyles.Normal);
            CreateText(worldPanel.transform, "Description", "Levels unlock sequentially. The first implementation starts with Level 1: The First Push.", 26, new Vector2(0f, 390f), FontStyles.Normal, new Vector2(820f, 170f));

            Button levelOneButton = CreateButton(worldPanel.transform, "LevelOneButton", "LEVEL 1  •  THE FIRST PUSH", new Vector2(0f, 120f), new Vector2(760f, 130f), true, true);
            CreateButton(worldPanel.transform, "LevelTwoButton", "LEVEL 2  •  LOCKED", new Vector2(0f, -40f), new Vector2(760f, 110f), false, false);
            CreateButton(worldPanel.transform, "LevelThreeButton", "LEVEL 3  •  LOCKED", new Vector2(0f, -180f), new Vector2(760f, 110f), false, false);
            Button worldBackButton = CreateButton(worldPanel.transform, "BackButton", "BACK", new Vector2(0f, -520f), new Vector2(380f, 100f), false, true);

            CreateText(settingsPanel.transform, "Title", "SETTINGS", 58, new Vector2(0f, 600f), FontStyles.Bold);
            CreateText(settingsPanel.transform, "Description", "Settings UI is connected to navigation. Audio, haptics and quality controls will be bound to SettingsService next.", 28, new Vector2(0f, 250f), FontStyles.Normal, new Vector2(820f, 260f));
            Button settingsBackButton = CreateButton(settingsPanel.transform, "BackButton", "BACK", new Vector2(0f, -520f), new Vector2(380f, 100f), false, true);

            CreateText(collectionPanel.transform, "Title", "COLLECTION", 58, new Vector2(0f, 600f), FontStyles.Bold);
            CreateText(collectionPanel.transform, "Description", "Cosmetic collection placeholder. This screen is reserved for visual-only unlocks such as skins, trails, particles and sanctuary decorations.", 28, new Vector2(0f, 250f), FontStyles.Normal, new Vector2(820f, 300f));
            Button collectionBackButton = CreateButton(collectionPanel.transform, "BackButton", "BACK", new Vector2(0f, -520f), new Vector2(380f, 100f), false, true);

            worldPanel.SetActive(false);
            settingsPanel.SetActive(false);
            collectionPanel.SetActive(false);

            var serialized = new SerializedObject(root.GetComponent<MainMenuController>());
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

        private static int MigratePrefabToTMP()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null)
                return 0;

            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            int count = ConvertLegacyTexts(root);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            return count;
        }

        private static int MigrateSceneToTMP()
        {
            if (!File.Exists(ScenePath))
                return 0;

            Scene previous = SceneManager.GetActiveScene();
            string previousPath = previous.path;
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            int count = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
                count += ConvertLegacyTexts(root);

            if (count > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            if (!string.IsNullOrEmpty(previousPath) && previousPath != ScenePath && File.Exists(previousPath))
                EditorSceneManager.OpenScene(previousPath, OpenSceneMode.Single);

            return count;
        }

        private static int ConvertLegacyTexts(GameObject root)
        {
            Text[] legacyTexts = root.GetComponentsInChildren<Text>(true);
            int count = legacyTexts.Length;
            foreach (Text oldText in legacyTexts)
            {
                GameObject go = oldText.gameObject;
                string value = oldText.text;
                Color color = oldText.color;
                int fontSize = oldText.fontSize;
                TextAnchor alignment = oldText.alignment;
                FontStyle style = oldText.fontStyle;
                bool raycastTarget = oldText.raycastTarget;

                Object.DestroyImmediate(oldText, true);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text = value;
                tmp.color = color;
                tmp.fontSize = fontSize;
                tmp.alignment = ConvertAlignment(alignment);
                tmp.fontStyle = ConvertStyle(style);
                tmp.enableWordWrapping = true;
                tmp.overflowMode = TextOverflowModes.Overflow;
                tmp.raycastTarget = raycastTarget;
                if (TMP_Settings.defaultFontAsset != null)
                    tmp.font = TMP_Settings.defaultFontAsset;
            }
            return count;
        }

        private static void InstallPrefabInScene()
        {
            Scene previous = SceneManager.GetActiveScene();
            string previousPath = previous.path;
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

            if (!string.IsNullOrEmpty(previousPath) && previousPath != ScenePath && File.Exists(previousPath))
                EditorSceneManager.OpenScene(previousPath, OpenSceneMode.Single);
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
            if (stretch) Stretch(go.GetComponent<RectTransform>());
            Image image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Button CreateButton(Transform parent, string objectName, string label, Vector2 position, Vector2 size, bool primary, bool interactable)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            Image image = go.GetComponent<Image>();
            image.color = interactable
                ? (primary ? new Color(0.95f, 0.58f, 0.14f, 1f) : new Color(0.13f, 0.18f, 0.21f, 0.96f))
                : new Color(0.1f, 0.12f, 0.13f, 0.65f);

            Button button = go.GetComponent<Button>();
            button.interactable = interactable;

            TextMeshProUGUI text = CreateTMPObject(go.transform, "Label");
            Stretch(text.rectTransform);
            text.text = label;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = primary ? 38 : 30;
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;
            return button;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string objectName, string value, int fontSize, Vector2 position, FontStyles style, Vector2? size = null)
        {
            TextMeshProUGUI text = CreateTMPObject(parent, objectName);
            RectTransform rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size ?? new Vector2(900f, 120f);
            rect.anchoredPosition = position;
            text.text = value;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = Color.white;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Overflow;
            return text;
        }

        private static TextMeshProUGUI CreateTMPObject(Transform parent, string objectName)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            if (TMP_Settings.defaultFontAsset != null)
                text.font = TMP_Settings.defaultFontAsset;
            return text;
        }

        private static TextAlignmentOptions ConvertAlignment(TextAnchor anchor)
        {
            return anchor switch
            {
                TextAnchor.UpperLeft => TextAlignmentOptions.TopLeft,
                TextAnchor.UpperCenter => TextAlignmentOptions.Top,
                TextAnchor.UpperRight => TextAlignmentOptions.TopRight,
                TextAnchor.MiddleLeft => TextAlignmentOptions.Left,
                TextAnchor.MiddleCenter => TextAlignmentOptions.Center,
                TextAnchor.MiddleRight => TextAlignmentOptions.Right,
                TextAnchor.LowerLeft => TextAlignmentOptions.BottomLeft,
                TextAnchor.LowerCenter => TextAlignmentOptions.Bottom,
                TextAnchor.LowerRight => TextAlignmentOptions.BottomRight,
                _ => TextAlignmentOptions.Center
            };
        }

        private static FontStyles ConvertStyle(FontStyle style)
        {
            return style switch
            {
                FontStyle.Bold => FontStyles.Bold,
                FontStyle.Italic => FontStyles.Italic,
                FontStyle.BoldAndItalic => FontStyles.Bold | FontStyles.Italic,
                _ => FontStyles.Normal
            };
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
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }
    }
}
#endif
