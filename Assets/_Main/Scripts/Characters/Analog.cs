using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Analog : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public RectTransform background; // The outer circle
    public RectTransform handle;     // The inner knob
    public float handleLimit = 1f;   // Range of the knob [0-1]
    [SerializeField] private Vector2 safeAreaPadding = new Vector2(64f, 64f);
    [SerializeField, Range(0f, 0.9f)] private float deadZone = 0.1f;

    private Vector2 inputVector = Vector2.zero;
    private Canvas rootCanvas;
    private RectTransform interactionControl;
    private Rect lastSafeArea;
    private float lastCanvasScale;
    private int lastScreenWidth;

    public Vector2 Direction => inputVector;

    private void Awake()
    {
        rootCanvas = background != null ? background.GetComponentInParent<Canvas>() : null;
        interactionControl = background != null ? background.parent.Find("Click") as RectTransform : null;
        Graphic handleGraphic = handle != null ? handle.GetComponent<Graphic>() : null;
        if (background != null && handleGraphic != null)
        {
            Vector2 expansion = (background.rect.size - handle.rect.size) * 0.5f;
            handleGraphic.raycastPadding = new Vector4(-expansion.x, -expansion.y, -expansion.x, -expansion.y);
        }
        ApplySafeArea();
    }

    private void Update() => ApplySafeArea();

    internal void ApplySafeArea()
    {
        if (background == null)
            return;

        Rect safeArea = Screen.safeArea;
        float canvasScale = rootCanvas != null ? rootCanvas.scaleFactor : 1f;
        Vector2 joystickPosition = GetSafeAreaPosition(safeArea, Screen.width, canvasScale, safeAreaPadding, false);
        Vector2 interactionPosition = GetSafeAreaPosition(safeArea, Screen.width, canvasScale, safeAreaPadding, true);
        if (safeArea == lastSafeArea && Mathf.Approximately(canvasScale, lastCanvasScale)
            && Screen.width == lastScreenWidth && background.anchoredPosition == joystickPosition
            && (interactionControl == null || interactionControl.anchoredPosition == interactionPosition))
            return;

        lastSafeArea = safeArea;
        lastCanvasScale = canvasScale;
        lastScreenWidth = Screen.width;
        background.anchoredPosition = joystickPosition;
        if (interactionControl != null)
            interactionControl.anchoredPosition = interactionPosition;
    }

    private static Vector2 GetSafeAreaPosition(Rect safeArea, int screenWidth, float canvasScale,
        Vector2 padding, bool rightAligned)
    {
        float scale = Mathf.Max(canvasScale, 0.01f);
        float horizontalInset = rightAligned ? screenWidth - safeArea.xMax : safeArea.xMin;
        float x = horizontalInset / scale + padding.x;
        return new Vector2(rightAligned ? -x : x, safeArea.yMin / scale + padding.y);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out pos))
        {
            inputVector = GetInputVector(pos, background.rect, deadZone);
            handle.anchoredPosition = inputVector * GetHandleTravel(background.rect, handle.rect, handleLimit);
        }
    }

    private static float GetHandleTravel(Rect backgroundRect, Rect handleRect, float handleLimit)
    {
        float backgroundRadius = Mathf.Min(backgroundRect.width, backgroundRect.height) * 0.5f;
        float handleRadius = Mathf.Min(handleRect.width, handleRect.height) * 0.5f;
        return Mathf.Max(backgroundRadius - handleRadius, 0f) * handleLimit;
    }

    private static Vector2 GetInputVector(Vector2 localPoint, Rect rect, float deadZone)
    {
        float radius = Mathf.Max(Mathf.Min(rect.width, rect.height) * 0.5f, 0.01f);
        Vector2 input = Vector2.ClampMagnitude((localPoint - rect.center) / radius, 1f);
        float magnitude = input.magnitude;
        return magnitude <= deadZone
            ? Vector2.zero
            : input.normalized * Mathf.InverseLerp(deadZone, 1f, magnitude);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}
