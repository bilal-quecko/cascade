using System.Collections.Generic;
using UnityEngine;

namespace Cascade.Levels
{
    [CreateAssetMenu(menuName = "Cascade/Levels/Level Catalog", fileName = "SO_LevelCatalog")]
    public sealed class LevelCatalogSO : ScriptableObject
    {
        public List<LevelDefinitionSO> levels = new();

        public LevelDefinitionSO Find(string levelId)
        {
            return levels.Find(level => level != null && level.levelId == levelId);
        }
    }
}
