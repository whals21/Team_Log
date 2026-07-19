using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TeamLog.Characters;
using TeamLog.Map;
using TeamLog.Reward;
using TeamLog.Skill;

namespace TeamLog.UI.Map.Rework
{
    /// <summary>
    /// 좌측 Party 사이드바 컨테이너.
    /// PartyMemberRow 리스트 + RelicGridPanel + AugmentListPanel 통합 관리.
    /// PartySelectionScene의 FooterPanel(PartySlotPanel 포함) 패턴 준거.
    /// </summary>
    public class PartySidebarPanel : MonoBehaviour
    {
        [SerializeField] private Transform _partyListContainer;
        [SerializeField] private GameObject _memberRowPrefab;
        [SerializeField] private RelicGridPanel _relicGrid;
        [SerializeField] private AugmentListPanel _augmentList;

        private readonly List<PartyMemberRow> _rows = new();
        private GameRunState _runState;

        private void Awake()
        {
            AutoBindMissingFields();
        }

        private void AutoBindMissingFields()
        {
            var root = transform;

            // ★ Priority 8 (치명 수정): PartyListContainer가 자기 자신이거나 형제일 수 있음.
            // Builder가 PartySidebarPanel을 어디에 붙이든 정상 작동하도록 다중 fallback.
            if (_partyListContainer == null)
            {
                // (1) 자기 자신이 PartyListContainer인 경우
                if (gameObject.name == "PartyListContainer")
                {
                    _partyListContainer = root;
                }
                else
                {
                    // (2) 자식에서 찾기
                    var go = UIAutoBindHelper.FindDescendantByName(root, "PartyListContainer");
                    if (go != null) _partyListContainer = go.transform;
                    else
                    {
                        // (3) 형제에서 찾기 (부모의 자식들)
                        var parent = root.parent;
                        if (parent != null)
                        {
                            var sibling = UIAutoBindHelper.FindDescendantByName(parent, "PartyListContainer");
                            if (sibling != null) _partyListContainer = sibling.transform;
                        }
                    }
                }
            }
            if (_relicGrid == null)
            {
                _relicGrid = GetComponentInChildren<RelicGridPanel>(true);
                // 형제에서 찾기 fallback
                if (_relicGrid == null && root.parent != null)
                    _relicGrid = root.parent.GetComponentInChildren<RelicGridPanel>(true);
            }
            if (_augmentList == null)
            {
                _augmentList = GetComponentInChildren<AugmentListPanel>(true);
                // 형제에서 찾기 fallback
                if (_augmentList == null && root.parent != null)
                    _augmentList = root.parent.GetComponentInChildren<AugmentListPanel>(true);
            }
            Debug.Log($"[PartySidebarPanel] AutoBind — partyList:{(_partyListContainer != null)}, relicGrid:{(_relicGrid != null)}, augmentList:{(_augmentList != null)}");
        }

        /// <summary>
        /// GameRunState 바인딩 후 파티/유물/증강 전체 갱신.
        /// </summary>
        public void Initialize(GameRunState runState)
        {
            _runState = runState;
            RenderAll();
        }

        /// <summary>
        /// 전투 종료 후 파티 HP/자원 변화 반영.
        /// ★ Priority 8 (치명 수정): 최초 Initialize 실패 시 Refresh에서 RenderParty 재호출 폴백.
        /// </summary>
        public void Refresh()
        {
            // _rows가 비어있는데 파티가 있으면 RenderParty 재시도
            if (_rows.Count == 0 && _runState?.PlayerParty?.Count > 0)
            {
                Debug.Log($"[PartySidebarPanel] Refresh — rows 비어있음, RenderParty 재호출 (partyCount={_runState.PlayerParty.Count})");
                RenderParty();
            }
            else
            {
                foreach (var row in _rows)
                {
                    if (row != null) row.Refresh();
                }
            }

            if (_relicGrid != null && _runState?.RelicHandler?.Relics != null)
                _relicGrid.Refresh(_runState.RelicHandler.Relics.ToList());

            if (_augmentList != null)
                _augmentList.Refresh(CollectAllAugments());
        }

        private void RenderAll()
        {
            if (_runState == null) return;
            RenderParty();
            RenderRelics();
            RenderAugments();
        }

        private void RenderParty()
        {
            if (_partyListContainer == null || _memberRowPrefab == null) return;
            if (_runState?.PlayerParty == null) return;

            // 기존 행 제거
            foreach (var row in _rows)
            {
                if (row != null) Destroy(row.gameObject);
            }
            _rows.Clear();

            foreach (var character in _runState.PlayerParty)
            {
                if (character == null) continue;
                var rowGo = Instantiate(_memberRowPrefab, _partyListContainer);
                rowGo.gameObject.SetActive(true);
                var row = rowGo.GetComponent<PartyMemberRow>();
                if (row == null) row = rowGo.AddComponent<PartyMemberRow>();

                Color memberColor = GetMemberColor(character);
                row.Initialize(character, memberColor);
                _rows.Add(row);
            }
        }

        private void RenderRelics()
        {
            if (_relicGrid == null || _runState?.RelicHandler?.Relics == null) return;
            _relicGrid.Initialize(_runState.RelicHandler.Relics.ToList());
        }

        private void RenderAugments()
        {
            if (_augmentList == null) return;
            _augmentList.Initialize(CollectAllAugments());
        }

        /// <summary>
        /// 파티원 전체의 SkillInventory에서 모든 적용된 증강 수집.
        /// ★ TODO: SkillInventoryComponent의 정확한 증강 노출 API 확인 필요.
        /// 현재는 빈 리스트 반환 — 런타임에서 빈 AugmentListPanel이 표시됨 (안전 폴백).
        /// </summary>
        private List<AugmentData> CollectAllAugments()
        {
            // TODO: member.SkillInventory에서 Augments 추출 API 확정 후 구현
            // 현재는 안전을 위해 빈 리스트 반환
            return new List<AugmentData>();
        }

        /// <summary>
        /// 캐릭터 ID 기반 자원색 매핑 (PartySelectionUIUtils와 동일 로직).
        /// 중복 방지용 로컬 복제. ResourceType enum에 없는 값(Corpse/Discover)은 제외.
        /// </summary>
        private static Color GetMemberColor(Character character)
        {
            if (character?.Data == null) return UIPalette.Default.ResourceDefault;
            var palette = UIPalette.Default;
            return character.Data.ResourceType switch
            {
                Characters.ResourceType.Ember     => palette.ResourceEmber,
                Characters.ResourceType.Vengeance => palette.ResourceVengeance,
                Characters.ResourceType.Frost     => palette.ResourceFrost,
                Characters.ResourceType.Prophecy  => palette.ResourceProphecy,
                Characters.ResourceType.Charge    => palette.ResourceCharge,
                Characters.ResourceType.Shadows   => palette.ResourceShadows,
                Characters.ResourceType.Combo     => palette.ResourceCombo,
                Characters.ResourceType.Melody    => palette.ResourceMelody,
                Characters.ResourceType.Mercy     => palette.ResourceMercy,
                _ => palette.ResourceDefault
            };
        }
    }
}
