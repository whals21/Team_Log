using TeamLog.Map;
using UnityEngine;

namespace TeamLog.UI.Map
{
    /// <summary>
    /// MapNodeButton에 노드 참조를 저장하기 위한 헬퍼 컴포넌트
    /// </summary>
    public class MapNodeReference : MonoBehaviour
    {
        public MapNode Node { get; set; }
    }
}
