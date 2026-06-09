namespace TeamLog.Skill
{
    /// <summary>
    /// 증강 런타임 인스턴스 — AugmentData(SO 템플릿)의 런타임 래퍼
    /// </summary>
    public class AugmentInstance
    {
        public AugmentData Data { get; }

        public AugmentInstance(AugmentData data)
        {
            Data = data;
        }
    }
}
