using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Reward;
using TeamLog.Skill;

namespace TeamLog.Map
{
    /// <summary>
    /// 저장 데이터 DTO — JsonUtility 직렬화용
    /// </summary>
    [System.Serializable]
    public class RunSaveData
    {
        // 런 상태
        public int CurrentFloor;
        public int Gold;
        public int BonusAP;
        public int RerollTokens;
        public bool IsRunActive;
        public bool IsRunComplete;

        // 파티원
        public List<CharacterSaveData> Party = new();

        // 획득 유물 (에셋 경로로 저장)
        public List<string> AcquiredRelicPaths = new();
    }

    [System.Serializable]
    public class CharacterSaveData
    {
        public string DataPath;         // CharacterData 에셋 경로
        public int CurrentHP;
        public int MaxHP;
        public int BaseATK;             // 영구 증가 포함
        public int BaseDEF;
        public bool IsDead;
        public List<string> SkillPaths = new();           // 스킬 경로
        public List<SkillAugmentSaveData> SkillAugments = new(); // 스킬별 증augment 데이터
    }

    [System.Serializable]
    public class SkillAugmentSaveData
    {
        public string SkillPath;
        public List<string> AugmentPaths = new();
    }

    /// <summary>
    /// 저장/불러오기 관리자 — JsonUtility + 파일 I/O 기반
    /// </summary>
    public static class SaveManager
    {
        private static readonly string SavePath =
            Path.Combine(Application.persistentDataPath, "run_save.json");

        private static readonly string MetaPath =
            Path.Combine(Application.persistentDataPath, "meta_save.json");

        private static MetaSaveData _metaCache;

        public static bool HasSave => File.Exists(SavePath);

        /// <summary>
        /// 런 간 영구 통계 — 없으면 새로 생성
        /// </summary>
        public static MetaSaveData Meta
        {
            get
            {
                if (_metaCache == null)
                    _metaCache = LoadOrCreateMeta();
                return _metaCache;
            }
        }

        /// <summary>
        /// 메타 데이터 로드 또는 새 인스턴스 생성
        /// </summary>
        public static MetaSaveData LoadOrCreateMeta()
        {
            if (File.Exists(MetaPath))
            {
                string json = File.ReadAllText(MetaPath);
                var meta = JsonUtility.FromJson<MetaSaveData>(json);
                if (meta != null)
                {
                    // UnlockedCharacterIds가 null이면 초기화
                    if (meta.UnlockedCharacterIds == null)
                        meta.UnlockedCharacterIds = new List<string>();
                    return meta;
                }
            }
            return new MetaSaveData();
        }

        /// <summary>
        /// 메타 데이터 저장
        /// </summary>
        public static void SaveMeta()
        {
            _metaCache = Meta; // 캐시 보장
            string json = JsonUtility.ToJson(_metaCache, true);
            File.WriteAllText(MetaPath, json);
#if UNITY_EDITOR
            Debug.Log($"[SaveManager] 메타 저장 — 총 런: {_metaCache.TotalRuns}, 승리: {_metaCache.Victories}");
#endif
        }

        /// <summary>
        /// 현재 런 상태 저장
        /// </summary>
        public static void Save()
        {
            var state = GameRunState.Instance;
            if (state == null || !state.IsRunActive) return;

            var data = new RunSaveData
            {
                CurrentFloor = state.CurrentFloor,
                Gold = state.Gold,
                BonusAP = state.BonusAP,
                RerollTokens = state.RerollTokens,
                IsRunActive = state.IsRunActive,
                IsRunComplete = state.IsRunComplete
            };

            // 파티원 데이터
            foreach (var c in state.PlayerParty)
            {
                var charData = new CharacterSaveData
                {
                    DataPath = GetAssetPath(c.Data),
                    CurrentHP = c.Health.CurrentHP,
                    MaxHP = c.Health.MaxHP,
                    BaseATK = c.Stats.GetBaseStat(StatType.ATK),
                    BaseDEF = c.Stats.GetBaseStat(StatType.DEF),
                    IsDead = c.IsDead
                };

                // 스킬 + 증강 저장
                foreach (var skillInst in c.SkillInventory.SkillInstances)
                {
                    charData.SkillPaths.Add(GetAssetPath(skillInst.Data));

                    var augmentSave = new SkillAugmentSaveData
                    {
                        SkillPath = GetAssetPath(skillInst.Data)
                    };
                    foreach (var augment in skillInst.Augments)
                        augmentSave.AugmentPaths.Add(GetAssetPath(augment.Data));
                    charData.SkillAugments.Add(augmentSave);
                }

                data.Party.Add(charData);
            }

            // 유물 경로
            foreach (var relic in state.RelicHandler.Relics)
                data.AcquiredRelicPaths.Add(GetAssetPath(relic));

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
#if UNITY_EDITOR
            Debug.Log($"[SaveManager] 저장 완료 — 층 {data.CurrentFloor}, 골드 {data.Gold}");
#endif
        }

        /// <summary>
        /// 저장 데이터 로드 — 새 GameRunState 생성 후 반환
        /// </summary>
        public static GameRunState Load()
        {
            if (!HasSave) return null;

            string json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<RunSaveData>(json);
            if (data == null || !data.IsRunActive) return null;

            // 파티 재구성
            var party = new List<Character>();
            foreach (var csd in data.Party)
            {
                var charData = LoadAsset<CharacterData>(csd.DataPath);
                if (charData == null) continue;

                var character = new Character(charData);
                character.Health.Initialize(csd.MaxHP);

                // HP 복원
                if (csd.IsDead)
                {
                    character.Health.TakeDamage(csd.MaxHP);
                }
                else if (csd.CurrentHP < csd.MaxHP)
                {
                    character.Health.TakeDamage(csd.MaxHP - csd.CurrentHP);
                }

                // 영구 스탯 증가 복원
                int baseATK = charData.BaseATK;
                int baseDEF = charData.BaseDEF;
                if (csd.BaseATK > baseATK)
                    character.Stats.AddPermanentBase(StatType.ATK, csd.BaseATK - baseATK);
                if (csd.BaseDEF > baseDEF)
                    character.Stats.AddPermanentBase(StatType.DEF, csd.BaseDEF - baseDEF);

                // 증augment 복원
                foreach (var augSave in csd.SkillAugments)
                {
                    var skillInst = character.SkillInventory.FindInstance(
                        LoadAsset<SkillData>(augSave.SkillPath));
                    if (skillInst == null) continue;
                    foreach (var augPath in augSave.AugmentPaths)
                    {
                        var augData = LoadAsset<AugmentData>(augPath);
                        if (augData != null)
                            skillInst.AddAugment(augData);
                    }
                }

                party.Add(character);
            }

            // GameRunState 복원
            var state = GameRunState.Create(party, data.Gold);
            if (data.BonusAP > 0)
                state.RestoreBonusAP(data.BonusAP);
            state.RestoreRerollTokens(data.RerollTokens);

            // 유물 복원
            foreach (var relicPath in data.AcquiredRelicPaths)
            {
                var relic = LoadAsset<RelicData>(relicPath);
                if (relic != null)
                    state.AcquireRelic(relic);
            }

#if UNITY_EDITOR
            Debug.Log($"[SaveManager] 로드 완료 — 층 {data.CurrentFloor}, 파티 {party.Count}명, 유물 {data.AcquiredRelicPaths.Count}개");
#endif
            return state;
        }

        /// <summary>
        /// 저장 데이터 삭제
        /// </summary>
        public static void DeleteSave()
        {
            if (HasSave)
                File.Delete(SavePath);
            _metaCache = null; // 캐시 무효화
        }

        /// <summary>
        /// 런 종료 시 메타 통계 갱신 + 캐릭터 잠금해제 체크 + 런 저장 삭제
        /// </summary>
        public static void RecordRunEnd(bool victory, int floor, int gold)
        {
            var meta = Meta;
            meta.TotalRuns++;
            meta.TotalGoldEarned += gold;
            if (victory) meta.Victories++;
            if (floor > meta.BestFloor) meta.BestFloor = floor;
            meta.HasPendingRun = false;

            // 캐릭터 잠금해제 조건 체크
            CheckCharacterUnlocks(meta, victory, floor);

            SaveMeta();

            // 런 종료 시 세이브 파일 삭제
            DeleteSave();
        }

        /// <summary>
        /// 캐릭터 잠금해제 조건 체크
        /// </summary>
        private static void CheckCharacterUnlocks(MetaSaveData meta, bool victory, int floor)
        {
            // 잠금해제 조건:
            // 1층 보스(고블린왕) 클리어 → 궁수 잠금해제
            // 2층 보스(드래곤) 클리어 → 네크로맨서 잠금해제
            // 3층 보스(마왕) 클리어 → 연금술사 잠금해제
            // 총 승리 3회 달성 → 음유시인 잠금해제

            if (meta.UnlockedCharacterIds == null)
                meta.UnlockedCharacterIds = new List<string>();

            if (floor >= 1 && !meta.UnlockedCharacterIds.Contains("궁수"))
                meta.UnlockedCharacterIds.Add("궁수");

            if (floor >= 2 && !meta.UnlockedCharacterIds.Contains("네크로맨서"))
                meta.UnlockedCharacterIds.Add("네크로맨서");

            if (floor >= 3 && !meta.UnlockedCharacterIds.Contains("연금술사"))
                meta.UnlockedCharacterIds.Add("연금술사");

            if (meta.Victories >= 3 && !meta.UnlockedCharacterIds.Contains("음유시인"))
                meta.UnlockedCharacterIds.Add("음유시인");
        }

        // ── 유틸리티 ──

        private static string GetAssetPath(Object asset)
        {
            if (asset == null) return "";
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetAssetPath(asset);
#else
            return asset.name;
#endif
        }

        private static T LoadAsset<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path)) return null;
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
#else
            return Resources.Load<T>(path);
#endif
        }
    }
}
