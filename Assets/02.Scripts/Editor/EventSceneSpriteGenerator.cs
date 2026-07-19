#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

namespace TeamLog.Editor
{
    /// <summary>
    /// ★ Stained Glass Event UI — procedural Sprite 생성기.
    /// EventReworkView가 사용하는 9종 Sprite를 코드로 생성 (외부 에셋 의존성 최소).
    ///
    /// 메뉴: TeamLog/UI/Generate Event Scene Sprites
    /// 출력: Assets/03.Data/UI/EventScene/
    ///
    /// 출력 Sprite 9종:
    ///   - GlassWindow_Story/Treasure/Trap/NPC/Shrine.png (EventType별 5종, 512x512)
    ///   - PanelBackground.png (9-slice, 글라스 패널 배경)
    ///   - DimBackground.png (전체 화면 어둠 오버레이)
    ///   - ChoiceRow_Bg.png (9-slice, 선택지 행 배경)
    ///   - ChoiceRow_RiskTag.png (작은 pill 배경)
    /// </summary>
    public static class EventSceneSpriteGenerator
    {
        private const string OUTPUT_DIR = "Assets/03.Data/UI/EventScene";

        // 다크 판타지 고딕 톤 — EventType별 색상 (EventTypeSkinDatabase와 일치)
        private static readonly Color GoldL      = HexColor("#f4d35e");
        private static readonly Color Gold       = HexColor("#c9a14a");
        private static readonly Color Void       = HexColor("#050507");
        private static readonly Color Abyss      = HexColor("#0a0a14");
        private static readonly Color Lead       = HexColor("#1a1a22");  // 스테인드글라스 리드(납선)

        [MenuItem("TeamLog/UI/Generate Event Scene Sprites")]
        public static void GenerateAll()
        {
            EnsureOutputDirectory();

            GenerateGlassWindow("Story",    HexColor("#6e7a9c"), HexColor("#9ba8c8"));
            GenerateGlassWindow("Treasure", HexColor("#d4af37"), HexColor("#f4d35e"));
            GenerateGlassWindow("Trap",     HexColor("#a83232"), HexColor("#c0392b"));
            GenerateGlassWindow("NPC",      HexColor("#c98a3a"), HexColor("#e0a85a"));
            GenerateGlassWindow("Shrine",   HexColor("#6fa8a3"), HexColor("#9bc8c4"));

            GeneratePanelBackground();
            GenerateDimBackground();
            GenerateChoiceRowBg();
            GenerateChoiceRowRiskTag();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[EventSceneSpriteGenerator] 9 sprites generated → {OUTPUT_DIR}");
        }

        // =========================================================
        // GlassWindow — EventType별 스테인드글라스 아치 (512x512)
        // =========================================================
        private static void GenerateGlassWindow(string typeName, Color primary, Color glow)
        {
            const int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];

            float cx = size * 0.5f;
            float cy = size * 0.55f; // 아치가 살짝 위로
            float archW = size * 0.45f; // 아치 반폭
            float archH = size * 0.45f; // 아치 높이
            float bottomY = size * 0.05f; // 사각 부분 시작

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    Color c = Color.clear;

                    bool insideArch = false;

                    if (y >= bottomY)
                    {
                        if (dy < 0)
                        {
                            // 사각 부분 — 너비 범위 안
                            if (Mathf.Abs(dx) < archW)
                                insideArch = true;
                        }
                        else
                        {
                            // 아치 부분 — 타원 경계
                            float nx = dx / archW;
                            float ny = dy / archH;
                            if (nx * nx + ny * ny < 1f)
                                insideArch = true;
                        }
                    }

                    if (insideArch)
                    {
                        // 중앙에서 외곽까지 거리 기반 라디얼 (빛이 중앙을 관통)
                        float rad = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy) / (archW * 1.1f));
                        c = Color.Lerp(glow * 0.9f, primary * 0.4f, rad);
                        c = Color.Lerp(c, Abyss * 0.6f, rad * 0.7f);

                        // 리드(납선) 패턴 — 가로/세로/대각선
                        int gx = Mathf.FloorToInt(x / 48f);
                        int gy = Mathf.FloorToInt(y / 48f);
                        bool onLead = (x % 48 < 2) || (y % 48 < 2) ||
                                      ((x + y) % 64 < 1) || ((x - y) % 64 < 1);
                        if (onLead)
                            c = Lead;

                        // 아치 외곽선 강조
                        if (dy >= 0)
                        {
                            float nx = dx / archW;
                            float ny = dy / archH;
                            float ellipse = nx * nx + ny * ny;
                            if (ellipse > 0.95f && ellipse < 1f)
                                c = GoldL;
                        }
                        if (Mathf.Abs(dx) > archW - 3f && Mathf.Abs(dx) < archW && dy < 0)
                            c = GoldL;
                    }

                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, $"GlassWindow_{typeName}.png", new Vector4(64, 64, 64, 64));
        }

        // =========================================================
        // PanelBackground — 글라스 패널 (9-slice, 256x256)
        // =========================================================
        private static void GeneratePanelBackground()
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Color c = Abyss;
                    // 미세 노이즈 (글라스 질감)
                    float n = (Mathf.Sin(x * 0.3f) * Mathf.Cos(y * 0.4f) + 1f) * 0.5f;
                    c = Color.Lerp(c, new Color(0.08f, 0.08f, 0.14f, 1f), n * 0.3f);

                    // 가장자리 8px — 금 테두리
                    if (x < 8 || x > size - 9 || y < 8 || y > size - 9)
                        c = Color.Lerp(Gold * 0.5f, GoldL, 0.3f);

                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "PanelBackground.png", new Vector4(12, 12, 12, 12));
        }

        // =========================================================
        // DimBackground — 전체 화면 어둠 오버레이 (반투명 검정)
        // =========================================================
        private static void GenerateDimBackground()
        {
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            Color dim = new Color(0.02f, 0.02f, 0.03f, 0.82f);
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = dim;
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "DimBackground.png", new Vector4(0, 0, 0, 0));
        }

        // =========================================================
        // ChoiceRow_Bg — 선택지 행 배경 (9-slice, 64x32)
        // =========================================================
        private static void GenerateChoiceRowBg()
        {
            const int w = 64, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color c = new Color(0.05f, 0.05f, 0.08f, 0.92f);
                    // 가장자리 4px — 어두운 금 테두리
                    if (x < 4 || x > w - 5 || y < 4 || y > h - 5)
                        c = new Color(0.35f, 0.27f, 0.1f, 1f); // 어두운 금
                    pixels[y * w + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "ChoiceRow_Bg.png", new Vector4(5, 5, 5, 5));
        }

        // =========================================================
        // ChoiceRow_RiskTag — 작은 pill 배경 (32x16)
        // =========================================================
        private static void GenerateChoiceRowRiskTag()
        {
            const int w = 32, h = 16;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color[w * h];
            Color transparent = new Color(1f, 1f, 1f, 0.15f); // 거의 투명한 흰색 — View에서 Color override

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // 모서리 둥글게 (8px 반경)
                    float dx = Mathf.Min(x, w - 1 - x);
                    float dy = Mathf.Min(y, h - 1 - y);
                    float dmin = Mathf.Min(dx, dy);
                    if (dmin < 2f)
                        pixels[y * w + x] = Color.clear;
                    else
                        pixels[y * w + x] = transparent;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "ChoiceRow_RiskTag.png", new Vector4(4, 4, 4, 4));
        }

        // =========================================================
        // 공통 헬퍼
        // =========================================================
        private static void EnsureOutputDirectory()
        {
            if (!AssetDatabase.IsValidFolder("Assets/03.Data/UI"))
                AssetDatabase.CreateFolder("Assets/03.Data", "UI");
            if (!AssetDatabase.IsValidFolder(OUTPUT_DIR))
                AssetDatabase.CreateFolder("Assets/03.Data/UI", "EventScene");
        }

        private static void SaveSprite(Texture2D tex, string fileName, Vector4 border,
            TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            string path = $"{OUTPUT_DIR}/{fileName}";
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            UnityEngine.Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[EventSceneSpriteGenerator] Importer null for {path}");
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
