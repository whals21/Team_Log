using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TeamLog.UI
{
    /// <summary>
    /// ★ UI 작업 공통 헬퍼 (UIBestPractices.md 기반).
    /// Party Selection Scene 작업에서 학습한 노하우를 재사용 가능한 유틸로 제공.
    ///
    /// 다음 UI 작업(전투 씬 UI 개편 등)에서 적극 활용하여 동일한 실수 방지.
    ///
    /// 핵심 기능:
    ///   1. EnsureButton — Button 컴포넌트 보완 (null이면 AddComponent, targetGraphic 설정)
    ///   2. FindDescendantByName — 자손 GameObject 재귀 검색 (비활성 포함)
    ///   3. DisableChildRaycastsExcept — 자식 Image raycastTarget=false 강제 (특정 Image 제외)
    ///   4. StretchToParent — RectTransform 부모 영역 전체 채우기
    ///   5. SafeText — 특수 Unicode 기호를 ASCII로 변환 (폰트 미지원 문자 □ 방지)
    ///   6. AddLayoutElementSafe — GameObject에 LayoutElement 자동 추가 (0 크기 붕괴 방지)
    /// </summary>
    public static class UIAutoBindHelper
    {
        // =========================================================
        // 1. Button 보완
        // =========================================================

        /// <summary>
        /// Button 컴포넌트가 null이면 자동으로 GetComponent/AddComponent.
        /// targetGraphic도 null이면 지정된 Graphic으로 설정.
        /// </summary>
        public static void EnsureButton(MonoBehaviour owner, ref Button button, Graphic targetGraphic = null)
        {
            if (button == null)
            {
                button = owner.GetComponent<Button>();
                if (button == null)
                {
                    button = owner.gameObject.AddComponent<Button>();
                }
            }
            if (targetGraphic != null)
            {
                targetGraphic.raycastTarget = true;
                if (button.targetGraphic == null)
                    button.targetGraphic = targetGraphic;
            }
        }

        // =========================================================
        // 2. 자손 검색
        // =========================================================

        /// <summary>
        /// Transform의 자손 중 지정 이름의 GameObject를 재귀 검색.
        /// 비활성 GameObject도 검색 포함.
        /// </summary>
        public static GameObject FindDescendantByName(Transform root, string name)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name == name) return child.gameObject;
                var found = FindDescendantByName(child, name);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// 자손에서 T 타입 컴포넌트 검색. includeInactive=true면 비활성도 포함.
        /// </summary>
        public static T FindDescendantComponent<T>(Transform root, string gameObjectName = null, bool includeInactive = true) where T : Component
        {
            if (gameObjectName != null)
            {
                var go = FindDescendantByName(root, gameObjectName);
                return go?.GetComponent<T>();
            }
            return root.GetComponentInChildren<T>(includeInactive);
        }

        // =========================================================
        // 3. Raycast 가로채기 방지
        // =========================================================

        /// <summary>
        /// root의 모든 자손 Image의 raycastTarget을 false로 설정.
        /// exclude에 지정된 Image는 true 유지 (Button 클릭 감지용).
        ///
        /// 부모 Button의 클릭이 자식 UI(오버레이/배지/텍스트 배경 등)에 가로채이지 않도록.
        /// </summary>
        public static void DisableChildRaycastsExcept(Transform root, params Graphic[] exclude)
        {
            var excludeSet = new HashSet<Graphic>(exclude);
            var graphics = root.GetComponentsInChildren<Graphic>(true);
            foreach (var graphic in graphics)
            {
                if (!excludeSet.Contains(graphic))
                    graphic.raycastTarget = false;
            }
        }

        // =========================================================
        // 4. RectTransform 스트레치
        // =========================================================

        /// <summary>
        /// RectTransform을 부모 영역 전체로 스트레치 (Anchor 0,0 ~ 1,1).
        /// Image/Text가 부모를 꽉 채우게 할 때 사용.
        /// </summary>
        public static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        // =========================================================
        // 5. SafeText — 특수 기호 ASCII 변환
        // =========================================================

        /// <summary>
        /// 특수 Unicode 기호를 ASCII로 변환.
        /// NanumGothic SDF/Cinzel SDF가 지원하지 않는 기호가 □로 표시되는 문제 방지.
        /// 사용자가 폰트 fallback을 확장하면 이 변환은 불필요 (제거 가능).
        /// </summary>
        public static string SafeText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text
                .Replace("⚙", "CFG")
                .Replace("⚜", "")
                .Replace("⚡", ">>")
                .Replace("⚠", "! ")
                .Replace("◈", "•")
                .Replace("✦", "*")
                .Replace("✕", "X")
                .Replace("▶", ">")
                .Replace("‹", "<")
                .Replace("›", ">")
                .Replace("◐", "G ")
                .Replace("✚", "+")
                .Replace("♪", "M")
                .Replace("☠", "X")
                .Replace("⚗", "A")
                .Replace("✓", "V")
                .Replace("🔒", "[L]");
        }

        // =========================================================
        // 6. LayoutElement 자동 추가
        // =========================================================

        /// <summary>
        /// GameObject에 LayoutElement가 없으면 추가하고 값 설정.
        /// LayoutGroup 안에서 0 크기로 붕괴하지 않도록.
        /// </summary>
        public static LayoutElement EnsureLayoutElement(GameObject go,
            float prefW = -1, float prefH = -1,
            float minW = -1, float minH = -1,
            float flexW = -1, float flexH = -1)
        {
            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            if (prefW >= 0) le.preferredWidth = prefW;
            if (prefH >= 0) le.preferredHeight = prefH;
            if (minW >= 0) le.minWidth = minW;
            if (minH >= 0) le.minHeight = minH;
            if (flexW >= 0) le.flexibleWidth = flexW;
            if (flexH >= 0) le.flexibleHeight = flexH;
            return le;
        }

        /// <summary>
        /// 자식 GameObject를 생성하면서 LayoutElement 자동 부여.
        /// SceneBuilder에서 LayoutGroup 자식 만들 때 사용.
        /// </summary>
        public static GameObject CreateLayoutChild(string name, Transform parent,
            float prefW = -1, float prefH = -1,
            float minW = -1, float minH = -1,
            float flexW = -1, float flexH = -1)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            EnsureLayoutElement(go, prefW, prefH, minW, minH, flexW, flexH);
            return go;
        }

        // =========================================================
        // 7. Initialize 패턴 헬퍼
        // =========================================================

        /// <summary>
        /// UI 컴포넌트 Initialize 공통 패턴:
        /// 1. Button 보완
        /// 2. targetGraphic 설정
        /// 3. 자식 raycast 비활성화
        ///
        /// 사용 예:
        ///   public void Initialize(MyData data) {
        ///       _data = data;
        ///       UIAutoBindHelper.InitializeInteractive(this, ref _button, _background);
        ///       Render();
        ///   }
        /// </summary>
        public static void InitializeInteractive(MonoBehaviour owner, ref Button button, Graphic clickTarget)
        {
            EnsureButton(owner, ref button, clickTarget);
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
            DisableChildRaycastsExcept(owner.transform, clickTarget);
        }
    }
}
