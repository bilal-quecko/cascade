#if UNITY_EDITOR
using System.Linq;
using UnityEditor;

namespace Cascade.Core.Editor
{
    [InitializeOnLoad]
    public static class BuildSettingsInitializer
    {
        private static readonly string[] RequiredScenes =
        {
            "Assets/_Cascade/Scenes/SCN_Boot.unity",
            "Assets/_Cascade/Scenes/SCN_MainMenu.unity",
            "Assets/_Cascade/Scenes/SCN_Gameplay.unity",
            "Assets/_Cascade/Scenes/SCN_Sanctuary.unity"
        };

        static BuildSettingsInitializer()
        {
            EditorApplication.delayCall += EnsureScenes;
        }

        private static void EnsureScenes()
        {
            var current = EditorBuildSettings.scenes.ToList();
            var changed = false;

            foreach (var path in RequiredScenes)
            {
                if (current.Any(scene => scene.path == path))
                    continue;

                current.Add(new EditorBuildSettingsScene(path, true));
                changed = true;
            }

            if (changed)
                EditorBuildSettings.scenes = current.ToArray();
        }
    }
}
#endif
