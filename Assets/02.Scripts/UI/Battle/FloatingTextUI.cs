using System.Collections;
using UnityEngine;
using TMPro;
using TeamLog.UI;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 피해량/힐량 위로 떠오르는 플로팅 텍스트
    /// </summary>
    public class FloatingTextUI : MonoBehaviour
    {
        [SerializeField] private float _duration = 1.2f;
        [SerializeField] private float _riseHeight = 60f;

        private TextMeshProUGUI _text;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Show(string message, Color color, Vector2 anchoredPosition)
        {
            if (_text == null) _text = GetComponent<TextMeshProUGUI>();
            if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();

            _text.text = message;
            _text.color = color;
            _rectTransform.anchoredPosition = anchoredPosition;
            gameObject.SetActive(true);

            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            float elapsed = 0f;
            Vector2 start = _rectTransform.anchoredPosition;
            Vector2 end = start + Vector2.up * _riseHeight;

            while (elapsed < _duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / _duration;

                _rectTransform.anchoredPosition = Vector2.Lerp(start, end, t);

                // 마지막 40%에서 페이드 아웃
                if (t > 0.6f)
                {
                    Color c = _text.color;
                    c.a = 1f - (t - 0.6f) / 0.4f;
                    _text.color = c;
                }

                yield return null;
            }

            Destroy(gameObject);
        }

        // ── Static Factory ──

        public static FloatingTextUI Spawn(Transform parent, string message, Color color, Vector2 position)
        {
            var go = new GameObject("FloatingText");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 40);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 24;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            // 폰트: UIKoreanFont로 명시적 할당
            UIKoreanFont.EnsureFont(tmp);

            var floating = go.AddComponent<FloatingTextUI>();
            floating.Show(message, color, position);
            return floating;
        }

        public static Color DamageColor => UIPalette.Default.DamageColor;
        public static Color HealColor => UIPalette.Default.HealColor;
        public static Color ShieldColor => UIPalette.Default.ShieldBrown;

        /// <summary>★ 2026-08-02 P1-4: 크리티컬 데미지 전용 색 (금색).</summary>
        public static Color CriticalColor => new Color(1.0f, 0.84f, 0.20f);

        /// <summary>★ 2026-08-02 P1-4: 크리티컬 플로팅 텍스트 — 금색 + 대형 폰트 + "CRIT!" 접두.</summary>
        public static FloatingTextUI SpawnCritical(Transform parent, int damage, Vector2 position)
        {
            var go = new GameObject("FloatingText_Critical");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280, 56);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 36;                  // 일반 24의 1.5배
            tmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            UIKoreanFont.EnsureFont(tmp);

            var floating = go.AddComponent<FloatingTextUI>();
            floating._duration = 1.5f;          // 일반 1.2초보다 0.3초 길게
            floating._riseHeight = 80f;         // 더 높이 떠오름
            floating.Show($"CRIT! -{damage}", CriticalColor, position);
            return floating;
        }
    }
}
