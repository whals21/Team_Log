using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TeamLog.Map
{
    /// <summary>
    /// 캐릭터→특성 바인딩 — JsonUtility 직렬화용.
    /// 캐릭터 CharacterName을 키로 사용 (CharacterData.CharacterName).
    /// </summary>
    [System.Serializable]
    public class TraitBindingEntry
    {
        public string CharacterName;
        public string TraitId;     // CharacterTraitData.TraitId

        public TraitBindingEntry() { }
        public TraitBindingEntry(string characterName, string traitId)
        {
            CharacterName = characterName;
            TraitId = traitId;
        }
    }

    /// <summary>
    /// 런 간 영구 통계 — JsonUtility 직렬화용
    /// Phase 8B: 메타 재화(기억의 조각/영혼) + 특성/유물 해금 상태 + 장착 바인딩 추가
    /// </summary>
    [System.Serializable]
    public class MetaSaveData
    {
        public int TotalRuns;
        public int Victories;
        public int BestFloor;
        public int TotalGoldEarned;
        public bool HasPendingRun;
        public bool HasCompletedTutorial;

        // 캐릭터 잠금해제 상태
        public List<string> UnlockedCharacterIds = new();

        // ── Phase 8B: 메타 재화 ──
        public int MemoryFragments;     // 기억의 조각 — 일반 메타 재화
        public int Souls;               // 영혼 — 희귀 해금용

        // ── Phase 8B: 해금 상태 ──
        public List<string> UnlockedTraitIds = new();          // 해금된 캐릭터 특성 TraitId
        public List<string> UnlockedRelicIds = new();          // 해금된 유물 (에셋 파일명)
        public List<string> PurchasedUpgradeIds = new();       // 구입한 일회성 메타 강화

        // ── Phase 8B: 장착 바인딩 (CharacterName → TraitId) ──
        public List<TraitBindingEntry> EquippedTraitBindings = new();

        // ── Ascension: 클리어 누적 난이도 ──
        // AscensionLevel: 달성한 최대 어센션 레벨 (0~15). 런 클리어 시 자동 +1.
        // SelectedAscensionLevel: 다음 런에 플레이할 어센션 레벨 (사용자 선택, 0~AscensionLevel).
        public int AscensionLevel;
        public int SelectedAscensionLevel;
    }
}
