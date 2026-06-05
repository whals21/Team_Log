using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Reward;

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
        public bool IsRunActive;
        public bool IsRunComplete;

        // 파티원
        public List<CharacterSaveData> Party = new();

        // 획득 아이템 (에셋 경로로 저장)
        public List<string> AcquiredItemPaths = new();

        // 획득 스킬 (에셋 경로)
        public List<string> AcquiredSkillPaths = new();
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
        public List<string> SkillPaths = new(); // 추가 스킬 경로
    }

    /// <summary>
    /// 저장/불러오기 관리자 — JsonUtility + 파일 I/O 기반
    /// </summary>
    public static class SaveManager
    {
        private static readonly string SavePath =
            Path.Combine(Application.persistentDataPath, "run_save.json");

        public static bool HasSave => File.Exists(SavePath);

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

                foreach (var skill in c.SkillInventory.Skills)
                    charData.SkillPaths.Add(GetAssetPath(skill));

                data.Party.Add(charData);
            }

            // 아이템 경로
            foreach (var item in state.AcquiredItems)
                data.AcquiredItemPaths.Add(GetAssetPath(item));

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[SaveManager] 저장 완료 — 층 {data.CurrentFloor}, 골드 {data.Gold}");
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

                // HP 복원: 현재 HP까지 데미지를 준 후 사망 처리
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

                // 추가 스킬 복원
                foreach (var skillPath in csd.SkillPaths)
                {
                    var skill = LoadAsset<SkillData>(skillPath);
                    if (skill != null)
                        character.SkillInventory.AddSkill(skill);
                }

                party.Add(character);
            }

            // GameRunState 복원
            var state = GameRunState.Create(party, data.Gold);
            if (data.BonusAP > 0)
                state.RestoreBonusAP(data.BonusAP);

            Debug.Log($"[SaveManager] 로드 완료 — 층 {data.CurrentFloor}, 파티 {party.Count}명");
            return state;
        }

        /// <summary>
        /// 저장 데이터 삭제
        /// </summary>
        public static void DeleteSave()
        {
            if (HasSave)
                File.Delete(SavePath);
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
