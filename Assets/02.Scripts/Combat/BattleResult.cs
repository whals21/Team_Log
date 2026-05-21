using TeamLog.Map;

namespace TeamLog.Combat
{
    /// <summary>
    /// 전투 결과를 씬 전환 간 전달하는 정적 클래스
    /// BattleSceneSetup이 결과를 기록하고, MapSceneSetup이 소비
    /// </summary>
    public static class BattleResult
    {
        private static bool _hasPending;
        private static bool _wasVictory;
        private static MapNodeType _battleType;

        public static bool HasPendingResult => _hasPending;
        public static bool WasVictory => _wasVictory;
        public static MapNodeType BattleType => _battleType;

        /// <summary>
        /// 전투 시작 전 노드 타입 설정 (MapSceneSetup에서 호출)
        /// </summary>
        public static void SetBattleType(MapNodeType nodeType)
        {
            _battleType = nodeType;
            _hasPending = false;
        }

        /// <summary>
        /// 전투 종료 시 결과 기록 (BattleSceneSetup에서 호출)
        /// </summary>
        public static void SetResult(bool victory)
        {
            _wasVictory = victory;
            _hasPending = true;
        }

        /// <summary>
        /// 보상 처리 완료 후 초기화 (MapSceneSetup에서 호출)
        /// </summary>
        public static void Clear()
        {
            _hasPending = false;
            _wasVictory = false;
            _battleType = MapNodeType.Battle;
        }
    }
}
