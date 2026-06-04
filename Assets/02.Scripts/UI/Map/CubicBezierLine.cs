using UnityEngine;
using UnityEngine.UI;

namespace TeamLog.UI.Map
{
    /// <summary>
    /// 3차 베지에 곡선 렌더러 — 맵 노드 간 S자 곡선 연결선
    /// </summary>
    public class CubicBezierLine : Graphic
    {
        [SerializeField] private int _segments = 20;
        [SerializeField] private float _thickness = 3f;

        private Vector2 _start;
        private Vector2 _end;
        private Color _lineColor = new Color(0.5f, 0.5f, 0.6f, 0.5f);

        public void SetPoints(Vector2 start, Vector2 end, Color color)
        {
            _start = start;
            _end = end;
            _lineColor = color;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (_thickness <= 0) return;

            // 제어점: S자 곡선
            float midY = (_start.y + _end.y) * 0.5f;
            Vector2 cp1 = new Vector2(_start.x, midY);
            Vector2 cp2 = new Vector2(_end.x, midY);

            // 곡선 위의 점들 계산
            var points = new Vector2[_segments + 1];
            for (int i = 0; i <= _segments; i++)
            {
                float t = (float)i / _segments;
                points[i] = CubicBezier(_start, cp1, cp2, _end, t);
            }

            // 두께가 있는 선으로 삼각형 생성
            float halfThickness = _thickness * 0.5f;
            var color32 = (Color32)_lineColor;

            for (int i = 0; i < _segments; i++)
            {
                Vector2 dir = (points[i + 1] - points[i]).normalized;
                Vector2 normal = new Vector2(-dir.y, dir.x) * halfThickness;

                int idx = vh.currentVertCount;
                vh.AddVert(points[i] + normal, color32, Vector2.zero);
                vh.AddVert(points[i] - normal, color32, Vector2.zero);
                vh.AddVert(points[i + 1] + normal, color32, Vector2.zero);
                vh.AddVert(points[i + 1] - normal, color32, Vector2.zero);

                vh.AddTriangle(idx, idx + 1, idx + 2);
                vh.AddTriangle(idx + 2, idx + 1, idx + 3);
            }
        }

        private static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1f - t;
            float uu = u * u;
            float uuu = uu * u;
            float tt = t * t;
            float ttt = tt * t;

            return uuu * p0 + 3f * uu * t * p1 + 3f * u * tt * p2 + ttt * p3;
        }
    }
}
