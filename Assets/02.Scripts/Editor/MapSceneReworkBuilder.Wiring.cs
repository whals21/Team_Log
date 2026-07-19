#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using TeamLog.UI;
using TeamLog.UI.Event;  // ★ Phase EVENT — EventReworkView
using TeamLog.UI.Shop;   // ★ Phase SHOP — ShopReworkView
using TeamLog.UI.Map.Rework;

namespace TeamLog.Editor
{
    /// <summary>
    /// MapSceneReworkBuilder Wiring partial — 프리팹 자동 생성 + 인스펙터 필드 자동 연결.
    ///
    /// ★ CLAUDE.md 가드레일 #15 (★ update_component 참조 필드 한계) 회피:
    /// Unity Object 참조 필드(Sprite/Prefab/GameObject)는 일반 직렬화로 할당 불가.
    /// SerializedObject.FindProperty + objectReferenceValue + ApplyModifiedProperties 패턴 사용.
    /// </summary>
    public static partial class MapSceneReworkBuilder
    {
        private const string PREFAB_DIR = "Assets/03.Data/UI/MapScene/Prefabs";
        private const string SPRITE_DIR = "Assets/03.Data/UI/MapScene";
        private const string SHARED_SPRITE_DIR = "Assets/03.Data/UI/PartySelection"; // ★ PartySelection과 공유 Sprite

        // =========================================================
        // 1. 프리팹 9종 생성 (★ Phase A: RelicSlotCellPrefab, AugmentRowPrefab / ★ Node Detail Preview: EnemyRowPrefab, RewardRowPrefab)
        // =========================================================
        private static void BuildAllPrefabs()
        {
            EnsurePrefabDirectory();

            BuildNodePrefab();
            BuildLabelPrefab("BranchLabelPrefab", "CHOICE OF PATH", UIPalette.Default.DFGold, 9);
            BuildLabelPrefab("BetweenLabelPrefab", "— between battles —", UIPalette.Default.DFInkFaint, 10, italic: true);
            BuildPlayerMarkerPrefab();
            BuildPartyMemberRowPrefab();
            BuildRelicSlotCellPrefab();  // ★ Phase A
            BuildAugmentRowPrefab();     // ★ Phase A
            BuildEnemyRowPrefab();       // ★ Node Detail Preview
            BuildRewardRowPrefab();      // ★ Node Detail Preview

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsurePrefabDirectory()
        {
            if (!AssetDatabase.IsValidFolder(PREFAB_DIR))
            {
                if (!AssetDatabase.IsValidFolder("Assets/03.Data/UI/MapScene"))
                    AssetDatabase.CreateFolder("Assets/03.Data/UI", "MapScene");
                AssetDatabase.CreateFolder("Assets/03.Data/UI/MapScene", "Prefabs");
            }
        }

        /// <summary>
        /// 노드 프리팹 — MapReworkNode + Icon + FrameGlow + Label 자식 구조.
        /// </summary>
        private static void BuildNodePrefab()
        {
            var go = new GameObject("NodePrefab", typeof(RectTransform), typeof(CanvasRenderer));
            var rt = go.GetComponent<RectTransform>();
            // ★ Priority 6 (치명 수정): anchor를 (0.5,0.5) 단일 모드로 설정해야
            // 런타임에 anchoredPosition (x, y)가 부모 중심에서 (x, y) 떨어진 위치로 정확히 적용됨.
            // 기본 (0,0)/(1,1) stretch 모드면 anchoredPosition이 offsetMin/Max로 해석되어 노드가 겹침.
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(56, 56);

            var node = go.AddComponent<MapReworkNode>();

            // Icon 자식 (raycastTarget=true)
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(go.transform, false);
            var iconRt = iconGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(iconRt);
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.color = Color.white;
            iconImg.raycastTarget = false; // Button이 부모에 있으므로 자식은 false

            // FrameGlow 자식 (비활성 기본, raycastTarget=false)
            var glowGo = new GameObject("FrameGlow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            glowGo.transform.SetParent(go.transform, false);
            var glowRt = glowGo.GetComponent<RectTransform>();
            // FrameGlow는 노드보다 크게 (중앙 정렬, 160x160 → 노드 48x48 주변)
            glowRt.anchorMin = new Vector2(0.5f, 0.5f);
            glowRt.anchorMax = new Vector2(0.5f, 0.5f);
            glowRt.pivot = new Vector2(0.5f, 0.5f);
            glowRt.sizeDelta = new Vector2(96, 96);
            glowRt.anchoredPosition = Vector2.zero;
            var glowImg = glowGo.GetComponent<Image>();
            glowImg.color = Color.white;
            glowImg.raycastTarget = false;
            glowGo.SetActive(false);

            // NodeLabel 자식 (TMP)
            var labelGo = new GameObject("NodeLabel", typeof(RectTransform), typeof(CanvasRenderer));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0.5f, 0);
            labelRt.anchorMax = new Vector2(0.5f, 0);
            labelRt.pivot = new Vector2(0.5f, 1);
            labelRt.sizeDelta = new Vector2(120, 20);
            labelRt.anchoredPosition = new Vector2(0, -6);
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.font = FontLabel();
            labelTmp.fontSize = 10;
            labelTmp.alignment = TextAlignmentOptions.Center;
            labelTmp.color = UIPalette.Default.DFGoldL;
            labelTmp.raycastTarget = false;

            // Button 컴포넌트 (MapReworkNode가 사용)
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = iconImg;
            iconImg.raycastTarget = true;

            SavePrefab(go, "NodePrefab.prefab");
            Object.DestroyImmediate(go);
        }

        /// <summary>
        /// 라벨 프리팹 — 단순 TextMeshProUGUI (분기 라벨 / 비전투 라벨 공용 패턴).
        /// </summary>
        private static void BuildLabelPrefab(string prefabName, string defaultText, Color color, int fontSize, bool italic = false)
        {
            var go = new GameObject(prefabName, typeof(RectTransform), typeof(CanvasRenderer));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(280, 24);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.font = italic ? FontItalic() : FontLabel();
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            if (italic) tmp.fontStyle = FontStyles.Italic;

            SavePrefab(go, $"{prefabName}.prefab");
            Object.DestroyImmediate(go);
        }

        /// <summary>
        /// 플레이어 마커 프리팹 — Image 하나.
        /// </summary>
        private static void BuildPlayerMarkerPrefab()
        {
            var go = new GameObject("PlayerMarkerPrefab", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(96, 96);

            var img = go.GetComponent<Image>();
            img.color = Color.white;
            img.raycastTarget = false;

            // Sprite 자동 할당
            var markerSprite = LoadSprite("PlayerMarker.png");
            if (markerSprite != null) img.sprite = markerSprite;

            SavePrefab(go, "PlayerMarkerPrefab.prefab");
            Object.DestroyImmediate(go);
        }

        /// <summary>
        /// ★ Phase 5 — 파티 멤버 행 프리팹.
        /// PartyMemberRow 컴포넌트 + 자식 구조 (Portrait/MemberName/MemberClass/HPFill/ResourceValue/ResourceBadge).
        /// AutoBindMissingFields가 자식 이름으로 자동 바인딩하므로 이름 규칙 준수 필수.
        /// </summary>
        private static void BuildPartyMemberRowPrefab()
        {
            var go = new GameObject("PartyMemberRowPrefab", typeof(RectTransform), typeof(CanvasRenderer));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(280, 60);

            // 외곽 Image (카드 배경) — SlatePanel, color 밝게
            var bgImg = go.AddComponent<Image>();
            var slateSprite = LoadSharedSprite("SlatePanel_9Slice.png");
            if (slateSprite != null)
            {
                bgImg.sprite = slateSprite;
                bgImg.type = Image.Type.Sliced;
                bgImg.color = new Color(0.75f, 0.75f, 0.9f, 1f);
            }
            else
            {
                bgImg.color = new Color(0.12f, 0.12f, 0.22f, 0.95f);
            }
            bgImg.raycastTarget = false;

            // CanvasGroup — 사망자 투명도 처리용
            go.AddComponent<CanvasGroup>();

            // HorizontalLayoutGroup: Portrait | (Name/Class/HP) | Resource
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.padding = new RectOffset(8, 8, 6, 6);
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            // Portrait (자원색 원형 + 이니셜 자식)
            var portraitGo = new GameObject("Portrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            portraitGo.transform.SetParent(go.transform, false);
            var portraitImg = portraitGo.GetComponent<Image>();
            portraitImg.color = Color.white;
            portraitImg.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(portraitGo, prefW: 44, prefH: 44);

            // Portrait 자식: 이니셜 TMP
            var initialGo = new GameObject("Initial", typeof(RectTransform), typeof(CanvasRenderer));
            initialGo.transform.SetParent(portraitGo.transform, false);
            var initialRt = initialGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(initialRt);
            var initialTmp = initialGo.AddComponent<TextMeshProUGUI>();
            initialTmp.text = "?";
            initialTmp.font = FontTitle(); // Cinzel Black — 초상화 이니셜
            initialTmp.fontSize = 20;
            initialTmp.color = Color.white;
            initialTmp.alignment = TextAlignmentOptions.Center;
            initialTmp.raycastTarget = false;

            // 중앙 컬럼 (Name / Class / HP 바)
            var centerGo = new GameObject("CenterColumn", typeof(RectTransform));
            centerGo.transform.SetParent(go.transform, false);
            UIAutoBindHelper.EnsureLayoutElement(centerGo, flexW: 1, prefH: 48);
            var centerVlg = centerGo.AddComponent<VerticalLayoutGroup>();
            centerVlg.childControlWidth = true;
            centerVlg.childControlHeight = true;
            centerVlg.spacing = 2;
            centerVlg.childAlignment = TextAnchor.UpperLeft;

            // MemberName (Cinzel Bold)
            var nameGo = new GameObject("MemberName", typeof(RectTransform), typeof(CanvasRenderer));
            nameGo.transform.SetParent(centerGo.transform, false);
            var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
            nameTmp.text = "Hero";
            nameTmp.font = FontLabel();
            nameTmp.fontSize = 13;
            nameTmp.color = UIPalette.Default.DFGoldL;
            nameTmp.alignment = TextAlignmentOptions.Left;
            nameTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(nameGo, flexW: 1, prefH: 18);

            // MemberClass (Cormorant Italic)
            var classGo = new GameObject("MemberClass", typeof(RectTransform), typeof(CanvasRenderer));
            classGo.transform.SetParent(centerGo.transform, false);
            var classTmp = classGo.AddComponent<TextMeshProUGUI>();
            classTmp.text = "Class";
            classTmp.font = FontItalic();
            classTmp.fontStyle = FontStyles.Italic;
            classTmp.fontSize = 10;
            classTmp.color = UIPalette.Default.DFInkDim;
            classTmp.alignment = TextAlignmentOptions.Left;
            classTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(classGo, flexW: 1, prefH: 13);

            // HP 바 (배경 + 채우기)
            var hpBarGo = new GameObject("HPBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            hpBarGo.transform.SetParent(centerGo.transform, false);
            var hpBarImg = hpBarGo.GetComponent<Image>();
            hpBarImg.color = new Color(0.15f, 0.05f, 0.05f, 0.9f); // 어두운 배경
            hpBarImg.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(hpBarGo, flexW: 1, prefH: 8);

            var hpFillGo = new GameObject("HPFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            hpFillGo.transform.SetParent(hpBarGo.transform, false);
            var hpFillRt = hpFillGo.GetComponent<RectTransform>();
            hpFillRt.anchorMin = new Vector2(0, 0);
            hpFillRt.anchorMax = new Vector2(1, 1);
            hpFillRt.pivot = new Vector2(0, 0.5f);
            hpFillRt.offsetMin = Vector2.zero;
            hpFillRt.offsetMax = Vector2.zero;
            var hpFillImg = hpFillGo.GetComponent<Image>();
            hpFillImg.color = UIPalette.Default.HPNormal;
            hpFillImg.raycastTarget = false;
            hpFillImg.fillAmount = 1f;
            hpFillImg.type = Image.Type.Filled;
            hpFillImg.fillMethod = Image.FillMethod.Horizontal;

            // Resource 배지 (우측)
            var badgeGo = new GameObject("ResourceBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            badgeGo.transform.SetParent(go.transform, false);
            var badgeImg = badgeGo.GetComponent<Image>();
            badgeImg.color = Color.white;
            badgeImg.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(badgeGo, prefW: 36, prefH: 36);

            // ResourceValue 자식
            var rvGo = new GameObject("ResourceValue", typeof(RectTransform), typeof(CanvasRenderer));
            rvGo.transform.SetParent(badgeGo.transform, false);
            var rvRt = rvGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(rvRt);
            var rvTmp = rvGo.AddComponent<TextMeshProUGUI>();
            rvTmp.text = "0";
            rvTmp.font = FontLabel();
            rvTmp.fontSize = 15;
            rvTmp.color = Color.white;
            rvTmp.alignment = TextAlignmentOptions.Center;
            rvTmp.raycastTarget = false;

            // ★ PartyMemberRow 컴포넌트 부착 — 자동 바인딩 활성
            go.AddComponent<PartyMemberRow>();

            SavePrefab(go, "PartyMemberRowPrefab.prefab");
            Object.DestroyImmediate(go);
        }

        /// <summary>
        /// ★ Phase A — RelicSlotCell 프리팹 (5×2 그리드의 단일 슬롯).
        /// 웹 목업: 정사각형, 어두운 배경, 골드 테두리, 중앙 Icon + 첫 글자.
        /// </summary>
        private static void BuildRelicSlotCellPrefab()
        {
            var go = new GameObject("RelicSlotCellPrefab", typeof(RectTransform), typeof(CanvasRenderer));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(48, 48);

            // 배경 — SlatePanel + Color 밝게
            var bgImg = go.AddComponent<Image>();
            var slateSprite = LoadSharedSprite("SlatePanel_9Slice.png");
            if (slateSprite != null)
            {
                bgImg.sprite = slateSprite;
                bgImg.type = Image.Type.Sliced;
                bgImg.color = new Color(0.6f, 0.6f, 0.75f, 1f);
            }
            else
            {
                bgImg.color = new Color(0.1f, 0.1f, 0.18f, 1f);
            }
            bgImg.raycastTarget = false;

            // Icon 자식 — 중앙 Image + TMP 이니셜
            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(go.transform, false);
            var iconRt = iconGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(iconRt);
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.color = UIPalette.Default.GradeCursed;
            iconImg.raycastTarget = false;

            // Icon 자식의 TMP (이니셜)
            var initialGo = new GameObject("Initial", typeof(RectTransform), typeof(CanvasRenderer));
            initialGo.transform.SetParent(iconGo.transform, false);
            var initialRt = initialGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(initialRt);
            var initialTmp = initialGo.AddComponent<TextMeshProUGUI>();
            initialTmp.text = "?";
            initialTmp.font = FontTitle();
            initialTmp.fontSize = 16;
            initialTmp.color = Color.white;
            initialTmp.alignment = TextAlignmentOptions.Center;
            initialTmp.raycastTarget = false;

            // RelicSlotCell 컴포넌트
            go.AddComponent<RelicSlotCell>();

            SavePrefab(go, "RelicSlotCellPrefab.prefab");
            Object.DestroyImmediate(go);
        }

        /// <summary>
        /// ★ Phase A — AugmentRow 프리팹.
        /// 웹 목업: 가로 행, 좌측 보더(자원색), Icon 24x24, Name/Owner, Rank 배지.
        /// </summary>
        private static void BuildAugmentRowPrefab()
        {
            var go = new GameObject("AugmentRowPrefab", typeof(RectTransform), typeof(CanvasRenderer));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(280, 40);

            // 배경 — SlatePanel
            var bgImg = go.AddComponent<Image>();
            var slateSprite = LoadSharedSprite("SlatePanel_9Slice.png");
            if (slateSprite != null)
            {
                bgImg.sprite = slateSprite;
                bgImg.type = Image.Type.Sliced;
                bgImg.color = new Color(0.55f, 0.55f, 0.75f, 1f);
            }
            else
            {
                bgImg.color = new Color(0.12f, 0.12f, 0.22f, 1f);
            }
            bgImg.raycastTarget = false;

            // HLG (가로 배치)
            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.padding = new RectOffset(8, 8, 4, 4);
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            // AugLeftBorder — 좌측 3px 자원색 보더 (별도 Image)
            var borderGo = new GameObject("AugLeftBorder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            borderGo.transform.SetParent(go.transform, false);
            var borderImg = borderGo.GetComponent<Image>();
            borderImg.color = UIPalette.Default.GradeCursed;
            borderImg.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(borderGo, prefW: 3, prefH: 32);

            // AugIcon — 24x24 Image + TMP 자식
            var iconGo = new GameObject("AugIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(go.transform, false);
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.color = UIPalette.Default.GradeCursed;
            iconImg.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(iconGo, prefW: 24, prefH: 24);

            var iconInitialGo = new GameObject("Initial", typeof(RectTransform), typeof(CanvasRenderer));
            iconInitialGo.transform.SetParent(iconGo.transform, false);
            var iconInitialRt = iconInitialGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(iconInitialRt);
            var iconInitialTmp = iconInitialGo.AddComponent<TextMeshProUGUI>();
            iconInitialTmp.text = "?";
            iconInitialTmp.font = FontTitle();
            iconInitialTmp.fontSize = 12;
            iconInitialTmp.color = Color.white;
            iconInitialTmp.alignment = TextAlignmentOptions.Center;
            iconInitialTmp.raycastTarget = false;

            // AugInfo 컨테이너 (Name/Owner 세로)
            var infoGo = new GameObject("AugInfo", typeof(RectTransform));
            infoGo.transform.SetParent(go.transform, false);
            UIAutoBindHelper.EnsureLayoutElement(infoGo, flexW: 1, prefH: 32);
            var infoVlg = infoGo.AddComponent<VerticalLayoutGroup>();
            infoVlg.childControlWidth = true;
            infoVlg.childControlHeight = true;
            infoVlg.spacing = 1;
            infoVlg.childAlignment = TextAnchor.UpperLeft;

            // AugName
            var nameGo = new GameObject("AugName", typeof(RectTransform), typeof(CanvasRenderer));
            nameGo.transform.SetParent(infoGo.transform, false);
            var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
            nameTmp.text = "Augment";
            nameTmp.font = FontLabel();
            nameTmp.fontSize = 11;
            nameTmp.color = UIPalette.Default.DFInk;
            nameTmp.alignment = TextAlignmentOptions.Left;
            nameTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(nameGo, flexW: 1, prefH: 14);

            // AugOwner
            var ownerGo = new GameObject("AugOwner", typeof(RectTransform), typeof(CanvasRenderer));
            ownerGo.transform.SetParent(infoGo.transform, false);
            var ownerTmp = ownerGo.AddComponent<TextMeshProUGUI>();
            ownerTmp.text = "Equipped";
            ownerTmp.font = FontItalic();
            ownerTmp.fontStyle = FontStyles.Italic;
            ownerTmp.fontSize = 9;
            ownerTmp.color = UIPalette.Default.DFInkDim;
            ownerTmp.alignment = TextAlignmentOptions.Left;
            ownerTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(ownerGo, flexW: 1, prefH: 12);

            // AugRank 배지 (우측)
            var rankGo = new GameObject("AugRank", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            rankGo.transform.SetParent(go.transform, false);
            var rankImg = rankGo.GetComponent<Image>();
            rankImg.color = new Color(0.08f, 0.08f, 0.14f, 1f);
            rankImg.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(rankGo, prefW: 28, prefH: 20);

            var rankTextGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer));
            rankTextGo.transform.SetParent(rankGo.transform, false);
            var rankTextRt = rankTextGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(rankTextRt);
            var rankTmp = rankTextGo.AddComponent<TextMeshProUGUI>();
            rankTmp.text = "R1";
            rankTmp.font = FontLabel();
            rankTmp.fontSize = 10;
            rankTmp.color = UIPalette.Default.DFGoldL;
            rankTmp.alignment = TextAlignmentOptions.Center;
            rankTmp.raycastTarget = false;

            // AugmentRow 컴포넌트
            go.AddComponent<AugmentRow>();

            SavePrefab(go, "AugmentRowPrefab.prefab");
            Object.DestroyImmediate(go);
        }

        /// <summary>
        /// ★ Node Detail Preview 파이프 — EnemyRowPrefab (1 적 분량).
        /// 구조: 루트(Image 배경 + HLG) / Name TMP (좌) / HP TMP (우).
        /// NodeDetailEnemyRow 컴포넌트가 자식 이름(Name/HP)으로 자동 바인딩.
        /// </summary>
        private static void BuildEnemyRowPrefab()
        {
            var go = new GameObject("EnemyRowPrefab", typeof(RectTransform), typeof(CanvasRenderer));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(280, 22);

            // 배경 — SlatePanel (어두워야 패널 위에서 돋보임)
            var bgImg = go.AddComponent<Image>();
            var slateSprite = LoadSharedSprite("SlatePanel_9Slice.png");
            if (slateSprite != null)
            {
                bgImg.sprite = slateSprite;
                bgImg.type = Image.Type.Sliced;
                bgImg.color = new Color(0.2f, 0.2f, 0.3f, 0.85f);
            }
            else
            {
                bgImg.color = new Color(0.1f, 0.1f, 0.18f, 0.9f);
            }
            bgImg.raycastTarget = false;

            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.padding = new RectOffset(8, 8, 2, 2);
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            // Name (좌, flexW=1)
            var nameGo = new GameObject("Name", typeof(RectTransform), typeof(CanvasRenderer));
            nameGo.transform.SetParent(go.transform, false);
            var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
            nameTmp.text = "Enemy";
            nameTmp.font = FontLabel();
            nameTmp.fontSize = 11;
            nameTmp.color = UIPalette.Default.DFParchment;
            nameTmp.alignment = TextAlignmentOptions.Left;
            nameTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(nameGo, flexW: 1, prefH: 18);

            // HP (우, prefW=60)
            var hpGo = new GameObject("HP", typeof(RectTransform), typeof(CanvasRenderer));
            hpGo.transform.SetParent(go.transform, false);
            var hpTmp = hpGo.AddComponent<TextMeshProUGUI>();
            hpTmp.text = "HP 100";
            hpTmp.font = FontItalic();
            hpTmp.fontStyle = FontStyles.Italic;
            hpTmp.fontSize = 10;
            hpTmp.color = UIPalette.Default.DFInkDim;
            hpTmp.alignment = TextAlignmentOptions.Right;
            hpTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(hpGo, prefW: 60, prefH: 18);

            // NodeDetailEnemyRow 컴포넌트
            go.AddComponent<NodeDetailEnemyRow>();

            SavePrefab(go, "EnemyRowPrefab.prefab");
            Object.DestroyImmediate(go);
        }

        /// <summary>
        /// ★ Node Detail Preview 파이프 — RewardRowPrefab (1 보상 분량).
        /// 구조: 루트(HLG, 투명 배경) / Label TMP (좌) / Value TMP (우).
        /// NodeDetailRewardRow 컴포넌트가 자식 이름(Label/Value)으로 자동 바인딩.
        /// </summary>
        private static void BuildRewardRowPrefab()
        {
            var go = new GameObject("RewardRowPrefab", typeof(RectTransform), typeof(CanvasRenderer));
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(280, 20);

            // 투명 배경 (리스트 밀집 방지용 얇은 구분선 느낌 — Image 없이도 됨)
            var bgImg = go.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0);  // 투명 — 레이아웃 용도
            bgImg.raycastTarget = false;

            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.padding = new RectOffset(10, 10, 2, 2);
            hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            // Label (좌, prefW=80)
            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
            labelGo.transform.SetParent(go.transform, false);
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text = "Gold";
            labelTmp.font = FontItalic();
            labelTmp.fontStyle = FontStyles.Italic;
            labelTmp.fontSize = 10;
            labelTmp.color = UIPalette.Default.DFGold;
            labelTmp.alignment = TextAlignmentOptions.Left;
            labelTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(labelGo, prefW: 80, prefH: 16);

            // Value (우, flexW=1)
            var valueGo = new GameObject("Value", typeof(RectTransform), typeof(CanvasRenderer));
            valueGo.transform.SetParent(go.transform, false);
            var valueTmp = valueGo.AddComponent<TextMeshProUGUI>();
            valueTmp.text = "10-25";
            valueTmp.font = FontLabel();
            valueTmp.fontSize = 11;
            valueTmp.color = UIPalette.Default.DFGoldL;
            valueTmp.alignment = TextAlignmentOptions.Right;
            valueTmp.raycastTarget = false;
            UIAutoBindHelper.EnsureLayoutElement(valueGo, flexW: 1, prefH: 16);

            // NodeDetailRewardRow 컴포넌트
            go.AddComponent<NodeDetailRewardRow>();

            SavePrefab(go, "RewardRowPrefab.prefab");
            Object.DestroyImmediate(go);
        }

        // =========================================================
        // 2. MapReworkView 아래 Container 자식 추가
        // =========================================================
        private static void SetupMapReworkViewContainers(Canvas canvas)
        {
            var viewGo = FindDescendant(canvas.transform, "MapReworkView");
            if (viewGo == null)
            {
                Debug.LogWarning("[MapSceneReworkBuilder] MapReworkView를 찾을 수 없음 — Container 설정 생략");
                return;
            }

            // NodeContainer — 노드들이 배치되는 RectTransform
            var nodeContainerGo = new GameObject("NodeContainer", typeof(RectTransform));
            nodeContainerGo.transform.SetParent(viewGo.transform, false);
            var nodeRt = nodeContainerGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(nodeRt);

            // LabelContainer — 라벨들이 배치되는 RectTransform
            var labelContainerGo = new GameObject("LabelContainer", typeof(RectTransform));
            labelContainerGo.transform.SetParent(viewGo.transform, false);
            var labelRt = labelContainerGo.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(labelRt);
        }

        // =========================================================
        // 3. Sprite/Prefab/Container를 MapReworkView에 자동 연결
        // =========================================================
        private static void WireAllFieldsToView(Canvas canvas)
        {
            var viewGo = FindDescendant(canvas.transform, "MapReworkView");
            if (viewGo == null)
            {
                Debug.LogWarning("[MapSceneReworkBuilder] MapReworkView를 찾을 수 없음 — Wire 생략");
                return;
            }

            var view = viewGo.GetComponent<MapReworkView>();
            if (view == null)
            {
                Debug.LogWarning("[MapSceneReworkBuilder] MapReworkView 컴포넌트 없음");
                return;
            }

            // Sprite 9종 연결
            WireField(view, "_iconStart",           LoadSprite("NodeIcon_Start.png"));
            WireField(view, "_iconBattle",          LoadSprite("NodeIcon_Battle.png"));
            WireField(view, "_iconElite",           LoadSprite("NodeIcon_Elite.png"));
            WireField(view, "_iconBoss",            LoadSprite("NodeIcon_Boss.png"));
            WireField(view, "_iconEvent",           LoadSprite("NodeIcon_Event.png"));
            WireField(view, "_iconShop",            LoadSprite("NodeIcon_Shop.png"));
            WireField(view, "_iconRest",            LoadSprite("NodeIcon_Rest.png"));
            WireField(view, "_frameGlow",           LoadSprite("NodeFrameGlow.png"));
            WireField(view, "_playerMarkerSprite",  LoadSprite("PlayerMarker.png"));

            // Prefab 4종 연결
            WireField(view, "_nodePrefab",          LoadPrefab("NodePrefab.prefab"));
            WireField(view, "_branchLabelPrefab",   LoadPrefab("BranchLabelPrefab.prefab"));
            WireField(view, "_betweenLabelPrefab",  LoadPrefab("BetweenLabelPrefab.prefab"));
            WireField(view, "_playerMarkerPrefab",  LoadPrefab("PlayerMarkerPrefab.prefab"));

            // Container 2종 연결
            var nodeContainerGo = FindDescendant(viewGo.transform, "NodeContainer");
            if (nodeContainerGo != null)
                WireField(view, "_nodeContainer", nodeContainerGo.GetComponent<RectTransform>());

            var labelContainerGo = FindDescendant(viewGo.transform, "LabelContainer");
            if (labelContainerGo != null)
                WireField(view, "_labelContainer", labelContainerGo.GetComponent<RectTransform>());

            // ★ Priority 7 (치명 수정): SerializeField spacing 값 강제 설정.
            // 씬에 저장된 인스펙터 값이 우선이라서, 코드에서 기본값을 바꿔도 씬 빌드 후에는
            // 이전 값(70/110/90)이 유지되는 문제 회피.
            var spacingSo = new SerializedObject(view);
            SetFloatProperty(spacingSo, "_layerSpacing", 110f);
            SetFloatProperty(spacingSo, "_nodeSpacing", 130f);
            SetFloatProperty(spacingSo, "_branchNodeSpacing", 140f);
            SetFloatProperty(spacingSo, "_singleNodeZigzag", 50f);
            spacingSo.ApplyModifiedProperties();

            // ★ 노드 프리팹 내부의 Sprite도 연결 (MapReworkNode가 _iconStart 등을 가지진 않지만,
            // 런타임에 Setup()에서 sprite를 받아 Icon에 적용하므로 프리팹 단위 Sprite는 불필요)
            // 단, FrameGlow의 Sprite는 NodeFrameGlow.png로 고정
            var nodePrefab = LoadPrefab("NodePrefab.prefab");
            if (nodePrefab != null)
            {
                var frameGlowTransform = nodePrefab.transform.Find("FrameGlow");
                if (frameGlowTransform != null)
                {
                    var frameGlowImg = frameGlowTransform.GetComponent<Image>();
                    var glowSprite = LoadSprite("NodeFrameGlow.png");
                    if (frameGlowImg != null && glowSprite != null)
                    {
                        frameGlowImg.sprite = glowSprite;
                        EditorUtility.SetDirty(nodePrefab);
                    }
                }
            }

            Debug.Log("[MapSceneReworkBuilder] MapReworkView 인스펙터 필드 자동 연결 완료");

            // ★ Phase UI-2: MapSceneSetup GameObject 생성 + 에셋 자동 로드
            SetupMapSceneSetup(canvas);
        }

        // =========================================================
        // 4. MapSceneSetup GameObject — GameRunState 생명주기 + Rework 뷰 연결
        // =========================================================
        private static void SetupMapSceneSetup(Canvas canvas)
        {
            // SceneContext라는 별도 GameObject에 MapSceneSetup 부착
            // (기존 MapScene.unity 패턴과 동일 — Canvas와 분리된 GameObject)
            var setupGo = new GameObject("MapSceneSetup");
            var setup = setupGo.AddComponent<TeamLog.UI.Map.MapSceneSetup>();

            // Rework 컴포넌트들 자동 연결
            WireField(setup, "_mapReworkView", FindComponentInScene<MapReworkView>(canvas.transform));
            WireField(setup, "_partySidebarPanel", FindComponentInScene<PartySidebarPanel>(canvas.transform));
            WireField(setup, "_themeBanner", FindComponentInScene<ThemeBanner>(canvas.transform));
            WireField(setup, "_nodeDetailPanel", FindComponentInScene<NodeDetailPanel>(canvas.transform));

            // ★ Phase 5 — HeaderController 자동 연결
            WireField(setup, "_headerController", FindComponentInScene<HeaderController>(canvas.transform));

            // ★ Node Detail Preview 파이프 — NodeDetailPanel에 컨테이너 2종 + 프리팹 2종 자동 연결
            var nodeDetail = FindComponentInScene<NodeDetailPanel>(canvas.transform);
            if (nodeDetail != null)
            {
                // 컨테이너는 자식 GameObject 이름으로 찾기 (NodeDetailPanel의 자손)
                var enemyContainerGo = FindDescendant(nodeDetail.transform, "EnemyListContainer");
                if (enemyContainerGo != null)
                    WireField(nodeDetail, "_enemyListContainer", enemyContainerGo.transform);

                var rewardContainerGo = FindDescendant(nodeDetail.transform, "RewardInfoContainer");
                if (rewardContainerGo != null)
                    WireField(nodeDetail, "_rewardInfoContainer", rewardContainerGo.transform);

                // 프리팹 2종 로드 + 연결
                var enemyRowPrefab = LoadPrefab("EnemyRowPrefab.prefab");
                if (enemyRowPrefab != null)
                    WireField(nodeDetail, "_enemyRowPrefab", enemyRowPrefab);

                var rewardRowPrefab = LoadPrefab("RewardRowPrefab.prefab");
                if (rewardRowPrefab != null)
                    WireField(nodeDetail, "_rewardRowPrefab", rewardRowPrefab);
            }

            // ★ Phase 5 — PartySidebarPanel에 memberRowPrefab 자동 연결
            var partySidebar = FindComponentInScene<PartySidebarPanel>(canvas.transform);
            if (partySidebar != null)
            {
                var memberRowPrefab = LoadPrefab("PartyMemberRowPrefab.prefab");
                WireField(partySidebar, "_memberRowPrefab", memberRowPrefab);
            }

            // ★ Phase B — RelicGridPanel._slotPrefab / AugmentListPanel._rowPrefab 자동 연결
            var relicGrid = FindComponentInScene<RelicGridPanel>(canvas.transform);
            if (relicGrid != null)
            {
                var slotPrefab = LoadPrefab("RelicSlotCellPrefab.prefab");
                WireField(relicGrid, "_slotPrefab", slotPrefab);
            }

            var augmentList = FindComponentInScene<AugmentListPanel>(canvas.transform);
            if (augmentList != null)
            {
                var rowPrefab = LoadPrefab("AugmentRowPrefab.prefab");
                WireField(augmentList, "_rowPrefab", rowPrefab);
            }

            // ★ Phase 4 — MapReworkDebugInitializer 부착 (GameRunState 없이 Play할 때 폴백)
            SetupDebugInitializer(canvas, setup);

            // ★ Phase EVENT — EventReworkView Prefab 인스턴스화 + MapSceneSetup에 연결
            SetupEventReworkView(canvas, setup);

            // ★ Phase SHOP — ShopReworkView Prefab 인스턴스화 + MapSceneSetup에 연결
            SetupShopReworkView(canvas, setup);

            // _useTestData = false (런타임에 SelectedParty 기반)
            var so = new SerializedObject(setup);
            var useTestDataProp = so.FindProperty("_useTestData");
            if (useTestDataProp != null)
            {
                useTestDataProp.boolValue = false;
                so.ApplyModifiedProperties();
            }

            // ★ Phase D — testData 자동 로드 (InitializeTestRun 빈 파티 방지).
            // TitleScene → PartySelectionScene 파이프를 안 거치고 직접 Play할 때 사용.
            // Ashe(Ember)/Lumi(Frost)/Duran(Vengeance)/Sibyl(Prophecy) — 4캐릭터 기본 파티.
            WireField(setup, "_testWarriorData", LoadCharacterByFileName("Char_Duran"));
            WireField(setup, "_testMageData",    LoadCharacterByFileName("Char_Ashe"));
            WireField(setup, "_testHealerData",  LoadCharacterByFileName("Char_Lumi"));
            WireField(setup, "_testRogueData",   LoadCharacterByFileName("Char_Sibyl"));

            // 핵심 에셋 자동 로드 — CharacterData / RelicData / AugmentData / CharacterTraitData / StageThemeData
            LoadCharacterAssets(setup);
            LoadRelicAssets(setup);
            LoadAugmentAssets(setup);
            LoadTraitAssets(setup);
            LoadStageThemeAssets(setup);

            Debug.Log("[MapSceneReworkBuilder] MapSceneSetup 자동 구성 완료");
        }

        private static T FindComponentInScene<T>(Transform root) where T : Component
        {
            var found = root.GetComponentInChildren<T>(true);
            if (found == null)
                Debug.LogWarning($"[MapSceneReworkBuilder] {typeof(T).Name}을(를) 씬에서 찾을 수 없음");
            return found;
        }

        /// <summary>
        /// ★ Phase 4 — 디버그 초기화 컴포넌트 부착.
        /// GameRunState 없이 Play할 때 헤더 칩 + 더미 맵 표시.
        /// </summary>
        private static void SetupDebugInitializer(Canvas canvas, TeamLog.UI.Map.MapSceneSetup setup)
        {
            var debugGo = new GameObject("MapReworkDebugInitializer");
            var debugInit = debugGo.AddComponent<TeamLog.UI.Map.Rework.MapReworkDebugInitializer>();

            // 자동 바인딩 — HeaderController / MapReworkView / ThemeBanner를 씬에서 찾아 연결
            WireField(debugInit, "_headerController", FindComponentInScene<HeaderController>(canvas.transform));
            WireField(debugInit, "_mapReworkView", FindComponentInScene<MapReworkView>(canvas.transform));
            WireField(debugInit, "_themeBanner", FindComponentInScene<ThemeBanner>(canvas.transform));

            Debug.Log("[MapSceneReworkBuilder] MapReworkDebugInitializer 부착 완료");
        }

        private static void LoadCharacterAssets(TeamLog.UI.Map.MapSceneSetup setup)
        {
            var guids = AssetDatabase.FindAssets("t:CharacterData", new[] { "Assets/03.Data/Characters" });
            var list = new List<UnityEngine.Object>();
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var asset = AssetDatabase.LoadAssetAtPath<TeamLog.Characters.CharacterData>(path);
                if (asset != null) list.Add(asset);
            }

            // _allCharacters 필드 — 배열로 WireField 불가 (SerializedProperty 처리)
            if (list.Count > 0)
            {
                var so = new SerializedObject(setup);
                var prop = so.FindProperty("_allCharacters");
                if (prop != null)
                {
                    prop.arraySize = list.Count;
                    for (int i = 0; i < list.Count; i++)
                        prop.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
                    so.ApplyModifiedProperties();
                }
            }
        }

        private static void LoadRelicAssets(TeamLog.UI.Map.MapSceneSetup setup)
        {
            var guids = AssetDatabase.FindAssets("t:RelicData", new[] { "Assets/03.Data" });
            var list = new List<UnityEngine.Object>();
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var asset = AssetDatabase.LoadAssetAtPath<TeamLog.Reward.RelicData>(path);
                if (asset != null) list.Add(asset);
            }
            if (list.Count > 0)
            {
                var so = new SerializedObject(setup);
                var prop = so.FindProperty("_relicPool");
                if (prop != null)
                {
                    prop.arraySize = list.Count;
                    for (int i = 0; i < list.Count; i++)
                        prop.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
                    so.ApplyModifiedProperties();
                }
            }
        }

        private static void LoadAugmentAssets(TeamLog.UI.Map.MapSceneSetup setup)
        {
            var guids = AssetDatabase.FindAssets("t:AugmentData", new[] { "Assets/03.Data" });
            var list = new List<UnityEngine.Object>();
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var asset = AssetDatabase.LoadAssetAtPath<TeamLog.Skill.AugmentData>(path);
                if (asset != null) list.Add(asset);
            }
            if (list.Count > 0)
            {
                var so = new SerializedObject(setup);
                var prop = so.FindProperty("_augmentPool");
                if (prop != null)
                {
                    prop.arraySize = list.Count;
                    for (int i = 0; i < list.Count; i++)
                        prop.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
                    so.ApplyModifiedProperties();
                }
            }
        }

        private static void LoadTraitAssets(TeamLog.UI.Map.MapSceneSetup setup)
        {
            var guids = AssetDatabase.FindAssets("t:CharacterTraitData", new[] { "Assets/03.Data" });
            var list = new List<UnityEngine.Object>();
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var asset = AssetDatabase.LoadAssetAtPath<TeamLog.Characters.CharacterTraitData>(path);
                if (asset != null) list.Add(asset);
            }
            if (list.Count > 0)
            {
                var so = new SerializedObject(setup);
                var prop = so.FindProperty("_allCharacterTraits");
                if (prop != null)
                {
                    prop.arraySize = list.Count;
                    for (int i = 0; i < list.Count; i++)
                        prop.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
                    so.ApplyModifiedProperties();
                }
            }
        }

        private static void LoadStageThemeAssets(TeamLog.UI.Map.MapSceneSetup setup)
        {
            // StageThemeCandidateList[] 배열은 복잡하므로 4스테이지 × 3테마 = 12 테마를 그대로 매핑
            var guids = AssetDatabase.FindAssets("t:StageThemeData", new[] { "Assets/03.Data" });
            var themesByStage = new Dictionary<int, List<TeamLog.Map.StageThemeData>>();
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var theme = AssetDatabase.LoadAssetAtPath<TeamLog.Map.StageThemeData>(path);
                if (theme == null) continue;
                int stage = Mathf.Clamp(theme.stageNumber, 1, 4);
                if (!themesByStage.ContainsKey(stage))
                    themesByStage[stage] = new List<TeamLog.Map.StageThemeData>();
                themesByStage[stage].Add(theme);
            }

            var so = new SerializedObject(setup);
            var prop = so.FindProperty("_stageThemeCandidates");
            if (prop == null) return;

            // 4스테이지 배열 구성
            prop.arraySize = 4;
            for (int stageIdx = 0; stageIdx < 4; stageIdx++)
            {
                int stage = stageIdx + 1;
                var entryProp = prop.GetArrayElementAtIndex(stageIdx);
                var candidatesProp = entryProp.FindPropertyRelative("candidates");

                if (!themesByStage.TryGetValue(stage, out var themes) || themes.Count == 0) continue;

                candidatesProp.arraySize = themes.Count;
                for (int i = 0; i < themes.Count; i++)
                    candidatesProp.GetArrayElementAtIndex(i).objectReferenceValue = themes[i];
            }
            so.ApplyModifiedProperties();
        }

        // =========================================================
        // 공통 유틸 — Sprite/Prefab 로드, GameObject 검색, 프리팹 저장
        // =========================================================
        private static Sprite LoadSprite(string fileName)
        {
            string path = $"{SPRITE_DIR}/{fileName}";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                Debug.LogWarning($"[MapSceneReworkBuilder] Sprite 로드 실패: {path} — SpriteGenerator 메뉴를 먼저 실행했는지 확인");
            return sprite;
        }

        /// <summary>
        /// ★ MapScene 전용 Sprite 로드 — SPRITE_DIR(Assets/03.Data/UI/MapScene) 기준.
        /// Parts.cs의 LoadMapSprite 호출이 이쪽으로 연결.
        /// </summary>
        private static Sprite LoadMapSprite(string fileName)
        {
            return LoadSprite(fileName);
        }

        /// <summary>
        /// ★ Phase 3 — 공유 Sprite 로드 (PartySelectionSpriteGenerator 출력).
        /// SlatePanel_9Slice / GoldBorderThin_9Slice / BloodButton_* / Crest_Logo / ParchmentDark_9Slice 등.
        /// </summary>
        private static Sprite LoadSharedSprite(string fileName)
        {
            string path = $"{SHARED_SPRITE_DIR}/{fileName}";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                Debug.LogWarning($"[MapSceneReworkBuilder] 공유 Sprite 로드 실패: {path} — PartySelectionSpriteGenerator 실행 필요");
            return sprite;
        }

        private static GameObject LoadPrefab(string fileName)
        {
            string path = $"{PREFAB_DIR}/{fileName}";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                Debug.LogWarning($"[MapSceneReworkBuilder] 프리팹 로드 실패: {path}");
            return prefab;
        }

        private static GameObject SavePrefab(GameObject tempGo, string fileName)
        {
            string path = $"{PREFAB_DIR}/{fileName}";
            var prefab = PrefabUtility.SaveAsPrefabAsset(tempGo, path);
            return prefab;
        }

        private static GameObject FindDescendant(Transform current, string name)
        {
            for (int i = 0; i < current.childCount; i++)
            {
                var child = current.GetChild(i);
                if (child.name == name) return child.gameObject;
                var found = FindDescendant(child, name);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// ★ Priority 7 — float SerializeField 강제 설정 헬퍼.
        /// </summary>
        private static void SetFloatProperty(SerializedObject so, string propName, float value)
        {
            var prop = so.FindProperty(propName);
            if (prop != null)
            {
                prop.floatValue = value;
            }
            else
            {
                Debug.LogWarning($"[MapSceneReworkBuilder] 필드 '{propName}' 없음 — MapReworkView에 추가되었는지 확인");
            }
        }

        /// <summary>
        /// ★ Phase D — CharacterData 에셋을 파일 이름으로 로드.
        /// 예: "Char_Ashe" → Assets/03.Data/Characters/Char_Ashe.asset
        /// </summary>
        private static TeamLog.Characters.CharacterData LoadCharacterByFileName(string fileName)
        {
            var guids = AssetDatabase.FindAssets(fileName + " t:CharacterData", new[] { "Assets/03.Data" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(fileName + ".asset"))
                {
                    return AssetDatabase.LoadAssetAtPath<TeamLog.Characters.CharacterData>(path);
                }
            }
            // 폴백: 그냥 첫 번째 매칭
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<TeamLog.Characters.CharacterData>(path);
            }
            Debug.LogWarning($"[MapSceneReworkBuilder] CharacterData '{fileName}' 찾을 수 없음");
            return null;
        }

        /// <summary>
        /// ★ Phase EVENT — EventReworkView Prefab 인스턴스화 + MapSceneSetup에 연결.
        /// EventSceneReworkBuilder.LoadEventReworkViewPrefab()이 Prefab을 보장 (없으면 빌드).
        /// </summary>
        private static void SetupEventReworkView(Canvas canvas, TeamLog.UI.Map.MapSceneSetup setup)
        {
            // EventSceneSpriteGenerator가 아직 실행 안 됐으면 자동 실행
            EnsureEventSceneSprites();

            var prefab = EventSceneReworkBuilder.LoadEventReworkViewPrefab();
            if (prefab == null)
            {
                Debug.LogWarning("[MapSceneReworkBuilder] EventReworkView Prefab 로드 실패 — Phase EVENT 스킵");
                return;
            }

            // Canvas 자식으로 인스턴스화 (초기 비활성 — EventReworkView Awake가 gameObject.SetActive(false) 수행)
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
            if (instance == null)
            {
                Debug.LogWarning("[MapSceneReworkBuilder] EventReworkView 인스턴스화 실패");
                return;
            }
            instance.name = "EventReworkView";
            var rt = instance.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(rt);

            var view = instance.GetComponent<EventReworkView>();
            if (view == null)
            {
                Debug.LogWarning("[MapSceneReworkBuilder] EventReworkView 컴포넌트 없음");
                return;
            }

            WireField(setup, "_eventReworkView", view);

            // ★ 연쇄 이벤트 지원 — 모든 EventData를 검색해서 EventReworkView._allEvents에 주입.
            // (Resources.Load("Events/...") 경로가 작동 안 하므로 직접 풀 주입)
            LoadAllEventsForReworkView(view);

            Debug.Log("[MapSceneReworkBuilder] EventReworkView 자동 연결 완료");
        }

        /// <summary>
        /// ★ EventReworkView._allEvents에 모든 EventData 에셋을 로드하여 주입.
        /// 연쇄 이벤트(NextEventId)가 Resources.Load 없이 작동하도록.
        /// </summary>
        private static void LoadAllEventsForReworkView(EventReworkView view)
        {
            if (view == null) return;
            var guids = AssetDatabase.FindAssets("t:EventData", new[] { "Assets/03.Data" });
            var list = new List<UnityEngine.Object>();
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var asset = AssetDatabase.LoadAssetAtPath<TeamLog.Event.EventData>(path);
                if (asset != null) list.Add(asset);
            }
            if (list.Count > 0)
            {
                var so = new SerializedObject(view);
                var prop = so.FindProperty("_allEvents");
                if (prop != null)
                {
                    prop.arraySize = list.Count;
                    for (int i = 0; i < list.Count; i++)
                        prop.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
                    so.ApplyModifiedProperties();
                }
                Debug.Log($"[MapSceneReworkBuilder] EventReworkView._allEvents에 {list.Count}개 이벤트 주입");
            }
        }

        /// <summary>
        /// ★ EventSceneSpriteGenerator 출력 9종 Sprite가 전부 있는지 검사 —
        /// 하나라도 없으면 자동으로 Generator 실행.
        /// </summary>
        private static void EnsureEventSceneSprites()
        {
            bool allPresent = true;
            string[] required = new[]
            {
                "GlassWindow_Story.png", "GlassWindow_Treasure.png", "GlassWindow_Trap.png",
                "GlassWindow_NPC.png", "GlassWindow_Shrine.png",
                "PanelBackground.png", "DimBackground.png",
                "ChoiceRow_Bg.png", "ChoiceRow_RiskTag.png"
            };
            foreach (var f in required)
            {
                if (!System.IO.File.Exists($"Assets/03.Data/UI/EventScene/{f}"))
                {
                    allPresent = false;
                    break;
                }
            }
            if (!allPresent)
            {
                Debug.Log("[MapSceneReworkBuilder] EventScene Sprite 누락 — EventSceneSpriteGenerator 자동 실행");
                EventSceneSpriteGenerator.GenerateAll();
            }
        }

        /// <summary>
        /// ★ Phase SHOP — ShopReworkView Prefab 인스턴스화 + MapSceneSetup에 연결.
        /// ConfirmationDialog / AugmentSelectPanel은 씬에서 자동 검색하여 연결.
        /// </summary>
        private static void SetupShopReworkView(Canvas canvas, TeamLog.UI.Map.MapSceneSetup setup)
        {
            EnsureShopSceneSprites();

            var prefab = ShopSceneReworkBuilder.LoadShopReworkViewPrefab();
            if (prefab == null)
            {
                Debug.LogWarning("[MapSceneReworkBuilder] ShopReworkView Prefab 로드 실패 — Phase SHOP 스킵");
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
            if (instance == null)
            {
                Debug.LogWarning("[MapSceneReworkBuilder] ShopReworkView 인스턴스화 실패");
                return;
            }
            instance.name = "ShopReworkView";
            var rt = instance.GetComponent<RectTransform>();
            UIAutoBindHelper.StretchToParent(rt);

            var view = instance.GetComponent<ShopReworkView>();
            if (view == null)
            {
                Debug.LogWarning("[MapSceneReworkBuilder] ShopReworkView 컴포넌트 없음");
                return;
            }

            WireField(setup, "_shopReworkView", view);

            // ConfirmationDialog / AugmentSelectPanel — 씬에서 자동 검색하여 연결
            var confirmDialog = FindComponentInScene<TeamLog.UI.ConfirmationDialog>(canvas.transform);
            WireField(view, "_confirmationDialog", confirmDialog);

            var augmentPanel = FindComponentInScene<TeamLog.UI.AugmentSelectPanel>(canvas.transform);
            WireField(view, "_augmentSelectPanel", augmentPanel);

            Debug.Log("[MapSceneReworkBuilder] ShopReworkView 자동 연결 완료");
        }

        /// <summary>
        /// ★ ShopSceneSpriteGenerator 출력 7종 Sprite 검사 — 누락 시 자동 실행.
        /// </summary>
        private static void EnsureShopSceneSprites()
        {
            bool allPresent = true;
            string[] required = new[]
            {
                "GlassCrown.png", "PanelBackground.png", "DimBackground.png",
                "SlotBg.png", "TabButton.png", "TabButtonActive.png", "LeaveButton.png"
            };
            foreach (var f in required)
            {
                if (!System.IO.File.Exists($"Assets/03.Data/UI/ShopScene/{f}"))
                {
                    allPresent = false;
                    break;
                }
            }
            if (!allPresent)
            {
                Debug.Log("[MapSceneReworkBuilder] ShopScene Sprite 누락 — ShopSceneSpriteGenerator 자동 실행");
                ShopSceneSpriteGenerator.GenerateAll();
            }
        }
    }
}
#endif
