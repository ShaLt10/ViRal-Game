using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class OpeningAnimator : MonoBehaviour
{
    #region Inspector Fields
    
    [Header("Root References")]
    [SerializeField] private Transform lampRoot;
    [SerializeField] private RectTransform flower;
    [SerializeField] private RectTransform leaves;
    [SerializeField] private Transform tablet;
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private Camera mainCamera;

    [Header("Play Settings")]
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private bool allowSkipByTouch = true;
    [SerializeField] private float globalSpeed = 1f;

    [Header("Lamp Blink")]
    [SerializeField] private Color lampTargetColor = new Color(0.95f, 0.78f, 0.78f, 1f);
    [SerializeField, Range(0f, 5f)] private float lampBlinkSpeed = 1.2f;
    [SerializeField, Range(0f, 1f)] private float lampBlinkAmount = 0.8f;

    [Header("Flower Fall")]
    [SerializeField] private Vector2 flowerFallDistance = new Vector2(-80f, -120f);
    [SerializeField] private float flowerFallDuration = 6f;
    [SerializeField] private AnimationCurve flowerFallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float flowerSwayAmount = 15f;
    [SerializeField] private float flowerSwaySpeed = 0.6f;
    [SerializeField] private float flowerRotateAmount = 6f;
    [SerializeField] private bool flowerLoop = true;

    [Header("Leaves Fall")]
    [SerializeField] private Vector2 leavesFallDistance = new Vector2(-50f, -100f);
    [SerializeField] private float leavesFallDuration = 7f;
    [SerializeField] private AnimationCurve leavesFallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float leavesSwayAmount = 10f;
    [SerializeField] private float leavesSwaySpeed = 0.5f;
    [SerializeField] private float leavesRotateAmount = 10f;
    [SerializeField] private bool leavesLoop = true;

    [Header("Focus to Tablet")]
    [SerializeField] private float focusDelay = 1.5f;
    [SerializeField] private float focusDuration = 1.2f;
    [SerializeField, Range(0f, 0.95f)] private float cameraZoomAmount = 0.5f;
    [SerializeField] private AnimationCurve focusCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("UI Tablet Zoom Simulation")]
    [Tooltip("For Canvas Overlay mode only")]
    [SerializeField] private Vector2 uiFocusOffset = Vector2.zero;
    [SerializeField] private Vector3 uiFocusScale = new Vector3(1.2f, 1.2f, 1f);

    [Header("Tablet Highlight")]
    [SerializeField] private float highlightDelay = 2.6f;
    [SerializeField] private float highlightDuration = 0.9f;
    [SerializeField] private Vector3 highlightScale = new Vector3(1.15f, 1.15f, 1f);
    [SerializeField] private AnimationCurve highlightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Tablet Absorption Effect")]
    [SerializeField] private ParticleSystem absorptionVFX;
    [SerializeField] private Image tabletGlowOverlay;
    [SerializeField] private float absorptionStartDelay = 3.5f;
    [SerializeField] private float absorptionDuration = 1.5f;
    [SerializeField] private Color glowColor = new Color(0.3f, 0.8f, 1f, 1f);
    [SerializeField] private float glowIntensity = 2f;
    [SerializeField] private AnimationCurve absorptionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Screen Distortion")]
    [SerializeField] private bool useScreenDistortion = true;
    [SerializeField] private float distortionAmount = 0.05f;

    [Header("Fade Overlay")]
    [SerializeField] private float fadeDelay = 3.6f;
    [SerializeField] private float fadeDuration = 0.9f;
    [SerializeField] private Color fadeTargetColor = new Color(0f, 0f, 0f, 1f);
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Transition")]
    [SerializeField] private float totalOpeningDuration = 5.5f;

    [Header("Events")]
    public UnityEvent OnOpeningComplete;

    [Header("Debug")]
    [SerializeField] private bool enableLogs = false;

    #endregion

    #region Private Fields

    private readonly List<Image> _lampImages = new();
    private readonly List<SpriteRenderer> _lampSprites = new();
    private Color[] _lampImgStart, _lampSprStart;

    private Vector2 _flowerStartPos, _leavesStartPos;
    private Vector3 _tabletStartScale;
    private Vector2 _tabletStartAnchoredPos;
    private Color _fadeStartColor;
    private Color _tabletGlowStartColor;

    private Vector3 _camStartPos;
    private float _camStartSize;
    private Vector3 _camTargetPos;
    private float _camTargetSize;

    private float _t0;
    private float _flowerTimer, _leavesTimer;
    private bool _playing = false;
    private bool _absorptionPlayed = false;
    private bool _uiOverlayMode = false;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeReferences();
        CacheLamps();
        CacheStartStates();
        CalculateCameraTargets();
    }

    private void Start()
    {
        if (playOnStart) BeginOpening();
    }

    private void Update()
    {
        if (!_playing) return;

        float now = Time.time * globalSpeed;
        float dt = Time.deltaTime * globalSpeed;
        float elapsed = (Time.time - _t0) * globalSpeed;

        if (allowSkipByTouch && Input.GetMouseButtonDown(0))
        {
            SkipOpening();
            return;
        }

        AnimateLamps(now);
        AnimateFlower(dt);
        AnimateLeaves(dt);
        AnimateFocus(elapsed);
        AnimateHighlight(elapsed);
        AnimateAbsorption(elapsed);
        AnimateFade(elapsed);

        if (elapsed >= totalOpeningDuration)
        {
            FinishOpening();
        }
    }

    #endregion

    #region Public API

    public void BeginOpening()
    {
        ResetTimers();
        ResetStates();
        
        _playing = true;
        Log("Opening started");
    }

    public void SkipOpening()
    {
        if (!_playing) return;

        ApplyFinalStates();
        FinishOpening();
    }

    #endregion

    #region Animation Methods

    private void AnimateLamps(float time)
    {
        if (_lampImages.Count == 0 && _lampSprites.Count == 0) return;

        float blink = (Mathf.Sin(time * Mathf.PI * 2f * lampBlinkSpeed) + 1f) * 0.5f;
        blink *= lampBlinkAmount;

        for (int i = 0; i < _lampImages.Count; i++)
            if (_lampImages[i])
                _lampImages[i].color = Color.Lerp(_lampImgStart[i], lampTargetColor, blink);

        for (int i = 0; i < _lampSprites.Count; i++)
            if (_lampSprites[i])
                _lampSprites[i].color = Color.Lerp(_lampSprStart[i], lampTargetColor, blink);
    }

    private void AnimateFlower(float deltaTime)
    {
        if (!flower) return;

        _flowerTimer += deltaTime;
        float progress = Mathf.Clamp01(_flowerTimer / Mathf.Max(0.0001f, flowerFallDuration));
        float curve = flowerFallCurve.Evaluate(progress);

        Vector2 fall = flowerFallDistance * curve;
        float sway = Mathf.Sin(Time.time * Mathf.PI * 2f * flowerSwaySpeed) * flowerSwayAmount * (1f - progress * 0.3f);
        flower.anchoredPosition = _flowerStartPos + fall + new Vector2(sway, 0f);

        float rotation = Mathf.Sin(Time.time * 2f) * flowerRotateAmount * curve + sway * 0.1f;
        flower.localRotation = Quaternion.Euler(0, 0, rotation);

        if (progress >= 1f && flowerLoop) _flowerTimer = 0f;
    }

    private void AnimateLeaves(float deltaTime)
    {
        if (!leaves) return;

        _leavesTimer += deltaTime;
        float progress = Mathf.Clamp01(_leavesTimer / Mathf.Max(0.0001f, leavesFallDuration));
        float curve = leavesFallCurve.Evaluate(progress);

        Vector2 fall = leavesFallDistance * curve;
        float sway = Mathf.Sin(Time.time * Mathf.PI * 2f * leavesSwaySpeed) * leavesSwayAmount * (1f - progress * 0.25f);
        leaves.anchoredPosition = _leavesStartPos + fall + new Vector2(sway, 0f);

        float rotation = Mathf.Sin(Time.time * 1.7f) * leavesRotateAmount * curve + sway * 0.12f;
        leaves.localRotation = Quaternion.Euler(0, 0, rotation);

        if (progress >= 1f && leavesLoop) _leavesTimer = 0f;
    }

    private void AnimateFocus(float elapsed)
    {
        if (elapsed < focusDelay) return;

        float t = Mathf.Clamp01((elapsed - focusDelay) / Mathf.Max(0.0001f, focusDuration));
        float k = focusCurve.Evaluate(t);

        if (_uiOverlayMode)
        {
            if (!tablet) return;
            var rt = tablet as RectTransform;
            if (rt) rt.anchoredPosition = Vector2.Lerp(_tabletStartAnchoredPos, _tabletStartAnchoredPos + uiFocusOffset, k);
            tablet.localScale = Vector3.Lerp(_tabletStartScale, uiFocusScale, k);
        }
        else
        {
            if (!mainCamera || !tablet) return;

            mainCamera.transform.position = Vector3.Lerp(_camStartPos, _camTargetPos, k);

            float size = Mathf.Lerp(_camStartSize, _camTargetSize, k);
            if (mainCamera.orthographic)
                mainCamera.orthographicSize = size;
            else
                mainCamera.fieldOfView = size;
        }
    }

    private void AnimateHighlight(float elapsed)
    {
        if (!tablet || elapsed < highlightDelay) return;

        float t = Mathf.Clamp01((elapsed - highlightDelay) / Mathf.Max(0.0001f, highlightDuration));
        float k = highlightCurve.Evaluate(t);

        tablet.localScale = Vector3.Lerp(_tabletStartScale, highlightScale, k);
    }

    private void AnimateAbsorption(float elapsed)
    {
        if (elapsed < absorptionStartDelay) return;

        float t = Mathf.Clamp01((elapsed - absorptionStartDelay) / Mathf.Max(0.0001f, absorptionDuration));
        float k = absorptionCurve.Evaluate(t);

        if (!_absorptionPlayed && absorptionVFX)
        {
            absorptionVFX.Play();
            _absorptionPlayed = true;
            Log("Absorption VFX started");
        }

        if (tabletGlowOverlay)
        {
            Color targetColor = glowColor;
            targetColor.a = k * glowIntensity;
            tabletGlowOverlay.color = Color.Lerp(_tabletGlowStartColor, targetColor, k);
        }

        if (useScreenDistortion && tablet)
        {
            float pulse = Mathf.Sin(t * Mathf.PI * 8f) * distortionAmount * k;
            Vector3 scale = _tabletStartScale * (1f + pulse);
            tablet.localScale = scale;
        }
    }

    private void AnimateFade(float elapsed)
    {
        if (!fadeOverlay || elapsed < fadeDelay) return;

        float t = Mathf.Clamp01((elapsed - fadeDelay) / Mathf.Max(0.0001f, fadeDuration));
        float k = fadeCurve.Evaluate(t);

        fadeOverlay.color = Color.Lerp(_fadeStartColor, fadeTargetColor, k);
    }

    #endregion

    #region Initialization

    private void InitializeReferences()
    {
        if (!mainCamera) mainCamera = Camera.main;

        _lampImages.Clear();
        _lampSprites.Clear();
        
        if (lampRoot)
        {
            _lampImages.AddRange(lampRoot.GetComponentsInChildren<Image>(true));
            _lampSprites.AddRange(lampRoot.GetComponentsInChildren<SpriteRenderer>(true));
        }
    }

    private void CacheLamps()
    {
        _lampImgStart = new Color[_lampImages.Count];
        for (int i = 0; i < _lampImages.Count; i++)
            _lampImgStart[i] = _lampImages[i].color;

        _lampSprStart = new Color[_lampSprites.Count];
        for (int i = 0; i < _lampSprites.Count; i++)
            _lampSprStart[i] = _lampSprites[i].color;

        Log($"Lamp caches: img={_lampImages.Count}, spr={_lampSprites.Count}");
    }

    private void CacheStartStates()
    {
        if (flower) _flowerStartPos = flower.anchoredPosition;
        if (leaves) _leavesStartPos = leaves.anchoredPosition;

        if (tablet)
        {
            _tabletStartScale = tablet.localScale;
            var rt = tablet as RectTransform;
            if (rt) _tabletStartAnchoredPos = rt.anchoredPosition;

            _uiOverlayMode = IsRectOnOverlayCanvas(rt);
            Log($"UI Overlay Mode = {_uiOverlayMode}");
        }

        if (fadeOverlay)
        {
            _fadeStartColor = fadeOverlay.color;
            var c = _fadeStartColor;
            c.a = 0f;
            fadeOverlay.color = c;
        }

        if (tabletGlowOverlay)
        {
            _tabletGlowStartColor = tabletGlowOverlay.color;
            var c = _tabletGlowStartColor;
            c.a = 0f;
            tabletGlowOverlay.color = c;
        }

        if (absorptionVFX)
        {
            absorptionVFX.Stop();
        }
    }

    private void CalculateCameraTargets()
    {
        if (!mainCamera) return;

        _camStartPos = mainCamera.transform.position;
        _camStartSize = mainCamera.orthographic ? mainCamera.orthographicSize : mainCamera.fieldOfView;

        if (!_uiOverlayMode && tablet)
        {
            _camTargetPos = tablet.position;
            _camTargetPos.z = _camStartPos.z;
            _camTargetSize = _camStartSize * (1f - cameraZoomAmount);
        }
        else
        {
            _camTargetPos = _camStartPos;
            _camTargetSize = _camStartSize;
        }
    }

    #endregion

    #region State Management

    private void ResetTimers()
    {
        _flowerTimer = _leavesTimer = 0f;
        _t0 = Time.time;
        _absorptionPlayed = false;
    }

    private void ResetStates()
    {
        if (flower) flower.anchoredPosition = _flowerStartPos;
        if (leaves) leaves.anchoredPosition = _leavesStartPos;

        if (tablet)
        {
            tablet.localScale = _tabletStartScale;
            if (_uiOverlayMode)
            {
                var rt = tablet as RectTransform;
                if (rt) rt.anchoredPosition = _tabletStartAnchoredPos;
            }
        }

        if (fadeOverlay)
        {
            var c = _fadeStartColor;
            c.a = 0f;
            fadeOverlay.color = c;
        }

        if (tabletGlowOverlay)
        {
            var c = _tabletGlowStartColor;
            c.a = 0f;
            tabletGlowOverlay.color = c;
        }

        if (absorptionVFX)
        {
            absorptionVFX.Stop();
            absorptionVFX.Clear();
        }

        if (mainCamera)
        {
            mainCamera.transform.position = _camStartPos;
            if (mainCamera.orthographic)
                mainCamera.orthographicSize = _camStartSize;
            else
                mainCamera.fieldOfView = _camStartSize;
        }
    }

    private void ApplyFinalStates()
    {
        if (fadeOverlay) fadeOverlay.color = fadeTargetColor;

        if (_uiOverlayMode && tablet)
        {
            var rt = tablet as RectTransform;
            if (rt) rt.anchoredPosition = _tabletStartAnchoredPos + uiFocusOffset;
            tablet.localScale = highlightScale;
        }
        else
        {
            if (mainCamera)
            {
                mainCamera.transform.position = _camTargetPos;
                if (mainCamera.orthographic)
                    mainCamera.orthographicSize = _camTargetSize;
                else
                    mainCamera.fieldOfView = _camTargetSize;
            }
            if (tablet) tablet.localScale = highlightScale;
        }

        if (tabletGlowOverlay)
        {
            Color finalGlow = glowColor;
            finalGlow.a = glowIntensity;
            tabletGlowOverlay.color = finalGlow;
        }
    }

    private void FinishOpening()
    {
        if (!_playing) return;
        _playing = false;

        Log("Opening complete");
        OnOpeningComplete?.Invoke();
    }

    #endregion

    #region Utilities

    private bool IsRectOnOverlayCanvas(RectTransform rt)
    {
        if (!rt) return false;
        var cv = rt.GetComponentInParent<Canvas>();
        return cv && cv.renderMode == RenderMode.ScreenSpaceOverlay;
    }

    private void Log(string message)
    {
        if (enableLogs) Debug.Log($"[OpeningAnimator] {message}");
    }

    #endregion
}