using UnityEngine;
using System.Collections.Generic;

namespace TeamLog.UI
{
    /// <summary>
    /// 오디오 클립 매핑 ScriptableObject — 효과음 이름→클립
    /// </summary>
    [CreateAssetMenu(fileName = "AudioPalette", menuName = "TeamLog/AudioPalette")]
    public class AudioPalette : ScriptableObject
    {
        [System.Serializable]
        public class AudioEntry
        {
            public string name;
            public AudioClip clip;
        }

        public List<AudioEntry> entries = new List<AudioEntry>();

        private Dictionary<string, AudioClip> _cache;

        public AudioClip GetClip(string name)
        {
            if (_cache == null)
            {
                _cache = new Dictionary<string, AudioClip>();
                foreach (var entry in entries)
                    if (entry.clip != null && !string.IsNullOrEmpty(entry.name))
                        _cache[entry.name] = entry.clip;
            }
            return _cache.TryGetValue(name, out var clip) ? clip : null;
        }
    }
}
