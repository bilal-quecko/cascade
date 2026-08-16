using Cascade.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Cascade.UI
{
    /// <summary>
    /// Behaviour for the editable Main Menu prefab.
    /// Layout and visuals live in PF_MainMenu; this class only owns interaction and navigation.
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject worldMapPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject collectionPanel;

        [Header("Main Menu")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button sanctuaryButton;
        [SerializeField] private Button collectionButton;
        [SerializeField] private Button settingsButton;

        [Header("World Map")]
        [SerializeField] private Button levelOneButton;
        [SerializeField] private Button worldBackButton;

        [Header("Secondary Panels")]
        [SerializeField] private Button settingsBackButton;
        [SerializeField] private Button collectionBackButton;

        private void Awake()
        {
            BindButtons();
            ShowMainMenu();
        }

        private void BindButtons()
        {
            Bind(playButton, ShowWorldMap);
            Bind(sanctuaryButton, SceneNavigator.LoadSanctuary);
            Bind(collectionButton, ShowCollection);
            Bind(settingsButton, ShowSettings);
            Bind(levelOneButton, SceneNavigator.LoadGameplay);
            Bind(worldBackButton, ShowMainMenu);
            Bind(settingsBackButton, ShowMainMenu);
            Bind(collectionBackButton, ShowMainMenu);
        }

        private static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null || action == null)
                return;

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        public void ShowMainMenu() => ShowOnly(mainPanel);
        public void ShowWorldMap() => ShowOnly(worldMapPanel);
        public void ShowSettings() => ShowOnly(settingsPanel);
        public void ShowCollection() => ShowOnly(collectionPanel);

        private void ShowOnly(GameObject activePanel)
        {
            if (mainPanel != null) mainPanel.SetActive(activePanel == mainPanel);
            if (worldMapPanel != null) worldMapPanel.SetActive(activePanel == worldMapPanel);
            if (settingsPanel != null) settingsPanel.SetActive(activePanel == settingsPanel);
            if (collectionPanel != null) collectionPanel.SetActive(activePanel == collectionPanel);
        }
    }
}
