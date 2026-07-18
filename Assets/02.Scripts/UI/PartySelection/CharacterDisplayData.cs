using UnityEngine;
using TeamLog.Characters;

namespace TeamLog.UI.PartySelection
{
    /// <summary>
    /// 캐릭터 선택 화면용 디스플레이 데이터 (UI-B.1).
    /// CharacterData(기존 SO) + 웹 목업 전용 확장 정보(정체성/강점/약점/자원 메커니즘/역할/태그).
    /// CharacterData SO 자체에 이 필드들이 추가되기 전까지 사용하는 임시 DTO.
    /// → 향후 CharacterData가 이 필드를 직접 보유하면 이 클래스는 deprecated.
    ///
    /// 사용 패턴:
    ///   var data = CharacterDisplayData.FromCharacterId("Ashe", characterDataSO);
    ///   portraitBig.Initialize(data);
    /// </summary>
    [System.Serializable]
    public class CharacterDisplayData
    {
        // ── 원본 참조 ──
        public CharacterData CharacterData;

        // ── 식별 ──
        public string CharacterId;       // "Ashe", "Duran" 등
        public string DisplayName;       // "ASHE"
        public string Title;             // "the Pyromancer"

        // ── 시각 ──
        public string Initial;           // 거대 초상화 이니셜 — 플레이스홀더용
        public Color ResourceColor;      // 자원별 시그니처 컬러
        public Sprite PortraitSprite;    // 실제 초상화 (없으면 null → 플레이스홀더)
        public Sprite ResourceBadgeSprite; // 자원 배지 Sprite

        // ── 자원 ──
        public ResourceType ResourceType;
        public string ResourceLabel;     // "EMBER"
        public string ResourceInitial;   // "E"
        public int ResourceMax;          // 0이면 무한/자원 없음
        public string ResourceMechanicText; // 메커니즘 설명 (HTML-like 태그 포함)

        // ── 텍스트 ──
        [TextArea(2, 4)] public string Identity;   // 정체성 한 문장
        [TextArea(1, 3)] public string Strength;
        [TextArea(1, 3)] public string Weakness;
        public string RoleEn;            // "Single Nuke"
        public string RoleKo;            // "단일 폭딜"

        // ── 메타 ──
        public CharacterTag Tag;         // NEW / REWORK / NONE
        public bool Locked;              // 잠금 여부 (메타프로세션)

        // ── 스탯 ──
        public int HP;

        // ── 팩토리 ──
        /// <summary>
        /// 캐릭터 ID로부터 CharacterDisplayData 생성.
        /// PartySelectionUIUtils에서 모든 표시 정보를 채움.
        /// </summary>
        public static CharacterDisplayData FromCharacterId(string charId, CharacterData sourceData = null)
        {
            var data = new CharacterDisplayData
            {
                CharacterData = sourceData,
                CharacterId = charId,
                DisplayName = PartySelectionUIUtils.GetCharacterDisplayName(charId),
                Title = PartySelectionUIUtils.GetCharacterTitle(charId),
                Initial = PartySelectionUIUtils.GetCharacterInitial(charId),
            };

            // 자원 해석
            // Mortis/Cael 예외 처리 → 일단 None으로 두고 charId로 색상/라벨/배지 조회
            data.ResourceType = sourceData != null ? ResolveResourceType(sourceData, charId) : ResourceType.None;
            data.ResourceColor = PartySelectionUIUtils.GetResourceColorByCharId(charId, data.ResourceType);
            data.ResourceLabel = ResolveResourceLabel(charId, data.ResourceType);
            data.ResourceInitial = ResolveResourceInitial(charId, data.ResourceType);
            data.ResourceMax = ResolveResourceMax(data.ResourceType, charId);
            data.ResourceMechanicText = PartySelectionUIUtils.GetResourceMechanicText(data.ResourceType, charId);
            data.ResourceBadgeSprite = PartySelectionUIUtils.GetResourceBadgeSprite(data.ResourceType, charId);

            // 정체성/강점/약점/역할
            data.Identity = PartySelectionUIUtils.GetCharacterIdentity(charId);
            var (str, weak) = PartySelectionUIUtils.GetCharacterStrengthWeakness(charId);
            data.Strength = str;
            data.Weakness = weak;
            var (roleEn, roleKo) = PartySelectionUIUtils.GetCharacterRole(charId);
            data.RoleEn = roleEn;
            data.RoleKo = roleKo;

            // 스탯
            data.HP = sourceData != null ? sourceData.BaseHP : 80;

            // 태그 (NEW: Phase CC 신규 6종, REWORK: 기존 캐릭터 리워크 5종)
            data.Tag = ResolveCharacterTag(charId);

            // 잠금 (기본 false — 메타 해금 상태에 따라 외부에서 설정)
            data.Locked = false;

            return data;
        }

        private static ResourceType ResolveResourceType(CharacterData cd, string charId)
        {
            // CharacterData가 CharacterResourceComponent를 가지고 있으면 그것을 사용
            // 하지만 CharacterData SO 자체는 자원 타입을 안 가질 수 있음
            // 일단 CharacterData에서 CharacterId → 자원 매핑
            string id = (charId ?? cd?.CharacterName ?? "").ToLowerInvariant();
            if (id.Contains("ashe"))     return ResourceType.Ember;
            if (id.Contains("duran"))    return ResourceType.Vengeance;
            if (id.Contains("lumi"))     return ResourceType.Frost;
            if (id.Contains("sibyl"))    return ResourceType.Prophecy;
            if (id.Contains("taranis"))  return ResourceType.Charge;
            if (id.Contains("umbra"))    return ResourceType.Shadows;
            if (id.Contains("aster"))    return ResourceType.Combo;
            if (id.Contains("elara"))    return ResourceType.Mercy;
            if (id.Contains("calliope")) return ResourceType.Melody;
            // Mortis / Cael — ResourceType에 없음 → None (색상/라벨은 charId 기반 처리)
            return ResourceType.None;
        }

        private static string ResolveResourceLabel(string charId, ResourceType type)
        {
            // Mortis/Cael 특수 케이스
            if (!string.IsNullOrEmpty(charId))
            {
                string id = charId.ToLowerInvariant();
                if (id.Contains("mortis")) return "CORPSE";
                if (id.Contains("cael"))   return "DISCOVER";
            }
            return PartySelectionUIUtils.GetResourceLabel(type);
        }

        private static string ResolveResourceInitial(string charId, ResourceType type)
        {
            if (!string.IsNullOrEmpty(charId))
            {
                string id = charId.ToLowerInvariant();
                if (id.Contains("mortis")) return "☠";
                if (id.Contains("cael"))   return "⚗";
            }
            return PartySelectionUIUtils.GetResourceInitial(type);
        }

        private static int ResolveResourceMax(ResourceType type, string charId)
        {
            if (!string.IsNullOrEmpty(charId))
            {
                string id = charId.ToLowerInvariant();
                if (id.Contains("mortis")) return 5;  // Corpse
                if (id.Contains("cael"))   return 0;  // Discover (무한 — 발견 시스템)
            }
            return type switch
            {
                ResourceType.Ember     => 5,
                ResourceType.Vengeance => 10,
                ResourceType.Frost     => 8,
                ResourceType.Prophecy  => 5,
                ResourceType.Charge    => 6,
                ResourceType.Shadows   => 8,
                ResourceType.Combo     => 5,
                ResourceType.Mercy     => 5,
                ResourceType.Melody    => 4,
                _ => 0,
            };
        }

        private static CharacterTag ResolveCharacterTag(string charId)
        {
            if (string.IsNullOrEmpty(charId)) return CharacterTag.None;
            string id = charId.ToLowerInvariant();

            // NEW: Phase CC 신규 캐릭터
            if (id.Contains("ashe") || id.Contains("duran") || id.Contains("lumi") ||
                id.Contains("sibyl") || id.Contains("taranis") || id.Contains("umbra"))
                return CharacterTag.New;

            // REWORK: 기존 캐릭터 리워크 (Phase CC-2)
            if (id.Contains("aster") || id.Contains("mortis") || id.Contains("cael") ||
                id.Contains("calliope") || id.Contains("elara"))
                return CharacterTag.Rework;

            return CharacterTag.None;
        }
    }

    /// <summary>
    /// 캐릭터 선택 화면에서 표시할 메타 태그.
    /// </summary>
    public enum CharacterTag
    {
        None,       // 기본
        New,        // Phase CC 신규 (Ashe/Duran/Lumi/Sibyl/Taranis/Umbra)
        Rework,     // Phase CC-2 리워크 (Aster/Mortis/Cael/Calliope/Elara)
    }
}
