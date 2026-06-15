using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TeamLog.UI
{
    /// <summary>
    /// 전투 이펙트 — URP Camera Stacking 방식
    ///
    /// 구조:
    ///   VFXCamera (URP Overlay, VFX 레이어만 렌더)
    ///   → Main Camera에 Stacking → BattleUICanvas 위에 자연스럽게 표시
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        private VFXPalette _palette;
        private Transform _vfxRoot;
        private Camera _mainCamera;
        private Camera _vfxCamera;

        private const float PIXELS_PER_UNIT = 100f;
        private const int VFX_LAYER = 30;

        [SerializeField, Range(0.5f, 5f)]
        private float _scaleFactor = 1.5f;

        public void Initialize(RectTransform parentCanvas)
        {
            _palette = Resources.Load<VFXPalette>("VFXPalette");
#if UNITY_EDITOR
            Debug.Log($"[VFXManager] Init — palette: {_palette != null}, entries: {_palette?.entries.Count ?? 0}");
#endif

            // 1) Main Camera 확보 — parentCanvas의 worldCamera 우선, 없으면 Camera.main
            var parentCanvasComponent = parentCanvas != null ? parentCanvas.GetComponent<Canvas>() : null;
            _mainCamera = parentCanvasComponent != null ? parentCanvasComponent.worldCamera : null;
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                var mainCamGO = new GameObject("Main Camera");
                _mainCamera = mainCamGO.AddComponent<Camera>();
                _mainCamera.orthographic = true;
                _mainCamera.orthographicSize = Screen.height * 0.5f / PIXELS_PER_UNIT;
                _mainCamera.cullingMask = ~(1 << VFX_LAYER);
                _mainCamera.transform.position = new Vector3(0f, 0f, -10f);
            }

            // Main Camera에 URP 데이터 보장
            var mainCamData = _mainCamera.GetUniversalAdditionalCameraData();
            if (mainCamData == null)
                mainCamData = _mainCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            mainCamData.renderType = CameraRenderType.Base;

            // 2) VFX Overlay Camera — Main Camera의 Projection/Transform 상속
            var vfxCamGO = new GameObject("VFXCamera");
            vfxCamGO.transform.SetParent(transform);
            _vfxCamera = vfxCamGO.AddComponent<Camera>();
            _vfxCamera.orthographic = true;
            _vfxCamera.orthographicSize = _mainCamera.orthographicSize;
            _vfxCamera.cullingMask = 1 << VFX_LAYER;
            _vfxCamera.clearFlags = CameraClearFlags.SolidColor;
            _vfxCamera.backgroundColor = Color.clear;
            _vfxCamera.depth = _mainCamera.depth + 1;
            // Main Camera와 동일한 Transform — 같은 좌표 공간 공유
            _vfxCamera.transform.position = _mainCamera.transform.position;
            _vfxCamera.transform.rotation = _mainCamera.transform.rotation;

            // URP Overlay 설정
            var vfxCamData = _vfxCamera.GetUniversalAdditionalCameraData();
            if (vfxCamData == null)
                vfxCamData = _vfxCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            vfxCamData.renderType = CameraRenderType.Overlay;

            // Main Camera에 Stacking
            if (!mainCamData.cameraStack.Contains(_vfxCamera))
                mainCamData.cameraStack.Add(_vfxCamera);

            // 3) VFX Root — 파티클 스폰 위치
            var rootGO = new GameObject("VFXRoot");
            rootGO.transform.SetParent(transform);
            _vfxRoot = rootGO.transform;

#if UNITY_EDITOR
            Debug.Log($"[VFXManager] Setup complete — MainCam: {_mainCamera.name}, VFXCam stacked on top");
#endif
        }

        /// <summary>
        /// 패널 월드 좌표 → VFX 스폰 좌표.
        /// ScreenSpaceCamera 모드에서 panelRT.position은 월드 좌표.
        /// VFX는 z=0 평면(UI 평면과 동일)에 배치.
        /// </summary>
        private Vector3 PanelToVFXWorld(Transform panelTransform)
        {
            if (panelTransform is RectTransform panelRT)
            {
                Vector3 p = panelRT.position;
                return new Vector3(p.x, p.y, 0f);
            }

            // 폴백: 화면 중앙
            Vector3 center = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 10f);
            return _mainCamera != null ? _mainCamera.ScreenToWorldPoint(center) : Vector3.zero;
        }

        public void PlayAtPanel(string effectName, Transform panelTransform)
        {
            var prefab = _palette?.GetPrefab(effectName);
            if (prefab == null || _vfxRoot == null) return;

            var instance = Instantiate(prefab, _vfxRoot);
            instance.transform.position = PanelToVFXWorld(panelTransform);
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one * _scaleFactor;
            SetLayerRecursively(instance, VFX_LAYER);

            float duration = GetParticleDuration(instance);
            Destroy(instance, duration + 0.5f);
        }

        public void PlayAtCenter(string effectName)
        {
            var prefab = _palette?.GetPrefab(effectName);
            if (prefab == null || _vfxRoot == null) return;

            var instance = Instantiate(prefab, _vfxRoot);
            // Main Camera 정중앙 (z=0 평면)
            Vector3 center = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 10f);
            instance.transform.position = _mainCamera != null
                ? _mainCamera.ScreenToWorldPoint(center)
                : Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one * _scaleFactor;
            SetLayerRecursively(instance, VFX_LAYER);

            float duration = GetParticleDuration(instance);
            Destroy(instance, duration + 0.5f);
        }

        private void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform t in go.transform)
                SetLayerRecursively(t.gameObject, layer);
        }

        private float GetParticleDuration(GameObject go)
        {
            float maxDuration = 1f;
            foreach (var ps in go.GetComponentsInChildren<ParticleSystem>())
            {
                float d = ps.main.duration + ps.main.startLifetime.constantMax;
                if (d > maxDuration) maxDuration = d;
            }
            return maxDuration;
        }

        // 편의 메서드
        public void PlayHitEffect(Transform panel) => PlayAtPanel("Hit", panel);
        public void PlayHealEffect(Transform panel) => PlayAtPanel("Heal", panel);
        public void PlayShieldEffect(Transform panel) => PlayAtPanel("Shield", panel);
        public void PlayDeathEffect(Transform panel) => PlayAtPanel("Death", panel);
        public void PlayBuffEffect(Transform panel) => PlayAtPanel("Buff", panel);
        public void PlayDebuffEffect(Transform panel) => PlayAtPanel("Debuff", panel);
        public void PlayBurnEffect(Transform panel) => PlayAtPanel("Burn", panel);
        public void PlayPoisonEffect(Transform panel) => PlayAtPanel("Poison", panel);
        public void PlayFreezeEffect(Transform panel) => PlayAtPanel("Freeze", panel);
        public void PlayCriticalEffect(Transform panel) => PlayAtPanel("Critical", panel);
        public void PlayPurifyEffect(Transform panel) => PlayAtPanel("Purify", panel);
        public void PlaySlashEffect(Transform panel) => PlayAtPanel("Slash", panel);
        public void PlayStunEffect(Transform panel) => PlayAtPanel("Stun", panel);
        public void PlayVictoryEffect() => PlayAtCenter("Victory");
        public void PlayDefeatEffect() => PlayAtCenter("Defeat");
    }
}
