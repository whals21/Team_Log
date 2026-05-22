namespace TeamLog.Combat.Turn
{
    /// <summary>
    /// 현재 턴의 컨텍스트 정보
    /// </summary>
    public class TurnContext
    {
        public int TurnNumber { get; private set; }
        public TurnPhase CurrentPhase { get; private set; }

        public event System.Action<TurnPhase, TurnPhase> OnPhaseChanged;
        public event System.Action<int> OnTurnStarted;
        public event System.Action<int, int> OnAPChanged;

        private int _currentAP;
        private int _maxAP;
        public int CurrentAP => _currentAP;
        public int MaxAP => _maxAP;

        public TurnContext()
        {
            TurnNumber = 0;
            CurrentPhase = TurnPhase.None;
        }

        public void StartNewTurn()
        {
            TurnNumber++;
            OnTurnStarted?.Invoke(TurnNumber);
        }

        public void SetPhase(TurnPhase newPhase)
        {
            var oldPhase = CurrentPhase;
            CurrentPhase = newPhase;
            OnPhaseChanged?.Invoke(oldPhase, newPhase);
        }

        public void ResetAP(int maxAP)
        {
            _maxAP = maxAP;
            _currentAP = maxAP;
            OnAPChanged?.Invoke(_currentAP, _maxAP);
        }

        public bool CanAfford(int cost)
        {
            return _currentAP >= cost;
        }

        public void SpendAP(int cost)
        {
            _currentAP -= cost;
            OnAPChanged?.Invoke(_currentAP, _maxAP);
        }
    }
}
