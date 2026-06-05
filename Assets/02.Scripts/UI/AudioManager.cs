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

            // AudioListener 보장 (씬에 없으면 자동 추가)
            if (FindFirstObjectByType<AudioListener>() == null)
                gameObject.AddComponent<AudioListener>();

            _palette = Resources.Load<AudioPalette>("AudioPalette");
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
