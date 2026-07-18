using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;
using TeamLog.UI;

namespace TeamLog.UI.PartySelection
{
    /// <summary>
    /// 자원 메커니즘 박스 (UI-B.4) — 웹 목업의 초상화 하단 자원 설명 박스.
    /// RESOURCE MECHANIC 타이틀 + 자원별 메커니즘 설명.
    /// 태그 지원: &lt;b&gt;bold&lt;/b&gt; (자원색 강조).
    ///
    /// 레이아웃:
    /// MechanicBox (Image — 9-slice ParchmentDark, 골드 테두리)
    /// ├── Title (TMP — "◈ RESOURCE MECHANIC", Cinzel 작게)
    /// └── Desc (TMP — 본문, &lt;b&gt; 강조)
    /// </summary>
    public class ResourceMechanicBox : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _background;
        [SerializeField] private Sprite _boxSprite;        // ParchmentDark_9Slice

        [Header("Border")]
        [SerializeField] private Image _borderHighlight;   // 자원색 테두리 (선택)

        [Header("Content")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descText;

        /// <summary>
        /// 자원 타입 + 캐릭터 ID로 초기화.
        /// Mortis/Cael은 ResourceType enum에 없으므로 charId로 처리.
        /// </summary>
        public void Initialize(ResourceType type, string charId = null)
        {
            Color resourceColor = PartySelectionUIUtils.GetResourceColorByCharId(charId, type);
            string mechanicText = PartySelectionUIUtils.GetResourceMechanicText(type, charId);
            string resLabel = ResolveResourceLabel(charId, type);

            Render(resourceColor, resLabel, mechanicText);
        }

        /// <summary>
        /// CharacterDisplayData로 초기화 (포함 정보 모두 활용).
        /// </summary>
        public void Initialize(CharacterDisplayData data)
        {
            if (data == null)
            {
                Clear();
                return;
            }
            Render(data.ResourceColor, data.ResourceLabel, data.ResourceMechanicText);
        }

        private void Render(Color resourceColor, string resourceLabel, string mechanicText)
        {
            var palette = UIPalette.Default;

            // 배경
            if (_background != null)
            {
                _background.sprite = _boxSprite;
                _background.color = Color.white;
            }

            // 타이틀 — "◈ EMBER MECHANIC"
            if (_titleText != null)
            {
                string label = string.IsNullOrEmpty(resourceLabel) || resourceLabel == "—"
                    ? "RESOURCE"
                    : resourceLabel;
                _titleText.text = $"◈  {label}  MECHANIC";
                _titleText.color = palette.DFGold;
            }

            // 설명 (태그 가공 — <b>를 자원색 <color> 태그로 변환)
            if (_descText != null)
            {
                string processed = ProcessRichText(mechanicText, resourceColor);
                _descText.text = processed;
                _descText.color = palette.DFParchment;
            }

            // 자원색 테두리 하이라이트
            if (_borderHighlight != null)
            {
                _borderHighlight.color = new Color(resourceColor.r, resourceColor.g, resourceColor.b, 0.4f);
            }
        }

        /// <summary>
        /// &lt;b&gt; 태그를 자원색 &lt;color&gt; 태그로 변환.
        /// TMP rich text 지원.
        /// </summary>
        private string ProcessRichText(string text, Color color)
        {
            if (string.IsNullOrEmpty(text)) return "";
            string hex = ColorUtility.ToHtmlStringRGB(color);
            // <b>...</b> → <color=#hex><b>...</b></color>
            return text.Replace("<b>", $"<color=#{hex}><b>")
                       .Replace("</b>", "</b></color>");
        }

        private string ResolveResourceLabel(string charId, ResourceType type)
        {
            if (!string.IsNullOrEmpty(charId))
            {
                string id = charId.ToLowerInvariant();
                if (id.Contains("mortis")) return "CORPSE";
                if (id.Contains("cael"))   return "DISCOVER";
            }
            return PartySelectionUIUtils.GetResourceLabel(type);
        }

        private void Clear()
        {
            if (_titleText != null) _titleText.text = "";
            if (_descText != null) _descText.text = "";
            if (_borderHighlight != null) _borderHighlight.color = Color.clear;
        }
    }
}
