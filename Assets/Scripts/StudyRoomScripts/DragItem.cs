using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace DigitalForensicsQuiz
{
    public class DragItemComponent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Components")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI scenarioIdText;
        [SerializeField] private Image iconImage;
        
        [Header("Drag Settings")]
        [SerializeField] private float dragAlpha = 0.7f;
        [SerializeField] private Vector3 dragScale = new Vector3(1.1f, 1.1f, 1.1f);
        
        private DragDropScenario itemData;
        private MinigameManager gameManager;
        private Canvas canvas;
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private Vector3 originalPosition;
        private Vector3 originalScale;
        private Color originalColor;
        private DropZoneComponent currentDropZone;
        private bool isDragging = false;
        
        // Mobile optimization
        private bool isHighlighted = false;
        private float touchStartTime;
        private const float LONG_PRESS_DURATION = 0.3f;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            canvas = GetComponentInParent<Canvas>();
        }

        public void Initialize(DragDropScenario data, MinigameManager manager)
        {
            itemData = data;
            gameManager = manager;
            
            // Set up UI
            if (descriptionText != null)
            {
                descriptionText.text = data.GetFormattedDescription();
                descriptionText.color = data.textColor;
            }
            
            if (scenarioIdText != null && !string.IsNullOrEmpty(data.scenarioId))
            {
                scenarioIdText.text = data.scenarioId;
                scenarioIdText.gameObject.SetActive(true);
            }
            else if (scenarioIdText != null)
            {
                scenarioIdText.gameObject.SetActive(false);
            }
            
            if (backgroundImage != null)
            {
                backgroundImage.color = data.backgroundColor;
                originalColor = data.backgroundColor;
            }
            
            if (iconImage != null && data.icon != null)
            {
                iconImage.sprite = data.icon;
                iconImage.gameObject.SetActive(true);
            }
            else if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false);
            }
            
            // Store original values
            originalPosition = rectTransform.anchoredPosition;
            originalScale = rectTransform.localScale;
            
            // Mobile optimization
            if (data.allowTouchHighlight)
            {
                var minSize = MobileOptimizationUtils.GetResponsiveSize(80f, data.minimumTouchSize);
                rectTransform.sizeDelta = Vector2.Max(rectTransform.sizeDelta, minSize);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsInteractable()) return;
            
            isDragging = true;
            touchStartTime = Time.time;
            
            // Visual feedback
            canvasGroup.alpha = dragAlpha;
            rectTransform.localScale = dragScale;
            
            // Bring to front
            transform.SetAsLastSibling();
            
            // Remove from current drop zone if any
            if (currentDropZone != null)
            {
                currentDropZone.RemoveItem(this);
                currentDropZone = null;
                NotifyGameManager();
            }
            
            // Mobile haptic feedback
            MobileOptimizationUtils.TriggerHapticFeedback(MobileOptimizationUtils.HapticFeedbackType.Light);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsInteractable() || !isDragging) return;
            
            // Move with pointer
            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out localPointerPosition))
            {
                rectTransform.anchoredPosition = localPointerPosition;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            
            isDragging = false;
            
            // Reset visual state
            canvasGroup.alpha = 1f;
            rectTransform.localScale = originalScale;
            
            // Check for drop zone
            DropZoneComponent targetDropZone = GetDropZoneUnderPointer(eventData);
            
            if (targetDropZone != null && targetDropZone.CanAcceptItem(this))
            {
                // Successfully dropped
                PlaceInDropZone(targetDropZone);
                MobileOptimizationUtils.TriggerHapticFeedback(MobileOptimizationUtils.HapticFeedbackType.Medium);
            }
            else
            {
                // Return to original position
                ReturnToOriginalPosition();
                MobileOptimizationUtils.TriggerHapticFeedback(MobileOptimizationUtils.HapticFeedbackType.Light);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsInteractable() || isDragging) return;
            
            // Hover effect
            if (backgroundImage != null)
            {
                backgroundImage.color = Color.Lerp(originalColor, Color.white, 0.2f);
            }
            
            isHighlighted = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isDragging) return;
            
            // Remove hover effect
            if (backgroundImage != null)
            {
                backgroundImage.color = originalColor;
            }
            
            isHighlighted = false;
        }

        private DropZoneComponent GetDropZoneUnderPointer(PointerEventData eventData)
        {
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            
            foreach (var result in results)
            {
                DropZoneComponent dropZone = result.gameObject.GetComponent<DropZoneComponent>();
                if (dropZone != null)
                {
                    return dropZone;
                }
                
                // Check parent objects too
                dropZone = result.gameObject.GetComponentInParent<DropZoneComponent>();
                if (dropZone != null)
                {
                    return dropZone;
                }
            }
            
            return null;
        }

        private void PlaceInDropZone(DropZoneComponent dropZone)
        {
            currentDropZone = dropZone;
            dropZone.AcceptItem(this);
            
            // Position in drop zone
            rectTransform.SetParent(dropZone.transform, false);
            rectTransform.anchoredPosition = Vector2.zero;
            
            NotifyGameManager();
        }

        private void ReturnToOriginalPosition()
        {
            // Return to original parent and position
            rectTransform.SetParent(transform.parent, false);
            rectTransform.anchoredPosition = originalPosition;
            
            currentDropZone = null;
            NotifyGameManager();
        }

        private void NotifyGameManager()
        {
            if (gameManager != null)
            {
                string dropZoneId = currentDropZone != null ? currentDropZone.GetZoneId() : null;
                gameManager.OnDragDropChanged(itemData.id, dropZoneId);
            }
        }

        private bool IsInteractable()
        {
            return canvasGroup.interactable && gameObject.activeInHierarchy;
        }

        // Public getters
        public string GetItemId() => itemData.id;
        public DragDropScenario GetItemData() => itemData;
        public DropZoneComponent GetCurrentDropZone() => currentDropZone;
        public bool IsDragging() => isDragging;
        
        // Force return to original position (for external calls)
        public void ForceReturnToOriginal()
        {
            if (currentDropZone != null)
            {
                currentDropZone.RemoveItem(this);
                currentDropZone = null;
            }
            
            ReturnToOriginalPosition();
        }

        // Mobile-specific long press handling
        private void Update()
        {
            if (isHighlighted && !isDragging && Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Stationary && 
                    Time.time - touchStartTime > LONG_PRESS_DURATION)
                {
                    // Show detailed info on long press
                    ShowDetailedInfo();
                }
            }
        }

        private void ShowDetailedInfo()
        {
            if (!string.IsNullOrEmpty(itemData.detailedInfo))
            {
                // You can implement a tooltip system here
                Debug.Log($"Detailed Info: {itemData.detailedInfo}");
            }
        }
    }
}