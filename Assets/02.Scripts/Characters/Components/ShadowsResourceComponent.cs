namespace TeamLog.Characters
{
    /// <summary>
    /// Shadows 자원 — Umbra (Rogue) 고유 메카닉 (Phase CC-2A).
    ///
    /// 핵심 루프 (기획: ReworkDrafts/02_Rogue.md):
    /// - 매 턴 종료 시 이번 턴 피해를 1도 받지 않았으면 Shadows +1 (최대 3)
    /// - HP 손상(직격/도트/Pierce 모두) 시 즉시 Shadows = 0 리셋
    /// - 쉴드 흡수(HP 손상 0)는 "안 맞음" 인정 (Healer Holy Shield 시너지 핵심)
    /// - Shadows 값에 따라 CritChance/CritDamageMul 동적 갱신:
    ///   0 → 0% / 1.5× | 1 → 50% / 1.5× | 2 → 75% / 1.5× | 3 → 100% / 2.0×
    ///
    /// 전략: 파티 보호(도발/쉴드/일점사)로 Umbra를 안 맞게 유지 → 치명타 폭딜.
    /// Ashe(Ember, 자해)와의 서사적 대칭 — "자해할수록 강해진다" vs "안 맞을수록 강해진다".
    /// </summary>
    public class ShadowsResourceComponent : CharacterResourceComponent
    {
        public override ResourceType Resource => ResourceType.Shadows;
        public override int MaxStacks => 3;

        /// <summary>Shadows 3 도달 시 기대 데미지 배율 (Eviscerate 기준 15 × 2.0 = 30).</summary>
        public const float Shadows3CritDamageMul = 2.0f;

        private bool _tookDamageThisTurn;
        private bool _subscribed;

        public override void OnTurnStart(Character owner)
        {
            // 첫 턴 시작 시 Health.OnDamageTaken 구독 (한 번만)
            // — Health.OnDamageTaken은 HP 실제 손실 시에만 발생 (쉴드 흡수만 있으면 발생 안 함)
            if (!_subscribed && owner != null)
            {
                owner.Health.OnDamageTaken += OnDamaged;
                _subscribed = true;
            }
            // 이번 턴 피해 플래그 리셋
            _tookDamageThisTurn = false;
        }

        public override void OnTurnEnd(Character owner)
        {
            if (_tookDamageThisTurn)
            {
                // 피해 받음 → 그림자 깨짐, Shadows 전부 상실
                if (CurrentStacks > 0)
                    ConsumeStacks(CurrentStacks);
            }
            else
            {
                // 완전 무피해 → 그림자 깊어짐, Shadows +1
                AddStacks(1);
            }
            // CritChance/CritDamageMul 갱신
            ApplyCritBonus(owner);
        }

        private void OnDamaged(int damage)
        {
            if (damage > 0)
                _tookDamageThisTurn = true;
        }

        /// <summary>Shadows 값에 따라 Character.CritChance/CritDamageMul 갱신.</summary>
        private void ApplyCritBonus(Character owner)
        {
            if (owner == null) return;

            switch (CurrentStacks)
            {
                case 0:
                    owner.CritChance = 0f;
                    owner.CritDamageMul = 1.5f;
                    break;
                case 1:
                    owner.CritChance = 0.50f;
                    owner.CritDamageMul = 1.5f;
                    break;
                case 2:
                    owner.CritChance = 0.75f;
                    owner.CritDamageMul = 1.5f;
                    break;
                case 3:
                    owner.CritChance = 1.0f;
                    owner.CritDamageMul = Shadows3CritDamageMul; // 2.0×
                    break;
                default: // 4+ ("그림자 심화" 특성 MaxStacksBonus=1 시)
                    owner.CritChance = 1.0f;
                    owner.CritDamageMul = 3.5f; // Shadows 4 = 치명타 피해 3.5배
                    break;
            }
        }

        /// <summary>
        /// Eviscerate 스킬 사용 후 호출 — Shadows 1 소모.
        /// 기획: 매 턴 연속 Eviscerate 허용 (파티 보호 시).
        /// 스킬 실행 파이프라인에서 호출.
        /// </summary>
        public void ConsumeOneForEviscerate()
        {
            ConsumeStacks(1);
            // 소모 후에도 이번 턴 "안 맞음"이면 다음 턴 종료 시 +1 복구
        }

        /// <summary>
        /// 스킬 사용 전 Shadow 조건 체크용 — Eviscerate가 Shadows 3인지 확인.
        /// </summary>
        public bool IsAtMax => CurrentStacks >= MaxStacks;
    }
}
