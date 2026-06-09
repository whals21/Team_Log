using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Characters;
using TeamLog.Map;
using TeamLog.Skill;

namespace TeamLog.UI
{
    /// <summary>
    /// 증강 적용 패널 — 캐릭터 선택 → 스킬 선택 → 증강 부착 서브플로우
    /// 보상/상점에서 공통 사용
    /// </summary>
    public class AugmentSelectPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private Transform _buttonContainer;
        [SerializeField] private Button _skipButton;

        private AugmentData _pendingAugment;
        private IReadOnlyList<Character> _party;
        private Character _selectedCharacter;
        private GameRunState _runState;
        private System.Action<bool> _onComplete;

        private void Awake()
        {
            if (_skipButton != null)
                _skipButton.onClick.AddListener(OnSkipped);
        }

        /// <summary>
        /// 증강 적용 패널 표시
        /// </summary>
        /// <param name="augment">적용할 증강</param>
        /// <param name="party">파티 목록</param>
        /// <param name="runState">현재 런 상태</param>
        /// <param name="onComplete">완료 콜백 (성공 여부)</param>
        public void Show(AugmentData augment, IReadOnlyList<Character> party,
            GameRunState runState, System.Action<bool> onComplete)
        {
            _pendingAugment = augment;
            _party = party;
            _runState = runState;
            _onComplete = onComplete;
            _selectedCharacter = null;

            ShowCharacterSelect();
        }

        private void ShowCharacterSelect()
        {
            if (_panel != null) _panel.SetActive(true);
            if (_titleLabel != null)
                _titleLabel.text = $"증강 적용: {_pendingAugment.AugmentName}\n캐릭터 선택";
            if (_skipButton != null)
                _skipButton.gameObject.SetActive(true);
            ClearButtons();

            foreach (var member in _party)
            {
                if (!member.IsAlive) continue;
                var captured = member;

                // 호환 스킬이 있는 캐릭터만 표시
                bool hasCompatible = false;
                foreach (var inst in member.SkillInventory.SkillInstances)
                {
                    if (IsCompatible(inst))
                    {
                        hasCompatible = true;
                        break;
                    }
                }
                if (!hasCompatible) continue;

                CreateButton(
                    $"{member.Name} ({member.SkillInventory.Count}스킬)",
                    new Color(0.15f, 0.15f, 0.25f),
                    () =>
                    {
                        AudioManager.Instance?.PlayUIConfirm();
                        _selectedCharacter = captured;
                        ShowSkillSelect(captured);
                    });
            }
        }

        private void ShowSkillSelect(Character member)
        {
            if (_titleLabel != null)
                _titleLabel.text = $"증강 적용: {_pendingAugment.AugmentName}\n스킬 선택";
            if (_skipButton != null)
                _skipButton.gameObject.SetActive(true); // 뒤로가기 역할
            ClearButtons();

            foreach (var inst in member.SkillInventory.SkillInstances)
            {
                if (!IsCompatible(inst)) continue;
                if (inst.HasAugment(_pendingAugment.Type)) continue;
                if (inst.Augments.Count >= SkillInstance.MaxAugments) continue;

                var captured = inst;
                string augmentList = inst.Augments.Count > 0
                    ? $" [{inst.Augments.Count}/{SkillInstance.MaxAugments}]"
                    : "";
                CreateButton($"{inst.Data.SkillName}{augmentList}", new Color(0.12f, 0.15f, 0.22f),
                    () =>
                    {
                        AudioManager.Instance?.PlayUIConfirm();
                        OnSkillSelected(captured);
                    });
            }
        }

        private bool IsCompatible(SkillInstance inst)
        {
            if (_pendingAugment.CompatibleSkillType == SkillType.Attack)
                return true; // All은 Attack으로 처리 (모든 스킬 호환)
            return inst.Data.Type == _pendingAugment.CompatibleSkillType;
        }

        private void OnSkillSelected(SkillInstance targetSkill)
        {
            bool applied = _runState.AcquireAugment(_pendingAugment, _selectedCharacter, targetSkill);
            if (applied)
                ToastUI.Show($"{_selectedCharacter.Name}의 {targetSkill.Data.SkillName}에 {_pendingAugment.AugmentName} 적용!");
            _onComplete?.Invoke(applied);
            Hide();
        }

        private void OnSkipped()
        {
            AudioManager.Instance?.PlayUICancel();
            _onComplete?.Invoke(false);
            Hide();
        }

        public void Hide()
        {
            _pendingAugment = null;
            _selectedCharacter = null;
            ClearButtons();
            if (_panel != null)
                _panel.SetActive(false);
        }

        private void CreateButton(string text, Color bgColor, System.Action onClick)
        {
            var btnObj = new GameObject("AugmentBtn");
            btnObj.transform.SetParent(_buttonContainer, false);
            btnObj.AddComponent<RectTransform>();

            var bg = btnObj.AddComponent<Image>();
            bg.color = bgColor;

            var button = btnObj.AddComponent<Button>();
            button.targetGraphic = bg;
            button.onClick.AddListener(() => onClick());

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 18;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            UIKoreanFont.EnsureFont(tmp);
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var layoutEl = btnObj.AddComponent<LayoutElement>();
            layoutEl.minHeight = 50;
        }

        private void ClearButtons()
        {
            if (_buttonContainer == null) return;
            for (int i = _buttonContainer.childCount - 1; i >= 0; i--)
                Destroy(_buttonContainer.GetChild(i).gameObject);
        }

        private void OnDestroy()
        {
            if (_skipButton != null)
                _skipButton.onClick.RemoveListener(OnSkipped);
        }
    }
}
