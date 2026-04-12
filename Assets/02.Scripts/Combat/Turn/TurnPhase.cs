namespace TeamLog.Combat.Turn
{
    /// <summary>
    /// 전투 페이즈 정의
    /// </summary>
    public enum TurnPhase
    {
        None,           // 전투 시작 전
        Draw,           // 스킬 드로우 페이즈
        PlayerAction,   // 플레이어 행동 선택
        Execution,      // 스킬 실행
        EnemyTurn,      // 적 턴
        BattleEnd       // 전투 종료
    }
}
