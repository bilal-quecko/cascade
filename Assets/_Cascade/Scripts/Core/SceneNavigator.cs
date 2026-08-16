using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cascade.Core
{
    /// <summary>
    /// Central scene navigation entry point for Cascade.
    /// Keeps scene names in one place so UI code does not hardcode them repeatedly.
    /// </summary>
    public static class SceneNavigator
    {
        public const string BootScene = "SCN_Boot";
        public const string MainMenuScene = "SCN_MainMenu";
        public const string GameplayScene = "SCN_Gameplay";
        public const string SanctuaryScene = "SCN_Sanctuary";

        public static void LoadMainMenu() => Load(MainMenuScene);
        public static void LoadGameplay() => Load(GameplayScene);
        public static void LoadSanctuary() => Load(SanctuaryScene);

        public static void Load(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("[SceneNavigator] Cannot load an empty scene name.");
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"[SceneNavigator] Scene '{sceneName}' is not available in Build Settings.");
                return;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
