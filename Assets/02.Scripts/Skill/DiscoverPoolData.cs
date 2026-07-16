using System;
using System.Collections.Generic;
using UnityEngine;
using SkillData = TeamLog.Characters.SkillData;

namespace TeamLog.Skill
{
    /// <summary>
    /// 발견(Discover) 풀 카테고리 — Cael(Alchemist) 4영역.
    /// 특성(DiscoverWeightBonus)이 카테고리별 가중치 배수 적용에 사용.
    /// </summary>
    public enum DiscoverCategory
    {
        None,
        Mending,        // 회복 물약
        Strengthening,  // 버프 물약
        Crippling,      // 디버프 물약
        Catalytic,      // 유틸리티 물약
    }

    /// <summary>
    /// 발견 풀의 개별 항목 — SkillData 직접 참조 + 가중치.
    /// 하스스톤 발견 메커니즘: 풀에서 N개를 가중치 기반 무작위 추출.
    /// </summary>
    [Serializable]
    public struct DiscoverEntry
    {
        [Tooltip("발견 시 실행될 스킬 (SkillData 직접 참조 — Pipeline.ExecuteSkill에 그대로 전달)")]
        public SkillData Skill;
        [Tooltip("추출 가중치 (높을수록 자주 등장). 기본 1.")]
        [Range(1, 100)]
        public int Weight;

        public DiscoverEntry(SkillData skill, int weight = 1)
        {
            Skill = skill;
            Weight = weight;
        }
    }

    /// <summary>
    /// 발견 풀 데이터 (ScriptableObject) — 각 발견 스킬마다 1개씩.
    /// Cael(Alchemist)의 4개 발견 스킬(Mending/Strengthening/Crippling/Catalytic Brew) 각각 1개 풀 보유.
    /// </summary>
    [CreateAssetMenu(fileName = "DiscoverPool", menuName = "TeamLog/Discover Pool")]
    public class DiscoverPoolData : ScriptableObject
    {
        [Header("풀 정보")]
        [SerializeField] private string _poolName;
        [SerializeField] private DiscoverCategory _category;
        [TextArea(1, 3)]
        [SerializeField] private string _description;

        [Header("발견 항목 (가중치 기반 추출)")]
        [SerializeField] private DiscoverEntry[] _entries = Array.Empty<DiscoverEntry>();

        public string PoolName => _poolName;
        public DiscoverCategory Category => _category;
        public string Description => _description;
        public IReadOnlyList<DiscoverEntry> Entries => _entries ?? Array.Empty<DiscoverEntry>();
        public int EntryCount => _entries?.Length ?? 0;
    }
}
