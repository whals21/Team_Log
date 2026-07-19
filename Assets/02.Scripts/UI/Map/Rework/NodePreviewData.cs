using System.Collections.Generic;
using UnityEngine;
using TeamLog.Map;

namespace TeamLog.UI.Map.Rework
{
    /// <summary>
    /// ★ Node Detail Preview 파이프 — 노드 클릭 시 사이드 패널에 표시할 미리보기 데이터.
    /// UI-로직 분리 원칙: NodeDetailPanel은 본 DTO만 소비 (게임 로직 금지).
    /// MapSceneSetup.PrepareNodePreview가 빌더 역할.
    /// </summary>
    public class NodePreviewData
    {
        public MapNodeType NodeType;

        // 헤더 정보
        public string Title;          // "BATTLE", "ELITE", "NEMESIS", "EVENT", "SHOP", "SANCTUARY"
        public string Subtitle;       // "Crimson Acolytes" / "Wandering Merchant" / ...
        public string Description;    // 분위기 묘사
        public string ActionLabel;    // "Enter Battle" / "Investigate" / "Browse Wares" / "Rest Here"
        public Color ThemeColor;      // 노드 타입별 색상
        public string IconSymbol;     // "B" / "E" / "X" / "?" / "$" / "+"

        // 전투 노드 전용 (Battle/Elite/Boss)
        public List<EnemyPreviewInfo> Enemies = new();

        // 보상 정보 (Battle/Elite/Boss만 의미, Event/Shop/Rest는 null 가능)
        public RewardPreviewInfo Rewards;
    }

    /// <summary>
    /// 미리보기용 적 정보 1개 분량.
    /// 런타임 Character 인스턴스 대신 정적 데이터 + 스케일링 후 HP 추정치만 보관.
    /// </summary>
    public class EnemyPreviewInfo
    {
        public string Name;           // CharacterData.CharacterName
        public int EstimatedHP;       // BaseHP × FloorScaling × AscensionMul (반올림)
        public Color Tint;            // 적 색상 (위협도별 — 일반/엘리트/보스)
    }

    /// <summary>
    /// 보상 미리보기 — 전투 승리 시 예상 보상 범위.
    /// RewardManager.GetPreview가 빌더.
    /// </summary>
    public class RewardPreviewInfo
    {
        public int GoldMin, GoldMax;  // 예상 골드 범위
        public int AugmentCount;      // 증강 제안 수 (보통 3)
        public float RelicChance;     // 0 / 0.5 / 1.0
        public bool IncludesSouls;    // 보스만 true (영혼 재화)
        public string Summary;        // "10-25G · 3 Augments" 등 한 줄 요약 (UI 바인딩용)
    }
}
