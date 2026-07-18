#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TeamLog.Editor
{
    /// <summary>
    /// Party Selection UI용 Sprite 자동 생성 도구 (UI-A.2).
    /// 웹 목업의 디자인을 procedural Sprite로 베이킹하여 9-slice로 사용 가능하게 만든다.
    /// 자원별 배지/골드 테두리/양피지 패널/핏빛 버튼/루나 패턴/문장 로고를 코드로 생성.
    ///
    /// 메뉴: TeamLog/UI/Generate Party Selection Sprites
    /// 출력: Assets/03.Data/UI/PartySelection/
    ///
    /// 산출물:
    ///  - ResourceBadge_{Name}.png         × 11종 (원형 128x128)
    ///  - GoldBorder_9Slice.png            (사각 48x48, border 12,12,12,12)
    ///  - GoldBorderThin_9Slice.png        (사각 48x48, border 6,6,6,6)
    ///  - ParchmentPanel_9Slice.png        (사각 64x64, border 8,8,8,8)
    ///  - ParchmentDark_9Slice.png         (사각 64x64, border 8,8,8,8)
    ///  - BloodButton_Normal.png           (사각 48x48, border 8,8,8,8)
    ///  - BloodButton_Hover.png
    ///  - BloodButton_Pressed.png
    ///  - SlatePanel_9Slice.png            (기본 남색 패널, border 8,8,8,8)
    ///  - SlatePanelLight_9Slice.png
    ///  - RuneOverlay_Tile.png             (타일 200x200)
    ///  - Crest_Logo.png                   (문장 로고 128x128)
    ///  - Shadow_Vignette.png              (코너 비네팅 256x256)
    /// </summary>
    public static class PartySelectionSpriteGenerator
    {
        private const string OUTPUT_DIR = "Assets/03.Data/UI/PartySelection";

        // 색상 (UIPalette DF 토큰과 동일 — 하드코딩으로 자체 포함)
        private static readonly Color GoldL   = HexColor("#f4d35e");
        private static readonly Color Gold    = HexColor("#d4af37");
        private static readonly Color GoldD   = HexColor("#8b6914");
        private static readonly Color GoldX   = HexColor("#4a3a0d");
        private static readonly Color Void    = HexColor("#050509");
        private static readonly Color Abyss   = HexColor("#0a0a14");
        private static readonly Color Slate   = HexColor("#1a1a2e");
        private static readonly Color Slate2  = HexColor("#232347");
        private static readonly Color Parchment  = HexColor("#c9b485");
        private static readonly Color ParchmentD = HexColor("#8a7752");
        private static readonly Color ParchmentX = HexColor("#2a2418");
        private static readonly Color Blood   = HexColor("#8b0000");
        private static readonly Color BloodL  = HexColor("#c0392b");
        private static readonly Color BloodDeep = HexColor("#5a0000");

        // 자원별 색상 (UIPalette Resource 토큰과 동일)
        private static readonly Dictionary<string, Color> ResourceColors = new()
        {
            { "Ember",     HexColor("#ff6b35") },
            { "Vengeance", HexColor("#a8324a") },
            { "Frost",     HexColor("#5ec5e8") },
            { "Prophecy",  HexColor("#6ed5b2") },
            { "Charge",    HexColor("#f7d046") },
            { "Shadows",   HexColor("#9b6ec2") },
            { "Combo",     HexColor("#d4a017") },
            { "Corpse",    HexColor("#7da34a") },
            { "Discover",  HexColor("#b388ff") },
            { "Melody",    HexColor("#ff8fab") },
            { "Mercy",     HexColor("#ffe082") }
        };

        [MenuItem("TeamLog/UI/Generate Party Selection Sprites")]
        public static void GenerateAll()
        {
            EnsureOutputDirectory();

            int generated = 0;
            foreach (var kvp in ResourceColors)
            {
                GenerateResourceBadge(kvp.Key, kvp.Value);
                generated++;
            }

            GenerateGoldBorder9Slice();
            GenerateGoldBorderThin9Slice();
            GenerateParchmentPanel();
            GenerateParchmentDarkPanel();
            GenerateSlatePanel(isLight: false);
            GenerateSlatePanel(isLight: true);
            GenerateBloodButton("Normal", BloodL, Blood, BloodDeep);
            GenerateBloodButton("Hover",  HexColor("#d04030"), HexColor("#a02020"), HexColor("#600000"));
            GenerateBloodButton("Pressed",Blood, BloodDeep, HexColor("#3a0000"));
            GenerateRuneOverlay();
            GenerateCrestLogo();
            GenerateVignette();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[PartySelectionSpriteGenerator] {generated + 12} sprites generated at {OUTPUT_DIR}");
        }

        // =========================================================
        // 자원별 배지 — 원형 + 라디얼 그라디언트 + 골드 외곽
        // =========================================================
        private static void GenerateResourceBadge(string name, Color resColor)
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            float center = size * 0.5f;
            float outerR = center - 2f;
            float innerR = center - 8f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - center;
                    float dy = y + 0.5f - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    Color c;
                    if (dist > outerR)
                    {
                        // 외곽선 영역 바깥 — 투명
                        c = Color.clear;
                    }
                    else if (dist > outerR - 2f)
                    {
                        // 골드 외곽선 (2px)
                        c = GoldL;
                    }
                    else if (dist > innerR)
                    {
                        // 골드 띠 외곽 페이드
                        c = Color.Lerp(Gold, GoldL, (dist - innerR) / (outerR - 2f - innerR));
                    }
                    else
                    {
                        // 자원 라디얼 그라디언트
                        float t = dist / innerR;
                        // 중심은 자원색, 외곽은 void
                        c = Color.Lerp(resColor, Void, Mathf.Clamp01(t * 1.1f));
                        // 자원색 광역 글로우 효과 (자원색 알파 약간)
                        if (t < 0.4f)
                            c = Color.Lerp(c, resColor, (0.4f - t) * 0.5f);
                    }
                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, $"ResourceBadge_{name}.png", new Vector4(0, 0, 0, 0));
        }

        // =========================================================
        // 골드 테두리 9-slice (일반)
        // =========================================================
        private static void GenerateGoldBorder9Slice()
        {
            const int size = 48;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            const int bw = 4; // 외곽선 두께

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isEdge = x < bw || x >= size - bw || y < bw || y >= size - bw;
                    bool isInnerEdge =
                        (x >= bw && x < bw + 1) ||
                        (x < size - bw && x >= size - bw - 1) ||
                        (y >= bw && y < bw + 1) ||
                        (y < size - bw && y >= size - bw - 1);

                    Color c;
                    if (isEdge)
                    {
                        // 외곽 4px — 골드 그라디언트
                        float edgeDist = Mathf.Min(x, y, size - 1 - x, size - 1 - y);
                        c = Color.Lerp(GoldL, GoldD, edgeDist / bw);
                    }
                    else if (isInnerEdge)
                    {
                        // 안쪽 1px 어두운 라인
                        c = GoldX;
                    }
                    else
                    {
                        // 투명 (배경)
                        c = Color.clear;
                    }
                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "GoldBorder_9Slice.png", new Vector4(12, 12, 12, 12));
        }

        // =========================================================
        // 골드 테두리 9-slice (얇은 버전)
        // =========================================================
        private static void GenerateGoldBorderThin9Slice()
        {
            const int size = 48;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            const int bw = 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isEdge = x < bw || x >= size - bw || y < bw || y >= size - bw;
                    Color c;
                    if (isEdge)
                    {
                        c = GoldL;
                    }
                    else if (x == bw || x == size - bw - 1 || y == bw || y == size - bw - 1)
                    {
                        c = GoldX;
                    }
                    else
                    {
                        c = Color.clear;
                    }
                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "GoldBorderThin_9Slice.png", new Vector4(6, 6, 6, 6));
        }

        // =========================================================
        // 양피지 패널 — 세피아 그라디언트
        // =========================================================
        private static void GenerateParchmentPanel()
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            const int bw = 4;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Color c;
                    bool isEdge = x < bw || x >= size - bw || y < bw || y >= size - bw;
                    if (isEdge)
                    {
                        // 양피지 외곽 — 어두운 갈색
                        c = ParchmentD;
                    }
                    else
                    {
                        // 양피지 본문 — 대각선 그라디언트 (밝은 갈색 → 어두운 갈색)
                        float t = (x + y) / (float)(size * 2);
                        c = Color.Lerp(Parchment, ParchmentX, t * 0.5f);
                        c.a = 0.85f;
                    }
                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "ParchmentPanel_9Slice.png", new Vector4(8, 8, 8, 8));
        }

        // =========================================================
        // 양피지 패널 (어두운 버전)
        // =========================================================
        private static void GenerateParchmentDarkPanel()
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            const int bw = 4;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isEdge = x < bw || x >= size - bw || y < bw || y >= size - bw;
                    Color c;
                    if (isEdge)
                    {
                        c = HexColor("#4d3f28");
                    }
                    else
                    {
                        float t = (x + y) / (float)(size * 2);
                        c = Color.Lerp(HexColor("#2a2418"), HexColor("#1a1410"), t);
                        c.a = 0.92f;
                    }
                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "ParchmentDark_9Slice.png", new Vector4(8, 8, 8, 8));
        }

        // =========================================================
        // 남색 패널 (Slate / Slate2)
        // =========================================================
        private static void GenerateSlatePanel(bool isLight)
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            const int bw = 4;
            Color top = isLight ? Slate2 : Slate;
            Color bottom = isLight ? Slate : HexColor("#11111f");

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isEdge = x < bw || x >= size - bw || y < bw || y >= size - bw;
                    Color c;
                    if (isEdge)
                    {
                        c = GoldD;
                        if (x == 0 || y == 0) c = GoldL; // 좌/상단 밝게
                    }
                    else
                    {
                        // 수직 그라디언트
                        float t = y / (float)size;
                        c = Color.Lerp(top, bottom, t);
                        c.a = 0.95f;
                    }
                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            string name = isLight ? "SlatePanelLight_9Slice.png" : "SlatePanel_9Slice.png";
            SaveSprite(tex, name, new Vector4(8, 8, 8, 8));
        }

        // =========================================================
        // 핏빛 버튼 3-state
        // =========================================================
        private static void GenerateBloodButton(string suffix, Color top, Color middle, Color bottom)
        {
            const int size = 48;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            const int bw = 4;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isEdge = x < bw || x >= size - bw || y < bw || y >= size - bw;
                    Color c;
                    if (isEdge)
                    {
                        c = GoldL;
                        if (x == 0 || y == 0) c = HexColor("#fff4b8"); // 밝은 하이라이트
                    }
                    else
                    {
                        float t = y / (float)size;
                        c = t < 0.5f
                            ? Color.Lerp(top, middle, t * 2f)
                            : Color.Lerp(middle, bottom, (t - 0.5f) * 2f);
                    }
                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, $"BloodButton_{suffix}.png", new Vector4(8, 8, 8, 8));
        }

        // =========================================================
        // 루나 오버레이 (양피지 알파 0.04 룬 패턴)
        // =========================================================
        private static void GenerateRuneOverlay()
        {
            const int size = 200;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            float cx = size * 0.5f;
            float cy = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    Color c = Color.clear;

                    // 외곽 원
                    if (Math.Abs(dist - 80f) < 1.5f) c = WithAlpha(Parchment, 0.06f);
                    // 중간 원
                    else if (Math.Abs(dist - 55f) < 1.2f) c = WithAlpha(Parchment, 0.05f);
                    // 내곽 원
                    else if (Math.Abs(dist - 30f) < 1f) c = WithAlpha(Parchment, 0.04f);
                    // 다이아몬드 (세로)
                    else if (Math.Abs(Math.Abs(dx) + Math.Abs(dy) - 65f) < 1.5f) c = WithAlpha(Parchment, 0.05f);
                    // 다이아몬드 (가로)
                    else if (Math.Abs(Math.Abs(dx) + Math.Abs(dy) - 45f) < 1.2f) c = WithAlpha(Parchment, 0.04f);

                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "RuneOverlay_Tile.png", new Vector4(0, 0, 0, 0), wrapMode: TextureWrapMode.Repeat);
        }

        // =========================================================
        // 문장 로고 (팀 로고)
        // =========================================================
        private static void GenerateCrestLogo()
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            float cx = size * 0.5f;
            float cy = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    Color c = Color.clear;

                    // 외곽 골드 링
                    if (Math.Abs(dist - 56f) < 2f)
                    {
                        c = Gold;
                    }
                    // 내부 글로우
                    else if (dist < 50f)
                    {
                        float t = dist / 50f;
                        c = Color.Lerp(GoldL, Color.clear, t);
                        c.a *= 0.3f;
                    }

                    // 중앙 핏빛 점
                    if (dist < 6f)
                    {
                        c = BloodL;
                    }

                    // 지그재그 (7개 뾰족점)
                    // y가 24, 32, 40, 48, 56, 64, 72, 80, 88, 96 일 때 가장자리 뾰족점
                    int spikeY = (int)(y);
                    bool onSpikeRow = (spikeY >= 24 && spikeY <= 96) && ((spikeY - 24) % 8 == 0);
                    if (onSpikeRow && Math.Abs(dx) < 18f && Math.Abs(dy) < 6f)
                    {
                        // 뾰족점 (가로 좁은 띠)
                        float spikeWidth = 6f - Math.Abs(dy);
                        if (spikeWidth > 0 && Math.Abs(dx) < spikeWidth * 1.5f)
                            c = GoldL;
                    }

                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "Crest_Logo.png", new Vector4(0, 0, 0, 0));
        }

        // =========================================================
        // 비네팅 (코너 그림자)
        // =========================================================
        private static void GenerateVignette()
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            float cx = size * 0.5f;
            float cy = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float t = Mathf.Clamp01(dist / (size * 0.5f));
                    // 외곽은 검정, 중심은 투명
                    float alpha = Mathf.Clamp01((t - 0.5f) * 2f) * 0.7f;
                    pixels[y * size + x] = new Color(0, 0, 0, alpha);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "Shadow_Vignette.png", new Vector4(0, 0, 0, 0));
        }

        // =========================================================
        // 공통 유틸
        // =========================================================
        private static Color HexColor(string hex)
        {
            hex = hex.Replace("#", "");
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            byte a = hex.Length >= 8 ? Convert.ToByte(hex.Substring(6, 2), 16) : (byte)255;
            return new Color32(r, g, b, a);
        }

        private static Color WithAlpha(Color c, float a)
        {
            c.a = a;
            return c;
        }

        private static void EnsureOutputDirectory()
        {
            if (!AssetDatabase.IsValidFolder("Assets/03.Data/UI"))
                AssetDatabase.CreateFolder("Assets/03.Data", "UI");
            if (!AssetDatabase.IsValidFolder(OUTPUT_DIR))
                AssetDatabase.CreateFolder("Assets/03.Data/UI", "PartySelection");
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
                Debug.LogWarning($"[PartySelectionSpriteGenerator] Importer null for {path}");
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

            // Debug.Log($"[PartySelectionSpriteGenerator] Saved {fileName} (border: {border})");
        }
    }
}
#endif
