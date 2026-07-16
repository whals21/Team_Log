using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using TeamLog.Characters;
using TeamLog.Combat;
using TeamLog.Map;
using TeamLog.Reward;
using TeamLog.Skill;
using TeamLog.UI;

namespace TeamLog.EditorDebug
{
    /// <summary>
    /// 인터랙티브 전투 테스트 씬(BattleTestScene) 진입점.
    /// 단일 씬에서 "설정 패널 → 전투 → 설정 패널" 사이클을 자기 리로드로 구현.
    ///
    /// 흐름:
    ///   1. 설정 모드(_pendingTestBattle == false): 드롭다운 표시, BattleSceneSetup GO 비활성
    ///   2. "전투 시작" 클릭: 파티/적/유물 생성 → BattleSceneSetup.SetBattleData + SetReturnScene("BattleTestScene")
    ///      → _pendingTestBattle = true → FadeToScene("BattleTestScene") 자기 리로드
    ///   3. 전투 모드(_pendingTestBattle == true): BattleSceneSetup GO 활성화 → BattleSceneSetup.Start()가 _pending 데이터 소비
    ///   4. 전투 종료: BattleSceneSetup.BattleEndTransition이 _returnSceneName(="BattleTestScene")으로 자기 리로드
    ///   5. OnDestroy에서 _pendingTestBattle = false → 다시 설정 모드 진입
    ///
    /// 드롭다운 선택 인덱스는 static 필드로 보존되어 사이클 간 유지.
    /// </summary>
    public class BattleTestSceneSetup : MonoBehaviour
    {
        [Header("에셋 풀 (빌더가 자동 바인딩)")]
        [SerializeField] private CharacterData[] _allPlayers;
        [SerializeField] private CharacterData[] _allEnemies;     // 일반 + 엘리트 통합
        [SerializeField] private CharacterData[] _allBosses;
        [SerializeField] private RelicData[] _allRelics;

        [Header("UI References")]
        [SerializeField] private TMP_Dropdown[] _partySlots = new TMP_Dropdown[4];
        [SerializeField] private TMP_Dropdown[] _relicSlots = new TMP_Dropdown[6];
        [SerializeField] private TMP_Dropdown[] _enemySlots = new TMP_Dropdown[4];
        [SerializeField] private TMP_Dropdown _floorDropdown;
        [SerializeField] private Toggle _bossToggle;
        [SerializeField] private Button _startButton;
        [SerializeField] private GameObject _configPanel;
        [SerializeField] private GameObject _battleSceneSetupGO; // BattleSceneSetup 컴포넌트 보유 GO
        [SerializeField] private Canvas _battleUICanvas; // 전투 모드 진입 시 활성화 (설정 모드에선 렌더링/레이캐스트 차단)

        [Header("Template UI — 파티")]
        [SerializeField] private TMP_InputField _partyTemplateNameInput;
        [SerializeField] private TMP_Dropdown _partyTemplateDropdown;
        [SerializeField] private Button _partyTemplateSaveButton;
        [SerializeField] private Button _partyTemplateLoadButton;
        [SerializeField] private Button _partyTemplateDeleteButton;

        [Header("Template UI — 유물")]
        [SerializeField] private TMP_InputField _relicTemplateNameInput;
        [SerializeField] private TMP_Dropdown _relicTemplateDropdown;
        [SerializeField] private Button _relicTemplateSaveButton;
        [SerializeField] private Button _relicTemplateLoadButton;
        [SerializeField] private Button _relicTemplateDeleteButton;

        [Header("Template UI — 적")]
        [SerializeField] private TMP_InputField _enemyTemplateNameInput;
        [SerializeField] private TMP_Dropdown _enemyTemplateDropdown;
        [SerializeField] private Button _enemyTemplateSaveButton;
        [SerializeField] private Button _enemyTemplateLoadButton;
        [SerializeField] private Button _enemyTemplateDeleteButton;

        // ── 사이클 간 상태 보존 (static) ──
        // _pendingTestBattle == true 이면 다음 Start()에서 전투 모드 진입
        private static bool _pendingTestBattle;

        // 드롭다운 선택 인덱스 (다음 설정 패널에서 복원)
        private static int[] _lastPartyIndices = { 1, 2, 3, 4 }; // W/M/H/R 기본
        private static int[] _lastRelicIndices = { 0, 0, 0, 0, 0, 0 };
        private static int[] _lastEnemyIndices = { 1, 1, 1, 0 };
        private static int _lastFloorIndex = 0; // F1
        private static bool _lastIsBoss = false;

        // 템플릿 스토어 — Play 세션 내 캐싱, 디스크에서 1회 로드. 씬 리로드 간 유지.
        private static BattleTestTemplateStore _templates;
        private static bool _templatesLoaded;

        private void Start()
        {
            if (_pendingTestBattle)
            {
                // 전투 모드 — 설정 패널 숨기고 BattleSceneSetup + BattleUICanvas 활성화
                // BattleSceneSetup.Start()는 _pendingParty/_pendingEnemies를 소비 (이미 static으로 주입됨)

                // ★ 플래그 소비 — OnDestroy가 아닌 Start에서 clearing해야
                //   config→battle 전환 시 옛 인스턴스 OnDestroy()가 새 인스턴스 Start()보다 먼저 실행되어
                //   플래그가 premature clear되는 버그 방지
                _pendingTestBattle = false;

                if (_configPanel != null) _configPanel.SetActive(false);
                // BattleUICanvas 렌더링 활성화 (빌더에서 비활성화된 상태로 저장됨)
                if (_battleUICanvas != null) _battleUICanvas.enabled = true;
                if (_battleSceneSetupGO != null) _battleSceneSetupGO.SetActive(true);
            }
            else
            {
                // 설정 모드 — BattleUICanvas 비활성화 + BattleSceneSetup 비활성 + 패널 표시
                // Canvas.enabled=false: 렌더링/레이캐스트만 차단 (GO는 활성 상태 유지 → Awake 정상 호출)
                if (_battleUICanvas != null) _battleUICanvas.enabled = false;
                if (_battleSceneSetupGO != null) _battleSceneSetupGO.SetActive(false);
                if (_configPanel != null) _configPanel.SetActive(true);

                PopulateDropdowns();
                RestoreSelections();

                if (_startButton != null)
                    _startButton.onClick.AddListener(OnStartBattleClicked);

                if (_bossToggle != null)
                {
                    _bossToggle.onValueChanged.AddListener(OnBossToggleChanged);
                    // 초기 상태 동기화
                    OnBossToggleChanged(_bossToggle.isOn);
                }

                InitTemplates();
            }
        }

        private void OnBossToggleChanged(bool isBoss)
        {
            // 보스 모드면 적 슬롯 1만 사용, 나머지 비활성
            for (int i = 1; i < _enemySlots.Length; i++)
            {
                if (_enemySlots[i] != null)
                    _enemySlots[i].interactable = !isBoss;
            }
        }

        private void PopulateDropdowns()
        {
            // 파티 슬롯 — "(없음)" + 플레이어 캐릭터
            for (int i = 0; i < _partySlots.Length; i++)
            {
                if (_partySlots[i] == null) continue;
                _partySlots[i].ClearOptions();
                var options = new List<string> { "(없음)" };
                if (_allPlayers != null)
                    foreach (var data in _allPlayers)
                        options.Add(data != null ? data.CharacterName : "(null)");
                _partySlots[i].AddOptions(options);
            }

            // 유물 슬롯 — "(없음)" + 전체 유물
            for (int i = 0; i < _relicSlots.Length; i++)
            {
                if (_relicSlots[i] == null) continue;
                _relicSlots[i].ClearOptions();
                var options = new List<string> { "(없음)" };
                if (_allRelics != null)
                    foreach (var data in _allRelics)
                        options.Add(data != null ? data.RelicName : "(null)");
                _relicSlots[i].AddOptions(options);
            }

            // 적 슬롯 — "(없음)" + 일반 적 + 엘리트 적 (통합)
            for (int i = 0; i < _enemySlots.Length; i++)
            {
                if (_enemySlots[i] == null) continue;
                _enemySlots[i].ClearOptions();
                var options = new List<string> { "(없음)" };
                if (_allEnemies != null)
                    foreach (var data in _allEnemies)
                        options.Add(data != null ? data.CharacterName : "(null)");
                _enemySlots[i].AddOptions(options);
            }

            // 층 드롭다운
            if (_floorDropdown != null)
            {
                _floorDropdown.ClearOptions();
                _floorDropdown.AddOptions(new List<string>
                {
                    "F1 (x1.0)", "F2 (x1.3)", "F3 (x1.6)", "F4 (x2.0)"
                });
            }
        }

        private void RestoreSelections()
        {
            for (int i = 0; i < _partySlots.Length && i < _lastPartyIndices.Length; i++)
                if (_partySlots[i] != null) _partySlots[i].value = _lastPartyIndices[i];

            for (int i = 0; i < _relicSlots.Length && i < _lastRelicIndices.Length; i++)
                if (_relicSlots[i] != null) _relicSlots[i].value = _lastRelicIndices[i];

            for (int i = 0; i < _enemySlots.Length && i < _lastEnemyIndices.Length; i++)
                if (_enemySlots[i] != null) _enemySlots[i].value = _lastEnemyIndices[i];

            if (_floorDropdown != null) _floorDropdown.value = _lastFloorIndex;
            if (_bossToggle != null) _bossToggle.isOn = _lastIsBoss;
        }

        private void CaptureSelections()
        {
            for (int i = 0; i < _partySlots.Length; i++)
                if (_partySlots[i] != null) _lastPartyIndices[i] = _partySlots[i].value;

            for (int i = 0; i < _relicSlots.Length; i++)
                if (_relicSlots[i] != null) _lastRelicIndices[i] = _relicSlots[i].value;

            for (int i = 0; i < _enemySlots.Length; i++)
                if (_enemySlots[i] != null) _lastEnemyIndices[i] = _enemySlots[i].value;

            if (_floorDropdown != null) _lastFloorIndex = _floorDropdown.value;
            if (_bossToggle != null) _lastIsBoss = _bossToggle.isOn;
        }

        private void OnStartBattleClicked()
        {
            CaptureSelections();

            int floor = _lastFloorIndex + 1; // F1=1, F2=2, ...

            // 파티 생성
            var party = BattleTestConfig.BuildParty(_allPlayers, _lastPartyIndices);
            if (party.Count == 0)
            {
                Debug.LogWarning("[BattleTestSceneSetup] 파티가 비어 있습니다. 최소 1명 이상 선택하세요.");
                ShowToast("파티를 최소 1명 선택하세요!");
                return;
            }

            // 적 생성
            var enemies = BattleTestConfig.BuildEnemies(
                _allEnemies, _allBosses, _lastEnemyIndices, floor, _lastIsBoss);
            if (enemies.Count == 0)
            {
                Debug.LogWarning("[BattleTestSceneSetup] 적이 비어 있습니다. 최소 1마리 이상 선택하세요.");
                ShowToast("적을 최소 1마리 선택하세요!");
                return;
            }

            // GameRunState — 유물 작동을 위해 싱글톤 설정 (기존 잔여 클린업 후 재생성)
            GameRunState.Destroy();
            var runState = GameRunState.Create(party, 100);
            var relicList = _allRelics != null ? new List<RelicData>(_allRelics) : new List<RelicData>();
            runState.SetDataPools(relicList, new List<AugmentData>());
            // ★ RelicHandler 생명주기: SetPlayerParty + SubscribeEvents 명시 호출 필수
            // (BalanceSimulator.Synergy.cs 모범 패턴 — GameRunState.Create만 호출하면 미구독 상태로 유물 무효)
            runState.RelicHandler.SetPlayerParty(party);
            runState.RelicHandler.SubscribeEvents();

            // 유물 지급
            foreach (var relicIdx in _lastRelicIndices)
            {
                if (relicIdx <= 0 || _allRelics == null || relicIdx > _allRelics.Length) continue;
                var relic = _allRelics[relicIdx - 1];
                if (relic != null) runState.AcquireRelic(relic);
            }

            // BattleSceneSetup에 데이터 주입 (static 필드 → 씬 리로드 후에도 유지)
            BattleSceneSetup.SetBattleData(party, enemies, 0, isBossBattle: _lastIsBoss);
            BattleSceneSetup.SetReturnScene("BattleTestScene");
            BattleResult.SetBattleType(_lastIsBoss ? MapNodeType.Boss : MapNodeType.Battle);

            _pendingTestBattle = true;

            // 자기 리로드 — 씬 전환 중 BattleTestSceneSetup이 파괴되어도 static 필드는 유지
            if (SceneTransition.Instance != null)
                SceneTransition.Instance.FadeToScene("BattleTestScene");
            else
                SceneManager.LoadScene("BattleTestScene");
        }

        /// <summary>
        /// 간단한 토스트 알림 — ToastUI.Show는 내부에서 싱글톤 자동 생성.
        /// </summary>
        private void ShowToast(string message)
        {
            ToastUI.Show(message);
        }

        // ══════════════════════════════════════════════════════════
        //  템플릿 시스템 — 파티/유물/적 조합 저장·불러오기·삭제
        // ══════════════════════════════════════════════════════════

        private enum TemplateCategory { Party, Relic, Enemy }

        private void InitTemplates()
        {
            if (!_templatesLoaded)
            {
                _templates = BattleTestTemplateStore.Load();
                _templatesLoaded = true;

                // 3 카테고리 모두 비어 있으면 컨셉별 기본 템플릿 자동 생성
                if (_templates.party.Count == 0 && _templates.relic.Count == 0 && _templates.enemy.Count == 0)
                {
                    PopulateDefaultTemplates();
                    _templates.Save();
                    Debug.Log("[BattleTestSceneSetup] 기본 컨셉 템플릿 자동 생성 완료");
                }
            }
            RefreshAllTemplateDropdowns();

            WireTemplateButton(_partyTemplateSaveButton, TemplateCategory.Party, SaveTemplate);
            WireTemplateButton(_partyTemplateLoadButton, TemplateCategory.Party, LoadTemplate);
            WireTemplateButton(_partyTemplateDeleteButton, TemplateCategory.Party, DeleteTemplate);
            WireTemplateButton(_relicTemplateSaveButton, TemplateCategory.Relic, SaveTemplate);
            WireTemplateButton(_relicTemplateLoadButton, TemplateCategory.Relic, LoadTemplate);
            WireTemplateButton(_relicTemplateDeleteButton, TemplateCategory.Relic, DeleteTemplate);
            WireTemplateButton(_enemyTemplateSaveButton, TemplateCategory.Enemy, SaveTemplate);
            WireTemplateButton(_enemyTemplateLoadButton, TemplateCategory.Enemy, LoadTemplate);
            WireTemplateButton(_enemyTemplateDeleteButton, TemplateCategory.Enemy, DeleteTemplate);
        }

        private static void WireTemplateButton(Button btn, TemplateCategory cat, System.Action<TemplateCategory> handler)
        {
            if (btn == null) return;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => handler(cat));
        }

        private void RefreshAllTemplateDropdowns()
        {
            RefreshTemplateDropdown(TemplateCategory.Party);
            RefreshTemplateDropdown(TemplateCategory.Relic);
            RefreshTemplateDropdown(TemplateCategory.Enemy);
        }

        private void RefreshTemplateDropdown(TemplateCategory cat)
        {
            var dropdown = GetTemplateDropdown(cat);
            var list = GetTemplateList(cat);
            if (dropdown == null) return;

            int prev = dropdown.value;
            dropdown.ClearOptions();
            var options = new List<string>();
            if (list != null)
                foreach (var t in list)
                    options.Add(t.name ?? "(이름 없음)");
            dropdown.AddOptions(options);
            if (options.Count > 0)
                dropdown.value = Mathf.Clamp(prev, 0, options.Count - 1);
            dropdown.RefreshShownValue();
        }

        private void SaveTemplate(TemplateCategory cat)
        {
            if (_templates == null) return;
            CaptureSelections(); // 드롭다운 → static 동기화

            var input = GetTemplateInput(cat);
            var name = input != null ? (input.text ?? "").Trim() : "";
            if (string.IsNullOrEmpty(name))
            {
                ShowToast("템플릿 이름을 입력하세요!");
                return;
            }

            var list = GetTemplateList(cat);
            // 동일 이름 덮어쓰기
            int existing = list.FindIndex(t => (t.name ?? "") == name);
            if (existing >= 0) list.RemoveAt(existing);

            var tmpl = new BattleTestTemplate { name = name };
            switch (cat)
            {
                case TemplateCategory.Party:
                    tmpl.indices = (int[])_lastPartyIndices.Clone();
                    break;
                case TemplateCategory.Relic:
                    tmpl.indices = (int[])_lastRelicIndices.Clone();
                    break;
                case TemplateCategory.Enemy:
                    tmpl.indices = (int[])_lastEnemyIndices.Clone();
                    tmpl.floorIndex = _lastFloorIndex;
                    tmpl.isBoss = _lastIsBoss;
                    break;
            }
            list.Add(tmpl);
            _templates.Save();
            RefreshTemplateDropdown(cat);

            // 새로 저장된 템플릿 선택
            var dropdown = GetTemplateDropdown(cat);
            if (dropdown != null)
            {
                int idx = list.FindIndex(t => (t.name ?? "") == name);
                if (idx >= 0)
                {
                    dropdown.value = idx;
                    dropdown.RefreshShownValue();
                }
            }
            ShowToast($"{GetCategoryLabel(cat)} 템플릿 저장: {name}");
        }

        private void LoadTemplate(TemplateCategory cat)
        {
            if (_templates == null) return;
            var dropdown = GetTemplateDropdown(cat);
            var list = GetTemplateList(cat);
            if (dropdown == null || list == null) return;
            int idx = dropdown.value;
            if (idx < 0 || idx >= list.Count) return;

            var tmpl = list[idx];
            if (tmpl.indices == null) return;

            switch (cat)
            {
                case TemplateCategory.Party:
                    for (int i = 0; i < _partySlots.Length && i < tmpl.indices.Length; i++)
                        if (_partySlots[i] != null) _partySlots[i].value = tmpl.indices[i];
                    break;
                case TemplateCategory.Relic:
                    for (int i = 0; i < _relicSlots.Length && i < tmpl.indices.Length; i++)
                        if (_relicSlots[i] != null) _relicSlots[i].value = tmpl.indices[i];
                    break;
                case TemplateCategory.Enemy:
                    for (int i = 0; i < _enemySlots.Length && i < tmpl.indices.Length; i++)
                        if (_enemySlots[i] != null) _enemySlots[i].value = tmpl.indices[i];
                    if (_floorDropdown != null) _floorDropdown.value = tmpl.floorIndex;
                    if (_bossToggle != null)
                    {
                        _bossToggle.isOn = tmpl.isBoss;
                        OnBossToggleChanged(tmpl.isBoss);
                    }
                    break;
            }
            CaptureSelections();
            ShowToast($"{GetCategoryLabel(cat)} 템플릿 불러오기: {tmpl.name}");
        }

        private void DeleteTemplate(TemplateCategory cat)
        {
            if (_templates == null) return;
            var dropdown = GetTemplateDropdown(cat);
            var list = GetTemplateList(cat);
            if (dropdown == null || list == null || list.Count == 0) return;
            int idx = dropdown.value;
            if (idx < 0 || idx >= list.Count) return;

            string name = list[idx].name;
            list.RemoveAt(idx);
            _templates.Save();
            RefreshTemplateDropdown(cat);
            ShowToast($"{GetCategoryLabel(cat)} 템플릿 삭제: {name}");
        }

        private List<BattleTestTemplate> GetTemplateList(TemplateCategory cat)
        {
            if (_templates == null) return null;
            return cat switch
            {
                TemplateCategory.Party => _templates.party,
                TemplateCategory.Relic => _templates.relic,
                TemplateCategory.Enemy => _templates.enemy,
                _ => _templates.party
            };
        }

        private TMP_Dropdown GetTemplateDropdown(TemplateCategory cat)
        {
            return cat switch
            {
                TemplateCategory.Party => _partyTemplateDropdown,
                TemplateCategory.Relic => _relicTemplateDropdown,
                TemplateCategory.Enemy => _enemyTemplateDropdown,
                _ => null
            };
        }

        private TMP_InputField GetTemplateInput(TemplateCategory cat)
        {
            return cat switch
            {
                TemplateCategory.Party => _partyTemplateNameInput,
                TemplateCategory.Relic => _relicTemplateNameInput,
                TemplateCategory.Enemy => _enemyTemplateNameInput,
                _ => null
            };
        }

        private static string GetCategoryLabel(TemplateCategory cat)
        {
            return cat switch
            {
                TemplateCategory.Party => "파티",
                TemplateCategory.Relic => "유물",
                TemplateCategory.Enemy => "적",
                _ => ""
            };
        }

        // ══════════════════════════════════════════════════════════
        //  기본 컨셉 템플릿 — 이름 기반 조회 (에셋 순서 무관)
        // ══════════════════════════════════════════════════════════

        /// <summary>에셋 파일명(data.name)으로 플레이어 드롭다운 인덱스 조회. 0 = "(없음)".</summary>
        private int FindPlayerIndexByName(string assetName)
        {
            if (_allPlayers == null || string.IsNullOrEmpty(assetName)) return 0;
            for (int i = 0; i < _allPlayers.Length; i++)
                if (_allPlayers[i] != null && _allPlayers[i].name == assetName) return i + 1;
            Debug.LogWarning($"[BattleTestSceneSetup] 플레이어 에셋 없음: {assetName}");
            return 0;
        }

        /// <summary>에셋 파일명(data.name)으로 적 드롭다운 인덱스 조회. 0 = "(없음)".</summary>
        private int FindEnemyIndexByName(string assetName)
        {
            if (_allEnemies == null || string.IsNullOrEmpty(assetName)) return 0;
            for (int i = 0; i < _allEnemies.Length; i++)
                if (_allEnemies[i] != null && _allEnemies[i].name == assetName) return i + 1;
            Debug.LogWarning($"[BattleTestSceneSetup] 적 에셋 없음: {assetName}");
            return 0;
        }

        /// <summary>에셋 파일명(data.name)으로 유물 드롭다운 인덱스 조회. 0 = "(없음)".</summary>
        private int FindRelicIndexByName(string assetName)
        {
            if (_allRelics == null || string.IsNullOrEmpty(assetName)) return 0;
            for (int i = 0; i < _allRelics.Length; i++)
                if (_allRelics[i] != null && _allRelics[i].name == assetName) return i + 1;
            Debug.LogWarning($"[BattleTestSceneSetup] 유물 에셋 없음: {assetName}");
            return 0;
        }

        /// <summary>
        /// 게임 컨셉별 주요 조합 프리셋 생성.
        /// 이름 기반 조회이므로 에셋 배열 순서가 바뀌어도 올바른 인덱스 매핑.
        /// </summary>
        private void PopulateDefaultTemplates()
        {
            // ── 파티 템플릿 (4 슬롯) ── Phase CC-2A GC: Warrior/Mage 제거, 신규 캐릭터로 대체
            AddPartyTemplate("균형 파티", "Char_Duran", "Char_Ashe", "Char_Healer", "Char_Umbra");
            AddPartyTemplate("물리 특화", "Char_Duran", "Char_Umbra", "Char_Archer", "Char_Bard");
            AddPartyTemplate("마법 폭격", "Char_Ashe", "Char_Lumi", "Char_Sibyl", "Char_Taranis");
            AddPartyTemplate("생존 극대화", "Char_Duran", "Char_Healer", "Char_Alchemist", "Char_Bard");
            AddPartyTemplate("그림자 암살", "Char_Umbra", "Char_Archer", "Char_Bard", "Char_Necromancer");

            // ── 유물 템플릿 (6 슬롯) ──
            AddRelicTemplate("화력 증강",
                "Relic_BurningSword", "Relic_WeaponStone", "Relic_CriticalFocus",
                "Relic_BerserkerMark", "Relic_WarBanner", "Relic_ExecutionerBlade");
            AddRelicTemplate("흡혈 생존",
                "Relic_VampireFang", "Relic_BloodFeast", "Relic_SanguineBond",
                "Relic_LifeCrystal", "Relic_DragonHeart", "Relic_IronHide");
            AddRelicTemplate("철벽 방어",
                "Relic_ShieldAmulet", "Relic_HardShell", "Relic_IronHide",
                "Relic_ThornArmor", "Relic_VowOfGuardian", "Relic_AegisCharm");
            AddRelicTemplate("파티 버프",
                "Relic_WarBanner", "Relic_UnitedFront", "Relic_BrothersInArms",
                "Relic_ArcaneFocus", "Relic_SpellWeaver", "Relic_WeaponStone");
            AddRelicTemplate("밸런스",
                "Relic_BurningSword", "Relic_VampireFang", "Relic_ShieldAmulet",
                "Relic_LuckyClover", "Relic_CardShark", "Relic_HealingHerb");

            // ── 적 템플릿 (4 슬롯 + floor + boss) ──
            // 보스 모드: floor로 보스 자동 선택 (F1=GoblinKing, F2=Dragon, F3+=DemonLord)
            AddEnemyTemplate("F1 기본전투", 1, false, "Enemy_Slime", "Enemy_Goblin", "Enemy_Wolf", null);
            AddEnemyTemplate("F1 군단전", 1, false, "Enemy_Slime", "Enemy_Goblin", "Enemy_Wolf", "Enemy_Bat");
            AddEnemyTemplate("F2 언데드", 2, false, "Enemy_Skeleton", "Enemy_SkeletonArcher", "Enemy_Mummy", "Enemy_Wraith");
            AddEnemyTemplate("F2 엘리트전", 2, false, "Enemy_EliteDarkSlime", "Enemy_EliteGoblinShaman", "Enemy_EliteKnight", null);
            AddEnemyTemplate("F1 보스", 1, true);
            AddEnemyTemplate("F2 보스", 2, true);
            AddEnemyTemplate("F4 최종보스", 4, true);
        }

        private void AddPartyTemplate(string name, params string[] assetNames)
        {
            var indices = new int[4];
            for (int i = 0; i < 4 && i < assetNames.Length; i++)
                indices[i] = FindPlayerIndexByName(assetNames[i]);
            _templates.party.Add(new BattleTestTemplate { name = name, indices = indices });
        }

        private void AddRelicTemplate(string name, params string[] assetNames)
        {
            var indices = new int[6];
            for (int i = 0; i < 6 && i < assetNames.Length; i++)
                indices[i] = FindRelicIndexByName(assetNames[i]);
            _templates.relic.Add(new BattleTestTemplate { name = name, indices = indices });
        }

        private void AddEnemyTemplate(string name, int floor, bool isBoss, params string[] assetNames)
        {
            var indices = new int[4];
            for (int i = 0; i < 4 && i < assetNames.Length; i++)
                indices[i] = FindEnemyIndexByName(assetNames[i]);
            _templates.enemy.Add(new BattleTestTemplate
            {
                name = name,
                indices = indices,
                floorIndex = floor - 1, // F1=0, F2=1, F3=2, F4=3 (드롭다운 value)
                isBoss = isBoss
            });
        }

        private void OnDestroy()
        {
            // ★ 플래그 클리어 금지 — Start()에서 consume
            // 옛 인스턴스 OnDestroy() → 새 인스턴스 Start() 순서로 실행되므로
            // 여기서 clear하면 config→battle 전환이 실패함
        }
    }
}
