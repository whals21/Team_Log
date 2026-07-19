#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

namespace TeamLog.Editor
{
    /// <summary>
    /// ★ Dark Sanctum Style (D 시안) — Title Scene용 procedural Sprite 생성기.
    ///
    /// 메뉴: TeamLog/UI/Generate Title Scene Sprites
    /// 출력: Assets/03.Data/UI/TitleScene/
    ///
    /// 출력 Sprite 7종:
    ///   - DarkGradientBg.png (1024×1024 — 중앙 밝, 외곽 어두움 + 미세 남/보라 틴트)
    ///   - MicroGrid.png (64×64 타일 — 미세 그리드 패턴)
    ///   - TitleEmblem.png (256×256 — 원형 황금 엠블럼 테두리)
    ///   - MenuBtnPrimary.png (64×32 9-slice — 핏빛 New Journey)
    ///   - MenuBtnSecondary.png (64×32 9-slice — 어두운 금 Continue)
    ///   - MenuBtnTertiary.png (64×32 9-slice — 투명 테두리 Sanctum Shop)
    ///   - ArrowBtn.png (32×32 — 어센션 ± 버튼)
    /// </summary>
    public static class TitleSceneSpriteGenerator
    {
        private const string OUTPUT_DIR = "Assets/03.Data/UI/TitleScene";

        private static readonly Color GoldL = HexColor("#e6c878");
        private static readonly Color Gold  = HexColor("#c9a14a");
        private static readonly Color GoldD = HexColor("#8b6e2f");
        private static readonly Color Void  = HexColor("#050507");
        private static readonly Color Void2 = HexColor("#0a0a14");
        private static readonly Color Blood = HexColor("#8b0000");
        private static readonly Color BloodL = HexColor("#c0392b");

        [MenuItem("TeamLog/UI/Generate Title Scene Sprites")]
        public static void GenerateAll()
        {
            EnsureOutputDirectory();

            GenerateDarkGradientBg();
            GenerateMicroGrid();
            GenerateTitleEmblem();
            GenerateMenuButton("MenuBtnPrimary.png",   primary: true);
            GenerateMenuButton("MenuBtnSecondary.png", primary: false);
            GenerateMenuButton("MenuBtnTertiary.png",  primary: false, transparent: true);
            GenerateArrowButton();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[TitleSceneSpriteGenerator] 7 sprites generated → {OUTPUT_DIR}");
        }

        // =========================================================
        // DarkGradientBg — 1024×1024 라디얼 그라데이션
        // =========================================================
        private static void GenerateDarkGradientBg()
        {
            const int size = 1024;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];

            // 중앙 위쪽에 옅은 황금빛, 하단에 옅은 보라빛
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (float)x / size;
                    float ny = (float)y / size;
                    float dx = nx - 0.5f;
                    float dy = ny - 0.5f;
                    float rad = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy) * 1.6f);

                    // 중앙은 Void2, 외곽은 Void
                    Color baseColor = Color.Lerp(Void2, Void, rad);

                    // 상단 황금빛 (ny 가까울수록)
                    if (ny > 0.5f)
                    {
                        float t = (ny - 0.5f) * 1.5f;
                        baseColor = Color.Lerp(baseColor, new Color(0.08f, 0.07f, 0.04f, 1f), t * (1f - rad) * 0.6f);
                    }
                    // 하단 보라빛
                    else
                    {
                        float t = (0.5f - ny) * 1.5f;
                        baseColor = Color.Lerp(baseColor, new Color(0.06f, 0.04f, 0.1f, 1f), t * (1f - rad) * 0.5f);
                    }

                    pixels[y * size + x] = baseColor;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "DarkGradientBg.png", new Vector4(0, 0, 0, 0));
        }

        // =========================================================
        // MicroGrid — 64×64 타일 (반복 패턴)
        // =========================================================
        private static void GenerateMicroGrid()
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];

            // 투명 배경 + 얇은 그리드 선 (1px, 2.5% alpha 흰색)
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool onLine = (x == 0 || y == 0);
                    pixels[y * size + x] = onLine
                        ? new Color(1f, 1f, 1f, 0.025f)
                        : Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "MicroGrid.png", new Vector4(0, 0, 0, 0), TextureWrapMode.Repeat);
        }

        // =========================================================
        // TitleEmblem — 원형 황금 테두리 (256×256)
        // =========================================================
        private static void GenerateTitleEmblem()
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];

            float center = size * 0.5f;
            float outerR = center - 6f;     // 외곽 원
            float glowR  = center - 20f;    // 글로우 끝
            float innerFadeR = center - 40f; // 내부 페이드 시작

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - center;
                    float dy = y + 0.5f - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    Color c = Color.clear;
                    if (dist > outerR + 1f)
                    {
                        // 외곽 바깥 — 투명
                        c = Color.clear;
                    }
                    else if (dist > outerR - 3f)
                    {
                        // 외곽선 — 황금
                        c = GoldL;
                    }
                    else if (dist > outerR - 6f)
                    {
                        // 외곽선 안쪽 글로우
                        float t = (dist - (outerR - 6f)) / 3f;
                        c = Color.Lerp(GoldD, GoldL, t);
                    }
                    else if (dist > glowR)
                    {
                        // 글로우 영역 — GoldD ~ 투명
                        float t = (dist - glowR) / (outerR - 6f - glowR);
                        c = Color.Lerp(Color.clear, new Color(GoldD.r, GoldD.g, GoldD.b, 0.15f), t);
                    }
                    else if (dist > innerFadeR)
                    {
                        // 내부 페이드 — 옅은 황금
                        float t = (dist - innerFadeR) / (glowR - innerFadeR);
                        c = new Color(Gold.r, Gold.g, Gold.b, Mathf.Lerp(0.02f, 0.15f, t));
                    }
                    else
                    {
                        // 중앙 — 거의 투명
                        c = new Color(Gold.r, Gold.g, Gold.b, 0.02f);
                    }

                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "TitleEmblem.png", new Vector4(0, 0, 0, 0));
        }

        // =========================================================
        // MenuButton — 9-slice (64×32)
        // =========================================================
        private static void GenerateMenuButton(string fileName, bool primary, bool transparent = false)
        {
            const int w = 64, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color[w * h];

            Color baseColor;
            Color borderColor;
            if (primary)
            {
                baseColor = new Color(0.35f, 0.05f, 0.05f, 1f);
                borderColor = GoldL;
            }
            else if (transparent)
            {
                baseColor = new Color(0f, 0f, 0f, 0f);
                borderColor = new Color(GoldD.r, GoldD.g, GoldD.b, 0.4f);
            }
            else
            {
                baseColor = new Color(0.06f, 0.06f, 0.1f, 0.85f);
                borderColor = GoldD;
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color c = baseColor;
                    // 위쪽이 살짝 밝음
                    if (primary || (!transparent))
                    {
                        float t = (float)y / h;
                        c = Color.Lerp(baseColor, new Color(baseColor.r + 0.08f, baseColor.g + 0.06f, baseColor.b + 0.04f, baseColor.a), t * 0.5f);
                    }

                    // 가장자리 2px — 테두리
                    if (x < 2 || x > w - 3 || y < 2 || y > h - 3)
                        c = borderColor;

                    pixels[y * w + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, fileName, new Vector4(4, 4, 4, 4));
        }

        // =========================================================
        // ArrowButton — 32×32 (어센션 ±)
        // =========================================================
        private static void GenerateArrowButton()
        {
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // 어두운 배경
                    Color c = new Color(0.04f, 0.03f, 0.06f, 0.95f);
                    // 가장자리 2px — 황금 테두리
                    if (x < 2 || x > size - 3 || y < 2 || y > size - 3)
                        c = GoldD;
                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "ArrowBtn.png", new Vector4(4, 4, 4, 4));
        }

        // =========================================================
        // 공통 헬퍼
        // =========================================================
        private static void EnsureOutputDirectory()
        {
            if (!AssetDatabase.IsValidFolder("Assets/03.Data/UI"))
                AssetDatabase.CreateFolder("Assets/03.Data", "UI");
            if (!AssetDatabase.IsValidFolder(OUTPUT_DIR))
                AssetDatabase.CreateFolder("Assets/03.Data/UI", "TitleScene");
        }

        private static void SaveSprite(Texture2D tex, string fileName, Vector4 border,
            TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            string path = $"{OUTPUT_DIR}/{fileName}";
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[TitleSceneSpriteGenerator] Importer null for {path}");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = wrapMode;
            importer.alphaIsTransparency = true;
            importer.spriteBorder = border;
            importer.SaveAndReimport();
        }

        private static Color HexColor(string hex)
        {
            hex = hex.Replace("#", "");
            byte r = System.Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = System.Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = System.Convert.ToByte(hex.Substring(4, 2), 16);
            byte a = hex.Length >= 8 ? System.Convert.ToByte(hex.Substring(6, 2), 16) : (byte)255;
            return new Color32(r, g, b, a);
        }
    }
}
#endif
