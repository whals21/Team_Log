using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using TeamLog.Combat;
using TeamLog.Map;
using TeamLog.Reward;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// 전투 씬 유물 바 — TopBar 좌측에 유물 아이콘 표시
    /// 희귀도 테두리, 호버 툴팁(발동 조건 + 효과), 발동 시 점멸 애니메이션
    /// </summary>
    public class BattleRelicBarUI : MonoBehaviour
    {
        [SerializeField] private Transform _iconContainer;

        private readonly List<GameObject> _iconObjects = new();
        private readonly Dictionary<RelicData, Image> _relicIconMap = new();

        private void Awake()
        {
            if (_iconContainer == null)
                _iconContainer = transform;
        }

        private void OnEnable()
        {
            CombatEventBus.OnRelicTriggered += OnRelicTriggered;
        }

        private void OnDisable()
        {
            CombatEventBus.OnRelicTriggered -= OnRelicTriggered;
        }

        public void Refresh()
        {
            // 기존 모든 자식 제거 (빌더 플레이스홀더 포함)
            for (int i = _iconContainer.childCount - 1; i >= 0; i--)
            {
                var child = _iconContainer.GetChild(i).gameObject;
                Destroy(child);
            }
            _iconObjects.Clear();
            _relicIconMap.Clear();

            var runState = GameRunState.Instance;
            if (runState == null) return;

            var relics = runState.RelicHandler.Relics;
            foreach (var relic in relics)
            {
                var iconObj = CreateRelicIcon(relic);
                _iconObjects.Add(iconObj);
            }
        }

        /// <summary>
        /// 특정 유물 발동 시 해당 아이콘 점멸
        /// </summary>
        public void FlashRelic(RelicData relic)
        {
            if (relic == null) return;
            if (!_relicIconMap.TryGetValue(relic, out var img)) return;
            if (img == null) return;

            // 짧은 흰색 플래시 → 원래 색상 복원
            var originalColor = img.color;
            img.color = Color.white;
            DOTween.To(() => img.color, x => img.color = x, originalColor, 0.4f);
        }

        private void OnRelicTriggered(RelicData relic)
        {
            FlashRelic(relic);
        }

        private GameObject CreateRelicIcon(RelicData relic)
        {
            var iconObj = new GameObject($"Relic_{relic.RelicName}");
            iconObj.transform.SetParent(_iconContainer, false);
            var rect = iconObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(40, 40);

            // 배경
            var bg = iconObj.AddComponent<Image>();
            bg.color = GetRarityBgColor(relic.Rarity);

            // 희귀도 테두리
            var outline = iconObj.AddComponent<Outline>();
            outline.effectColor = GetRarityOutlineColor(relic.Rarity);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            // 아이콘 내용
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
                tmp.text = relic.RelicName.Length > 0
                    ? relic.RelicName.Substring(0, System.Math.Min(2, relic.RelicName.Length))
                    : "?";
                tmp.fontSize = 13;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color = GetRarityTextColor(relic.Rarity);
                tmp.alignment = TextAlignmentOptions.Center;
                UIKoreanFont.EnsureFont(tmp);
                var textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                tmp.raycastTarget = false;
            }

            // 툴팁: 이름 + 설명 + 발동 조건
            string triggerLabel = GetTriggerLabel(relic.Trigger);
            string tooltipDesc = $"{relic.Description}\n<size=80%><color=#AAAAAA>[{triggerLabel}]</color></size>";
            var tooltip = iconObj.AddComponent<TooltipTarget>();
            tooltip.SetContent(relic.RelicName, tooltipDesc);

            _relicIconMap[relic] = bg;
            return iconObj;
        }

        private static string GetTriggerLabel(RelicTrigger trigger)
        {
            return trigger switch
            {
                RelicTrigger.BattleStart => "전투 시작",
                RelicTrigger.TurnStart => "턴 시작",
                RelicTrigger.TurnEnd => "턴 종료",
                RelicTrigger.OnDamageDealt => "피해를 입힐 때",
                RelicTrigger.OnDamageReceived => "피해를 받을 때",
                RelicTrigger.OnKill => "적 처치 시",
                RelicTrigger.OnHealApplied => "회복 시",
                RelicTrigger.OnShieldGained => "쉴드 획득 시",
                RelicTrigger.OnGoldEarned => "골드 획득 시",
                RelicTrigger.OnSkillUsed => "스킬 사용 시",
                _ => "패시브"
            };
        }

        private static Color GetRarityBgColor(RewardRarity rarity)
        {
            return rarity switch
            {
                RewardRarity.Rare => new Color(0.08f, 0.15f, 0.3f),
                RewardRarity.Unique => new Color(0.18f, 0.06f, 0.28f),
                _ => new Color(0.12f, 0.12f, 0.18f)
            };
        }

        private static Color GetRarityOutlineColor(RewardRarity rarity)
        {
            return rarity switch
            {
                RewardRarity.Rare => new Color(0.3f, 0.6f, 1f),
                RewardRarity.Unique => new Color(0.7f, 0.3f, 0.9f),
                _ => new Color(0.5f, 0.5f, 0.5f)
            };
        }

        private static Color GetRarityTextColor(RewardRarity rarity)
        {
            return rarity switch
            {
                RewardRarity.Rare => new Color(0.4f, 0.7f, 1f),
                RewardRarity.Unique => new Color(0.8f, 0.4f, 1f),
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
            _relicIconMap.Clear();
        }

        private void OnDestroy()
        {
            ClearIcons();
        }
    }
}
