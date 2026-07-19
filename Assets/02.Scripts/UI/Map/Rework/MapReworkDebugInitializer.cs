using System.Collections.Generic;
using UnityEngine;
using TeamLog.Map;

namespace TeamLog.UI.Map.Rework
{
    /// <summary>
    /// 맵 씬 Rework 디버그 초기화 (Phase UI-2 — 2026-07-19).
    ///
    /// ★ GameRunState / SelectedParty 없이 씬을 직접 Play할 때 사용.
    /// TitleScene → PartySelectionScene 파이프를 거치지 않은 에디터 검증용.
    ///
    /// 동작:
    ///  - HeaderController가 있으면 더미 값으로 헤더 칩 채움
    ///  - MapReworkView가 있으면 더미 9 레이어 맵 생성하여 표시
    ///
    /// MapSceneSetup이 정상적으로 GameRunState를 초기화한 경우 자동으로 비활성화
    /// (Awake에서 GameRunState.Instance 존재 여부 체크).
    /// </summary>
    [AddComponentMenu("TeamLog/UI/Map Rework/Debug Initializer")]
    public class MapReworkDebugInitializer : MonoBehaviour
    {
        [SerializeField] private HeaderController _headerController;
        [SerializeField] private MapReworkView _mapReworkView;
        [SerializeField] private ThemeBanner _themeBanner;
        [SerializeField] private bool _autoRunOnStart = true;

        private bool _initialized = false;

        private void Awake()
        {
            // 자동 바인딩 — 같은 씬의 컴포넌트 탐색
            // ★ 주의: Awake에서 enabled=false로 끄면 Start()가 호출되지 않음 (Unity 함정).
            // GameRunState 체크는 Start에서만 수행.
            if (_headerController == null)
                _headerController = FindObjectOfType<HeaderController>(true);
            if (_mapReworkView == null)
                _mapReworkView = FindObjectOfType<MapReworkView>(true);
            if (_themeBanner == null)
                _themeBanner = FindObjectOfType<ThemeBanner>(true);
        }

        private void Start()
        {
            // ★ 초기화는 Update 1프레임 뒤에 수행 — 모든 Awake/Start가 끝난 후
            // GameRunState.Instance가 다른 Start()에서 설정될 수 있으므로.
        }

        private void Update()
        {
            if (_initialized) return;
            if (!_autoRunOnStart)
            {
                _initialized = true;
                return;
            }

            // 첫 프레임에 모든 Start()가 완료된 상태에서 체크
            _initialized = true;
            if (GameRunState.Instance != null) return; // 정상 파이프 진입 시 스킵

            RunDebugInit();
        }

        /// <summary>
        /// 디버그용 더미 데이터로 헤더/맵/배너 초기화.
        /// </summary>
        public void RunDebugInit()
        {
            Debug.Log("[MapReworkDebugInitializer] 디버그 초기화 실행 — 더미 데이터 표시");

            if (_headerController != null)
            {
                _headerController.RefreshWithDummy(
                    floor: 1,
                    totalFloors: 4,
                    gold: 50,
                    ascLevel: 0,
                    themeName: "Grey Forest");
            }

            if (_themeBanner != null)
            {
                // StageThemeData 에셋을 에디터에서 로드하지 않고 더미 텍스트만 표시
                _themeBanner.Initialize(BuildDummyTheme(), 1);
            }

            if (_mapReworkView != null)
            {
                var dummyFloor = BuildDummyFloor();
                _mapReworkView.Initialize(dummyFloor, OnDummyNodeClicked);
            }
        }

        private void OnDummyNodeClicked(MapNode node)
        {
            Debug.Log($"[MapReworkDebugInitializer] 더미 노드 클릭: {node}");
        }

        /// <summary>
        /// 더미 9 레이어 맵 생성 (Start → Battle → Event → Battle/Elite → Event → Battle → Event → Battle/Elite → Boss).
        /// MapGenerator의 표준 9 레이어 구조와 동일.
        /// </summary>
        private static MapFloor BuildDummyFloor()
        {
            var floor = new MapFloor(1);

            // 9 레이어 × 단일/분기 노드
            var layer0 = floor.AddLayer(); // Start
            var layer1 = floor.AddLayer(); // Battle
            var layer2 = floor.AddLayer(); // Event (NonCombat)
            var layer3 = floor.AddLayer(); // Battle + Elite (분기)
            var layer4 = floor.AddLayer(); // Event
            var layer5 = floor.AddLayer(); // Battle
            var layer6 = floor.AddLayer(); // Event
            var layer7 = floor.AddLayer(); // Battle + Elite (분기)
            var layer8 = floor.AddLayer(); // Boss

            var start = new MapNode(MapNodeType.Start, 0, 0);
            var b1 = new MapNode(MapNodeType.Battle, 1, 0);
            var e1 = new MapNode(MapNodeType.Event, 2, 0);
            var b2 = new MapNode(MapNodeType.Battle, 3, 0);
            var el1 = new MapNode(MapNodeType.Elite, 3, 1);
            var e2 = new MapNode(MapNodeType.Event, 4, 0);
            var b3 = new MapNode(MapNodeType.Battle, 5, 0);
            var e3 = new MapNode(MapNodeType.Event, 6, 0);
            var b4 = new MapNode(MapNodeType.Battle, 7, 0);
            var el2 = new MapNode(MapNodeType.Elite, 7, 1);
            var boss = new MapNode(MapNodeType.Boss, 8, 0);

            layer0.Add(start);
            layer1.Add(b1);
            layer2.Add(e1);
            layer3.Add(b2); layer3.Add(el1);
            layer4.Add(e2);
            layer5.Add(b3);
            layer6.Add(e3);
            layer7.Add(b4); layer7.Add(el2);
            layer8.Add(boss);

            // 연결 (단순 선형 + 분기 병합)
            start.AddConnection(b1);
            b1.AddConnection(e1);
            e1.AddConnection(b2); e1.AddConnection(el1);
            b2.AddConnection(e2); el1.AddConnection(e2);
            e2.AddConnection(b3);
            b3.AddConnection(e3);
            e3.AddConnection(b4); e3.AddConnection(el2);
            b4.AddConnection(boss); el2.AddConnection(boss);

            floor.StartFloor();
            return floor;
        }

        /// <summary>
        /// 더미 StageThemeData — 씬에서 직접 에셋 로드하지 않고 텍스트만 표시하기 위한 임시 객체.
        /// </summary>
        private static StageThemeData BuildDummyTheme()
        {
            var theme = ScriptableObject.CreateInstance<StageThemeData>();
            theme.displayName = "Grey Forest";
            theme.stageNumber = 1;
            theme.description = "고요한 숲. 안개 속에서 오래된 발자국이 보인다.";
            theme.themeKeywords = new List<string> { "Verdant", "Foggy", "Ancient" };
            return theme;
        }
    }
}
