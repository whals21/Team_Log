using System.Collections.Generic;
using TeamLog.Characters;
using UnityEngine;

namespace TeamLog.Combat.AI
{
    /// <summary>
    /// 적 AI 행동 패턴 데이터 (ScriptableObject)
    /// CSV(EnemyPatternTable)에서 skillId를 참조하여 SkillData 에셋과 연결
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyPatternData", menuName = "TeamLog/Enemy Pattern Data")]
    public class EnemyPatternData : ScriptableObject
    {
        [SerializeField] private string _enemyId;
        [SerializeField] private List<SkillData> _skills = new List<SkillData>();

        public string EnemyId => _enemyId;
        public IReadOnlyList<SkillData> Skills => _skills;

        /// <summary>
        /// 이 데이터로부터 EnemyActionPattern 런타임 객체 생성
        /// </summary>
        public EnemyActionPattern CreateRuntimePattern()
        {
            return new EnemyActionPattern(_skills);
        }
    }
}
