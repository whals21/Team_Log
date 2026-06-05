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
        private Camera _vfxCamera;

        private const float PIXELS_PER_UNIT = 100f;
        private const int VFX_LAYER = 30;

        [SerializeField, Range(0.5f, 5f)]
        private float _scaleFactor = 1.5f;

        public void Initialize(RectTransform parentCanvas)
        {
            _palette = Resources.Load<VFXPalette>("VFXPalette");
            Debug.Log($"[VFXManager] Init — palette: {_palette != null}, entries: {_palette?.entries.Count ?? 0}");

            // 1) Main Camera 찾기 또는 생성
            var mainCam = Camera.main;
            if (mainCam == null)
            {
                var mainCamGO = new GameObject("Main Camera");
                mainCam = mainCamGO.AddComponent<Camera>();
                mainCam.orthographic = true;
                mainCam.orthographicSize = Screen.height * 0.5f / PIXELS_PER_UNIT;
                mainCam.cullingMask = 0; // UI는 Canvas가 직접 렌더
                mainCam.transform.position = new Vector3(
                    Screen.width * 0.5f / PIXELS_PER_UNIT,
                    Screen.height * 0.5f / PIXELS_PER_UNIT,
                    -10f);
            }

            // Main Camera에 URP 데이터 보장
            var mainCamData = mainCam.GetUniversalAdditionalCameraData();
            if (mainCamData == null)
                mainCamData = mainCam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            mainCamData.renderType = CameraRenderType.Base;

            // 2) VFX Overlay Camera
            var vfxCamGO = new GameObject("VFXCamera");
            vfxCamGO.transform.SetParent(transform);
            _vfxCamera = vfxCamGO.AddComponent<Camera>();
            _vfxCamera.orthographic = true;
            _vfxCamera.orthographicSize = Screen.height * 0.5f / PIXELS_PER_UNIT;
            _vfxCamera.cullingMask = 1 << VFX_LAYER;
            _vfxCamera.clearFlags = CameraClearFlags.SolidColor;
            _vfxCamera.backgroundColor = Color.clear;
            _vfxCamera.depth = mainCam.depth + 1;
            _vfxCamera.transform.position = new Vector3(
                Screen.width * 0.5f / PIXELS_PER_UNIT,
                Screen.height * 0.5f / PIXELS_PER_UNIT,
                -10f);

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

            Debug.Log("[VFXManager] Setup complete — URP Camera Stacking");
        }

        /// <summary>
        /// 화면 좌표 → VFX 카메라 월드 좌표
        /// </summary>
        private Vector3 ScreenToVFXWorld(Vector2 screenPos)
        {
            return new Vector3(
                screenPos.x / PIXELS_PER_UNIT,
                screenPos.y / PIXELS_PER_UNIT,
                0f);
        }

        public void PlayAtPanel(string effectName, Transform panelTransform)
        {
            var prefab = _palette?.GetPrefab(effectName);
            if (prefab == null || _vfxRoot == null) return;

            Vector2 screenPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            if (panelTransform is RectTransform panelRT)
                screenPos = RectTransformUtility.WorldToScreenPoint(null, panelRT.position);

            var instance = Instantiate(prefab, _vfxRoot);
            instance.transform.position = ScreenToVFXWorld(screenPos);
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

            Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            var instance = Instantiate(prefab, _vfxRoot);
            instance.transform.position = ScreenToVFXWorld(center);
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
        public void PlayVictoryEffect() => PlayAtCenter("Victory");
        public void PlayDefeatEffect() => PlayAtCenter("Defeat");
    }
}
