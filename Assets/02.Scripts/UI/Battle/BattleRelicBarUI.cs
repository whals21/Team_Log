using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Map;
using TeamLog.Reward;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 전투 씬 유물 바 — GameRunState.Instance에서 유물을 읽어 하단에 아이콘 표시
    /// </summary>
    public class BattleRelicBarUI : MonoBehaviour
    {
        [SerializeField] private Transform _iconContainer;
        [SerializeField] private TextMeshProUGUI _countLabel;

        private readonly List<GameObject> _iconObjects = new();

        public void Refresh()
        {
            ClearIcons();

            var runState = GameRunState.Instance;
            if (runState == null) return;

            var relics = runState.RelicHandler.Relics;
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
            var iconObj = new GameObject($"Relic_{relic.RelicName}");
            iconObj.transform.SetParent(_iconContainer, false);
            var rect = iconObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(36, 36);

            var bg = iconObj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.25f);

            if (relic.Icon != null)
            {
                bg.sprite = relic.Icon;
                bg.color = Color.white;
            }
            else
            {
                var textObj = new GameObject("Label");
                textObj.transform.SetParent(iconObj.transform, false);
                var tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.text = relic.RelicName.Substring(0, System.Math.Min(2, relic.RelicName.Length));
                tmp.fontSize = 12;
                tmp.color = GetRarityColor(relic.Rarity);
                tmp.alignment = TextAlignmentOptions.Center;
                var textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
            }

            // 툴팁 설정
            var tooltip = iconObj.AddComponent<TooltipTarget>();
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
