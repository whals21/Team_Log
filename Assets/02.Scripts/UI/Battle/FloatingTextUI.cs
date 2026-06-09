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
        public static Color ShieldColor => UIPalette.Default.ShieldColor;
    }
}
