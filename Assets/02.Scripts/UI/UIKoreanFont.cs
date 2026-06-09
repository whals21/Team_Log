using TMPro;
using UnityEngine;

namespace TeamLog.UI
{
    /// <summary>
    /// 런타임 TMP 텍스트에 NanumGothic SDF 폰트를 제공하는 정적 헬퍼
    /// TMP Settings fallback이 설정되어 있지만, 명시적 할당이 더 안전함
    /// </summary>
    public static class UIKoreanFont
    {
        private static TMP_FontAsset s_cachedFont;

        /// <summary>
        /// NanumGothic SDF 폰트 에셋 (첫 접근 시 TMP_Settings fallback에서 로드 후 캐시)
        /// </summary>
        public static TMP_FontAsset Font
        {
            get
            {
                if (s_cachedFont != null) return s_cachedFont;

                // 1. TMP_Settings fallback에서 찾기
                if (TMP_Settings.fallbackFontAssets != null)
                {
                    foreach (var fb in TMP_Settings.fallbackFontAssets)
                    {
                        if (fb != null) { s_cachedFont = fb; return s_cachedFont; }
                    }
                }

                // 2. 기존 씬의 TMP 컴포넌트에서 빌려오기
                var existing = Object.FindObjectOfType<TextMeshProUGUI>();
                if (existing != null && existing.font != null)
                    s_cachedFont = existing.font;

                return s_cachedFont;
            }
        }

        /// <summary>
        /// TMP 텍스트에 한국어 폰트가 설정되어 있지 않으면 할당
        /// </summary>
        public static void EnsureFont(TextMeshProUGUI tmp)
        {
            if (tmp == null) return;
            if (tmp.font == null || tmp.font.name == "LiberationSans SDF")
            {
                var font = Font;
                if (font != null) tmp.font = font;
            }
        }
    }
}
