using Cascade.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cascade.UI
{
    /// <summary>
    /// Builds the first functional Cascade main menu at runtime.
    /// This is intentionally asset-light so the project is immediately navigable
    /// before final UI art and prefabs are introduced.
    /// </summary>
    public sealed class MainMenuRuntimeUI : MonoBehaviour
    {
        private GameObject _rootPanel;
        private GameObject _worldPanel;
        private GameObject _settingsPanel;
        private GameObject _collectionPanel;
        private Font _runtimeFont;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != SceneNavigator.MainMenuScene)
                return;

            if (FindFirstObjectByType<MainMenuRuntimeUI>() != null)
                return;

            var root = new GameObject("MainMenuRuntimeUI");
            root.AddComponent<MainMenuRuntimeUI>();
        }

        private void Awake()
        {
            _runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            EnsureEventSystem();
            BuildMenu();
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
                return;

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(eventSystem);
        }

        private void BuildMenu()
        {
            var canvasObject = new GameObject("MainMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            CreateBackground(canvasObject.transform);
            _rootPanel = CreatePanel(canvasObject.transform, "MainPanel");
            _worldPanel = CreatePanel(canvasObject.transform, "WorldMapPanel");
            _settingsPanel = CreatePanel(canvasObject.transform, "SettingsPanel");
            _collectionPanel = CreatePanel(canvasObject.transform, "CollectionPanel");

            BuildMainPanel(_rootPanel.transform);
            BuildWorldPanel(_worldPanel.transform);
            BuildSettingsPanel(_settingsPanel.transform);
            BuildCollectionPanel(_collectionPanel.transform);

            ShowOnly(_rootPanel);
        }

        private void CreateBackground(Transform parent)
        {
            var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(parent, false);
            Stretch(background.GetComponent<RectTransform>());
            background.GetComponent<Image>().color = new Color(0.055f, 0.075f, 0.09f, 1f);
        }

        private GameObject CreatePanel(Transform parent, string name)
        {
            var panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            Stretch(panel.GetComponent<RectTransform>());
            return panel;
        }

        private void BuildMainPanel(Transform panel)
        {
            CreateText(panel, "CASCADE", 84, new Vector2(0f, 520f), FontStyle.Bold);
            CreateText(panel, "Small action. Massive reaction.", 30, new Vector2(0f, 430f), FontStyle.Normal);

            CreateButton(panel, "PLAY", new Vector2(0f, 180f), new Vector2(620f, 150f), true, () => ShowOnly(_worldPanel));
            CreateButton(panel, "SANCTUARY", new Vector2(0f, -20f), new Vector2(520f, 110f), false, SceneNavigator.LoadSanctuary);
            CreateButton(panel, "COLLECTION", new Vector2(0f, -165f), new Vector2(520f, 110f), false, () => ShowOnly(_collectionPanel));
            CreateButton(panel, "SETTINGS", new Vector2(0f, -310f), new Vector2(520f, 110f), false, () => ShowOnly(_settingsPanel));
        }

        private void BuildWorldPanel(Transform panel)
        {
            CreateText(panel, "WORLD PROGRESS", 58, new Vector2(0f, 600f), FontStyle.Bold);
            CreateText(panel, "Opening World", 36, new Vector2(0f, 500f), FontStyle.Normal);
            CreateText(panel, "Levels unlock sequentially. The first implementation starts with Level 1: The First Push.", 26, new Vector2(0f, 390f), FontStyle.Normal, new Vector2(820f, 170f));

            CreateButton(panel, "LEVEL 1  •  THE FIRST PUSH", new Vector2(0f, 120f), new Vector2(760f, 130f), true, SceneNavigator.LoadGameplay);
            CreateButton(panel, "LEVEL 2  •  LOCKED", new Vector2(0f, -40f), new Vector2(760f, 110f), false, null, false);
            CreateButton(panel, "LEVEL 3  •  LOCKED", new Vector2(0f, -180f), new Vector2(760f, 110f), false, null, false);
            CreateButton(panel, "BACK", new Vector2(0f, -520f), new Vector2(380f, 100f), false, () => ShowOnly(_rootPanel));
        }

        private void BuildSettingsPanel(Transform panel)
        {
            CreateText(panel, "SETTINGS", 58, new Vector2(0f, 600f), FontStyle.Bold);
            CreateText(panel, "Settings UI is connected to navigation. Audio, haptics and quality controls will be bound to SettingsService next.", 28, new Vector2(0f, 250f), FontStyle.Normal, new Vector2(820f, 260f));
            CreateButton(panel, "BACK", new Vector2(0f, -520f), new Vector2(380f, 100f), false, () => ShowOnly(_rootPanel));
        }

        private void BuildCollectionPanel(Transform panel)
        {
            CreateText(panel, "COLLECTION", 58, new Vector2(0f, 600f), FontStyle.Bold);
            CreateText(panel, "Cosmetic collection placeholder. The PRD reserves this screen for visual-only unlocks such as ball skins, trails, particles and sanctuary decorations.", 28, new Vector2(0f, 250f), FontStyle.Normal, new Vector2(820f, 300f));
            CreateButton(panel, "BACK", new Vector2(0f, -520f), new Vector2(380f, 100f), false, () => ShowOnly(_rootPanel));
        }

        private void ShowOnly(GameObject activePanel)
        {
            _rootPanel.SetActive(activePanel == _rootPanel);
            _worldPanel.SetActive(activePanel == _worldPanel);
            _settingsPanel.SetActive(activePanel == _settingsPanel);
            _collectionPanel.SetActive(activePanel == _collectionPanel);
        }

        private void CreateButton(Transform parent, string label, Vector2 position, Vector2 size, bool primary, UnityEngine.Events.UnityAction onClick, bool interactable = true)
        {
            var buttonObject = new GameObject(label.Replace(" ", "") + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var image = buttonObject.GetComponent<Image>();
            image.color = primary ? new Color(0.95f, 0.58f, 0.14f, 1f) : new Color(0.13f, 0.18f, 0.21f, 0.96f);

            var button = buttonObject.GetComponent<Button>();
            button.interactable = interactable;
            if (onClick != null)
                button.onClick.AddListener(onClick);

            if (!interactable)
                image.color = new Color(0.1f, 0.12f, 0.13f, 0.65f);

            var textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);
            Stretch(textObject.GetComponent<RectTransform>());

            var text = textObject.GetComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = _runtimeFont;
            text.fontSize = primary ? 38 : 30;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
        }

        private void CreateText(Transform parent, string value, int fontSize, Vector2 position, FontStyle style, Vector2? size = null)
        {
            var textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);

            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size ?? new Vector2(900f, 120f);
            rect.anchoredPosition = position;

            var text = textObject.GetComponent<Text>();
            text.text = value;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = _runtimeFont;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
