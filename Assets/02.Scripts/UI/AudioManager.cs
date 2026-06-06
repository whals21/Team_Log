using UnityEngine;
using UnityEngine.SceneManagement;

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

        [SerializeField, Range(0f, 1f)]
        private float _masterVolume = 0.75f;

        public float MasterVolume
        {
            get => _masterVolume;
            set => _masterVolume = Mathf.Clamp01(value);
        }

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

            // AudioListener 보장 — 기존 씬 카메라의 리스너 제거 후 1개만 유지
            var existingListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            if (existingListeners.Length > 0)
            {
                // 기존 리스너가 있으면 AudioManager에 없으면 그것을 사용, 중복 제거
                bool hasOwn = false;
                foreach (var listener in existingListeners)
                {
                    if (listener.gameObject == gameObject)
                        hasOwn = true;
                    else
                        Destroy(listener);
                }
                if (!hasOwn)
                    gameObject.AddComponent<AudioListener>();
            }
            else
            {
                gameObject.AddComponent<AudioListener>();
            }

            _palette = Resources.Load<AudioPalette>("AudioPalette");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RemoveDuplicateListeners();
        }

        private void RemoveDuplicateListeners()
        {
            var listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            bool ownFound = false;
            foreach (var listener in listeners)
            {
                if (listener.gameObject == gameObject)
                    ownFound = true;
                else
                    Destroy(listener);
            }
            if (!ownFound && listeners.Length == 0)
                gameObject.AddComponent<AudioListener>();
        }

        public void PlaySFX(string clipName, float volumeScale = 1f)
        {
            if (_palette == null || _sfxSource == null) return;
            var clip = _palette.GetClip(clipName);
            if (clip != null)
                _sfxSource.PlayOneShot(clip, _masterVolume * volumeScale);
        }

        // 전투 SFX — 기본
        public void PlayAttackHit() => PlaySFX("AttackHit");
        public void PlayHeal() => PlaySFX("Heal");
        public void PlayShieldApply() => PlaySFX("ShieldApply");
        public void PlayStatusEffectApply() => PlaySFX("StatusEffectApply");
        public void PlayPurify() => PlaySFX("Purify");
        public void PlayMiss() => PlaySFX("Miss");
        public void PlayCharacterDeath() => PlaySFX("CharacterDeath");
        public void PlaySkillDraw() => PlaySFX("SkillDraw");
        public void PlaySkillReroll() => PlaySFX("SkillReroll");
        public void PlayTurnStart() => PlaySFX("TurnStart");
        public void PlayBuffApply() => PlaySFX("BuffApply");
        public void PlayDebuffApply() => PlaySFX("DebuffApply");
        public void PlayEnemyAttack() => PlaySFX("EnemyAttack");
        public void PlayVictory() => PlaySFX("Victory");
        public void PlayDefeat() => PlaySFX("Defeat");

        // 전투 SFX — 스킬 타입별
        public void PlayFireImpact() => PlaySFX("FireImpact");
        public void PlayIceImpact() => PlaySFX("IceImpact");
        public void PlayThunderImpact() => PlaySFX("ThunderImpact");
        public void PlayDarkImpact() => PlaySFX("DarkImpact");
        public void PlayPoisonImpact() => PlaySFX("PoisonImpact");
        public void PlayBurnImpact() => PlaySFX("BurnImpact");
        public void PlayFreezeImpact() => PlaySFX("FreezeImpact");
        public void PlayHealImpact() => PlaySFX("HealImpact");
        public void PlayBuffCast() => PlaySFX("BuffCast");
        public void PlayDebuffCast() => PlaySFX("DebuffCast");
        public void PlayPurifyCast() => PlaySFX("PurifyCast");
        public void PlayShieldCast() => PlaySFX("ShieldCast");
        public void PlayCriticalHit() => PlaySFX("CriticalHit");
        public void PlayEnemySkillHit() => PlaySFX("EnemySkillHit");

        // UI SFX
        public void PlayUIClick() => PlaySFX("UIClick");
        public void PlayUIShopPurchase() => PlaySFX("UIShopPurchase");
        public void PlayUIShopOpen() => PlaySFX("UIShopOpen");
        public void PlayUIGoldEarn() => PlaySFX("UIGoldEarn");
        public void PlayUIGoldSpend() => PlaySFX("UIGoldSpend");
        public void PlayUIWarning() => PlaySFX("UIWarning");
        public void PlayUICancel() => PlaySFX("UICancel");
        public void PlayUIConfirm() => PlaySFX("UIConfirm");
        public void PlayUITransition() => PlaySFX("UITransition");
        public void PlayUIToast() => PlaySFX("UIToast");
        public void PlayUINodeClick() => PlaySFX("UINodeClick");
    }
}
