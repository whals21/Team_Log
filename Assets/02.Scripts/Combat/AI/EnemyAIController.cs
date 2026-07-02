using System.Collections.Generic;
using TeamLog.Characters;
using TeamLog.Combat.Turn;

// 네임스페이스 충돌 해결
using Character = TeamLog.Characters.Character;
using SkillData = TeamLog.Characters.SkillData;
using SkillType = TeamLog.Characters.SkillType;
using TargetType = TeamLog.Characters.TargetType;
using StatusEffectType = TeamLog.Characters.StatusEffectType;

namespace TeamLog.Combat.AI
{
    /// <summary>
    /// 적 AI 컨트롤러 — SkillData 기반 행동 실행
    /// PrepareNextAction에서 스킬+타겟을 미리 결정
    /// </summary>
    public class EnemyAIController
    {
        private readonly Character _owner;
        private readonly EnemyActionPattern _pattern;
        private readonly List<Character> _players;

        private EnemyIntent _currentIntent;
        private SkillData _nextSkill;
        private List<Character> _nextTargets;

        public Character Owner => _owner;
        public EnemyIntent CurrentIntent => _currentIntent;

        public event System.Action<EnemyIntent> OnIntentChanged;

        public EnemyAIController(Character owner, EnemyActionPattern pattern, List<Character> players)
        {
            _owner = owner;
            _pattern = pattern;
            _players = players;
            _nextTargets = new List<Character>();
        }

        /// <summary>
        /// 턴 시작 시 다음 스킬과 타겟을 결정하여 의도 표시
        /// </summary>
        public void PrepareNextAction()
        {
            _nextSkill = _pattern.GetNextSkill(_owner, _players);
            _nextTargets = ResolveTargets(_nextSkill);
            _currentIntent = EnemyIntent.FromSkill(_nextSkill, _nextTargets, _owner);
            OnIntentChanged?.Invoke(_currentIntent);
        }

        /// <summary>
        /// 준비된 스킬 실행 — 미리 선택된 타겟 사용
        /// </summary>
        public void ExecuteAction()
        {
            if (_nextSkill == null) return;

            foreach (var target in _nextTargets)
                ExecuteSkillInternal(_owner, _nextSkill, target);
        }

        /// <summary>
        /// SkillData.Target에 따라 타겟 리스트를 결정
        /// </summary>
        private List<Character> ResolveTargets(SkillData skill)
        {
            var targets = new List<Character>();

            if (skill == null) return targets;

            switch (skill.Target)
            {
                case TargetType.SingleEnemy:
                    var target = SelectRandomAlivePlayer();
                    if (target != null) targets.Add(target);
                    break;

                case TargetType.AllEnemies:
                    foreach (var player in _players)
                        if (player.IsAlive) targets.Add(player);
                    break;

                case TargetType.Self:
                    targets.Add(_owner);
                    break;

                case TargetType.SingleAlly:
                    targets.Add(_owner);
                    break;

                case TargetType.AllAllies:
                    targets.Add(_owner);
                    break;
            }

            return targets;
        }

        private void ExecuteSkillInternal(Character caster, SkillData skill, Character target)
        {
            switch (skill.Type)
            {
                case SkillType.Attack:
                    DamageCalculator.DealDamage(caster, target, skill.Power);
                    break;
                case SkillType.Shield:
                    target.Health.AddShield(caster, skill.Power);
                    break;
                case SkillType.Heal:
                    target.Health.Heal(skill.Power);
                    break;
                case SkillType.Buff:
                    ApplyEffect(skill, target);
                    break;
                case SkillType.Debuff:
                    ApplyEffect(skill, target);
                    break;
                case SkillType.Purify:
                    target.StatusEffects.ClearAllEffects();
                    target.ApplyStatModifiers();
                    break;
            }
        }

        private void ApplyEffect(SkillData skill, Character target)
        {
            if (skill.StatusEffect != StatusEffectType.None)
            {
                // Shell 특성: 매 턴 첫 상태이상 무효화
                if (target.TraitHandler.ShouldBlockEffect())
                    return;

                target.StatusEffects.ApplyEffect(skill.StatusEffect, skill.EffectDuration, skill.EffectValue);
                target.ApplyStatModifiers();
            }
        }

        private Character SelectRandomAlivePlayer()
        {
            var alive = _players.FindAll(p => p.IsAlive);
            if (alive.Count == 0) return null;

            // Opportunist 특성: 항상 최저 HP 타겟, Taunt 무시
            if (_owner.TraitHandler.HasTrait && _owner.TraitHandler.Trait == EnemyTrait.Opportunist)
            {
                Character lowestHP = null;
                foreach (var p in alive)
                    if (lowestHP == null || p.Health.CurrentHP < lowestHP.Health.CurrentHP)
                        lowestHP = p;
                return lowestHP;
            }

            // Taunt 상태인 캐릭터가 있으면 우선 타겟
            var tauntTarget = alive.Find(p => p.StatusEffects.HasEffect(StatusEffectType.Taunt));
            if (tauntTarget != null)
                return tauntTarget;

            return alive[UnityEngine.Random.Range(0, alive.Count)];
        }
    }
}
