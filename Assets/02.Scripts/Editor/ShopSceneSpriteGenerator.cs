#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

namespace TeamLog.Editor
{
    /// <summary>
    /// ★ Stained Glass Shop UI — procedural Sprite 생성기 (B 시안).
    /// ShopReworkView가 사용하는 7종 Sprite를 코드로 생성.
    ///
    /// 메뉴: TeamLog/UI/Generate Shop Scene Sprites
    /// 출력: Assets/03.Data/UI/ShopScene/
    ///
    /// 출력 Sprite 7종:
    ///   - GlassCrown.png (900×80 — 상단 스테인드글라스 장식)
    ///   - PanelBackground.png (256×256 9-slice — 글라스 패널)
    ///   - DimBackground.png (32×32 — 전체 화면 어둠)
    ///   - SlotBg.png (64×64 9-slice — 상점 슬롯 배경)
    ///   - TabButton.png (64×32 9-slice — Buy/Sell 탭)
    ///   - TabButtonActive.png (64×32 9-slice — 활성 탭)
    ///   - LeaveButton.png (64×32 9-slice — Leave 버튼 핏빛)
    /// </summary>
    public static class ShopSceneSpriteGenerator
    {
        private const string OUTPUT_DIR = "Assets/03.Data/UI/ShopScene";

        private static readonly Color GoldL = HexColor("#f4d35e");
        private static readonly Color Gold  = HexColor("#c9a14a");
        private static readonly Color GoldD = HexColor("#8b6e2f");
        private static readonly Color Void  = HexColor("#050507");
        private static readonly Color Abyss = HexColor("#0a0a14");
        private static readonly Color Lead  = HexColor("#1a1a22");
        private static readonly Color Blood = HexColor("#8b0000");
        private static readonly Color BloodL = HexColor("#c0392b");

        [MenuItem("TeamLog/UI/Generate Shop Scene Sprites")]
        public static void GenerateAll()
        {
            EnsureOutputDirectory();

            GenerateGlassCrown();
            GeneratePanelBackground();
            GenerateDimBackground();
            GenerateSlotBg();
            GenerateTabButton(active: false, fileName: "TabButton.png");
            GenerateTabButton(active: true, fileName: "TabButtonActive.png");
            GenerateLeaveButton();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[ShopSceneSpriteGenerator] 7 sprites generated → {OUTPUT_DIR}");
        }

        // =========================================================
        // GlassCrown — 가로형 스테인드글라스 (900×80)
        // =========================================================
        private static void GenerateGlassCrown()
        {
            const int w = 900, h = 80;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color[w * h];

            // 아치형이 아니라 가로 보폭 전체를 채우는 패턴 — Event의 GlassWindow를 가로로 늘린 형태
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = (float)x / w;
                    float ny = (float)y / h;
                    Color c;

                    // 라디얼 그라데이션 — 중앙 위쪽에서 황금빛이 퍼짐
                    float dx = nx - 0.5f;
                    float dy = 0.5f - ny;
                    float rad = Mathf.Clamp01(Mathf.Sqrt(dx * dx * 2f + dy * dy) * 1.5f);

                    // 3구역 색상 — 가로 방향 밴드
                    float band = Mathf.Sin(nx * Mathf.PI * 6f) * 0.5f + 0.5f;
                    Color bandColor = Color.Lerp(GoldL, GoldD, band);

                    c = Color.Lerp(GoldL * 0.9f, bandColor * 0.4f, rad);
                    c = Color.Lerp(c, Abyss * 0.7f, rad * 0.6f);

                    // 리드(납선) — 60px 간격
                    bool onLead = (x % 60 < 2) || (y % 20 < 1);
                    // 대각선 리드
                    int diag = (x + y * 2) % 80;
                    if (diag < 1) onLead = true;
                    if (onLead) c = Lead;

                    // 가장자리 (위/아래) — 어두운 테두리
                    if (y < 3 || y > h - 4) c = Color.Lerp(c, Void, 0.7f);

                    pixels[y * w + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "GlassCrown.png", new Vector4(16, 16, 16, 16));
        }

        // =========================================================
        // PanelBackground — 어두운 글라스 패널 (256×256 9-slice)
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
                    // 미세 노이즈
                    float n = (Mathf.Sin(x * 0.3f) * Mathf.Cos(y * 0.4f) + 1f) * 0.5f;
                    c = Color.Lerp(c, new Color(0.1f, 0.08f, 0.16f, 1f), n * 0.3f);

                    // 가장자리 8px — 황금 테두리
                    if (x < 8 || x > size - 9 || y < 8 || y > size - 9)
                        c = Color.Lerp(GoldD * 0.6f, GoldL, 0.25f);

                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "PanelBackground.png", new Vector4(12, 12, 12, 12));
        }

        // =========================================================
        // DimBackground — 전체 화면 어둠 오버레이
        // =========================================================
        private static void GenerateDimBackground()
        {
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            Color dim = new Color(0.02f, 0.02f, 0.03f, 0.85f);
            for (int i = 0; i < pixels.Length; i++) pixels[i] = dim;
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "DimBackground.png", new Vector4(0, 0, 0, 0));
        }

        // =========================================================
        // SlotBg — 상점 슬롯 배경 (64×64 9-slice)
        // =========================================================
        private static void GenerateSlotBg()
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // 어두운 갈색 배경
                    Color c = new Color(0.08f, 0.06f, 0.04f, 0.95f);
                    // 미세 질감
                    float n = (Mathf.Sin(x * 1.5f) * Mathf.Cos(y * 1.7f) + 1f) * 0.5f;
                    c = Color.Lerp(c, new Color(0.12f, 0.09f, 0.06f, 0.95f), n * 0.4f);

                    // 가장자리 4px — 어두운 금 테두리
                    if (x < 4 || x > size - 5 || y < 4 || y > size - 5)
                        c = new Color(0.35f, 0.27f, 0.1f, 1f);

                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "SlotBg.png", new Vector4(5, 5, 5, 5));
        }

        // =========================================================
        // TabButton — Buy/Sell 탭 (64×32 9-slice)
        // =========================================================
        private static void GenerateTabButton(bool active, string fileName)
        {
            const int w = 64, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color[w * h];

            // 활성 — 진한 황금 / 비활성 — 어두운 갈색
            Color baseColor = active ? new Color(0.55f, 0.4f, 0.12f, 1f) : new Color(0.2f, 0.15f, 0.05f, 0.7f);
            Color borderColor = active ? GoldL : GoldD;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color c = baseColor;
                    // 미세 그라데이션 — 위쪽이 살짝 밝음
                    if (active)
                        c = Color.Lerp(baseColor, new Color(0.7f, 0.5f, 0.15f, 1f), (float)y / h * 0.4f);

                    // 가장자리 2px — 황금 테두리 (활성만)
                    if (active && (x < 2 || x > w - 3 || y < 2 || y > h - 3))
                        c = borderColor;
                    else if (!active && (x < 2 || x > w - 3 || y < 2 || y > h - 3))
                        c = new Color(0.25f, 0.18f, 0.07f, 1f);

                    pixels[y * w + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, fileName, new Vector4(4, 4, 4, 4));
        }

        // =========================================================
        // LeaveButton — 핏빛 Leave 버튼 (64×32 9-slice)
        // =========================================================
        private static void GenerateLeaveButton()
        {
            const int w = 64, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // 진한 핏빛 — 위쪽이 살짝 밝음
                    float t = (float)y / h;
                    Color c = Color.Lerp(new Color(0.4f, 0.05f, 0.05f, 1f), Blood, t * 0.6f);

                    // 가장자리 2px — 황금 테두리
                    if (x < 2 || x > w - 3 || y < 2 || y > h - 3)
                        c = GoldD;

                    pixels[y * w + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "LeaveButton.png", new Vector4(4, 4, 4, 4));
        }

        // =========================================================
        // 공통 헬퍼 (EventSceneSpriteGenerator와 동일 패턴)
        // =========================================================
        private static void EnsureOutputDirectory()
        {
            if (!AssetDatabase.IsValidFolder("Assets/03.Data/UI"))
                AssetDatabase.CreateFolder("Assets/03.Data", "UI");
            if (!AssetDatabase.IsValidFolder(OUTPUT_DIR))
                AssetDatabase.CreateFolder("Assets/03.Data/UI", "ShopScene");
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
                Debug.LogWarning($"[ShopSceneSpriteGenerator] Importer null for {path}");
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
