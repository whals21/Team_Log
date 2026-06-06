using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;
using TeamLog.Map;

namespace TeamLog.UI.Map
{
    /// <summary>
    /// 스킬 강화 UI — 파티 전체 스킬 목록에서 업그레이드 가능 스킬 선택
    /// </summary>
    public class SkillUpgradeUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private GameObject _skillSlotPrefab;

        private System.Action _onComplete;

        private void Awake()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(OnClose);
        }

        public void Initialize(System.Action onComplete)
        {
            _onComplete = onComplete;
            _panel.SetActive(false);
        }

        public void Show(GameRunState runState)
        {
            _panel.SetActive(true);
            BuildSkillList(runState);
        }

        private void BuildSkillList(GameRunState runState)
        {
            ClearSlots();

            if (_titleLabel != null)
                _titleLabel.text = "스킬 강화";

            foreach (var member in runState.PlayerParty)
            {
                if (!member.IsAlive) continue;

                foreach (var instance in member.SkillInventory.SkillInstances)
                {
                    if (!instance.CanUpgrade) continue;

                    var slotObj = CreateSlot(instance, member);
                    slotObj.transform.SetParent(_slotContainer, false);
                }
            }
        }

        private GameObject CreateSlot(SkillInstance instance, Character owner)
        {
            var obj = new GameObject("UpgradeSlot");
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 50);

            var bg = obj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.25f);

            var btn = obj.AddComponent<Button>();
            btn.targetGraphic = bg;

            // 스킬 이름 + 레벨
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = $"{owner.Name}: {instance.Data.SkillName} Lv.{instance.UpgradeLevel} → Lv.{instance.UpgradeLevel + 1}";
            tmp.fontSize = 16;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Left;
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.05f, 0);
            textRect.anchorMax = new Vector2(0.95f, 1);
            textRect.sizeDelta = Vector2.zero;

            btn.onClick.AddListener(() =>
            {
                instance.Upgrade();
                ToastUI.Show($"{instance.Data.SkillName} 강화! Lv.{instance.UpgradeLevel}");
                _onComplete?.Invoke();
                Hide();
            });

            return obj;
        }

        private void OnClose()
        {
            Hide();
            _onComplete?.Invoke();
        }

        private void Hide()
        {
            _panel.SetActive(false);
        }

        private void ClearSlots()
        {
            if (_slotContainer == null) return;
            for (int i = _slotContainer.childCount - 1; i >= 0; i--)
                Destroy(_slotContainer.GetChild(i).gameObject);
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(OnClose);
        }
    }
}
