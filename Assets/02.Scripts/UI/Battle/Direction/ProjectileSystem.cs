using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TeamLog.Characters;

namespace TeamLog.UI.Battle.Direction
{
    /// <summary>
    /// ★ Phase GF (2026-07-21): B1 — 투사체 시스템.
    /// 시전자 패널 → 타겟 패널 포물선 비행, 스킬 타입별 색상, 잔상 트레일, 도착 폭발 콜백.
    ///
    /// 구현 전략:
    ///   - UI Image (게임오브젝트 아님) — ScreenSpaceOverlay 자식
    ///   - 부모: DirectionLayer (BattleDirectionController가 생성)
    ///   - 좌표 변환: WorldToScreenPoint + ScreenPointToLocalPointInRectangle
    ///   - 포물선: 2차 Bezier (시작 → 중간(위로 arcHeight) → 도착)
    ///   - 잔상: 일정 간격으로 동일 Image 복제, 알파 페이드
    ///   - 도착: onArrive 콜백 → 외부에서 폭발 VFX 처리
    ///
    /// 리소스 전략:
    ///   - 초기: WhiteSprite (Texture2D.whiteTexture) + Image.color로 속성 표현
    ///   - 후속: procedural Sprite 에디터 메뉴로 스킬별 스프라이트 생성 (Phase 5)
    /// </summary>
    public class ProjectileSystem : MonoBehaviour
    {
        [Header("Flight")]
        [SerializeField] private float _flightDuration = 0.4f;
        [SerializeField] private float _arcHeight = 100f;

        [Header("Trail")]
        [SerializeField] private int _maxTrails = 5;
        [SerializeField] private float _trailInterval = 0.04f;
        [SerializeField] private float _trailFadeDuration = 0.25f;

        [Header("Projectile Visual")]
        [SerializeField] private float _projectileSize = 24f;

        private RectTransform _layer;
        private Sprite _whiteSprite;
        // ★ P0-2 수정 (2026-07-21): ScreenSpaceCamera 모드 대응 — 부모 Canvas worldCamera 캐싱.
        // 기존 null 전달은 ScreenSpaceOverlay 전용이라 BattleUICanvas(Camera 모드)에서 왜곡 발생.
        private Canvas _parentCanvas;
        private Camera _uiCamera;

        // 스킬 타입/속성 → 색상 매핑
        private static Color ResolveColor(SkillType type, StatusEffectType element)
        {
            if (element == StatusEffectType.Burn)   return new Color(1.0f, 0.45f, 0.10f); // 불 주황
            if (element == StatusEffectType.Freeze) return new Color(0.30f, 0.65f, 1.0f); // 얼음 파랑
            if (element == StatusEffectType.Poison) return new Color(0.45f, 0.85f, 0.20f); // 독 녹색
            if (element == StatusEffectType.Stun)   return new Color(1.0f, 0.85f, 0.20f); // 번개 노랑

            switch (type)
            {
                case SkillType.Heal:   return new Color(1.0f, 0.85f, 0.30f); // 금빛
                case SkillType.Shield: return new Color(0.55f, 0.30f, 0.85f); // 보라
                case SkillType.Buff:   return new Color(0.95f, 0.75f, 0.20f); // 황금
                case SkillType.Debuff: return new Color(0.55f, 0.30f, 0.85f); // 보라
                case SkillType.Purify: return new Color(0.20f, 0.85f, 0.90f); // 청록
                default:               return new Color(1.0f, 0.45f, 0.10f); // 기본 Attack 주황
            }
        }

        private void Awake()
        {
            _layer = transform as RectTransform;
            if (_layer != null)
            {
                // 부모 DirectionLayer 전체를 덮도록 stretch 설정 —
                // WorldToScreenPoint → ScreenPointToLocalPointInRectangle이 정확하려면 전체 화면覆盖 필요
                _layer.anchorMin = Vector2.zero;
                _layer.anchorMax = Vector2.one;
                _layer.offsetMin = Vector2.zero;
                _layer.offsetMax = Vector2.zero;
                _layer.anchoredPosition = Vector2.zero;
            }

            // ★ P0-2 수정: ScreenSpaceCamera 모드 대응 — 부모 Canvas와 worldCamera 캐싱.
            _parentCanvas = GetComponentInParent<Canvas>();
            _uiCamera = _parentCanvas != null ? _parentCanvas.worldCamera : null;

            _whiteSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0, 0, 4, 4),
                new Vector2(0.5f, 0.5f), 100f);
        }

        public void SpawnProjectile(Transform from, Transform to, SkillType skillType,
            StatusEffectType element, Action onArrive = null)
        {
            if (_layer == null || from == null || to == null)
            {
                onArrive?.Invoke();
                return;
            }

            // 시전자/타겟 스크린 좌표 → 레이어 로컬 좌표
            var (startLocal, endLocal) = WorldToLocalPoints(from, to);
            var color = ResolveColor(skillType, element);

            StartCoroutine(FlightRoutine(startLocal, endLocal, color, onArrive));
        }

        private (Vector2, Vector2) WorldToLocalPoints(Transform from, Transform to)
        {
            // ★ ScreenSpaceCamera 모드에서는 부모 Canvas의 worldCamera 사용.
            // ScreenSpaceOverlay에서는 null. 부모 Canvas renderMode로 자동 분기.
            Camera cam = _parentCanvas != null && _parentCanvas.renderMode == RenderMode.ScreenSpaceCamera
                ? _uiCamera
                : null;

            var fromScreen = RectTransformUtility.WorldToScreenPoint(cam, from.position);
            var toScreen = RectTransformUtility.WorldToScreenPoint(cam, to.position);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_layer, fromScreen, cam, out var startLocal);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_layer, toScreen, cam, out var endLocal);
            return (startLocal, endLocal);
        }

        private IEnumerator FlightRoutine(Vector2 start, Vector2 end, Color color, Action onArrive)
        {
            // 투사체 생성
            var projGo = CreateProjectileGo(color);
            var rt = projGo.transform as RectTransform;
            rt.anchoredPosition = start;

            // 잔상 관리
            var trailPool = new List<GameObject>();
            float trailTimer = 0f;
            float elapsed = 0f;

            // Bezier 중간점 (위로 arcHeight)
            Vector2 mid = Vector2.Lerp(start, end, 0.5f) + Vector2.up * _arcHeight;

            while (elapsed < _flightDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                trailTimer += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / _flightDuration);
                // 2차 Bezier: B(t) = (1-t)²·start + 2(1-t)t·mid + t²·end
                Vector2 pos = (1 - t) * (1 - t) * start + 2 * (1 - t) * t * mid + t * t * end;
                rt.anchoredPosition = pos;

                // 잔상 생성
                if (trailTimer >= _trailInterval)
                {
                    trailTimer = 0f;
                    var trailGo = CreateTrailGo(pos, color);
                    trailPool.Add(trailGo);

                    // 최대 수 초과 시 가장 오래된 것 즉시 제거
                    if (trailPool.Count > _maxTrails)
                    {
                        var oldest = trailPool[0];
                        trailPool.RemoveAt(0);
                        if (oldest != null) Destroy(oldest);
                    }
                }

                yield return null;
            }

            // 도착 — 잔상 모두 페이드아웃
            foreach (var trail in trailPool)
            {
                if (trail != null) StartCoroutine(FadeOutAndDestroy(trail, _trailFadeDuration));
            }
            Destroy(projGo);

            // 도착 콜백 (CLAUDE.md #1: 콜백을 먼저 호출, 그 다음 파괴는 이미 됨)
            onArrive?.Invoke();
        }

        private GameObject CreateProjectileGo(Color color)
        {
            var go = new GameObject("Projectile");
            go.transform.SetParent(_layer, false);

            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(_projectileSize, _projectileSize);
            rt.localScale = Vector3.one;

            var img = go.AddComponent<Image>();
            img.sprite = _whiteSprite;
            img.color = color;
            img.raycastTarget = false;

            // 글로우 효과 (크고 밝은 자식)
            var glowGo = new GameObject("Glow");
            glowGo.transform.SetParent(go.transform, false);
            var glowRt = glowGo.AddComponent<RectTransform>();
            glowRt.sizeDelta = new Vector2(_projectileSize * 2.5f, _projectileSize * 2.5f);
            var glowImg = glowGo.AddComponent<Image>();
            glowImg.sprite = _whiteSprite;
            glowImg.color = new Color(color.r, color.g, color.b, 0.35f);
            glowImg.raycastTarget = false;

            return go;
        }

        private GameObject CreateTrailGo(Vector2 pos, Color color)
        {
            var go = new GameObject("Trail");
            go.transform.SetParent(_layer, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(_projectileSize * 0.7f, _projectileSize * 0.7f);

            var img = go.AddComponent<Image>();
            img.sprite = _whiteSprite;
            img.color = new Color(color.r, color.g, color.b, 0.7f);
            img.raycastTarget = false;

            StartCoroutine(FadeOutAndDestroy(go, _trailFadeDuration));
            return go;
        }

        private IEnumerator FadeOutAndDestroy(GameObject go, float duration)
        {
            if (go == null) yield break;

            var img = go.GetComponent<Image>();
            if (img == null) { Destroy(go); yield break; }

            float elapsed = 0f;
            var startColor = img.color;

            while (elapsed < duration && go != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                img.color = new Color(startColor.r, startColor.g, startColor.b, startColor.a * (1 - t));
                yield return null;
            }

            if (go != null) Destroy(go);
        }
    }
}
