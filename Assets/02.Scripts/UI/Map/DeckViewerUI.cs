using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;
using TeamLog.Map;
using TeamLog.Reward;

namespace TeamLog.UI.Map
{
    /// <summary>
    /// 파티 전체 스킬/유물 뷰어 — 전체 화면 오버레이
    /// 캐릭터별 스킬 목록 + 획득 유물 목록
    /// </summary>
    public class DeckViewerUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform _contentContainer;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TextMeshProUGUI _titleLabel;

        private GameRunState _runState;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Hide);
            _canvasGroup = UIAnimationHelper.EnsureCanvasGroup(gameObject);
        }

        public void Initialize(GameRunState runState)
        {
            _runState = runState;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                UIAnimationHelper.FadeIn(_canvasGroup);
            }

            if (_titleLabel != null)
                _titleLabel.text = "덱 조회";

            Refresh();
        }

        public void Hide()
        {
            _canvasGroup = UIAnimationHelper.EnsureCanvasGroup(gameObject);
            UIAnimationHelper.FadeOut(_canvasGroup);
        }

        private void Refresh()
        {
            if (_contentContainer == null || _runState == null) return;

            ClearContent();

            var font = _titleLabel?.font;

            // 캐릭터별 스킬
            foreach (var member in _runState.PlayerParty)
            {
                if (member == null) continue;
                CreateHeader(member.Data.CharacterName, font);

                var skills = member.SkillInventory.SkillInstances;
                if (skills.Count == 0)
                {
                    CreateLabel("  (스킬 없음)", font, new Color(0.6f, 0.6f, 0.6f));
                    continue;
                }

                foreach (var skillInst in skills)
                {
                    var data = skillInst.Data;
                    string augmentStr = skillInst.Augments.Count > 0
                        ? $" [{string.Join(", ", skillInst.Augments.Select(a => a.Data.AugmentName))}]"
                        : "";
                    string info = $"  {data.SkillName}{augmentStr}  |  위력 {skillInst.EffectivePower}  |  비용 {skillInst.EffectiveCost}  |  가중치 {skillInst.EffectiveWeight}";
                    CreateLabel(info, font, Color.white);
                }
            }

            // 유물
            if (_runState.RelicHandler.Relics.Count > 0)
            {
                CreateHeader("유물", font);
                foreach (var relic in _runState.RelicHandler.Relics)
                    CreateLabel($"  {relic.RelicName} — {relic.Description}", font, new Color(1f, 0.85f, 0.4f));
            }
        }

        private void CreateHeader(string text, TMP_FontAsset font)
        {
            var obj = new GameObject("Header");
            obj.transform.SetParent(_contentContainer, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 32);

            var bg = obj.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.12f, 0.25f, 0.9f);

            var labelObj = new GameObject("T");
            labelObj.transform.SetParent(obj.transform, false);
            var tmp = labelObj.AddComponent<TextMeshProUGUI>();
            SetFillParent(labelObj.GetComponent<RectTransform>());
            tmp.font = font;
            tmp.text = text;
            tmp.fontSize = 18;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(0.96f, 0.82f, 0.25f);
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.margin = new Vector4(12, 0, 0, 0);
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;
        }

        private void CreateLabel(string text, TMP_FontAsset font, Color color)
        {
            var obj = new GameObject("Label");
            obj.transform.SetParent(_contentContainer, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 24);
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = text;
            tmp.fontSize = 15;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.margin = new Vector4(12, 0, 0, 0);
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;
        }

        private void ClearContent()
        {
            if (_contentContainer == null) return;
            for (int i = _contentContainer.childCount - 1; i >= 0; i--)
                Destroy(_contentContainer.GetChild(i).gameObject);
        }

        private static void SetFillParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(Hide);
        }
    }
}
