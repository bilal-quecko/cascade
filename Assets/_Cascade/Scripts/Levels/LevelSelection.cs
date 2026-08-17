namespace Cascade.Levels
{
    /// <summary>
    /// Lightweight handoff between level-selection UI and the shared Gameplay scene.
    /// Runtime level content is still loaded from LevelDefinitionSO/Addressables.
    /// </summary>
    public static class LevelSelection
    {
        public static string SelectedLevelId { get; private set; } = "L01";

        public static void Select(string levelId)
        {
            if (!string.IsNullOrWhiteSpace(levelId))
                SelectedLevelId = levelId;
        }
    }
}
