#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TeamLog.UI;

namespace TeamLog.Editor
{
    /// <summary>
    /// DataGenerator — 팔레트 데이터 생성 (UIPalette + AudioPalette + VFXPalette)
    /// 진입점/스킬/캐릭터/유틸리티: DataGenerator.cs
    /// 증강 데이터/스폰 패턴: DataGenerator.Augments.cs
    /// 이벤트 데이터: DataGenerator.Events.cs
    /// 유물 데이터: DataGenerator.Relics.cs
    /// </summary>
    public static partial class DataGenerator
    {
        #region UI Palette

        private static void GenerateUIPalette()
        {
            const string path = "Assets/03.Data/UIPalette.asset";
            var palette = GetOrCreateAsset<UIPalette>(path);
            palette.name = "UIPalette";

            // ── 배경 (기존) ──
            palette.BgDark        = Hex("#141428");
            palette.BgPanel       = WithA(Hex("#0a0a1a"), 0.8f);
            palette.BgPanelLight  = WithA(Hex("#0f0f24"), 0.95f);
            palette.BgTopBar      = WithA(Hex("#080814"), 0.95f);

            // ── 다크 판타지 고딕 (Party Select 신규) ──
            // 깊이 5단계
            palette.DFVoid    = Hex("#050509");
            palette.DFAbyss   = Hex("#0a0a14");
            palette.DFDepth   = Hex("#11111f");
            palette.DFSlate   = Hex("#1a1a2e");
            palette.DFSlate2  = Hex("#232347");

            // 골드 4단계
            palette.DFGoldL   = Hex("#f4d35e");
            palette.DFGold    = Hex("#d4af37");
            palette.DFGoldD   = Hex("#8b6914");
            palette.DFGoldX   = Hex("#4a3a0d");

            // 핏빛 3단계
            palette.DFBloodDeep = Hex("#5a0000");
            palette.DFBlood     = Hex("#8b0000");
            palette.DFBloodL    = Hex("#c0392b");

            // 양피지 4단계
            palette.DFParchment  = Hex("#c9b485");
            palette.DFParchmentD = Hex("#8a7752");
            palette.DFParchmentDd= Hex("#4d3f28");
            palette.DFParchmentX = Hex("#2a2418");

            // 잉크 3단계
            palette.DFInk      = Hex("#f0e6d0");
            palette.DFInkDim   = Hex("#a89878");
            palette.DFInkFaint = Hex("#6b5e44");

            // ── 강조/텍스트/HP/쉴드 (기존) ──
            palette.AccentRed    = Hex("#c41f3b");
            palette.AccentGreen  = Hex("#26ae61");
            palette.AccentYellow = Hex("#f4d35e");
            palette.TextWhite    = Hex("#f0e6d0");
            palette.TextDim      = Hex("#a89878");

            palette.HPNormal       = Hex("#26ae61");
            palette.HPLow           = Hex("#ff8000");
            palette.HPLowThreshold  = Hex("#4d4d4d");
            palette.HPEnemy         = Hex("#c41f3b");
            palette.ShieldBrown     = Hex("#b8732e");
            palette.DamageColor     = Hex("#d93333");
            palette.HealColor       = Hex("#26ae61");
            palette.APNormal        = Hex("#f4d35e");
            palette.APShortage      = Hex("#d93333");

            // ── 스킬 타입 (신규 2종 포함) ──
            palette.SkillAttack  = Hex("#c41f3b");
            palette.SkillHeal    = Hex("#26ae61");
            palette.SkillBuff    = Hex("#f4d35e");
            palette.SkillDebuff  = Hex("#9933cc");
            palette.SkillShield  = Hex("#b8732e");
            palette.SkillPurify  = Hex("#66ccf2");
            palette.SkillSummon  = Hex("#7da34a");
            palette.SkillSpecial = Hex("#b388ff");

            palette.BorderRed = WithA(Hex("#992a30"), 0.8f);

            // ── 자원 11종 (Party Select 기준 — Sibyl Prophecy를 청록으로 변경) ──
            palette.ResourceEmber     = Hex("#ff6b35");
            palette.ResourceVengeance = Hex("#a8324a");
            palette.ResourceFrost     = Hex("#5ec5e8");
            palette.ResourceProphecy  = Hex("#6ed5b2"); // 청록 (시간/예언 컨셉)
            palette.ResourceCharge    = Hex("#f7d046");
            palette.ResourceShadows   = Hex("#9b6ec2");
            palette.ResourceCombo     = Hex("#d4a017");
            palette.ResourceCorpse    = Hex("#7da34a");
            palette.ResourceDiscover  = Hex("#b388ff");
            palette.ResourceMelody    = Hex("#ff8fab");
            palette.ResourceMercy     = Hex("#ffe082");
            palette.ResourceDefault   = Hex("#999999");

            // ── 상태이상/특성/등급 등은 기존 default 유지 (변경 X) ──
            // 필요 시 이곳에 추가 명시

            EditorUtility.SetDirty(palette);
            Debug.Log("[DataGenerator] UIPalette asset generated with Dark Fantasy Gothic tokens (Party Select).");
        }

        // 헥스 컬러 파싱 헬퍼
        private static Color Hex(string hex)
        {
            hex = hex.Replace("#", "");
            byte r = System.Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = System.Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = System.Convert.ToByte(hex.Substring(4, 2), 16);
            return new Color32(r, g, b, 255);
        }

        private static Color WithA(Color c, float a)
        {
            c.a = a;
            return c;
        }

        #endregion

        #region Audio Palette

        private static void GenerateAudioPalette()
        {
            EnsureFolder("Assets/Resources");
            const string path = "Assets/Resources/AudioPalette.asset";
            var palette = GetOrCreateAsset<AudioPalette>(path);
            palette.name = "AudioPalette";

            // 28개 사운드 매핑: (키, Assets/ 이하 상대경로)
            var mappings = new (string key, string assetPath)[]
            {
                // 전투 SFX — 기본 (13)
                ("AttackHit",          "Assets/Epic Toon FX/Sound/etfx_target_hit.wav"),
                ("Heal",               "Assets/CombatMagicSpellsVIISFX/Heal1/SFX_Spell01Heal.wav"),
                ("ShieldApply",        "Assets/Fantasy UI SFX - Lite Edition/Armor 1-1.wav"),
                ("StatusEffectApply",  "Assets/CombatMagicSpellsVIISFX/Dark1/SFX_Spell01Dark.wav"),
                ("Purify",             "Assets/CombatMagicSpellsVIISFX/Ice1/SFX_Spell01Ice.wav"),
                ("Miss",               "Assets/Epic Toon FX/Sound/etfx_explosion_poof.wav"),
                ("CharacterDeath",     "Assets/Epic Toon FX/Sound/etfx_explosion_dark01.wav"),
                ("SkillDraw",          "Assets/Fantasy UI SFX - Lite Edition/Card Draw 1-1.wav"),
                ("SkillReroll",        "Assets/Fantasy UI SFX - Lite Edition/Card Place 1-1.wav"),
                ("TurnStart",          "Assets/Fantasy UI SFX - Lite Edition/Interface 1-1.wav"),
                ("BuffApply",          "Assets/CombatMagicSpellsVIISFX/Thunder1/SFX_Spell01Cast01.wav"),
                ("DebuffApply",        "Assets/CombatMagicSpellsVIISFX/Water1/SFX_Spell01Water.wav"),
                ("EnemyAttack",        "Assets/CombatMagicSpellsVIISFX/Fire1/SFX_Spell01Fire.wav"),
                // 전투 SFX — 스킬 타입별 (14)
                ("FireImpact",         "Assets/CombatMagicSpellsVIISFX/Fire5/SFX_Spell02Fire.wav"),
                ("IceImpact",          "Assets/CombatMagicSpellsVIISFX/Ice5/SFX_Spell01Ice.wav"),
                ("ThunderImpact",      "Assets/CombatMagicSpellsVIISFX/Thunder7/SFX_Spell01Thunder01.wav"),
                ("DarkImpact",         "Assets/CombatMagicSpellsVIISFX/Dark5/SFX_Spell01Dark.wav"),
                ("PoisonImpact",       "Assets/CombatMagicSpellsVIISFX/Earth5/SFX_Spell01Earth.wav"),
                ("BurnImpact",         "Assets/CombatMagicSpellsVIISFX/Fire3/SFX_Spell02Fire.wav"),
                ("FreezeImpact",       "Assets/CombatMagicSpellsVIISFX/Ice3/SFX_Spell01Ice.wav"),
                ("HealImpact",         "Assets/CombatMagicSpellsVIISFX/Heal5/SFX_Spell01Heal.wav"),
                ("BuffCast",           "Assets/CombatMagicSpellsVIISFX/Heal2/SFX_Spell01Heal.wav"),
                ("DebuffCast",         "Assets/CombatMagicSpellsVIISFX/Dark3/SFX_Spell01Dark.wav"),
                ("PurifyCast",         "Assets/CombatMagicSpellsVIISFX/Heal7/SFX_Spell01Swoosh01Heal.wav"),
                ("ShieldCast",         "Assets/CombatMagicSpellsVIISFX/Water3/SFX_Spell01Water.wav"),
                ("CriticalHit",        "Assets/CombatMagicSpellsVIISFX/Thunder9/SFX_Spell01Thunder.wav"),
                ("EnemySkillHit",      "Assets/CombatMagicSpellsVIISFX/Dark4/SFX_Spell01Dark.wav"),
                // 전투 결과 (2)
                ("Victory",            "Assets/Fantasy UI SFX - Lite Edition/Magical Texture Chimes 1-1.wav"),
                ("Defeat",             "Assets/Epic Toon FX/Sound/etfx_explosion_dark02.wav"),
                // UI SFX (13)
                ("UIClick",            "Assets/Fantasy UI SFX - Lite Edition/Interface 2-1.wav"),
                ("UIShopPurchase",     "Assets/UI SFX Mega Pack/Assets/Audio/Purchase/coins_1.wav"),
                ("UIShopOpen",         "Assets/Fantasy UI SFX - Lite Edition/Bag Handle 1-1.wav"),
                ("UIGoldEarn",         "Assets/Fantasy UI SFX - Lite Edition/Coins 1-5.wav"),
                ("UIGoldSpend",        "Assets/Fantasy UI SFX - Lite Edition/Coin Bag 1-1.wav"),
                ("UIWarning",          "Assets/UI SFX Mega Pack/Assets/Audio/Warning_Popup/warning_1.wav"),
                ("UICancel",           "Assets/UI SFX Mega Pack/Assets/Audio/Cancel/cancel_1.wav"),
                ("UIConfirm",          "Assets/UI SFX Mega Pack/Assets/Audio/Ok/ok_1.wav"),
                ("UITransition",       "Assets/Fantasy UI SFX - Lite Edition/Magical Interface 1-1.wav"),
                ("UIToast",            "Assets/Fantasy UI SFX - Lite Edition/Special Interface 1-1.wav"),
                ("UINodeClick",        "Assets/Fantasy UI SFX - Lite Edition/Building interface 1-1.wav"),
                ("UIReroll",           "Assets/Fantasy UI SFX - Lite Edition/Card Draw 2-1.wav"),
                ("UIPotion",           "Assets/Fantasy UI SFX - Lite Edition/Potion Item 1-1.wav"),
            };

            // 기존 엔트리 클리어 후 재구성
            palette.entries.Clear();

            int loaded = 0;
            foreach (var (key, assetPath) in mappings)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                if (clip == null)
                    Debug.LogWarning($"[DataGenerator] AudioPalette: 클립을 찾을 수 없음 — {key}: {assetPath}");
                else
                    loaded++;

                palette.entries.Add(new AudioPalette.AudioEntry { name = key, clip = clip });
            }

            EditorUtility.SetDirty(palette);
            Debug.Log($"[DataGenerator] AudioPalette 생성 완료 — {loaded}/{mappings.Length} 클립 로드됨.");
        }

        #endregion

        #region VFX Palette

        private static void GenerateVFXPalette()
        {
            EnsureFolder("Assets/Resources");
            const string path = "Assets/Resources/VFXPalette.asset";
            var palette = GetOrCreateAsset<VFXPalette>(path);
            palette.name = "VFXPalette";

            var mappings = new (string key, string assetPath)[]
            {
                // 전투 이펙트
                ("Hit",       "Assets/Epic Toon FX/Prefabs/Combat/Sword/Hit/SwordHit/SwordHitRed.prefab"),
                ("Heal",      "Assets/Epic Toon FX/Prefabs/Interactive/Healing/HealOnce.prefab"),
                ("Shield",    "Assets/Epic Toon FX/Prefabs/Combat/Magic/Shield/MagicShieldBlue.prefab"),
                ("Death",     "Assets/Epic Toon FX/Prefabs/Combat/Death/Skulls/GenericDeath.prefab"),
                ("Buff",      "Assets/Epic Toon FX/Prefabs/Combat/Magic/Buff/MagicBuffBlue.prefab"),
                ("Debuff",    "Assets/Epic Toon FX/Prefabs/Combat/Magic/Enchant/MagicEnchantYellow.prefab"),
                ("Burn",      "Assets/Epic Toon FX/Prefabs/Combat/Explosions/FireballSoftExplosion/ExplosionFireballSoftFire.prefab"),
                ("Poison",    "Assets/Epic Toon FX/Prefabs/Combat/Explosions (Misc)/PoisonExplosion.prefab"),
                ("Freeze",    "Assets/Epic Toon FX/Prefabs/Combat/Explosions/FrostExplosion/FrostExplosion.prefab"),
                ("Critical",  "Assets/Epic Toon FX/Prefabs/Combat/Sword/Hit/SwordHitCritical/SwordHitRedCritical.prefab"),
                ("Slash",     "Assets/Epic Toon FX/Prefabs/Combat/Sword/Slash/SwordSlashThick/SwordSlashThickWhite.prefab"),
                ("Stun",      "Assets/Epic Toon FX/Prefabs/Combat/Explosions (Misc)/StunStarExplosion.prefab"),
                ("Purify",    "Assets/Epic Toon FX/Prefabs/Interactive/Healing/HealNova.prefab"),
                // 전투 결과
                ("Victory",   "Assets/Epic Toon FX/Prefabs/Combat/Explosions (Text)/Critical.prefab"),
                ("Defeat",    "Assets/Epic Toon FX/Prefabs/Combat/Death/Skulls/EvilDeath.prefab"),
            };

            palette.entries.Clear();

            int loaded = 0;
            foreach (var (key, assetPath) in mappings)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null)
                    Debug.LogWarning($"[DataGenerator] VFXPalette: 프리팹을 찾을 수 없음 — {key}: {assetPath}");
                else
                    loaded++;

                palette.entries.Add(new VFXPalette.VFXEntry { name = key, prefab = prefab });
            }

            EditorUtility.SetDirty(palette);
            Debug.Log($"[DataGenerator] VFXPalette 생성 완료 — {loaded}/{mappings.Length} 프리팹 로드됨.");
        }

        #endregion
    }
}
#endif
