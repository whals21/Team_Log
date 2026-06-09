using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TeamLog.Map
{
    /// <summary>
    /// 런 간 영구 통계 — JsonUtility 직렬화용
    /// </summary>
    [System.Serializable]
    public class MetaSaveData
    {
        public int TotalRuns;
        public int Victories;
        public int BestFloor;
        public int TotalGoldEarned;
        public bool HasPendingRun;
        public bool HasCompletedTutorial;

        // 캐릭터 잠금해제 상태
        public List<string> UnlockedCharacterIds = new();
    }
}
