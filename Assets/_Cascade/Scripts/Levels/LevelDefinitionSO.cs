using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Cascade.Levels
{
    [Flags]
    public enum PuzzleCategory
    {
        None = 0,
        Place = 1 << 0,
        Aim = 1 << 1,
        Choose = 1 << 2,
        Trigger = 1 << 3,
        Connect = 1 << 4,
        MultiConnect = 1 << 5
    }

    [CreateAssetMenu(menuName = "Cascade/Levels/Level Definition", fileName = "SO_Level_")]
    public sealed class LevelDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        public string levelId;
        public string displayName;
        public int sequenceIndex;
        public string worldId = "opening";

        [Header("Content")]
        public AssetReferenceGameObject levelPrefab;
        public PuzzleCategory puzzleCategories;
        [TextArea] public string primaryObjective;
        [TextArea] public string teaching;
        [TextArea] public string toolSummary;
        [TextArea] public string targetCascade;
        public int maxToolsUsable = 1;

        [Header("Runtime")]
        public string nextLevelId;
        public bool productionReady;
    }
}
