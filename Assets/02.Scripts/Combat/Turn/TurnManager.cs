using System.Collections.Generic;
using TeamLog.Combat.AI;
using TeamLog.Combat.Draw;
using TeamLog.Map;
using TeamLog.Skill;

// 네임스페이스 충돌 해결
using Character = TeamLog.Characters.Character;
using SkillData = TeamLog.Characters.SkillData;
using SkillInstance = TeamLog.Characters.SkillInstance;
using StatusEffectType = TeamLog.Characters.StatusEffectType;
using TargetType = TeamLog.Characters.TargetType;
using ResourceType = TeamLog.Characters.ResourceType;
using ProphecyResourceComponent = TeamLog.Characters.ProphecyResourceComponent;
using CorpseComponent = TeamLog.Characters.CorpseComponent;

// Phase BK: 행동 키워드 타입 별칭
using BehaviorKeyword = TeamLog.Skill.BehaviorKeyword;

namespace TeamLog.Combat.Turn
{
    /// <summary>
    /// 턴 진행 오케스트레이터 — 턴 라이프사이클, AP 관리, 대상 분해만 담당
    /// 데미지 계산: DamageCalculator, 스킬 실행: SkillExecutor
    /// </summary>
    public class TurnManager
    {
        private readonly TurnContext _context;
        private readonly List<Character> _playerParty;
        private readonly List<Character> _enemies;
        private readonly List<EnemyAIController> _enemyControllers;
        private readonly SkillDrawSystem _drawSystem;
        private readonly SkillExecutor _skillExecutor;
        private readonly int _bonusFirstTurnAP;

        public TurnContext Context => _context;
        public SkillDrawSystem DrawSystem => _drawSystem;
        public SkillExecutor SkillExecutor => _skillExecutor;
        public TurnPhase CurrentPhase => _context.CurrentPhase;
        public int TurnNumber => _context.TurnNumber;

        /// <summary>코루틴이 순회할 적 컨트롤러 목록 (읽기 전용).</summary>
        public IReadOnlyList<EnemyAIController> EnemyControllers => _enemyControllers;

        public event System.Action<TurnPhase, TurnPhase> OnPhaseChanged;
        public event System.Action<int> OnTurnStarted;
        public event System.Action OnBattleEnded;
        public event System.Action<int, int> OnAPChanged;

        // 순차 적 턴 모드 — 코루틴 주도 실행 시 사용 (BattleSceneSetup)
        public event System.Action OnEnemyTurnSequenceStarted; // 코루틴 시작 알림
        public event System.Action<Character> OnEnemyActing;   // 개별 적 행동 시작 (하이라이트용)

        // Phase CC-2F: 시체 자동 행동/스킬 교체 알림 (UI 피드백용)
        // necromancer, slotIndex, skill, target, damageDealt, healFromSoulLink
        public event System.Action<Character, int, SkillData, Character, int, int> OnCorpseAction;
        // necromancer, slotIndex, oldSkill, newSkill
        public event System.Action<Character, int, SkillData, SkillData> OnCorpseSkillSwapped;

        private bool _sequentialEnemyTurn;

        // Phase CC-2F: 적 처치 큐 — ProcessCorpseAction 시작 전 시체 스킬 자동 교체에 사용
        private readonly List<Character> _pendingKilledEnemies = new();

        public int CurrentAP => _context.CurrentAP;
        public int MaxAP => _context.MaxAP;

        public TurnManager(List<Character> playerParty, List<Character> enemies,
            List<EnemyAIController> enemyControllers = null, int maxRerolls = 1, int bonusFirstTurnAP = 0)
        {
            _playerParty = playerParty;
            _enemies = enemies;
            _enemyControllers = enemyControllers ?? new List<EnemyAIController>();
            _context = new TurnContext();
            _drawSystem = new SkillDrawSystem(playerParty, maxRerolls);
            _skillExecutor = new SkillExecutor(playerParty, enemies);
            _bonusFirstTurnAP = bonusFirstTurnAP;

            _context.OnPhaseChanged += (old, newPhase) => OnPhaseChanged?.Invoke(old, newPhase);
            _context.OnTurnStarted += turn => OnTurnStarted?.Invoke(turn);
            _context.OnAPChanged += (current, max) => OnAPChanged?.Invoke(current, max);
        }

        public void StartBattle()
        {
            // Phase 8C: 플레이어 장착 특성 처리기 구독
            foreach (var c in _playerParty)
            {
                if (c.PlayerTraitHandler != null && c.PlayerTraitHandler.HasTrait)
                    c.PlayerTraitHandler.SubscribeEvents();

                // Phase ARCH-5: 모든 스킬의 UsesThisBattle 리셋 (Fatigue/Momentum/Escalation/Mastery)
                foreach (var inst in c.SkillInventory.SkillInstances)
                    inst.ResetUsesThisBattle();

                // Phase CC-2F: Necromancer 시체 리셋 (매 전투 시작 시 기본 4스킬로)
                if (c.Corpse != null)
                    c.Corpse.ResetToBaseSkills();
            }

            // Phase CC-2F: 적 처치 이벤트 구독 — 시체 스킬 자동 교체용 큐
            CombatEventBus.OnKill += OnEnemyKilledForCorpse;

            CombatEventBus.FireBattleStart();
            StartNewTurn();
        }

        /// <summary>Phase CC-2F: 적 처치 시 큐에 추가 — 다음 ProcessCorpseAction에서 시체 스킬 교체.</summary>
        private void OnEnemyKilledForCorpse(Character killed)
        {
            if (killed == null) return;
            if (!_enemies.Contains(killed)) return; // 적만 처리 (플레이어 사망 스킵)
            _pendingKilledEnemies.Add(killed);
        }

        public void StartNewTurn()
        {
            _context.StartNewTurn();

            // 턴 시작 시 모든 캐릭터의 스탯 수정자 재계산
            foreach (var c in _playerParty) if (c.IsAlive) c.ApplyStatModifiers();
            foreach (var c in _enemies) if (c.IsAlive) c.ApplyStatModifiers();

            // 쉴드 리셋 — 각 진영은 자기 턴 시작 시 자기 진영 쉴드만 리셋.
            // ★ Phase GF (2026-07-22): A안 — 적 쉴드는 적 턴 시작(StartEnemyTurn)에 리셋.
            // 이렇게 해야 적이 자기 턴에 건 쉴드가 다음 플레이어 턴에 흡수됨 (Wisp_Wobble 등).
            // 기존에는 매 턴 시작에 둘 다 리셋해서 적 쉴드가 플레이어 턴에 의미 없었음.
            foreach (var c in _playerParty) if (c.IsAlive) c.Health.ResetShield();

            // Phase CC-2G-5: FollowUp 추적 — 매 턴 시작 시 HitThisTurn 리셋
            foreach (var c in _playerParty) c.ResetHitThisTurn();
            foreach (var c in _enemies) c.ResetHitThisTurn();

            // ★ Phase CC (2026-07-22): 적 특성(Regenerate 등)은 적 턴 시작으로 이동.
            // 기존에는 플레이어 턴 시작에 발동했으나, 적 턴에 발동하는 것이 자연스러움.
            // 또한 Charge 도트(적 턴 종료)와 같은 적 턴 내에서 회복/데미지가 일어나 사용자가 한눈에 파악 가능.
            // foreach (var c in _enemies) if (c.IsAlive) c.TraitHandler.OnTurnStart(_context.TurnNumber);

            // 키워드: HPPerTurn — 매턴 HP 변화 (저주: 감소, 재생: 회복)
            foreach (var c in _playerParty)
            {
                if (!c.IsAlive) continue;
                int hpChange = SkillExecutor.GetKeywordSumForCharacter(c, KeywordType.HPPerTurn);
                if (hpChange > 0) c.Health.Heal(hpChange);
                else if (hpChange < 0) c.Health.TakeDamage(-hpChange);
            }

            // Phase CC: 캐릭터 고유 자원 턴 시작 훅 (Ashe Ember +1, Lumi Frost 유지 등)
            foreach (var c in _playerParty)
                if (c.IsAlive && c.Resource != null) c.Resource.OnTurnStart(c);

            // Phase CC: Prophecy 예약된 스킬 발동 (Sibyl — 1턴 뒤 발동)
            foreach (var c in _playerParty)
            {
                if (!c.IsAlive) continue;
                if (c.Resource is ProphecyResourceComponent prophecy && prophecy.PendingCount > 0)
                {
                    var pending = prophecy.ConsumePending();
                    foreach (var (pSkill, pInstance, pTarget) in pending)
                    {
                        if (pTarget != null && pTarget.IsAlive)
                            _skillExecutor.ExecuteSkillInternal(c, pSkill, pTarget, pInstance);
                    }
                }
            }

            // AP 리셋: 기본 1 + 생존 파티원 수 (+ 첫 턴 보너스 + 유물 ExtraAP + 장착 특성 ExtraAP)
            int aliveCount = _playerParty.FindAll(p => p.IsAlive).Count;
            int bonus = (_context.TurnNumber == 1) ? _bonusFirstTurnAP : 0;
            int relicAP = GameRunState.Instance?.RelicHandler.GetExtraAP() ?? 0;
            int traitAP = 0;
            foreach (var c in _playerParty)
            {
                if (c.IsAlive && c.PlayerTraitHandler != null && c.PlayerTraitHandler.HasTrait)
                    traitAP += c.PlayerTraitHandler.GetExtraAP();
            }
            _context.ResetAP(1 + aliveCount + bonus + relicAP + traitAP);

            ExecuteDrawPhase();

            CombatEventBus.FireTurnStart(_context.TurnNumber);
        }

        private void ExecuteDrawPhase()
        {
            _context.SetPhase(TurnPhase.Draw);
            _drawSystem.ExecuteDraw();
            _context.SetPhase(TurnPhase.PlayerAction);
        }

        public bool RerollSlot(int slotIndex)
        {
            if (CurrentPhase != TurnPhase.PlayerAction) return false;
            return _drawSystem.RerollSlot(slotIndex);
        }

        /// <summary>
        /// 스킬 즉시 시전 — 대상 분해 + AP 관리 후 SkillExecutor에 위임
        /// </summary>
        public bool ExecuteSkillImmediately(Character caster, SkillData skill, Character target)
        {
            return ExecuteSkillImmediately(caster, skill, target, null);
        }

        public bool ExecuteSkillImmediately(Character caster, SkillData skill, Character target, SkillInstance instance)
        {
            if (caster.IsDead) return false;

            // ★ Phase GF (2026-07-22): CC (Stun/Freeze/Sleep) 행동 불가.
            // 시전자가 CC 상태면 스킬 시전 차단. UI는 ActionBarUI에서 별도 처리 권장.
            if (caster.StatusEffects.IsIncapacitated)
            {
                return false;
            }

            // AP 체크 — 증강 + 장착 특성 반영 코스트 사용
            int effectiveCost = instance != null ? instance.EffectiveCost : skill.Cost;
            // Phase 8C: 장착 특성 CostAdd (궁수 속사 등)
            if (caster.PlayerTraitHandler != null && caster.PlayerTraitHandler.HasTrait)
                effectiveCost += caster.PlayerTraitHandler.QueryKeywordSum(KeywordType.CostAdd);
            effectiveCost = System.Math.Max(0, effectiveCost);
            if (!_context.CanAfford(effectiveCost))
                return false;

            // Phase CC: 자원 소모 체크 (Ember/Vengeance/Frost 등)
            // ConsumeAllResource(전량 소모)인 경우 체크 스킵 — 자원 0이어도 사용 가능 (위력만 낮아짐).
            if (caster.Resource != null
                && skill.ResourceCostType != ResourceType.None
                && skill.ResourceCostType == caster.Resource.Resource
                && !skill.ConsumeAllResource
                && !caster.Resource.CanConsume(skill.ResourceCostAmount))
            {
                return false;
            }

            // Phase CC-2A: 자원 최소치 검사 (Umbra Eviscerate — Shadows 3 필수 등)
            // MinResourceRequired > 0이면 현재 스택이 이 값 이상이어야 사용 가능.
            if (caster.Resource != null
                && skill.ResourceCostType != ResourceType.None
                && skill.ResourceCostType == caster.Resource.Resource
                && skill.MinResourceRequired > 0
                && caster.Resource.CurrentStacks < skill.MinResourceRequired)
            {
                return false;
            }

            _context.SpendAP(effectiveCost);

            // Phase CC: Prophecy — Sibyl 스킬은 1턴 뒤 발동 예약 (즉시 실행 안 함)
            if (caster.Resource is ProphecyResourceComponent prophecy)
            {
                prophecy.Reserve(skill, target, instance);
                instance?.IncrementUsesThisBattle();
                CheckBattleEnd();
                return CurrentPhase == TurnPhase.BattleEnd;
            }

            switch (skill.Target)
            {
                case TargetType.Self:
                case TargetType.SingleAlly:
                    if (target != null)
                        _skillExecutor.ExecuteSkillInternal(caster, skill, target, instance);
                    break;
                case TargetType.SingleEnemy:
                    if (target != null && target.IsAlive)
                    {
                        // Phase BK: Spread 행동 키워드 — 단일 → 광역 (위력 100%)
                        bool spread = instance != null && instance.HasBehavior(BehaviorKeyword.Spread);
                        // Phase BK: AOEAuto 행동 키워드 — 자동 광역 (위력 100%, 코스트 +2)
                        bool aoeAuto = instance != null && instance.HasBehavior(BehaviorKeyword.AOEAuto);

                        if (spread || aoeAuto)
                        {
                            foreach (var enemy in _enemies)
                                if (enemy.IsAlive) _skillExecutor.ExecuteSkillInternal(caster, skill, enemy, instance, 1f);

                            // Explosion: 광역 후 무작위 N명 추가 타격 (중복 허용, 위력 그대로)
                            int explosionN = instance?.GetBehaviorRank(BehaviorKeyword.Explosion) ?? 0;
                            if (explosionN > 0)
                            {
                                var aliveForExplosion = _enemies.FindAll(e => e.IsAlive);
                                if (aliveForExplosion.Count > 0)
                                {
                                    for (int i = 0; i < explosionN; i++)
                                    {
                                        int idx = UnityEngine.Random.Range(0, aliveForExplosion.Count);
                                        _skillExecutor.ExecuteSkillInternal(caster, skill, aliveForExplosion[idx], instance, 1f);
                                        aliveForExplosion.RemoveAll(e => !e.IsAlive);
                                        if (aliveForExplosion.Count == 0) break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // 기본 단일 타격
                            _skillExecutor.ExecuteSkillInternal(caster, skill, target, instance, 1f);

                            // Bounce: 무작위 적 N회 추가 (같은 적 중복 허용, 위력 100%)
                            int bounceN = instance?.GetBehaviorRank(BehaviorKeyword.Bounce) ?? 0;
                            if (bounceN > 0)
                            {
                                var aliveForBounce = _enemies.FindAll(e => e.IsAlive);
                                if (aliveForBounce.Count > 0)
                                {
                                    for (int i = 0; i < bounceN; i++)
                                    {
                                        int idx = UnityEngine.Random.Range(0, aliveForBounce.Count);
                                        _skillExecutor.ExecuteSkillInternal(caster, skill, aliveForBounce[idx], instance, 1f);
                                        aliveForBounce.RemoveAll(e => !e.IsAlive);
                                        if (aliveForBounce.Count == 0) break;
                                    }
                                }
                            }

                            // MultiHit: 동일 대상 N회 추가 (위력 100%)
                            int multiHitN = instance?.GetBehaviorRank(BehaviorKeyword.MultiHit) ?? 0;
                            if (multiHitN > 0 && target.IsAlive)
                            {
                                for (int i = 0; i < multiHitN; i++)
                                {
                                    if (!target.IsAlive) break;
                                    _skillExecutor.ExecuteSkillInternal(caster, skill, target, instance, 1f);
                                }
                            }

                            // Phase CC-2B: ComboMultiHit — Aster Multi-Shot
                            // caster의 Combo 스택수만큼 추가 타격 (위력 100%).
                            // Combo 소모는 costType=Combo, costAmount=1 설정으로 TurnManager 기본 자원 소모 파이프라인에서 처리.
                            // Combo 0에서는 스킬 자체가 사용 불가 (MinResourceRequired=1 사전 검사).
                            bool comboMultiHit = instance != null && instance.HasBehavior(BehaviorKeyword.ComboMultiHit);
                            if (comboMultiHit && target.IsAlive && caster.Resource != null
                                && caster.Resource.Resource == ResourceType.Combo
                                && caster.Resource.CurrentStacks > 0)
                            {
                                int extraHits = caster.Resource.CurrentStacks; // Combo 1 → 1회, 3 → 3회
                                for (int i = 0; i < extraHits; i++)
                                {
                                    if (!target.IsAlive) break;
                                    _skillExecutor.ExecuteSkillInternal(caster, skill, target, instance, 1f);
                                }
                            }

                            // Phase CC-2B: ComboFinisher — Aster Execute Shot
                            // 스킬 사용으로 target이 사망하면 Combo 3 복구 (스노우볼).
                            bool comboFinisher = instance != null && instance.HasBehavior(BehaviorKeyword.ComboFinisher);
                            if (comboFinisher && !target.IsAlive && caster.Resource != null
                                && caster.Resource.Resource == ResourceType.Combo
                                && caster.IsAlive)
                            {
                                caster.Resource.AddStacks(3); // Execute Shot 킬 시 Combo 3 복구
                            }
                        }
                    }
                    break;
                case TargetType.AllEnemies:
                    // 기본 광역 타격
                    foreach (var enemy in _enemies)
                        if (enemy.IsAlive) _skillExecutor.ExecuteSkillInternal(caster, skill, enemy, instance, 1f);

                    // Explosion: 광역 후 무작위 N명 추가 타격
                    int allEnemyExplosionN = instance?.GetBehaviorRank(BehaviorKeyword.Explosion) ?? 0;
                    if (allEnemyExplosionN > 0)
                    {
                        var aliveForExplosion = _enemies.FindAll(e => e.IsAlive);
                        if (aliveForExplosion.Count > 0)
                        {
                            for (int i = 0; i < allEnemyExplosionN; i++)
                            {
                                int idx = UnityEngine.Random.Range(0, aliveForExplosion.Count);
                                _skillExecutor.ExecuteSkillInternal(caster, skill, aliveForExplosion[idx], instance, 1f);
                                aliveForExplosion.RemoveAll(e => !e.IsAlive);
                                if (aliveForExplosion.Count == 0) break;
                            }
                        }
                    }
                    break;
                case TargetType.AllAllies:
                    foreach (var ally in _playerParty)
                        if (ally.IsAlive) _skillExecutor.ExecuteSkillInternal(caster, skill, ally, instance);
                    break;
            }

            // Phase CC: 자원 획득/소모 적용 (스킬 실행 후 — 1회 사용당 1번)
            // Ember/Vengeance/Frost 스킬이 자원을 획득 또는 소모.
            if (caster.Resource != null && caster.IsAlive)
            {
                if (skill.ResourceGainType != ResourceType.None
                    && skill.ResourceGainType == caster.Resource.Resource
                    && skill.ResourceGainAmount > 0)
                    caster.Resource.AddStacks(skill.ResourceGainAmount);
                if (skill.ResourceCostType != ResourceType.None
                    && skill.ResourceCostType == caster.Resource.Resource)
                {
                    // Phase CC: 전량 소모 (Revenge Strike) 또는 고정량 소모
                    if (skill.ConsumeAllResource)
                        caster.Resource.Reset();
                    else if (skill.ResourceCostAmount > 0)
                        caster.Resource.ConsumeStacks(skill.ResourceCostAmount);
                }
            }

            // Phase ARCH-5: 스킬 사용 후 UsesThisBattle 증가
            // (Fatigue/Momentum/Escalation/Mastery의 다음 사용 EffectivePower/Cost에 반영)
            instance?.IncrementUsesThisBattle();

            CheckBattleEnd();
            return CurrentPhase == TurnPhase.BattleEnd;
        }

        public void ConfirmActions()
        {
            if (CurrentPhase != TurnPhase.PlayerAction) return;
            _context.SetPhase(TurnPhase.Execution);

            // Phase CC-2F: Mortis(Necromancer) 시체 자동 행동 — 플레이어 턴 종료 후 적 턴 전.
            // 시체가 무작위 슬롯 스킬 1개를 무작위 적에게 시전. Soul Link 회복 적용.
            if (CurrentPhase != TurnPhase.BattleEnd)
                ProcessCorpseAction();

            // ★ Phase GF (2026-07-22): 플레이어 턴 종료 처리 — 적 턴 진입 전.
            // 플레이어 상태이상 감소 (Poison 2턴 등이 정확히 2번의 플레이어 턴에만 적용).
            // 기존에는 ProcessTurnEnd가 적 턴 종료 시에만 호출되어 양 진영 모두 1번씩 감소.
            // 이제 플레이어는 자기 턴 종료 시, 적은 자기 턴 종료 시 각각 감소 (StS 표준).
            if (CurrentPhase != TurnPhase.BattleEnd)
                ProcessPlayerTurnEnd();

            if (CurrentPhase != TurnPhase.BattleEnd)
                StartEnemyTurn();
        }

        /// <summary>
        /// Phase CC-2F: 파티의 Necromancer 시체들이 매 턴 종료 후 자동 행동.
        /// 시체는 무작위 슬롯 스킬을 무작위 살아있는 적에게 시전.
        /// Soul Link 활성화 시 시체가 준 데미지의 Mul%를 Necromancer HP 회복.
        /// </summary>
        private void ProcessCorpseAction()
        {
            // Phase CC-2F: 직전 턴에서 죽은 적들로부터 시체 스킬 자동 교체
            ProcessPendingCorpseSkillSwaps();

            foreach (var member in _playerParty)
            {
                if (member?.Corpse == null || !member.Corpse.IsActive || !member.IsAlive) continue;

                var (slotIdx, skill) = member.Corpse.GetRandomSkillWithIndex();
                if (skill == null) continue;

                // 무작위 살아있는 적 선택
                var aliveEnemies = _enemies.FindAll(e => e.IsAlive);
                if (aliveEnemies.Count == 0) break;
                var target = aliveEnemies[UnityEngine.Random.Range(0, aliveEnemies.Count)];

                // EmpowerNext/MassEmpower 보정 — powerMultiplier로 변환
                int bonus = member.Corpse.MassEmpowerBonus + member.Corpse.ConsumeEmpowerNext();
                float powerMul = 1f;
                if (bonus > 0 && skill.Power > 0)
                    powerMul = (float)(skill.Power + bonus) / skill.Power;

                int hpBefore = target.Health.CurrentHP;
                _skillExecutor.ExecuteSkillInternal(member, skill, target, null, powerMul);
                int damageDealt = hpBefore - target.Health.CurrentHP;
                if (damageDealt < 0) damageDealt = 0;

                // Soul Link — 시체가 준 데미지의 Mul%를 Necromancer 회복
                int soulLinkHeal = 0;
                if (damageDealt > 0 && member.Corpse.SoulLinkRemainingTurns > 0)
                {
                    soulLinkHeal = System.Math.Max(1, (int)(damageDealt * member.Corpse.GetSoulLinkMultiplier()));
                    member.Health.Heal(soulLinkHeal);
                    CombatEventBus.FireHealApplied(member, soulLinkHeal);
                }

                // Phase CC-2F: UI 피드백 이벤트 발생
                OnCorpseAction?.Invoke(member, slotIdx, skill, target, damageDealt, soulLinkHeal);

                // Soul Link 턴 감소
                member.Corpse.TickSoulLink();

                // 시체 행동 후 전투 종료 체크
                if (IsBattleEndedEarly()) break;
            }
        }

        /// <summary>
        /// Phase CC-2F: 적 처치 시 시체 스킬 자동 교체 처리.
        /// 모달 UI 없이 자동 — 죽은 적의 스킬 중 무작위 1개를 시체 슬롯 무작위 1개에 교체.
        /// 사유: 모달 동기화가 복잡하여 자동 교체로 단순화. 추후 모달 UI 확장 가능.
        /// </summary>
        private void ProcessPendingCorpseSkillSwaps()
        {
            if (_pendingKilledEnemies.Count == 0) return;

            foreach (var necromancer in _playerParty)
            {
                if (necromancer?.Corpse == null || !necromancer.Corpse.IsActive) continue;

                foreach (var killed in _pendingKilledEnemies)
                {
                    if (killed?.SkillInventory?.Skills == null) continue;
                    var enemySkills = killed.SkillInventory.Skills;
                    if (enemySkills.Count == 0) continue;

                    // 무작위 적 스킬 1개 선택
                    int skillIdx = UnityEngine.Random.Range(0, enemySkills.Count);
                    var newSkill = enemySkills[skillIdx];
                    if (newSkill == null) continue;

                    // 무작위 시체 슬롯 교체
                    int slotIdx = UnityEngine.Random.Range(0, CorpseComponent.CORPSE_SLOT_COUNT);
                    var oldSkill = necromancer.Corpse.Slots[slotIdx];
                    necromancer.Corpse.ReplaceSlot(slotIdx, newSkill);

                    // Phase CC-2F: UI 피드백 — 스킬 교체 알림
                    OnCorpseSkillSwapped?.Invoke(necromancer, slotIdx, oldSkill, newSkill);
                }
            }

            _pendingKilledEnemies.Clear();
        }

        public void StartEnemyTurn()
        {
            _context.SetPhase(TurnPhase.EnemyTurn);

            // ★ Phase GF (2026-07-22): 적 턴 시작 시 적 쉴드 리셋.
            // 적이 자기 턴에 건 쉴드는 다음 플레이어 턴까지 유지 → 이번 적 턴 시작에 리셋.
            foreach (var c in _enemies) if (c.IsAlive) c.Health.ResetShield();

            // ★ Phase CC (2026-07-22): 적 특성(Regenerate, Sturdy, PhaseShift, Provoke 등) 발동.
            // 기존 StartNewTurn(플레이어 턴 시작)에서 발동했으나 적 턴 시작으로 이동.
            // 이제 적 턴 내에서 재생(시작) → 행동 → Charge 도트(종료)가 모두 일어남.
            foreach (var c in _enemies) if (c.IsAlive) c.TraitHandler.OnTurnStart(_context.TurnNumber);

            if (_sequentialEnemyTurn)
            {
                // 코루틴에게 제어 위임 — 동기 실행하지 않음 (런타임 시각화용)
                OnEnemyTurnSequenceStarted?.Invoke();
                return;
            }

            ExecuteEnemyActions(); // 기존 동기 경로 (시뮬레이터)
        }

        /// <summary>순차 적 턴 모드 활성화 — BattleSceneSetup에서 호출.</summary>
        public void EnableSequentialEnemyTurn() => _sequentialEnemyTurn = true;

        /// <summary>
        /// 코루틴에서 적 한 명 행동 실행. 하이라이트 후 호출됨.
        /// ★ Phase GF (2026-07-22): CC (Stun/Freeze/Sleep) 보유 시 행동 스킵 — OnEnemyActing 호출하지 않음.
        /// </summary>
        public void ExecuteSingleEnemyAction(EnemyAIController controller)
        {
            if (controller == null || !controller.Owner.IsAlive) return;

            // CC 행동 불가 — 하이라이트 없이 스킵 (의도는 유지하되 시각적으로 자연스럽게 넘어감)
            if (controller.Owner.StatusEffects.IsIncapacitated) return;

            OnEnemyActing?.Invoke(controller.Owner);
            controller.ExecuteAction();
        }

        /// <summary>
        /// 코루틴 종료 후 호출 — ProcessTurnEnd + CheckBattleEnd + StartNewTurn.
        /// ExecuteEnemyActions의 후반부와 동일한 로직.
        /// </summary>
        public void CompleteEnemyTurn()
        {
            // ★ Phase GF (2026-07-22): 적 턴 종료 처리 (양 진영이 아닌 적만).
            ProcessEnemyTurnEnd();
            CheckBattleEnd();

            if (CurrentPhase != TurnPhase.BattleEnd)
                StartNewTurn();
        }

        /// <summary>
        /// 매 행동 후 전멸 조기 종료 체크 (코루틴이 사용).
        /// </summary>
        public bool IsBattleEndedEarly()
        {
            return _playerParty.TrueForAll(p => p.IsDead) || _enemies.TrueForAll(e => e.IsDead);
        }

        private void ExecuteEnemyActions()
        {
            // AI 컨트롤러가 있으면 패턴 기반 행동 사용
            if (_enemyControllers.Count > 0)
            {
                foreach (var controller in _enemyControllers)
                {
                    // ★ Phase GF: CC 보유 시 행동 스킵
                    if (controller.Owner.IsAlive && !controller.Owner.StatusEffects.IsIncapacitated)
                        controller.ExecuteAction();
                }
            }
            else
            {
                // 폴백: AI 없으면 기본 공격
                foreach (var enemy in _enemies)
                {
                    // ★ Phase GF: CC 보유 시 행동 스킵
                    if (enemy.IsAlive && !enemy.StatusEffects.IsIncapacitated)
                        ExecuteFallbackEnemyAction(enemy);
                }
            }

            // ★ Phase GF (2026-07-22 P0-2 수정): 적 턴 종료 처리만 —
            // 플레이어는 이미 ConfirmActions에서 ProcessPlayerTurnEnd 처리됨.
            // 기존 ProcessTurnEnd() (양쪽)는 플레이어 상태이상/자원 효과가 2배 적용되는 회귀가 있었음.
            ProcessEnemyTurnEnd();
            CheckBattleEnd();

            if (CurrentPhase != TurnPhase.BattleEnd)
                StartNewTurn();
        }

        private void ExecuteFallbackEnemyAction(Character enemy)
        {
            var alivePlayers = _playerParty.FindAll(p => p.IsAlive);
            if (alivePlayers.Count == 0) return;

            // Taunt 상태인 캐릭터가 있으면 우선 타겟
            var tauntTarget = alivePlayers.Find(p => p.StatusEffects.HasEffect(StatusEffectType.Taunt));
            var target = tauntTarget ?? alivePlayers[UnityEngine.Random.Range(0, alivePlayers.Count)];
            DamageCalculator.DealDamage(enemy, target);
        }

        /// <summary>
        /// 턴 종료 처리 — 양 진영 모두 (시뮬레이터 비순차 모드 전용).
        /// ★ Phase GF (2026-07-22): 진영별 분리 — ProcessPlayerTurnEnd / ProcessEnemyTurnEnd로 분리됨.
        /// 런타임(순차 모드)에서는 분리된 함수들을 별도 시점에 호출하여 CC duration이 정확히 적용.
        /// </summary>
        private void ProcessTurnEnd()
        {
            ProcessPlayerTurnEnd();
            ProcessEnemyTurnEnd();
        }

        /// <summary>
        /// ★ Phase GF (2026-07-22): 플레이어 턴 종료 처리.
        /// 플레이어 행동 종료 직후(ConfirmActions) 호출 — 플레이어 상태이상 감소.
        /// 적 턴 중에도 플레이어 상태이상은 감소하지 않음 (StS 표준).
        /// </summary>
        private void ProcessPlayerTurnEnd()
        {
            foreach (var c in _playerParty) if (c.IsAlive) c.OnTurnEnd();

            // Phase CC: 캐릭터 고유 자원 턴 종료 훅 (Ashe Ember×2 자해, Lumi Frost 절반 소실 등)
            foreach (var c in _playerParty)
                if (c.IsAlive && c.Resource != null) c.Resource.OnTurnEnd(c);

            // 만료된 효과 제거 후 스탯 수정자 재계산 (플레이어만)
            foreach (var c in _playerParty) if (c.IsAlive) c.ApplyStatModifiers();

            // ★ Phase GF (2026-07-22 P0-1 수정): FireTurnEnd — StS 표준 "플레이어 턴 종료 시" 발생.
            // RelicHandler/CharacterTraitHandler OnTurnEnd 트리거가 적 턴 진입 전에 정상 작동.
            // 기존에는 ProcessEnemyTurnEnd에 있어서 "적 턴 종료 시"에만 발생하는 회귀가 있었음.
            CombatEventBus.FireTurnEnd();
        }

        /// <summary>
        /// ★ Phase GF (2026-07-22): 적 턴 종료 처리.
        /// 적 행동 종료 직후(CompleteEnemyTurn) 호출 — 적 상태이상 감소 + Charge 연쇄.
        /// </summary>
        private void ProcessEnemyTurnEnd()
        {
            foreach (var c in _enemies) if (c.IsAlive) c.OnTurnEnd();

            // ★ Phase CC (2026-07-22 재설계): Taranis Charge 도트 — 단순화.
            // 규칙: Charge 보유 적이 매 턴 종료 시 고정 3 데미지를 자기 자신에게 받음.
            // 도트 후 Charge -1 (0이면 소멸). Charge 스택 = 지속 턴 수.
            // 기존 연쇄/광역/자폭 분기 제거. 단순하게 "자기 자신에게 3".
            const int ChargeDamage = 3;
            var chargedEnemies = _enemies.FindAll(e => e.IsAlive && e.StatusEffects.HasEffect(StatusEffectType.Charge));

            foreach (var enemy in chargedEnemies)
            {
                enemy.Health.TakeDamage(ChargeDamage);
                CombatEventBus.FireDamageReceived(enemy, ChargeDamage);
            }

            // Charge -1 (자연 소멄)
            var toRemove = new List<Character>();
            foreach (var enemy in chargedEnemies)
            {
                foreach (var eff in enemy.StatusEffects.GetAllEffects())
                {
                    if (eff.Type != StatusEffectType.Charge) continue;
                    eff.Value -= 1;
                    if (eff.Value <= 0) toRemove.Add(enemy);
                    break;
                }
            }
            foreach (var enemy in toRemove)
                enemy.StatusEffects.RemoveEffect(StatusEffectType.Charge);

            // 만료된 효과 제거 후 스탯 수정자 재계산 (적만 — 플레이어는 ProcessPlayerTurnEnd에서)
            foreach (var c in _enemies) if (c.IsAlive) c.ApplyStatModifiers();

            // ★ Phase GF (2026-07-22 P0-1): FireTurnEnd 제거 — ProcessPlayerTurnEnd로 이동.
            // CombatEventBus.FireTurnEnd();
        }

        private void CheckBattleEnd()
        {
            bool allPlayersDead = _playerParty.TrueForAll(p => p.IsDead);
            bool allEnemiesDead = _enemies.TrueForAll(e => e.IsDead);

            if (allPlayersDead || allEnemiesDead)
            {
                // Phase 8C: 플레이어 장착 특성 처리기 정리
                foreach (var c in _playerParty)
                {
                    if (c.PlayerTraitHandler != null && c.PlayerTraitHandler.HasTrait)
                        c.PlayerTraitHandler.UnsubscribeEvents();
                }
                _context.SetPhase(TurnPhase.BattleEnd);
                CombatEventBus.FireBattleEnd(allEnemiesDead);
                // Phase CC-2F: 구독 해제 + 큐 비우기
                CombatEventBus.OnKill -= OnEnemyKilledForCorpse;
                _pendingKilledEnemies.Clear();
                CombatEventBus.Clear();
                DamageCalculator.ClearEvents();
                SkillExecutor.ClearEvents();
                OnBattleEnded?.Invoke();
            }
        }
    }
}
