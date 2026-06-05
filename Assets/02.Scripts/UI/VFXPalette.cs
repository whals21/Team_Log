using UnityEngine;
using System.Collections.Generic;

namespace TeamLog.UI
{
    /// <summary>
    /// VFX 프리팹 매핑 ScriptableObject — 효과 이름→프리팹
    /// </summary>
    [CreateAssetMenu(fileName = "VFXPalette", menuName = "TeamLog/VFXPalette")]
    public class VFXPalette : ScriptableObject
    {
        [System.Serializable]
        public class VFXEntry
        {
            public string name;
            public GameObject prefab;
        }

        public List<VFXEntry> entries = new List<VFXEntry>();

        private Dictionary<string, GameObject> _cache;

        public GameObject GetPrefab(string name)
        {
            if (_cache == null)
            {
                _cache = new Dictionary<string, GameObject>();
                foreach (var entry in entries)
                    if (entry.prefab != null && !string.IsNullOrEmpty(entry.name))
                        _cache[entry.name] = entry.prefab;
            }
            return _cache.TryGetValue(name, out var prefab) ? prefab : null;
        }
    }
}
