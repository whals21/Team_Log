using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TeamLog.Map;

namespace TeamLog.UI.Map.Rework
{
    /// <summary>
    /// 우측 패널의 노드 상세 카드.
    /// 선택된 노드의 아이콘 / 제목 / 분위기 묘사 / 적 목록 / 보상 / 행동 버튼 표시.
    ///
    /// ★ Node Detail Preview 파이프 (2단계 흐름):
    ///   노드 클릭 → MapSceneSetup.PrepareNodePreview가 NodePreviewData 빌드 →
    ///   Initialize(node, preview, onAction) → 본 컴포넌트는 표시만 담당 (UI-로직 분리).
    ///   "Enter Battle" 버튼 클릭 → onAction(node) 콜백 → MapSceneSetup이 실제 액션.
    ///
    /// PartySelectionScene의 SkillDetailCard 패턴 준거.
    /// </summary>
    public class NodeDetailPanel : MonoBehaviour
    {
        [SerializeField] private Image _iconLarge;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _subtitleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Transform _statContainer;             // 레거시 호환 (사용 가능)
        [SerializeField] private Button _actionButton;
        [SerializeField] private TextMeshProUGUI _actionLabel;

        [Header("★ Preview Containers (Node Detail Preview 파이프)")]
        [SerializeField] private Transform _enemyListContainer;        // 적 목록 행들이 인스턴스화되는 부모
        [SerializeField] private Transform _rewardInfoContainer;       // 보상 행들이 인스턴스화되는 부모
        [SerializeField] private GameObject _enemyRowPrefab;           // EnemyRowPrefab (Builder가 생성)
        [SerializeField] private GameObject _rewardRowPrefab;          // RewardRowPrefab (Builder가 생성)

        private MapNode _bound;
        private NodePreviewData _preview;
        private System.Action<MapNode> _onAction;

        private void Awake()
        {
            AutoBindMissingFields();
        }

        private void AutoBindMissingFields()
        {
            var root = transform;
            if (_iconLarge == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "NodeIcon");
                if (go != null) _iconLarge = go.GetComponent<Image>();
            }
            if (_titleText == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "NodeTitle");
                if (go != null) _titleText = go.GetComponent<TextMeshProUGUI>();
            }
            if (_subtitleText == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "NodeSubtitle");
                if (go != null) _subtitleText = go.GetComponent<TextMeshProUGUI>();
            }
            if (_descriptionText == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "NodeDescription");
                if (go != null) _descriptionText = go.GetComponent<TextMeshProUGUI>();
            }
            if (_statContainer == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "StatContainer");
                if (go != null) _statContainer = go.transform;
            }
            if (_enemyListContainer == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "EnemyListContainer");
                if (go != null) _enemyListContainer = go.transform;
            }
            if (_rewardInfoContainer == null)
            {
                var go = UIAutoBindHelper.FindDescendantByName(root, "RewardInfoContainer");
                if (go != null) _rewardInfoContainer = go.transform;
            }
            if (_actionButton == null)
            {
                _actionButton = GetComponentInChildren<Button>(true);
            }
            if (_actionLabel == null && _actionButton != null)
            {
                _actionLabel = _actionButton.GetComponentInChildren<TextMeshProUGUI>(true);
            }
            UIAutoBindHelper.DisableChildRaycastsExcept(transform, _actionButton?.targetGraphic);
        }

        /// <summary>
        /// 레거시 Initialize — preview 데이터 없이 노드 타입 정보만으로 렌더링.
        /// Start 노드 초기 표시 / 폴백.
        /// </summary>
        public void Initialize(MapNode node, System.Action<MapNode> onAction)
        {
            _bound = node;
            _preview = null;
            _onAction = onAction;
            Render();
            BindActionButton();
        }

        /// <summary>
        /// ★ Node Detail Preview 파이프 — preview 데이터와 함께 초기화.
        /// 적 목록 + 보상을 동적으로 컨테이너에 채움.
        /// </summary>
        public void Initialize(MapNode node, NodePreviewData preview, System.Action<MapNode> onAction)
        {
            _bound = node;
            _preview = preview;
            _onAction = onAction;
            Render();
            RenderPreviewLists();
            BindActionButton();

            Debug.Log($"[NodeDetailPanel] Initialize — title:{preview?.Title} enemies:{preview?.Enemies?.Count ?? 0}");
        }

        private void Render()
        {
            if (_bound == null)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);

            // preview 우선, 폴백은 정적 타입 정보
            var typeInfo = _preview != null
                ? new NodeTypeInfo
                {
                    DisplayName = _preview.Title,
                    Subtitle = _preview.Subtitle,
                    Description = _preview.Description,
                    ActionLabel = _preview.ActionLabel,
                    Color = _preview.ThemeColor,
                    IconSymbol = _preview.IconSymbol
                }
                : GetStaticNodeTypeInfo(_bound.NodeType);

            if (_iconLarge != null)
            {
                _iconLarge.color = typeInfo.Color;
                var label = _iconLarge.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = typeInfo.IconSymbol;
                    label.color = typeInfo.Color;
                }
            }

            if (_titleText != null)
                _titleText.text = typeInfo.DisplayName;

            if (_subtitleText != null)
                _subtitleText.text = typeInfo.Subtitle;

            if (_descriptionText != null)
                _descriptionText.text = typeInfo.Description;

            if (_actionLabel != null)
                _actionLabel.text = typeInfo.ActionLabel;
        }

        /// <summary>
        /// ★ preview의 적 목록 + 보상 행을 각 컨테이너에 렌더링.
        /// </summary>
        private void RenderPreviewLists()
        {
            if (_preview == null) return;

            RenderEnemyList(_preview.Enemies);
            RenderRewardInfo(_preview.Rewards);
        }

        private void RenderEnemyList(List<EnemyPreviewInfo> enemies)
        {
            if (_enemyListContainer == null) return;
            ClearContainer(_enemyListContainer);

            if (enemies == null || enemies.Count == 0 || _enemyRowPrefab == null)
            {
                // 비전투 노드이거나 프리팹 미연결 — 빈 컨테이너 (감춰질 수도 있음)
                return;
            }

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;
                var row = Instantiate(_enemyRowPrefab, _enemyListContainer);
                var binder = row.GetComponent<NodeDetailEnemyRow>();
                if (binder == null) binder = row.AddComponent<NodeDetailEnemyRow>();
                binder.SetData(enemy);
            }
        }

        private void RenderRewardInfo(RewardPreviewInfo reward)
        {
            if (_rewardInfoContainer == null) return;
            ClearContainer(_rewardInfoContainer);

            if (reward == null || _rewardRowPrefab == null) return;

            // 골드 범위
            if (reward.GoldMax > 0)
            {
                AddRewardRow("Gold", $"{reward.GoldMin}-{reward.GoldMax}");
            }
            // 증강 수
            if (reward.AugmentCount > 0)
            {
                AddRewardRow("Augments", $"{reward.AugmentCount}");
            }
            // 유물 확률
            if (reward.RelicChance > 0f)
            {
                string relicStr = reward.RelicChance >= 1f ? "Guaranteed" : $"{reward.RelicChance * 100f:F0}%";
                AddRewardRow("Relic", relicStr);
            }
            // 영혼 (보스만)
            if (reward.IncludesSouls)
            {
                AddRewardRow("Souls", "+");
            }
        }

        private void AddRewardRow(string label, string value)
        {
            var row = Instantiate(_rewardRowPrefab, _rewardInfoContainer);
            var binder = row.GetComponent<NodeDetailRewardRow>();
            if (binder == null) binder = row.AddComponent<NodeDetailRewardRow>();
            binder.SetData(label, value);
        }

        private static void ClearContainer(Transform container)
        {
            if (container == null) return;
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                var child = container.GetChild(i);
                if (child != null) Destroy(child.gameObject);
            }
        }

        private void BindActionButton()
        {
            if (_actionButton == null) return;
            _actionButton.onClick.RemoveAllListeners();
            if (_onAction != null && _bound != null)
            {
                _actionButton.onClick.AddListener(() => _onAction.Invoke(_bound));
            }
        }

        /// <summary>
        /// 노드 타입별 시각 정보 매핑 (정적 폴백).
        /// preview가 없는 경우 사용. MapSceneSetup.BuildPreviewData도 이 매핑을 참고용으로 사용.
        /// </summary>
        public static NodeTypeInfo GetStaticNodeTypeInfo(MapNodeType type)
        {
            return type switch
            {
                MapNodeType.Start => new NodeTypeInfo
                {
                    DisplayName = "Origin",
                    Subtitle = "Your journey begins",
                    Description = "The threshold between safety and the unknown.",
                    ActionLabel = "Begin",
                    Color = HexColor("#6ed5b2"),
                    IconSymbol = "S",
                },
                MapNodeType.Battle => new NodeTypeInfo
                {
                    DisplayName = "Battle",
                    Subtitle = "Crimson Acolytes",
                    Description = "Hooded figures chant in forgotten tongues. Their blades hunger.",
                    ActionLabel = "Enter Battle",
                    Color = HexColor("#c0392b"),
                    IconSymbol = "B",
                },
                MapNodeType.Elite => new NodeTypeInfo
                {
                    DisplayName = "Elite",
                    Subtitle = "Greater Threat",
                    Description = "A powerful foe guards this path. Greater risk, greater reward.",
                    ActionLabel = "Challenge Elite",
                    Color = HexColor("#f4d35e"),
                    IconSymbol = "E",
                },
                MapNodeType.Boss => new NodeTypeInfo
                {
                    DisplayName = "Nemesis",
                    Subtitle = "Floor Boss",
                    Description = "The master of this domain awaits. There is no turning back.",
                    ActionLabel = "Confront Boss",
                    Color = HexColor("#8b0000"),
                    IconSymbol = "X",
                },
                MapNodeType.Event => new NodeTypeInfo
                {
                    DisplayName = "Event",
                    Subtitle = "Unknown Encounter",
                    Description = "A curious sight demands your attention. Choose wisely.",
                    ActionLabel = "Investigate",
                    Color = HexColor("#b388ff"),
                    IconSymbol = "?",
                },
                MapNodeType.Shop => new NodeTypeInfo
                {
                    DisplayName = "Shop",
                    Subtitle = "Wandering Merchant",
                    Description = "Gold opens many doors. Spend wisely, traveler.",
                    ActionLabel = "Browse Wares",
                    Color = HexColor("#5ec5e8"),
                    IconSymbol = "$",
                },
                MapNodeType.Rest => new NodeTypeInfo
                {
                    DisplayName = "Sanctuary",
                    Subtitle = "Rest Site",
                    Description = "Cold stone whispers old prayers. The wounded may drink.",
                    ActionLabel = "Rest Here",
                    Color = HexColor("#4caf50"),
                    IconSymbol = "+",
                },
                _ => new NodeTypeInfo
                {
                    DisplayName = "Unknown",
                    Subtitle = "",
                    Description = "",
                    ActionLabel = "Confirm",
                    Color = Color.white,
                    IconSymbol = "?",
                }
            };
        }

        private static Color HexColor(string hex)
        {
            hex = hex.Replace("#", "");
            byte r = System.Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = System.Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = System.Convert.ToByte(hex.Substring(4, 2), 16);
            return new Color32(r, g, b, 255);
        }

        /// <summary>
        /// 노드 타입별 시각 정보 (정적 폴백 + MapSceneSetup.BuildPreviewData 참고용).
        /// </summary>
        public struct NodeTypeInfo
        {
            public string DisplayName;
            public string Subtitle;
            public string Description;
            public string ActionLabel;
            public Color Color;
            public string IconSymbol;
        }
    }
}
