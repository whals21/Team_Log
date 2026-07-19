using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using TeamLog.Characters;
using TeamLog.UI;

namespace TeamLog.UI.PartySelection
{
    /// <summary>
    /// Party Selection Scene 런타임 컨트롤러 (UI-C.3).
    /// 캐릭터 데이터 로드 → 캐러셀 동적 생성 → 메인 디스플레이 갱신 → 파티 구성 → 런 시작.
    ///
    /// ★ 인터랙션 모델 (웹 목업 기준):
    ///   - Carousel 썸네일 클릭 → 파티에 추가 (또는 이미 파티에 있으면 무시)
    ///   - 파티 슬롯 클릭 → 파티에서 제거
    ///   - 좌/우 버튼, 키보드 ←/→ → 현재 캐릭터 전환 (InfoArea 갱신)
    /// </summary>
    public class PartySelectionController : MonoBehaviour
    {
        /// <summary>
        /// ★ 정적 파티 데이터 — EMBARK 시점에 설정됨.
        /// MapScene 측에서 PartySelectionController.SelectedParty로 읽어서 GameRunState에 전달.
        /// 씬 전환 후에도 유지 (정적 필드).
        /// </summary>
        public static List<CharacterData> SelectedParty { get; private set; }
        [Header("Character Data Source")]
        [SerializeField] private CharacterData[] _availableCharacters;
        [SerializeField] private CharacterTraitData[] _allTraits;  // ★ SceneBuilder가 빌드 타임에 할당
        [SerializeField] private string _returnSceneName = "MapScene";

        [Header("UI References (SceneBuilder가 자동 바인딩)")]
        [SerializeField] private CharacterPortraitBig _portraitBig;
        [SerializeField] private ResourceMechanicBox _mechanicBox;
        [SerializeField] private TextMeshProUGUI _identityQuoteText;
        [SerializeField] private List<SkillDetailCard> _skillCards = new();
        [SerializeField] private List<TraitDetailCard> _traitCards = new();

        [Header("Stat Cells (3 — Vigor/Resource/Role)")]
        [SerializeField] private TextMeshProUGUI _statHpValue;
        [SerializeField] private TextMeshProUGUI _statResValue;
        [SerializeField] private TextMeshProUGUI _statResName;
        [SerializeField] private TextMeshProUGUI _statRoleValue;
        [SerializeField] private TextMeshProUGUI _statRoleKo;

        [Header("Strength / Weakness")]
        [SerializeField] private TextMeshProUGUI _strengthText;
        [SerializeField] private TextMeshProUGUI _weaknessText;

        [Header("Carousel")]
        [SerializeField] private Transform _carouselContent;
        [SerializeField] private CharacterCarouselItem _carouselItemPrefab;
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _nextButton;

        [Header("Party Slot Panel")]
        [SerializeField] private PartySlotPanel _partySlotPanel;

        [Header("Transition")]
        [SerializeField] private string _nextSceneName = "MapSceneRework";

        // ── 내부 상태 ──
        private readonly List<CharacterDisplayData> _displayData = new();
        private readonly List<CharacterCarouselItem> _carouselItems = new();
        private int _currentIndex = 0;
        private readonly Dictionary<string, int> _selectedTraitIndex = new();
        private readonly HashSet<string> _partyCharacterIds = new();
        private const int MAX_PARTY = 4;

        private void Awake()
        {
            LoadCharacterData();
            AutoBindMissingFields();
        }

        /// <summary>
        /// 인스펙터에서 바인딩되지 않은 필드를 자동 검색.
        /// 씬 빌더의 FindDescendant가 일부 필드를 못 찾았을 때의 폴백.
        /// </summary>
        private void AutoBindMissingFields()
        {
            var root = transform.root;

            // PortraitBig
            if (_portraitBig == null)
                _portraitBig = GetComponentInChildren<CharacterPortraitBig>(true);

            // MechanicBox
            if (_mechanicBox == null)
                _mechanicBox = GetComponentInChildren<ResourceMechanicBox>(true);

            // IdentityQuoteText
            if (_identityQuoteText == null)
            {
                var quoteGo = FindChildRecursive(root, "IdentityQuote");
                if (quoteGo != null)
                    _identityQuoteText = quoteGo.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
            }

            // Stat 셀들 — 이름으로 검색
            if (_statHpValue == null)
            {
                var cell = FindChildRecursive(root, "Stat_Vigor");
                if (cell != null) _statHpValue = cell.transform.Find("Value")?.GetComponent<TextMeshProUGUI>();
            }
            if (_statResValue == null)
            {
                var cell = FindChildRecursive(root, "Stat_Resource");
                if (cell != null)
                {
                    _statResValue = cell.transform.Find("Value")?.GetComponent<TextMeshProUGUI>();
                    _statResName = cell.transform.Find("Sub")?.GetComponent<TextMeshProUGUI>();
                }
            }
            if (_statRoleValue == null)
            {
                var cell = FindChildRecursive(root, "Stat_Role");
                if (cell != null)
                {
                    _statRoleValue = cell.transform.Find("Value")?.GetComponent<TextMeshProUGUI>();
                    _statRoleKo = cell.transform.Find("Sub")?.GetComponent<TextMeshProUGUI>();
                }
            }

            // Strength/Weakness
            if (_strengthText == null)
            {
                var box = FindChildRecursive(root, "StrengthBox");
                if (box != null) _strengthText = box.transform.Find("Desc")?.GetComponent<TextMeshProUGUI>();
            }
            if (_weaknessText == null)
            {
                var box = FindChildRecursive(root, "WeaknessBox");
                if (box != null) _weaknessText = box.transform.Find("Desc")?.GetComponent<TextMeshProUGUI>();
            }

            // Skill 카드 4개
            if (_skillCards == null || _skillCards.Count == 0)
            {
                _skillCards.Clear();
                for (int i = 1; i <= 4; i++)
                {
                    var cardGo = FindChildRecursive(root, $"Skill{i}");
                    if (cardGo != null)
                    {
                        var card = cardGo.GetComponent<SkillDetailCard>();
                        if (card != null) _skillCards.Add(card);
                    }
                }
            }

            // 특성 카드 3개
            if (_traitCards == null || _traitCards.Count == 0)
            {
                _traitCards.Clear();
                for (int i = 1; i <= 3; i++)
                {
                    var cardGo = FindChildRecursive(root, $"Trait{i}");
                    if (cardGo != null)
                    {
                        var card = cardGo.GetComponent<TraitDetailCard>();
                        if (card != null) _traitCards.Add(card);
                    }
                }
            }

            // Carousel
            if (_carouselContent == null)
            {
                var contentGo = FindChildRecursive(root, "CarouselContent");
                if (contentGo != null) _carouselContent = contentGo.transform;
            }

            // 좌/우 네비게이션 버튼
            if (_prevButton == null)
            {
                var prevGo = FindChildRecursive(root, "BtnPrev");
                if (prevGo != null) _prevButton = prevGo.GetComponent<Button>();
            }
            if (_nextButton == null)
            {
                var nextGo = FindChildRecursive(root, "BtnNext");
                if (nextGo != null) _nextButton = nextGo.GetComponent<Button>();
            }

            // PartySlotPanel — FooterPanel에서 찾고, 없으면 AddComponent로 보완
            if (_partySlotPanel == null)
            {
                _partySlotPanel = GetComponentInChildren<PartySlotPanel>(true);
                if (_partySlotPanel == null)
                {
                    var footerPanelGo = FindChildRecursive(root, "FooterPanel");
                    if (footerPanelGo != null)
                    {
                        _partySlotPanel = footerPanelGo.GetComponent<PartySlotPanel>();
                        if (_partySlotPanel == null)
                        {
                            _partySlotPanel = footerPanelGo.AddComponent<PartySlotPanel>();
                            Debug.Log("[PartySelectionController] Auto-added PartySlotPanel to FooterPanel.");
                        }
                    }
                }
            }

            // CarouselItemPrefab — CarouselItemTemplate에서 찾고, 없으면 AddComponent로 보완
            if (_carouselItemPrefab == null)
            {
                _carouselItemPrefab = GetComponentInChildren<CharacterCarouselItem>(true);
                if (_carouselItemPrefab == null)
                {
                    var templateGo = FindChildRecursive(root, "CarouselItemTemplate");
                    if (templateGo != null)
                    {
                        _carouselItemPrefab = templateGo.GetComponent<CharacterCarouselItem>();
                        if (_carouselItemPrefab == null)
                        {
                            _carouselItemPrefab = templateGo.AddComponent<CharacterCarouselItem>();
                            Debug.Log("[PartySelectionController] Auto-added CharacterCarouselItem to CarouselItemTemplate.");
                        }
                    }
                }
            }

            // AutoBind 완료 — 상세 로그 제거됨 (GC 2026-07-18)
        }

        private static GameObject FindChildRecursive(Transform current, string name)
        {
            for (int i = 0; i < current.childCount; i++)
            {
                var child = current.GetChild(i);
                if (child.name == name) return child.gameObject;
                var found = FindChildRecursive(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private void Start()
        {
            BuildCarousel();
            BindNavigationButtons();
            BindPartySlotPanel();
            if (_displayData.Count > 0) SelectCharacter(0);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) SelectPrev();
            if (Input.GetKeyDown(KeyCode.RightArrow)) SelectNext();
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                AddCurrentToParty();
        }

        // =========================================================
        // 데이터 로드
        // =========================================================
        private void LoadCharacterData()
        {
            _displayData.Clear();

            // 외부에서 지정된 배열이 있으면 그것 사용
            if (_availableCharacters != null && _availableCharacters.Length > 0)
            {
                foreach (var cd in _availableCharacters)
                {
                    if (cd == null) continue;
                    _displayData.Add(CharacterDisplayData.FromCharacterId(cd.CharacterName, cd));
                }
                return;
            }

            // 없으면 알려진 캐릭터 ID 목록으로 빈 데이터 생성 (기획 데이터만 표시)
            string[] knownIds =
            {
                "Ashe", "Duran", "Lumi", "Sibyl", "Taranis", "Umbra",
                "Aster", "Mortis", "Cael", "Calliope", "Elara"
            };
            foreach (var id in knownIds)
            {
                _displayData.Add(CharacterDisplayData.FromCharacterId(id, null));
            }
        }

        // =========================================================
        // 캐러셀 빌드
        // =========================================================
        private void BuildCarousel()
        {
            if (_carouselContent == null || _carouselItemPrefab == null) return;

            // 기존 아이템 제거
            foreach (var item in _carouselItems)
            {
                if (item != null) Destroy(item.gameObject);
            }
            _carouselItems.Clear();

            // 아이템 생성
            foreach (var data in _displayData)
            {
                var item = Instantiate(_carouselItemPrefab, _carouselContent);
                item.gameObject.SetActive(true);  // 비활성 템플릿 복제본 활성화
                item.Initialize(data, OnCarouselItemClicked);
                _carouselItems.Add(item);
            }
        }

        private void OnCarouselItemClicked(CharacterDisplayData data)
        {
            if (data == null) return;

            // ★ 웹 목업 방식: Carousel 썸네일 클릭 → 파티에 추가
            // 이미 파티에 있으면 무시 (파티에서 제거는 파티 슬롯 클릭으로만)
            if (_partyCharacterIds.Contains(data.CharacterId))
            {
                // 이미 파티에 있음 — 현재 캐릭터로만 전환 (시각적 피드백)
                int idx = _displayData.IndexOf(data);
                if (idx >= 0) SelectCharacter(idx);
                return;
            }

            // 파티에 추가 + 현재 캐릭터 전환 (사용자가 방금 추가한 캐릭터를 중앙에서 확인)
            int index = _displayData.IndexOf(data);
            if (index >= 0) SelectCharacter(index);
            AddCurrentToParty();
        }

        // =========================================================
        // 네비게이션
        // =========================================================
        private void BindNavigationButtons()
        {
            if (_prevButton != null) _prevButton.onClick.AddListener(SelectPrev);
            if (_nextButton != null) _nextButton.onClick.AddListener(SelectNext);
        }

        public void SelectPrev()
        {
            if (_displayData.Count == 0) return;
            int idx = (_currentIndex - 1 + _displayData.Count) % _displayData.Count;
            SelectCharacter(idx);
        }

        public void SelectNext()
        {
            if (_displayData.Count == 0) return;
            int idx = (_currentIndex + 1) % _displayData.Count;
            SelectCharacter(idx);
        }

        public void SelectCharacter(int index)
        {
            if (index < 0 || index >= _displayData.Count) return;
            _currentIndex = index;

            // 캐러셀 active 상태 업데이트
            for (int i = 0; i < _carouselItems.Count; i++)
            {
                if (_carouselItems[i] != null)
                    _carouselItems[i].SetActive(i == index);
            }

            RenderCurrent();
        }

        // =========================================================
        // 현재 캐릭터 렌더링
        // =========================================================
        private void RenderCurrent()
        {
            var data = CurrentData;
            if (data == null) return;

            // 초상화
            if (_portraitBig != null) _portraitBig.Initialize(data);

            // 자원 메커니즘 박스
            if (_mechanicBox != null) _mechanicBox.Initialize(data);

            // 정체성 인용구
            if (_identityQuoteText != null)
            {
                _identityQuoteText.text = data.Identity ?? "";
            }

            // 스탯
            if (_statHpValue != null) _statHpValue.text = data.HP.ToString();
            if (_statResValue != null)
            {
                _statResValue.text = data.ResourceMax > 0 ? $"0 / {data.ResourceMax}" : "∞";
            }
            if (_statResName != null) _statResName.text = data.ResourceLabel ?? "";
            if (_statRoleValue != null) _statRoleValue.text = data.RoleEn ?? "";
            if (_statRoleKo != null) _statRoleKo.text = data.RoleKo ?? "";

            // 강점/약점
            if (_strengthText != null) _strengthText.text = data.Strength ?? "";
            if (_weaknessText != null) _weaknessText.text = data.Weakness ?? "";

            // 스킬 카드 — CharacterData가 있으면 스킬 4개 로드
            if (data.CharacterData != null)
            {
                RenderSkillCards(data);
                RenderTraitCards(data);
            }
            else
            {
                // 데이터 없으면 카드 비활성 또는 더미
                ClearSkillCards();
                ClearTraitCards();
            }
        }

        private void RenderSkillCards(CharacterDisplayData data)
        {
            // CharacterData에서 스킬 목록 로드
            var skills = ResolveCharacterSkills(data.CharacterData);
            Color resColor = data.ResourceColor;

            for (int i = 0; i < _skillCards.Count; i++)
            {
                if (_skillCards[i] == null) continue;
                if (i < skills.Count && skills[i] != null)
                {
                    _skillCards[i].gameObject.SetActive(true);
                    _skillCards[i].Initialize(skills[i], resColor);
                }
                else
                {
                    _skillCards[i].gameObject.SetActive(false);
                }
            }
        }

        private List<SkillData> ResolveCharacterSkills(CharacterData cd)
        {
            var result = new List<SkillData>();
            if (cd == null) return result;
            if (cd.Skills == null) return result;

            // ★ CharacterData.Skills 프로퍼티 (IReadOnlyList<SkillData>) 직접 사용
            foreach (var skill in cd.Skills)
            {
                if (skill != null) result.Add(skill);
            }
            return result;
        }

        private void ClearSkillCards()
        {
            foreach (var card in _skillCards)
            {
                if (card != null) card.gameObject.SetActive(false);
            }
        }

        private void RenderTraitCards(CharacterDisplayData data)
        {
            var traits = ResolveCharacterTraits(data.CharacterData);
            Color resColor = data.ResourceColor;

            // 선택된 특성 인덱스 초기화
            if (!_selectedTraitIndex.ContainsKey(data.CharacterId))
                _selectedTraitIndex[data.CharacterId] = 0;

            int selectedIdx = _selectedTraitIndex[data.CharacterId];

            for (int i = 0; i < _traitCards.Count; i++)
            {
                if (_traitCards[i] == null) continue;
                if (i < traits.Count && traits[i] != null)
                {
                    _traitCards[i].gameObject.SetActive(true);
                    bool locked = !traits[i].IsDefault; // 메타 해금 필요 (간단 처리 — 실제로는 MetaSaveData 참조)
                    bool selected = (i == selectedIdx) && !locked;
                    int idxCopy = i;
                    _traitCards[i].Initialize(traits[i], i, locked, selected,
                        (clickedIdx) => OnTraitClicked(data.CharacterId, clickedIdx));
                }
                else
                {
                    _traitCards[i].gameObject.SetActive(false);
                }
            }
        }

        private List<CharacterTraitData> ResolveCharacterTraits(CharacterData cd)
        {
            var result = new List<CharacterTraitData>();
            if (cd == null) return result;
            if (_allTraits == null || _allTraits.Length == 0) return result;

            // ★ 캐릭터 Class와 TraitData.TargetClass 매칭
            foreach (var trait in _allTraits)
            {
                if (trait == null) continue;
                if (trait.TargetClass == cd.Class)
                    result.Add(trait);
            }
            return result;
        }

        private void ClearTraitCards()
        {
            foreach (var card in _traitCards)
            {
                if (card != null) card.gameObject.SetActive(false);
            }
        }

        private void OnTraitClicked(string charId, int traitIndex)
        {
            _selectedTraitIndex[charId] = traitIndex;
            RenderCurrent();
        }

        // =========================================================
        // 파티 슬롯 패널
        // =========================================================
        private void BindPartySlotPanel()
        {
            if (_partySlotPanel == null) return;
            _partySlotPanel.Initialize(
                MAX_PARTY,
                onEmbark: OnEmbark,
                onRandom: OnRandomParty,
                onClear: OnClearParty,
                onSlotClicked: OnSlotClicked
            );
        }

        public void AddCurrentToParty()
        {
            var data = CurrentData;
            if (data == null || data.Locked) return;
            if (_partyCharacterIds.Contains(data.CharacterId)) return;
            if (_partySlotPanel == null) return;

            bool added = _partySlotPanel.TryFillSlot(data);
            if (added)
            {
                _partyCharacterIds.Add(data.CharacterId);
                UpdateCarouselPartyState();
            }
        }

        private void OnSlotClicked(int slotIndex)
        {
            if (_partySlotPanel == null) return;
            var party = _partySlotPanel.GetFilledParty();
            if (slotIndex < party.Count)
            {
                // ★ 채워진 슬롯 클릭 → 파티에서 제거 (웹 목업 방식)
                var removed = party[slotIndex];
                _partyCharacterIds.Remove(removed.CharacterId);
                _partySlotPanel.ClearSlot(slotIndex);
                UpdateCarouselPartyState();
            }
            // 빈 슬롯 클릭은 무시 (실수로 파티 추가 방지)
        }

        private void OnRandomParty()
        {
            OnClearParty();
            if (_partySlotPanel == null) return;

            var pool = _displayData.FindAll(d => !d.Locked);
            pool.Sort((a, b) => UnityEngine.Random.value.CompareTo(0.5f));
            int count = Mathf.Min(MAX_PARTY, pool.Count);
            for (int i = 0; i < count; i++)
            {
                _partySlotPanel.TryFillSlot(pool[i]);
                _partyCharacterIds.Add(pool[i].CharacterId);
            }
            UpdateCarouselPartyState();
        }

        private void OnClearParty()
        {
            if (_partySlotPanel == null) return;
            _partySlotPanel.ClearAll();
            _partyCharacterIds.Clear();
            UpdateCarouselPartyState();
        }

        private void OnEmbark(List<CharacterDisplayData> party)
        {
            if (party == null || party.Count == 0)
            {
                Debug.LogWarning("[PartySelectionController] Empty party — embark cancelled.");
                return;
            }

            var names = party.ConvertAll(p => p.DisplayName);
            Debug.Log($"[PartySelectionController] EMBARK — Party: {string.Join(", ", names)}");

            // ★ 정적 SelectedParty에 CharacterData[] 저장 → MapScene에서 읽기
            SelectedParty = party
                .Where(d => d.CharacterData != null)
                .Select(d => d.CharacterData)
                .ToList();

            Debug.Log($"[PartySelectionController] SelectedParty set: {SelectedParty.Count} characters.");

            // 씬 전환
            if (!string.IsNullOrEmpty(_nextSceneName))
            {
                SceneManager.LoadScene(_nextSceneName);
            }
        }

        private void UpdateCarouselPartyState()
        {
            foreach (var item in _carouselItems)
            {
                if (item == null || item.Data == null) continue;
                item.SetInParty(_partyCharacterIds.Contains(item.Data.CharacterId));
            }
        }

        // =========================================================
        // 프로퍼티
        // =========================================================
        public CharacterDisplayData CurrentData =>
            (_currentIndex >= 0 && _currentIndex < _displayData.Count) ? _displayData[_currentIndex] : null;

        public IReadOnlyList<CharacterDisplayData> AllDisplayData => _displayData;
        public int CurrentIndex => _currentIndex;
    }
}
