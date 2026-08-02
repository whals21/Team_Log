using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using TeamLog.Characters;

namespace TeamLog.UI.Battle
{
    /// <summary>
    /// ★ Phase CC (2026-07-22): 슬롯 위 CC(Stun/Freeze/Sleep) 시각 연출.
    /// 웹 목업 시안(Assets/UI_Mockup/CC_Slot_Visual_Proposal.html)의 연출을 Unity로 이식.
    ///
    /// 연출:
    ///   - Stun (기절): 금색 별 3개 회전 + 슬롯 미세 흔들림
    ///   - Freeze (빙결): 시안 결정 점멸 + 서리 패턴 오버레이
    ///   - Sleep (수면): 보라 z자 3개 위로 떠다님
    ///
    /// 공통:
    ///   - 회색 처리 (반투명 어두운 오버레이)
    ///   - 클릭 차단 (별도 — ActionSlotUI.SetCC에서 button.interactable=false)
    ///   - CC 라벨 (하단 "기절"/"빙결"/"수면")
    ///
    /// 사용: PlayerSidebarPanel/EnemyDetailPanel에서 Show(StatusEffectType) 호출.
    /// ★ 2026-07-22 수정: ActionSlotUI 전용 → 캐릭터 패널通用 (사용자 피드백 "반대로 됨").
    /// </summary>
    public class CCVisualizer : MonoBehaviour
    {
        // === 색상 (웹 시안 기준) ===
        private static readonly Color StunColor = new Color(1.0f, 0.94f, 0.25f); // 번개 노랑
        private static readonly Color FreezeColor = new Color(0.37f, 0.78f, 0.91f); // 시안
        private static readonly Color SleepColor = new Color(0.65f, 0.55f, 1.0f); // 보라
        private static readonly Color GrayscaleOverlayColor = new Color(0.25f, 0.25f, 0.30f, 0.45f);

        // === 현재 상태 ===
        private StatusEffectType _currentCC = StatusEffectType.None;
        private GameObject _overlay;
        private Image _grayscaleOverlay;
        private readonly List<GameObject> _animatedParts = new();

        // === 애니메이션 내부 상태 ===
        private RectTransform _stunStarsParent;
        private Image _freezeCrystal;
        private readonly List<RectTransform> _sleepZs = new();
        private float _animTimer;

        /// <summary>현재 표시 중인 CC (None이면 비활성).</summary>
        public StatusEffectType CurrentCC => _currentCC;

        /// <summary>CC 표시 — 타입에 따라 오버레이 생성.</summary>
        public void Show(StatusEffectType ccType)
        {
            if (ccType == _currentCC) return; // 이미 같은 CC 표시 중
            Hide();

            if (ccType != StatusEffectType.Stun
                && ccType != StatusEffectType.Freeze
                && ccType != StatusEffectType.Sleep)
                return;

            _currentCC = ccType;
            CreateOverlay(ccType);
        }

        /// <summary>CC 표시 제거.</summary>
        public void Hide()
        {
            _currentCC = StatusEffectType.None;

            foreach (var part in _animatedParts)
            {
                if (part != null) Destroy(part);
            }
            _animatedParts.Clear();

            if (_overlay != null) Destroy(_overlay);
            _overlay = null;
            _grayscaleOverlay = null;
            _stunStarsParent = null;
            _freezeCrystal = null;
            _sleepZs.Clear();
        }

        private void CreateOverlay(StatusEffectType ccType)
        {
            // === 1. 회색 처리 (반투명 어두운 덮개) ===
            _overlay = new GameObject("CC_Overlay");
            _overlay.transform.SetParent(transform, false);
            var overlayRt = _overlay.AddComponent<RectTransform>();
            StretchToParent(overlayRt);

            // 회색 오버레이
            var grayGo = new GameObject("Grayscale");
            grayGo.transform.SetParent(_overlay.transform, false);
            var grayRt = grayGo.AddComponent<RectTransform>();
            StretchToParent(grayRt);
            _grayscaleOverlay = grayGo.AddComponent<Image>();
            _grayscaleOverlay.color = GrayscaleOverlayColor;
            _grayscaleOverlay.raycastTarget = false;

            // === 2. CC별 전용 연출 ===
            switch (ccType)
            {
                case StatusEffectType.Stun:
                    CreateStunStars();
                    CreateCCTag("기절", StunColor);
                    break;
                case StatusEffectType.Freeze:
                    CreateFreezeCrystal();
                    CreateFrostPattern();
                    CreateCCTag("빙결", FreezeColor);
                    break;
                case StatusEffectType.Sleep:
                    CreateSleepZs();
                    CreateCCTag("수면", SleepColor);
                    break;
            }
        }

        // ════════════════════════════════════════════════════════════════
        // Stun: 별 3개 회전
        // ════════════════════════════════════════════════════════════════
        private void CreateStunStars()
        {
            var parent = new GameObject("StunStars");
            parent.transform.SetParent(_overlay.transform, false);
            _stunStarsParent = parent.AddComponent<RectTransform>();
            _stunStarsParent.anchorMin = new Vector2(0.5f, 1f);
            _stunStarsParent.anchorMax = new Vector2(0.5f, 1f);
            _stunStarsParent.pivot = new Vector2(0.5f, 0.5f);
            _stunStarsParent.anchoredPosition = new Vector2(0, 20f);
            _stunStarsParent.sizeDelta = new Vector2(50f, 50f);

            for (int i = 0; i < 3; i++)
            {
                var starGo = new GameObject($"Star_{i}");
                starGo.transform.SetParent(_stunStarsParent, false);
                var starRt = starGo.AddComponent<RectTransform>();
                float angle = i * 120f * Mathf.Deg2Rad;
                starRt.anchoredPosition = new Vector2(Mathf.Cos(angle) * 18f, Mathf.Sin(angle) * 18f);
                starRt.sizeDelta = new Vector2(20f, 20f);

                var starTmp = starGo.AddComponent<TextMeshProUGUI>();
                starTmp.text = "✦";
                starTmp.fontSize = 18;
                starTmp.alignment = TextAlignmentOptions.Center;
                starTmp.color = StunColor;
                starTmp.raycastTarget = false;
                starTmp.enableWordWrapping = false;
                _animatedParts.Add(starGo);
            }
        }

        // ════════════════════════════════════════════════════════════════
        // Freeze: 얼음 결정 (상단 중앙) + 서리 패턴 (배경)
        // ════════════════════════════════════════════════════════════════
        private void CreateFreezeCrystal()
        {
            var crystalGo = new GameObject("FreezeCrystal");
            crystalGo.transform.SetParent(_overlay.transform, false);
            var crystalRt = crystalGo.AddComponent<RectTransform>();
            crystalRt.anchorMin = new Vector2(0.5f, 1f);
            crystalRt.anchorMax = new Vector2(0.5f, 1f);
            crystalRt.pivot = new Vector2(0.5f, 0.5f);
            crystalRt.anchoredPosition = new Vector2(0, 18f);
            crystalRt.sizeDelta = new Vector2(30f, 30f);

            var crystalTmp = crystalGo.AddComponent<TextMeshProUGUI>();
            crystalTmp.text = "❄";
            crystalTmp.fontSize = 22;
            crystalTmp.alignment = TextAlignmentOptions.Center;
            crystalTmp.color = Color.white;
            crystalTmp.raycastTarget = false;
            crystalTmp.enableWordWrapping = false;
            _freezeCrystal = crystalGo.AddComponent<Image>();
            _freezeCrystal.color = new Color(1, 1, 1, 0); // Image는 투명 (TMP가 보임), raycast 차단
            _freezeCrystal.raycastTarget = false;
            _animatedParts.Add(crystalGo);
        }

        private void CreateFrostPattern()
        {
            // 서리 패턴 — 반투명 시안 틴트 추가
            if (_grayscaleOverlay != null)
            {
                _grayscaleOverlay.color = new Color(0.20f, 0.45f, 0.55f, 0.50f);
            }
        }

        // ════════════════════════════════════════════════════════════════
        // Sleep: z 3개 위로 떠다님
        // ════════════════════════════════════════════════════════════════
        private void CreateSleepZs()
        {
            var zContainer = new GameObject("SleepZs");
            zContainer.transform.SetParent(_overlay.transform, false);
            var zContainerRt = zContainer.AddComponent<RectTransform>();
            zContainerRt.anchorMin = new Vector2(0.5f, 1f);
            zContainerRt.anchorMax = new Vector2(0.5f, 1f);
            zContainerRt.pivot = new Vector2(0.5f, 0.5f);
            zContainerRt.anchoredPosition = new Vector2(0, 25f);
            zContainerRt.sizeDelta = new Vector2(60f, 50f);

            for (int i = 0; i < 3; i++)
            {
                var zGo = new GameObject($"Z_{i}");
                zGo.transform.SetParent(zContainerRt, false);
                var zRt = zGo.AddComponent<RectTransform>();
                zRt.anchoredPosition = new Vector2(-15f + i * 10f, 0);
                zRt.sizeDelta = new Vector2(20f, 20f);

                var zTmp = zGo.AddComponent<TextMeshProUGUI>();
                zTmp.text = "z";
                zTmp.fontSize = 14 + i * 4;
                zTmp.alignment = TextAlignmentOptions.Center;
                zTmp.color = SleepColor;
                zTmp.raycastTarget = false;
                zTmp.enableWordWrapping = false;

                _sleepZs.Add(zRt);
                _animatedParts.Add(zGo);
            }
        }

        // ════════════════════════════════════════════════════════════════
        // CC 라벨 (하단)
        // ════════════════════════════════════════════════════════════════
        private void CreateCCTag(string label, Color color)
        {
            var tagGo = new GameObject("CCTag");
            tagGo.transform.SetParent(_overlay.transform, false);
            var tagRt = tagGo.AddComponent<RectTransform>();
            tagRt.anchorMin = new Vector2(0.5f, 0f);
            tagRt.anchorMax = new Vector2(0.5f, 0f);
            tagRt.pivot = new Vector2(0.5f, 0.5f);
            tagRt.anchoredPosition = new Vector2(0, 10f);
            tagRt.sizeDelta = new Vector2(60f, 18f);

            var tagBg = tagGo.AddComponent<Image>();
            tagBg.color = new Color(color.r, color.g, color.b, 0.25f);
            tagBg.raycastTarget = false;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(tagGo.transform, false);
            var labelRt = labelGo.AddComponent<RectTransform>();
            StretchToParent(labelRt);

            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text = label;
            labelTmp.fontSize = 11;
            labelTmp.fontStyle = FontStyles.Bold;
            labelTmp.alignment = TextAlignmentOptions.Center;
            labelTmp.color = color;
            labelTmp.raycastTarget = false;
            labelTmp.enableWordWrapping = false;
            UIKoreanFont.EnsureFont(labelTmp);

            _animatedParts.Add(tagGo);
        }

        // ════════════════════════════════════════════════════════════════
        // 매 프레임 애니메이션
        // ════════════════════════════════════════════════════════════════
        private void Update()
        {
            if (_currentCC == StatusEffectType.None) return;
            _animTimer += Time.unscaledDeltaTime;

            switch (_currentCC)
            {
                case StatusEffectType.Stun:
                    UpdateStunAnimation();
                    break;
                case StatusEffectType.Freeze:
                    UpdateFreezeAnimation();
                    break;
                case StatusEffectType.Sleep:
                    UpdateSleepAnimation();
                    break;
            }
        }

        private void UpdateStunAnimation()
        {
            // 별 부모 회전 (시계 방향, 2초에 1바퀴)
            if (_stunStarsParent != null)
            {
                float angle = (_animTimer / 2f) * 360f;
                _stunStarsParent.localRotation = Quaternion.Euler(0, 0, -angle);
            }

            // ★ 2026-07-22: 슬롯 흔들림(transform.localPosition 변경) 제거 —
            // 사용자 피드백 "옆으로 이동하면서 슬롯 내용이 안 보임". 회전만 유지.
        }

        private void UpdateFreezeAnimation()
        {
            // 결정 점멸 (1.5초 주기)
            if (_freezeCrystal != null)
            {
                float pulse = 0.6f + 0.4f * (0.5f + 0.5f * Mathf.Sin(_animTimer * Mathf.PI / 0.75f));
                _freezeCrystal.color = new Color(1, 1, 1, pulse);
            }
        }

        private void UpdateSleepAnimation()
        {
            // z 3개 시차 배치 위로 이동 + 페이드
            for (int i = 0; i < _sleepZs.Count; i++)
            {
                var zRt = _sleepZs[i];
                if (zRt == null) continue;

                // 2.4초 주기, 0.8초 간격
                float phase = ((_animTimer + i * 0.8f) % 2.4f) / 2.4f;
                // 0~1: 위로 이동 (0 → -25f local Y)
                float yOffset = Mathf.Lerp(0, -25f, phase);
                zRt.anchoredPosition = new Vector2(zRt.anchoredPosition.x, yOffset);

                // alpha: 0~0.2: fade in, 0.2~0.8: 유지, 0.8~1: fade out
                float alpha;
                if (phase < 0.2f) alpha = phase / 0.2f;
                else if (phase > 0.8f) alpha = (1f - phase) / 0.2f;
                else alpha = 1f;

                var tmp = zRt.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    var c = tmp.color;
                    c.a = alpha;
                    tmp.color = c;
                }
            }
        }

        // ════════════════════════════════════════════════════════════════
        // 유틸
        // ════════════════════════════════════════════════════════════════
        private static void StretchToParent(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void OnDestroy()
        {
            // ActionSlotUI 파괴 시 자식 오버레이는 자동 파괴. 별도 처리 불필요.
        }
    }
}
