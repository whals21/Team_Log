using System.Collections.Generic;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Event;

namespace TeamLog.Map
{
    /// <summary>
    /// 스테이지 테마 정의 — 한 스테이지(층)의 적 구성/보스/키워드를 묶은 ScriptableObject.
    /// 각 스테이지마다 3개 후보 중 1개가 런 시작 시 무작위 채택됨 (StageDesign.md 참조).
    /// 4스테이지 × 3테마 = 81가지 조합으로 반복 플레이 유지.
    /// </summary>
    [CreateAssetMenu(fileName = "Theme_", menuName = "TeamLog/Stage Theme")]
    public class StageThemeData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("에셋 식별자 (예: GreyForest)")]
        public string themeId = "";
        [Tooltip("한국어 표시명 (예: 잿빛 숲)")]
        public string displayName = "";
        [Tooltip("1~4. 이 테마가 속한 스테이지 번호 (참고용)")]
        public int stageNumber = 1;
        [TextArea(2, 6)] public string description = "";

        [Header("Enemies")]
        public List<CharacterData> normalEnemies = new();
        public List<CharacterData> eliteEnemies = new();
        public CharacterData boss;

        [Header("Spawn Pattern Table (optional)")]
        [Tooltip("미지정 시 normalEnemies/elites에서 무작위 추출")]
        public SpawnPatternTable spawnPatternTable;

        [Header("Theme Keywords (gimmick summary)")]
        [Tooltip("테마 기믹 요약 — 적 intent/특성 배지에 표시용")]
        public List<string> themeKeywords = new();

        [Header("Theme-Specific Events (Phase E3)")]
        [Tooltip("이 테마에서만 등장하는 전용 이벤트들. 미지정 시 공통 이벤트 풀만 사용")]
        public List<EventData> themeEvents = new();
    }
}
