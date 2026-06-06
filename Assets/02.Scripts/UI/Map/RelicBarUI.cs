using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using TeamLog.Map;
using TeamLog.Reward;
using TeamLog.UI.Battle;

namespace TeamLog.UI.Map
{
    /// <summary>
    /// 유물 바 UI — 획득한 유물 아이콘을 하단에 표시, 호버 시 툴팁으로 이름+효과 설명
    /// </summary>
    public class RelicBarUI : MonoBehaviour
    {
        [SerializeField] private Transform _iconContainer;
        [SerializeField] private GameObject _relicIconPrefab;
        [SerializeField] private TextMeshProUGUI _countLabel;

        private GameRunState _runState;
        private readonly List<GameObject> _iconObjects = new();

        public void Initialize(GameRunState runState)
        {
            _runState = runState;
            Refresh();
        }

        public void Refresh()
        {
            if (_runState == null) return;

            ClearIcons();

            var relics = _runState.RelicHandler.Relics;
            foreach (var relic in relics)
            {
                var iconObj = CreateRelicIcon(relic);
                _iconObjects.Add(iconObj);
            }

            if (_countLabel != null)
                _countLabel.text = relics.Count > 0 ? $"유물 {relics.Count}" : "";
        }

        private GameObject CreateRelicIcon(RelicData relic)
        {
            GameObject iconObj;

            if (_relicIconPrefab != null)
            {
                iconObj = Instantiate(_relicIconPrefab, _iconContainer);
            }
            else
            {
                iconObj = new GameObject($"Relic_{relic.RelicName}");
                iconObj.transform.SetParent(_iconContainer, false);
                var rect = iconObj.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(48, 48);

                var bg = iconObj.AddComponent<Image>();
                bg.color = new Color(0.15f, 0.15f, 0.25f);

                // 아이템 이름 텍스트 (아이콘이 없을 때 폴백)
                var textObj = new GameObject("Label");
                textObj.transform.SetParent(iconObj.transform, false);
                var tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.text = relic.RelicName.Substring(0, System.Math.Min(2, relic.RelicName.Length));
                tmp.fontSize = 14;
                tmp.color = GetRarityColor(relic.Rarity);
                tmp.alignment = TextAlignmentOptions.Center;
                var textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
            }

            // 아이콘이 있으면 설정
            if (relic.Icon != null)
            {
                var img = iconObj.GetComponentInChildren<Image>();
                if (img != null)
                {
                    img.sprite = relic.Icon;
                    img.color = Color.white;
                }
            }

            // 툴팁 설정
            var tooltip = iconObj.GetComponent<TooltipTarget>();
            if (tooltip == null)
                tooltip = iconObj.AddComponent<TooltipTarget>();
            tooltip.SetContent(relic.RelicName, relic.Description);

            return iconObj;
        }

        private Color GetRarityColor(RewardRarity rarity)
        {
            return rarity switch
            {
                RewardRarity.Rare => new Color(0.3f, 0.6f, 1f),
                RewardRarity.Unique => new Color(0.7f, 0.3f, 0.9f),
                _ => Color.white
            };
        }

        private void ClearIcons()
        {
            foreach (var obj in _iconObjects)
            {
                if (obj != null)
                    Destroy(obj);
            }
            _iconObjects.Clear();
        }

        private void OnDestroy()
        {
            ClearIcons();
        }
    }
}
