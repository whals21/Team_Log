#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using TeamLog.UI;

namespace TeamLog.Editor
{
    /// <summary>
    /// ★ Dark Sanctum Style (D 시안) — 기존 TitleScene.unity의 GameObject들을 찾아
    /// 디자인만 인플레이스 수정. 씬 구조/이름/계층은 유지.
    ///
    /// 메뉴: TeamLog/Scene/Apply Dark Sanctum Style to Title
    ///
    /// 변경 사항:
    ///   - Background → DarkGradientBg Sprite + MicroGrid 자식 오버레이
    ///   - TitleLabel → Cinzel Black 96pt 황금빛 + 자식 TitleEmblem 신규 추가
    ///   - TitleText → Cormorant Italic 부제
    ///   - NewGameButton → MenuBtnPrimary Sprite + 황금빛 텍스트
    ///   - ContinueButton → MenuBtnSecondary Sprite
    ///   - MetaShopButton → MenuBtnTertiary Sprite
    ///   - AscensionPanel → 하단 우측 anchor 재배치 + 어센션 화살표 ArrowBtn Sprite
    ///   - StatsLabel / TopBar / MemoryLabel / SoulLabel → SetActive(false)
    /// </summary>
    public static class TitleSceneStyleApplicator
    {
        private const string TITLE_SCENE_PATH = "Assets/01.Scenes/TitleScene.unity";
        private const string SPRITE_DIR = "Assets/03.Data/UI/TitleScene";

        private static TMP_FontAsset _fontCinzelBlack;
        private static TMP_FontAsset _fontCinzelBold;
        private static TMP_FontAsset _fontCormorantItalic;
        private static TMP_FontAsset _fontKorean;

        [MenuItem("TeamLog/Scene/Apply Dark Sanctum Style to Title")]
        public static void ApplyStyle()
        {
            // 1. 폰트 로드
            LoadFonts();

            // 2. Sprite 보장 (누락 시 자동 생성)
            EnsureSprites();

            // 3. 씬이 안 열려 있으면 로드
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            if (scene == null || !scene.name.Equals("TitleScene"))
            {
                scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(TITLE_SCENE_PATH, OpenSceneMode.Single);
            }

            // 4. 주요 GameObject 검색 + 스타일 적용
            ApplyBackgroundStyle();
            ApplyTitleStyle();
            ApplyMenuButtonsStyle();
            ApplyAscensionStyle();
            HideUnusedStatsUI();

            // 5. ★ z-order 강제 정렬 — TitleLabel이 다른 UI에 가려지지 않도록
            ReorderCanvasChildren();

            // 6. 씬 저장
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

            Debug.Log("[TitleSceneStyleApplicator] Dark Sanctum 스타일 적용 완료 → TitleScene.unity 저장");
        }

        /// <summary>
        /// ★ Canvas 자식 순서 강제 정렬 — Background/ButtonContainer를 아래로,
        /// TitleLabel/TitleText/TitleEmblem을 위로. ScreenSpaceOverlay는 sibling 순서가 곧 렌더링 순서.
        /// </summary>
        private static void ReorderCanvasChildren()
        {
            var canvas = FindByName("Canvas");
            if (canvas == null) return;
            var canvasTransform = canvas.transform;

            // (A) 배경/컨테이너류 — 맨 앞으로 (맨 아래 렌더링)
            var background = FindByNameIncludingInactive("Background");
            if (background != null && background.transform.parent == canvasTransform)
                background.transform.SetAsFirstSibling();

            var buttonContainer = FindByNameIncludingInactive("ButtonContainer");
            if (buttonContainer != null && buttonContainer.transform.parent == canvasTransform)
            {
                buttonContainer.transform.SetAsFirstSibling();
                // Background가 있으면 Background 다음
                if (background != null)
                    buttonContainer.transform.SetSiblingIndex(1);
            }

            // (B) 타이틀류 — 맨 뒤로 (맨 위 렌더링)
            // 순서: TitleEmblem (아래) → TitleLabel → TitleText (위)
            var emblem = FindByNameIncludingInactive("TitleEmblem");
            var titleLabel = FindByNameIncludingInactive("TitleLabel");
            var titleText = FindByNameIncludingInactive("TitleText");

            if (emblem != null && emblem.transform.parent == canvasTransform)
                emblem.transform.SetAsLastSibling();
            if (titleLabel != null && titleLabel.transform.parent == canvasTransform)
                titleLabel.transform.SetAsLastSibling();
            if (titleText != null && titleText.transform.parent == canvasTransform)
                titleText.transform.SetAsLastSibling();

            // (C) TitleLabel TMP 강제 dirty — Editor에서 가끔 빌드가 안 되는 문제 회피
            if (titleLabel != null)
            {
                var tmp = titleLabel.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.SetAllDirty();
                    tmp.ForceMeshUpdate();
                    EditorUtility.SetDirty(tmp);
                }
            }
            if (titleText != null)
            {
                var tmp = titleText.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.SetAllDirty();
                    tmp.ForceMeshUpdate();
                    EditorUtility.SetDirty(tmp);
                }
            }

            // 자식 구조 로그
            Debug.Log($"[TitleSceneStyleApplicator] Canvas 자식 수: {canvasTransform.childCount}");
            for (int i = 0; i < canvasTransform.childCount; i++)
            {
                var child = canvasTransform.GetChild(i);
                Debug.Log($"  [{i}] {child.name} (active={child.gameObject.activeSelf})");
            }
        }

        // =========================================================
        // 폰트 로드
        // =========================================================
        private static void LoadFonts()
        {
            _fontCinzelBlack     = PartySelectionSceneBuilder.LoadFont("Cinzel-Black SDF");
            _fontCinzelBold      = PartySelectionSceneBuilder.LoadFont("Cinzel-Bold SDF");
            _fontCormorantItalic = PartySelectionSceneBuilder.LoadFont("CormorantGaramond-Italic SDF");
            _fontKorean          = PartySelectionSceneBuilder.LoadFont("NanumGothic SDF");

            Debug.Log($"[TitleSceneStyleApplicator] Fonts — Black:{(_fontCinzelBlack != null)} " +
                      $"Bold:{(_fontCinzelBold != null)} Italic:{(_fontCormorantItalic != null)}");
        }

        // =========================================================
        // Sprite 자동 생성
        // =========================================================
        private static void EnsureSprites()
        {
            string[] required =
            {
                "DarkGradientBg.png", "MicroGrid.png", "TitleEmblem.png",
                "MenuBtnPrimary.png", "MenuBtnSecondary.png", "MenuBtnTertiary.png",
                "ArrowBtn.png"
            };
            bool allPresent = true;
            foreach (var f in required)
            {
                if (!File.Exists($"{SPRITE_DIR}/{f}"))
                {
                    allPresent = false;
                    break;
                }
            }
            if (!allPresent)
            {
                Debug.Log("[TitleSceneStyleApplicator] Sprite 누락 — TitleSceneSpriteGenerator 자동 실행");
                TitleSceneSpriteGenerator.GenerateAll();
            }
        }

        private static Sprite LoadSprite(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{SPRITE_DIR}/{fileName}");
        }

        // =========================================================
        // GameObject 검색 헬퍼 (전체 씬 루트부터 깊이 우선)
        // =========================================================
        private static GameObject FindByName(string name)
        {
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var found = FindRecursive(root.transform, name);
                if (found != null) return found;
            }
            return null;
        }

        private static GameObject FindRecursive(Transform t, string name)
        {
            if (t.name == name) return t.gameObject;
            for (int i = 0; i < t.childCount; i++)
            {
                var found = FindRecursive(t.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        // =========================================================
        // Background 스타일 — DarkGradientBg + MicroGrid 오버레이
        // =========================================================
        private static void ApplyBackgroundStyle()
        {
            var bg = FindByName("Background");
            if (bg == null)
            {
                Debug.LogWarning("[TitleSceneStyleApplicator] Background GameObject 못 찾음 — 스킵");
                return;
            }

            var bgImage = bg.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.sprite = LoadSprite("DarkGradientBg.png");
                bgImage.type = Image.Type.Simple;
                bgImage.color = Color.white;
                EditorUtility.SetDirty(bgImage);
            }

            // MicroGrid 오버레이 자식 (없으면 생성)
            var gridTransform = bg.transform.Find("MicroGrid");
            if (gridTransform == null)
            {
                var gridGo = new GameObject("MicroGrid", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                gridGo.transform.SetParent(bg.transform, false);
                var gridRt = gridGo.GetComponent<RectTransform>();
                gridRt.anchorMin = Vector2.zero;
                gridRt.anchorMax = Vector2.one;
                gridRt.offsetMin = Vector2.zero;
                gridRt.offsetMax = Vector2.zero;
                gridTransform = gridGo.transform;

                var gridImg = gridGo.GetComponent<Image>();
                gridImg.sprite = LoadSprite("MicroGrid.png");
                gridImg.type = Image.Type.Tiled;
                gridImg.color = new Color(1f, 1f, 1f, 0.6f);
                gridImg.raycastTarget = false;
            }
            EditorUtility.SetDirty(bg);
        }

        // =========================================================
        // TitleLabel + TitleText + Emblem 스타일
        // =========================================================
        private static void ApplyTitleStyle()
        {
            // Canvas 검색 (없으면 에러)
            var canvas = FindByName("Canvas");
            if (canvas == null)
            {
                Debug.LogError("[TitleSceneStyleApplicator] Canvas 못 찾음 — TitleLabel 생성 불가");
                return;
            }
            var canvasTransform = canvas.transform;

            // TitleLabel — 메인 "TEAM LOG". 없거나 비활성이어도 강제로 확보.
            var titleLabel = FindByNameIncludingInactive("TitleLabel");
            if (titleLabel == null)
            {
                // 새로 생성
                titleLabel = new GameObject("TitleLabel", typeof(RectTransform), typeof(CanvasRenderer));
                titleLabel.transform.SetParent(canvasTransform, false);
                titleLabel.AddComponent<TextMeshProUGUI>();
                Debug.Log("[TitleSceneStyleApplicator] TitleLabel 새로 생성 (기존 없음)");
            }
            // ★ 핵심: 무조건 Canvas 직접 자식으로 이동 (TopBar/ButtonContainer 등
            // 비활성 컨테이너의 자식이면 SetActive(true)가 안 먹힘)
            if (titleLabel.transform.parent != canvasTransform)
            {
                titleLabel.transform.SetParent(canvasTransform, false);
                Debug.Log("[TitleSceneStyleApplicator] TitleLabel을 Canvas 직접 자식으로 이동 (비활성 부모 회피)");
            }
            // 무조건 활성화
            titleLabel.SetActive(true);

            // RectTransform — 화면 중앙 상단 (anchor 0.5/0.5, 약간 위로)
            var titleRt = titleLabel.GetComponent<RectTransform>();
            if (titleRt != null)
            {
                titleRt.anchorMin = new Vector2(0.5f, 0.5f);
                titleRt.anchorMax = new Vector2(0.5f, 0.5f);
                titleRt.pivot = new Vector2(0.5f, 0.5f);
                titleRt.sizeDelta = new Vector2(900, 140);
                // ★ 위로 충분히 올림 (버튼 컨테이너와 분리)
                titleRt.anchoredPosition = new Vector2(0, 140);
                EditorUtility.SetDirty(titleRt);
            }

            // TMP 컴포넌트가 없으면 추가
            var titleTmp = titleLabel.GetComponent<TextMeshProUGUI>();
            if (titleTmp == null)
            {
                titleTmp = titleLabel.AddComponent<TextMeshProUGUI>();
            }
            titleTmp.text = "TEAM LOG";
            titleTmp.font = _fontCinzelBlack ?? TMP_Settings.defaultFontAsset;
            titleTmp.fontSize = 96;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color = Color.white;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.raycastTarget = false;
            titleTmp.enableWordWrapping = false;
            EditorUtility.SetDirty(titleTmp);

            // TitleEmblem — TitleLabel 위쪽에 황금 원형 엠블럼
            var emblemGo = FindByNameIncludingInactive("TitleEmblem");
            if (emblemGo == null)
            {
                emblemGo = new GameObject("TitleEmblem", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                emblemGo.transform.SetParent(canvasTransform, false);
                var emblemRt = emblemGo.GetComponent<RectTransform>();
                emblemRt.anchorMin = new Vector2(0.5f, 0.5f);
                emblemRt.anchorMax = new Vector2(0.5f, 0.5f);
                emblemRt.pivot = new Vector2(0.5f, 0.5f);
                emblemRt.sizeDelta = new Vector2(140, 140);
                emblemRt.anchoredPosition = new Vector2(0, 320);
                Debug.Log("[TitleSceneStyleApplicator] TitleEmblem 새로 생성");
            }
            emblemGo.SetActive(true);
            // ★ 기존 Emblem도 위로 이동 (y=320) — TitleText 공간 확보
            var emblemRtExisting = emblemGo.GetComponent<RectTransform>();
            if (emblemRtExisting != null)
            {
                emblemRtExisting.anchoredPosition = new Vector2(0, 320);
                EditorUtility.SetDirty(emblemRtExisting);
            }
            var emblemImg = emblemGo.GetComponent<Image>();
            if (emblemImg == null) emblemImg = emblemGo.AddComponent<Image>();
            emblemImg.sprite = LoadSprite("TitleEmblem.png");
            emblemImg.color = Color.white;
            emblemImg.raycastTarget = false;
            EditorUtility.SetDirty(emblemImg);

            // Emblem 내부 ✦ 기호 자식 (없으면 생성)
            if (emblemGo.transform.Find("Symbol") == null)
            {
                var symbolGo = new GameObject("Symbol", typeof(RectTransform), typeof(CanvasRenderer));
                symbolGo.transform.SetParent(emblemGo.transform, false);
                var symbolRt = symbolGo.GetComponent<RectTransform>();
                symbolRt.anchorMin = Vector2.zero;
                symbolRt.anchorMax = Vector2.one;
                symbolRt.offsetMin = Vector2.zero;
                symbolRt.offsetMax = Vector2.zero;
                var symbolTmp = symbolGo.AddComponent<TextMeshProUGUI>();
                symbolTmp.text = "✦";
                symbolTmp.font = _fontCinzelBlack ?? TMP_Settings.defaultFontAsset;
                symbolTmp.fontSize = 56;
                symbolTmp.color = new Color(0.9f, 0.78f, 0.31f, 1f); // gold-light
                symbolTmp.alignment = TextAlignmentOptions.Center;
                symbolTmp.raycastTarget = false;
            }

            // TitleText — 부제 (TitleLabel 아래). 기존 것은 삭제하고 새로 만들어서
            // 숨겨진 layout override / 잘못된 참조를 완전히 제거.
            var oldTitleText = FindByNameIncludingInactive("TitleText");
            if (oldTitleText != null)
            {
                Object.DestroyImmediate(oldTitleText);
                Debug.Log("[TitleSceneStyleApplicator] 기존 TitleText 삭제 후 재생성");
            }

            var titleTextGo = new GameObject("TitleText", typeof(RectTransform), typeof(CanvasRenderer));
            titleTextGo.transform.SetParent(canvasTransform, false);
            var subRt = titleTextGo.GetComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0.5f, 0.5f);
            subRt.anchorMax = new Vector2(0.5f, 0.5f);
            subRt.pivot = new Vector2(0.5f, 0.5f);
            subRt.sizeDelta = new Vector2(900, 40);
            // ★ TitleLabel (y=140) 바로 위 — TitleLabel 위쪽 끝(210) 바로 위
            subRt.anchoredPosition = new Vector2(0, 235);

            var subTmp = titleTextGo.AddComponent<TextMeshProUGUI>();
            subTmp.text = "Dark Fantasy Roguelike";
            subTmp.font = _fontCinzelBold ?? TMP_Settings.defaultFontAsset;
            subTmp.fontSize = 20;
            subTmp.characterSpacing = 35;  // ★ 자간 넓힘 — TEAM LOG(96pt)와 비슷한 폭
            subTmp.fontStyle = FontStyles.Normal;
            subTmp.color = new Color(0.7f, 0.55f, 0.25f, 1f); // gold (조금 더 밝게)
            subTmp.alignment = TextAlignmentOptions.Center;
            subTmp.raycastTarget = false;
            EditorUtility.SetDirty(titleTextGo);
            EditorUtility.SetDirty(subRt);
            EditorUtility.SetDirty(subTmp);

            // 부모를 맨 위로 정렬 (Canvas 자식 중 가장 나중에 = 가장 위에 렌더링)
            titleLabel.transform.SetAsLastSibling();
            titleTextGo.transform.SetAsLastSibling();
            emblemGo.transform.SetAsLastSibling();
            // Emblem은 Title보다 먼저 렌더링되어야 (뒤에 깔려야) 하므로
            emblemGo.transform.SetSiblingIndex(Mathf.Max(0, titleLabel.transform.GetSiblingIndex()));
        }

        // =========================================================
        // 비활성 GameObject 포함 검색 헬퍼
        // =========================================================
        private static GameObject FindByNameIncludingInactive(string name)
        {
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                var found = FindRecursiveIncludingInactive(root.transform, name);
                if (found != null) return found;
            }
            return null;
        }

        private static GameObject FindRecursiveIncludingInactive(Transform t, string name)
        {
            if (t.name == name) return t.gameObject;
            for (int i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i);
                var found = FindRecursiveIncludingInactive(child, name);
                if (found != null) return found;
            }
            return null;
        }

        // =========================================================
        // 메뉴 버튼 3종 스타일
        // =========================================================
        private static void ApplyMenuButtonsStyle()
        {
            ApplyButtonStyle("NewGameButton", "MenuBtnPrimary.png",
                label: "NEW JOURNEY", labelColor: new Color(0.95f, 0.78f, 0.31f, 1f), fontSize: 18);
            ApplyButtonStyle("ContinueButton", "MenuBtnSecondary.png",
                label: "CONTINUE", labelColor: new Color(0.95f, 0.78f, 0.31f, 1f), fontSize: 16);
            ApplyButtonStyle("MetaShopButton", "MenuBtnTertiary.png",
                label: "SANCTUM SHOP", labelColor: new Color(0.84f, 0.78f, 0.65f, 1f), fontSize: 14);
        }

        private static void ApplyButtonStyle(string buttonName, string spriteFile, string label, Color labelColor, int fontSize)
        {
            var btn = FindByName(buttonName);
            if (btn == null)
            {
                Debug.LogWarning($"[TitleSceneStyleApplicator] {buttonName} 못 찾음 — 스킵");
                return;
            }

            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = LoadSprite(spriteFile);
                img.type = Image.Type.Sliced;
                img.color = Color.white;
                EditorUtility.SetDirty(img);
            }

            // 자식 Text (TMP) — 라벨 갱신
            var labelTmp = btn.GetComponentInChildren<TextMeshProUGUI>(true);
            if (labelTmp != null)
            {
                labelTmp.text = label;
                labelTmp.font = _fontCinzelBold ?? TMP_Settings.defaultFontAsset;
                labelTmp.fontSize = fontSize;
                labelTmp.color = labelColor;
                labelTmp.alignment = TextAlignmentOptions.Center;
                labelTmp.fontStyle = FontStyles.Bold;
                labelTmp.raycastTarget = false;
                EditorUtility.SetDirty(labelTmp);
            }

            EditorUtility.SetDirty(btn);
        }

        // =========================================================
        // Ascension 스타일 — 하단 우측 anchor + 화살표 Sprite
        // =========================================================
        private static void ApplyAscensionStyle()
        {
            var panel = FindByName("AscensionPanel");
            if (panel != null)
            {
                // 하단 우측으로 재배치
                var rt = panel.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(1f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(1f, 0f);
                    rt.anchoredPosition = new Vector2(-60, 60);
                    EditorUtility.SetDirty(rt);
                }
                EditorUtility.SetDirty(panel);
            }

            // AscensionLabel — 폰트/색상
            var label = FindByName("AscensionLabel");
            if (label != null)
            {
                var tmp = label.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.font = _fontCinzelBold ?? TMP_Settings.defaultFontAsset;
                    tmp.fontSize = 14;
                    tmp.color = new Color(0.95f, 0.78f, 0.31f, 1f);
                    tmp.alignment = TextAlignmentOptions.Center;
                    EditorUtility.SetDirty(tmp);
                }
            }

            // AscensionUpButton / DownButton — ArrowBtn Sprite
            ApplyArrowButton("AscensionUpButton", "▲");
            ApplyArrowButton("AscensionDownButton", "▼");
        }

        private static void ApplyArrowButton(string buttonName, string symbol)
        {
            var btn = FindByName(buttonName);
            if (btn == null) return;

            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = LoadSprite("ArrowBtn.png");
                img.type = Image.Type.Sliced;
                img.color = Color.white;
                EditorUtility.SetDirty(img);
            }

            var labelTmp = btn.GetComponentInChildren<TextMeshProUGUI>(true);
            if (labelTmp != null)
            {
                labelTmp.text = symbol;
                labelTmp.font = _fontCinzelBold ?? TMP_Settings.defaultFontAsset;
                labelTmp.fontSize = 14;
                labelTmp.color = new Color(0.95f, 0.78f, 0.31f, 1f);
                labelTmp.alignment = TextAlignmentOptions.Center;
                EditorUtility.SetDirty(labelTmp);
            }
            EditorUtility.SetDirty(btn);
        }

        // =========================================================
        // 미사용 통계 UI 비활성 (사용자 요청 — 표시 제거)
        // =========================================================
        private static void HideUnusedStatsUI()
        {
            // StatsLabel — 총 런/승리/최고 층/캐릭터/기억/영혼
            HideIfFound("StatsLabel");
            // TopBar — 메모리/영혼 상단 표시
            HideIfFound("TopBar");
            HideIfFound("MemoryLabel");
            HideIfFound("SoulLabel");
        }

        private static void HideIfFound(string name)
        {
            var go = FindByName(name);
            if (go != null)
            {
                go.SetActive(false);
                EditorUtility.SetDirty(go);
                Debug.Log($"[TitleSceneStyleApplicator] {name} 비활성 (사용자 요청 — 통계 UI 제거)");
            }
        }
    }
}
#endif
