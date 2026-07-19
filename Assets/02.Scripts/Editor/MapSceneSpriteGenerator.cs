#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TeamLog.Editor
{
    /// <summary>
    /// Map Scene UI용 Sprite 자동 생성 도구 (Phase 2 — 2026-07-19).
    /// 웹 목업(MapScene_Mockup.html v2)의 디자인을 procedural Sprite로 베이킹.
    ///
    /// ★ PartySelectionSpriteGenerator와 동일 패턴 (UIBestPractices §9Sprite 생성 전략).
    /// 공통 에셋(골드 테두리, 양피지 패널, 남색 패널, 핏빛 버튼)은 PartySelection 것을 재사용.
    /// MapScene 전용 — 노드 아이콘 7종, 노드 프레임 글로우, 플레이어 마커, 양피지 라디얼 배경.
    ///
    /// 메뉴: TeamLog/UI/Generate Map Scene Sprites
    /// 출력: Assets/03.Data/UI/MapScene/
    ///
    /// 산출물:
    ///   - NodeIcon_Start.png   (원형 96x96 — 청록 + ⊙ 문양)
    ///   - NodeIcon_Battle.png  (원형 96x96 — 핏빛 + ⚔)
    ///   - NodeIcon_Elite.png   (원형 96x96 — 골드 + ★)
    ///   - NodeIcon_Boss.png    (원형 128x128 — 흑핏 + ☠ + 외곽 오라)
    ///   - NodeIcon_Event.png   (원형 96x96 — 연보라 + ?)
    ///   - NodeIcon_Shop.png    (원형 96x96 — 하늘 + $)
    ///   - NodeIcon_Rest.png    (원형 96x96 — 초록 + ⚜)
    ///   - NodeFrameGlow.png    (원형 링 160x160 — 활성 노드 외곽 글로우)
    ///   - PlayerMarker.png     (원형 96x96 — 골드 라디얼 + 중앙 고리)
    ///   - ParchmentRadial.png  (라디얼 512x512 — 중앙 밝 → 외곽 어두움)
    ///   - ThemeBannerBg.png    (가로 배너 512x128 — 위/아래 투명 그라디언트)
    /// </summary>
    public static class MapSceneSpriteGenerator
    {
        private const string OUTPUT_DIR = "Assets/03.Data/UI/MapScene";

        // 다크 판타지 고딕 토큰 (UIPalette DF 토큰과 동일)
        private static readonly Color GoldL   = HexColor("#f4d35e");
        private static readonly Color Gold    = HexColor("#d4af37");
        private static readonly Color GoldD   = HexColor("#8b6914");
        private static readonly Color GoldX   = HexColor("#4a3a0d");
        private static readonly Color Void    = HexColor("#050509");
        private static readonly Color Abyss   = HexColor("#0a0a14");
        private static readonly Color Depth   = HexColor("#11111f");
        private static readonly Color Slate   = HexColor("#1a1a2e");
        private static readonly Color Parchment  = HexColor("#c9b485");
        private static readonly Color ParchmentD = HexColor("#8a7752");
        private static readonly Color ParchmentX = HexColor("#2a2418");
        private static readonly Color Blood   = HexColor("#8b0000");
        private static readonly Color BloodL  = HexColor("#c0392b");
        private static readonly Color BloodDeep = HexColor("#5a0000");

        // 노드 타입 색상 (UIPalette와 동일)
        private static readonly Color NodeStart  = HexColor("#6ed5b2");
        private static readonly Color NodeBattle = HexColor("#c0392b");
        private static readonly Color NodeElite  = HexColor("#f4d35e");
        private static readonly Color NodeBoss   = HexColor("#8b0000");
        private static readonly Color NodeEvent  = HexColor("#b388ff");
        private static readonly Color NodeShop   = HexColor("#5ec5e8");
        private static readonly Color NodeRest   = HexColor("#4caf50");

        [MenuItem("TeamLog/UI/Generate Map Scene Sprites")]
        public static void GenerateAll()
        {
            EnsureOutputDirectory();

            // 노드 아이콘 7종
            GenerateNodeIcon("Start",  NodeStart,  iconType: "start");
            GenerateNodeIcon("Battle", NodeBattle, iconType: "battle");
            GenerateNodeIcon("Elite",  NodeElite,  iconType: "elite");
            GenerateNodeIcon("Boss",   NodeBoss,   iconType: "boss", size: 128);
            GenerateNodeIcon("Event",  NodeEvent,  iconType: "event");
            GenerateNodeIcon("Shop",   NodeShop,   iconType: "shop");
            GenerateNodeIcon("Rest",   NodeRest,   iconType: "rest");

            GenerateNodeFrameGlow();
            GeneratePlayerMarker();
            GenerateParchmentRadial();
            GenerateThemeBannerBg();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[MapSceneSpriteGenerator] 11 sprites generated at {OUTPUT_DIR}");
        }

        // =========================================================
        // 노드 아이콘 — 원형 + 라디얼 글로우 + 중앙 문양
        // =========================================================
        private static void GenerateNodeIcon(string name, Color nodeColor, string iconType, int size = 96)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            float center = size * 0.5f;
            float outerR = center - 2f;
            float midR = center - 8f;
            float innerR = center - 18f;

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
                        c = Color.clear;
                    }
                    else if (dist > outerR - 2f)
                    {
                        // 외곽선 2px — 골드 라이트
                        c = GoldL;
                    }
                    else if (dist > midR)
                    {
                        // 외곽 글로우 — 노드 색상
                        float t = (dist - midR) / (outerR - 2f - midR);
                        c = Color.Lerp(nodeColor, GoldL, t * 0.6f);
                    }
                    else
                    {
                        // 중앙 — 어두운 배경 + 노드 색상 미세 글로우
                        float t = dist / midR;
                        Color baseCol = Color.Lerp(Abyss, Depth, t * 0.5f);

                        // 중앙 문양은 별도로 그림 (아래)
                        c = baseCol;

                        // 노드 색상 미세 라디얼 글로우 (가장자리에 살짝)
                        if (t > 0.6f)
                        {
                            float glow = (t - 0.6f) / 0.4f;
                            c = Color.Lerp(c, nodeColor, glow * 0.3f);
                        }
                    }

                    // 중앙 문양 — iconType별로 픽셀 단위로 그림
                    if (dist < innerR)
                    {
                        Color iconColor = GetIconPixelColor(dx, dy, iconType, innerR);
                        if (iconColor.a > 0f)
                        {
                            c = Color.Lerp(c, iconColor, iconColor.a);
                        }
                    }

                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, $"NodeIcon_{name}.png", new Vector4(0, 0, 0, 0));
        }

        /// <summary>
        /// 중앙 아이콘 문양 — 간단한 픽셀 패턴.
        /// 복잡한 글리프는 폰트(TMP)로 처리하므로 여기서는 단순한 기하학적 형태만.
        /// </summary>
        private static Color GetIconPixelColor(float dx, float dy, string iconType, float radius)
        {
            float absX = Mathf.Abs(dx);
            float absY = Mathf.Abs(dy);
            float r = Mathf.Sqrt(dx * dx + dy * dy);

            switch (iconType)
            {
                case "start":
                    // ⊙ — 중앙 점 + 외곽 원
                    if (r < radius * 0.18f) return HexColor("#6ed5b2");
                    if (Mathf.Abs(r - radius * 0.65f) < 1.5f) return HexColor("#6ed5b2");
                    return Color.clear;

                case "battle":
                    // ⚔ — 십자가 (수직/수평선)
                    if (absX < 2f && dy < radius * 0.6f && dy > -radius * 0.6f) return HexColor("#ff6b35");
                    if (absY < 2f && dx < radius * 0.6f && dx > -radius * 0.6f) return HexColor("#ff6b35");
                    // 대각선 (칼날)
                    if (Mathf.Abs(dx - dy) < 2.5f && r < radius * 0.7f) return HexColor("#c0392b");
                    if (Mathf.Abs(dx + dy) < 2.5f && r < radius * 0.7f) return HexColor("#c0392b");
                    return Color.clear;

                case "elite":
                    // ★ — 5점 별
                    return GetStarPixel(dx, dy, radius, HexColor("#f4d35e"), 5, 0.45f);

                case "boss":
                    // ☠ — 해골 단순화 (두 원 + 아래 삼각형)
                    if (r < radius * 0.55f)
                    {
                        // 두 눈 ( 좌/우 원)
                        float leftEyeDist = Mathf.Sqrt((dx + radius * 0.22f) * (dx + radius * 0.22f) + dy * dy);
                        float rightEyeDist = Mathf.Sqrt((dx - radius * 0.22f) * (dx - radius * 0.22f) + dy * dy);
                        if (leftEyeDist < radius * 0.16f) return Color.clear; // 눈 구멍
                        if (rightEyeDist < radius * 0.16f) return Color.clear;
                        return HexColor("#c0392b");
                    }
                    return Color.clear;

                case "event":
                    // ? — 물음표 단순화 (위쪽 호 + 아래 점)
                    if (r > radius * 0.4f && r < radius * 0.6f && dy > 0f)
                    {
                        if (dx > -radius * 0.2f && dx < radius * 0.4f) return HexColor("#b388ff");
                    }
                    if (absX < 2.5f && dy > -radius * 0.4f && dy < 0f) return HexColor("#b388ff");
                    if (r < radius * 0.15f && dy < -radius * 0.5f) return HexColor("#b388ff");
                    return Color.clear;

                case "shop":
                    // $ — 달러 사인 (수직선 + S 곡선 단순화)
                    if (absX < 1.5f && dy < radius * 0.6f && dy > -radius * 0.6f) return HexColor("#5ec5e8");
                    // S 곡선 근사 (위쪽/아래쪽 호)
                    if (r > radius * 0.3f && r < radius * 0.5f)
                    {
                        if (dy > radius * 0.2f && dx < 0f) return HexColor("#5ec5e8");
                        if (dy < -radius * 0.2f && dx > 0f) return HexColor("#5ec5e8");
                    }
                    return Color.clear;

                case "rest":
                    // ⚜ — fleur-de-lis 단순화 (중앙 수직 + 좌우 곡선)
                    if (absX < 1.5f && dy < radius * 0.5f && dy > -radius * 0.5f) return HexColor("#4caf50");
                    if (r > radius * 0.35f && r < radius * 0.55f)
                    {
                        if (dx < 0f && dy > -radius * 0.1f && dy < radius * 0.3f) return HexColor("#4caf50");
                        if (dx > 0f && dy > -radius * 0.1f && dy < radius * 0.3f) return HexColor("#4caf50");
                    }
                    return Color.clear;

                default:
                    return Color.clear;
            }
        }

        /// <summary>
        /// 별 모양 픽셀 패턴 (n점별)
        /// </summary>
        private static Color GetStarPixel(float dx, float dy, float radius, Color color, int points, float innerRatio)
        {
            float angle = Mathf.Atan2(dy, dx); // -π ~ π
            float normalized = (angle + Mathf.PI) / (2f * Mathf.PI); // 0~1
            float segmentAngle = 1f / points;
            float localAngle = normalized % segmentAngle / segmentAngle; // 0~1 (한 변 내에서)
            // 별 꼭짓점/오목점 교번 — localAngle 0/1은 꼭짓점, 0.5는 오목점
            float pointDist = Mathf.Lerp(radius * 0.85f, radius * innerRatio * 0.85f,
                Mathf.Abs(localAngle - 0.5f) * 2f);
            float r = Mathf.Sqrt(dx * dx + dy * dy);
            if (r < pointDist) return color;
            return Color.clear;
        }

        // =========================================================
        // 노드 외곽 글로우 (활성 노드 강조)
        // =========================================================
        private static void GenerateNodeFrameGlow()
        {
            const int size = 160;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            float center = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - center;
                    float dy = y + 0.5f - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    Color c = Color.clear;
                    // 외곽 링 (점선 효과는 shader/material에서 처리 — 여기선 단순 라디얼 글로우)
                    if (dist > center - 16f && dist < center - 4f)
                    {
                        float t = (dist - (center - 16f)) / 12f;
                        // 외곽은 투명, 중간이 가장 밝음, 내곽은 투명
                        float alpha = Mathf.Sin(t * Mathf.PI);
                        c = new Color(GoldL.r, GoldL.g, GoldL.b, alpha * 0.6f);
                    }

                    pixels[y * size + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "NodeFrameGlow.png", new Vector4(0, 0, 0, 0));
        }

        // =========================================================
        // 플레이어 마커 — 골드 라디얼 글로우 + 중앙 고리
        // =========================================================
        private static void GeneratePlayerMarker()
        {
            const int size = 96;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            float center = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - center;
                    float dy = y + 0.5f - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    Color c;
                    if (dist > center - 2f)
                    {
                        c = Color.clear;
                    }
                    else if (dist > 14f && dist < 22f)
                    {
                        // 중앙 고리 (골드 라이트)
                        float t = Mathf.Abs(dist - 18f) / 4f;
                        c = Color.Lerp(GoldL, Gold, t);
                    }
                    else if (dist < 40f)
                    {
                        // 외곽 라디얼 글로우 (골드 → 투명)
                        float t = dist / 40f;
                        float alpha = (1f - t) * 0.4f;
                        c = new Color(GoldL.r, GoldL.g, GoldL.b, alpha);
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
            SaveSprite(tex, "PlayerMarker.png", new Vector4(0, 0, 0, 0));
        }

        // =========================================================
        // 양피지 라디얼 배경 — 중앙 밝 → 외곽 어두움
        // =========================================================
        private static void GenerateParchmentRadial()
        {
            const int size = 512;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            float center = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float t = Mathf.Clamp01(dist / center);

                    // ★ Priority 3 — alpha 상향 (0.6 → 0.95), 외곽 감쇠 완만하게 (0.7 → 0.5)
                    // 중앙은 양피지 + 핏빛 틴트, 외곽은 void로 페이드
                    Color parchment = Color.Lerp(ParchmentX, Void, t);
                    parchment = Color.Lerp(parchment, BloodDeep, 0.18f * (1f - t));
                    parchment.a = 0.95f * (1f - t * 0.5f);
                    pixels[y * size + x] = parchment;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "ParchmentRadial.png", new Vector4(0, 0, 0, 0));
        }

        // =========================================================
        // 테마 배너 배경 — 위/아래 투명한 가로 그라디언트
        // =========================================================
        private static void GenerateThemeBannerBg()
        {
            const int w = 512;
            const int h = 128;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                // 위/아래는 투명, 중앙은 양피지 어두운
                float verticalT = Mathf.Abs(y - h * 0.5f) / (h * 0.5f);
                // ★ Priority 3 — alpha 상향 (0.5 → 0.85)
                float alpha = Mathf.Sin((1f - verticalT) * Mathf.PI) * 0.85f;

                for (int x = 0; x < w; x++)
                {
                    // 좌우도 투명하게 (중앙 강조)
                    float horizontalT = Mathf.Abs(x - w * 0.5f) / (w * 0.5f);
                    alpha *= Mathf.Sin((1f - horizontalT) * Mathf.PI);

                    Color c = Color.Lerp(ParchmentX, ParchmentD, 0.3f);
                    c.a = alpha;
                    pixels[y * w + x] = c;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            SaveSprite(tex, "ThemeBannerBg.png", new Vector4(0, 0, 0, 0));
        }

        // =========================================================
        // 공통 유틸 (PartySelectionSpriteGenerator와 동일 패턴)
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

        private static void EnsureOutputDirectory()
        {
            if (!AssetDatabase.IsValidFolder("Assets/03.Data/UI"))
                AssetDatabase.CreateFolder("Assets/03.Data", "UI");
            if (!AssetDatabase.IsValidFolder(OUTPUT_DIR))
                AssetDatabase.CreateFolder("Assets/03.Data/UI", "MapScene");
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
                Debug.LogWarning($"[MapSceneSpriteGenerator] Importer null for {path}");
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
    }
}
#endif
