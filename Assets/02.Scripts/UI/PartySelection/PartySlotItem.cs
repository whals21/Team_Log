using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.UI;

namespace TeamLog.UI.PartySelection
{
    /// <summary>
    /// 개별 파티 슬롯 항목 (UI-B.7).
    /// ★ 별도 파일로 분리됨 — PartySlotPanel.cs 안에 정의하면 Unity가 씬 저장/로드 시
    ///   컴포넌트 참조를 깨뜨리는 문제("Missing Script")가 발생함.
    /// 채워지면 자원색 원형 + 이니셜, 비면 점선 원 + '+'. 슬롯 번호 배지.
    /// </summary>
    public class PartySlotItem : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private Button _button;

        [Header("Visuals")]
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _slotNumberText;
        [SerializeField] private Image _contentImage;
        [SerializeField] private TMP_Text _initialText;

        [Header("Sprites")]
        [SerializeField] private Sprite _emptySprite;   // 점선 원
        [SerializeField] private Sprite _filledSprite;  // 채워진 원 (자원색)

        private int _index;
        private CharacterDisplayData _data;
        private Action<int> _onClicked;

        public CharacterDisplayData Data => _data;
        public bool IsFilled => _data != null;

        public void Initialize(int index, Action<int> onClicked)
        {
            _index = index;
            _onClicked = onClicked;

            // ★ Button 자동 보완
            if (_button == null)
            {
                _button = GetComponent<Button>();
                if (_button == null)
                {
                    _button = gameObject.AddComponent<Button>();
                    Debug.Log($"[PartySlotItem] Auto-added Button to slot {index}");
                }
            }
            // ★ targetGraphic 보완 — Background Image가 클릭 감지
            if (_button.targetGraphic == null && _background != null)
            {
                _background.raycastTarget = true;
                _button.targetGraphic = _background;
            }

            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(OnClicked);
            }

            // ★ 자식 Image raycastTarget=false 강제 (버튼 클릭 가로채기 방지)
            var images = GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img == _background) continue;  // 클릭 감지용 유지
                img.raycastTarget = false;
            }

            if (_slotNumberText != null)
                _slotNumberText.text = (index + 1).ToString();

            Clear();
        }

        public void SetData(CharacterDisplayData data)
        {
            _data = data;
            Render();
        }

        public void Clear()
        {
            _data = null;
            Render();
        }

        private void Render()
        {
            var palette = UIPalette.Default;

            if (_data == null)
            {
                // 빈 슬롯
                if (_background != null)
                {
                    _background.sprite = _emptySprite;
                    _background.color = palette.DFVoid;
                }
                if (_contentImage != null)
                {
                    _contentImage.color = Color.clear;
                }
                if (_initialText != null)
                {
                    _initialText.text = "+";
                    _initialText.color = palette.DFInkFaint;
                }
            }
            else
            {
                // 채워진 슬롯
                Color resColor = _data.ResourceColor;
                if (_background != null)
                {
                    _background.sprite = _filledSprite;
                    _background.color = resColor;
                }
                if (_contentImage != null)
                {
                    if (_data.ResourceBadgeSprite != null)
                    {
                        _contentImage.sprite = _data.ResourceBadgeSprite;
                        _contentImage.color = Color.white;
                    }
                    else
                    {
                        _contentImage.color = new Color(resColor.r, resColor.g, resColor.b, 0.3f);
                    }
                }
                if (_initialText != null)
                {
                    _initialText.text = _data.Initial ?? "?";
                    _initialText.color = Color.white;
                }
            }
        }

        private void OnClicked()
        {
            _onClicked?.Invoke(_index);
        }
    }
}
