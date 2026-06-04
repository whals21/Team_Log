using UnityEngine;

namespace TeamLog.UI
{
    /// <summary>
    /// 사운드 재생 싱글톤 — SFX PlayOneShot 편의 메서드 제공
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        private AudioSource _sfxSource;
        private AudioPalette _palette;

        public static AudioManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindFirstObjectByType<AudioManager>();
                if (_instance == null)
                {
                    var go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.spatialBlend = 0f; // 2D

            // AudioPalette 로드 — Resources 또는 직접 경로
            _palette = Resources.Load<AudioPalette>("AudioPalette");
#if UNITY_EDITOR
            if (_palette == null)
                _palette = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioPalette>("Assets/03.Data/AudioPalette.asset");
#endif
        }

        public void PlaySFX(string clipName)
        {
            if (_palette == null || _sfxSource == null) return;
            var clip = _palette.GetClip(clipName);
            if (clip != null)
                _sfxSource.PlayOneShot(clip);
        }

        public void PlayAttackHit() => PlaySFX("AttackHit");
        public void PlayHeal() => PlaySFX("Heal");
        public void PlayUIClick() => PlaySFX("UIClick");
        public void PlayVictory() => PlaySFX("Victory");
        public void PlayDefeat() => PlaySFX("Defeat");
    }
}
