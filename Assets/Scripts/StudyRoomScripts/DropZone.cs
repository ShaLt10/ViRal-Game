using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

namespace DigitalForensicsQuiz
{
    public class DropZoneComponent : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Components")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI categoryNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Transform itemContainer;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject highlightEffect;
        [SerializeField] private ParticleSystem dropParticles;
        
        private DropZone zoneData;
        private DragItemComponent currentItem;
        private Color originalBackgroundColor;
        private bool isHighlighted = false;
        private bool isDragOver = false;
        
        // Animation
        private Coroutine highlightCoroutine;
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            
            if (itemContainer == null)
            {
                // Create item container if not assigned
                GameObject container = new GameObject("ItemContainer");
                container.transform.SetParent(transform, false);
                itemContainer = container.transform;
                
                RectTransform containerRect = container.AddComponent<RectTransform>();
                containerRect.anchorMin = Vector2.zero;
                containerRect.anchorMax = Vector2.one;
                containerRect.sizeDelta = Vector2.zero;
                containerRect.anchoredPosition = Vector2.zero;
            }
        }

        public void Initialize(DropZone data)
        {
            zoneData = data;
            
            // Set up UI
            if (categoryNameText != null)
            {
                categoryNameText.text = data.categoryName;
                categoryNameText.color = data.textColor;
            }
            
            if (descriptionText != null)
            {
                descriptionText.text = data.description;
                descriptionText.color = data.textColor;
            }
            
            if (backgroundImage != null)
            {
                backgroundImage.color = data.zoneColor;
                originalBackgroundColor = data.zoneColor;
                
                if (data.backgroundImage != null)
                {
                    backgroundImage.sprite = data.backgroundImage;
                }
            }
            
            // Mobile optimization
            Vector2 minSize = MobileOptimizationUtils.GetResponsiveSize(100f, data.minimumSize);
            rectTransform.sizeDelta = Vector2.Max(rectTransform.sizeDelta, minSize);
            
            // Setup highlight effect
            if (highlightEffect != null)
            {
                highlightEffect.SetActive(false);
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            DragItemComponent dragItem = eventData.pointerDrag?.GetComponent<DragItemComponent>();
            
            if (dragItem != null && CanAcceptItem(dragItem))
            {
                AcceptItem(dragItem);
                
                // Visual feedback
                if (zoneData.enableDropAnimation)
                {
                    PlayDropAnimation();
                }
                
                // Audio feedback could be added here
                MobileOptimizationUtils.TriggerHapticFeedback(MobileOptimizationUtils.HapticFeedbackType.Medium);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!zoneData.enableHoverEffect) return;
            
            // Check if we're dragging a compatible item
            DragItemComponent dragItem = eventData.pointerDrag?.GetComponent<DragItemComponent>();
            if (dragItem != null && dragItem.IsDragging())
            {
                isDragOver = true;
                ShowHighlight(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isDragOver = false;
            ShowHighlight(false);
        }

        public bool CanAcceptItem(DragItemComponent item)
        {
            if (item == null) return false;
            
            // Check if zone already has an item (assuming one item per zone)
            if (currentItem != null) return false;
            
            // Additional validation logic can be added here
            return true;
        }

        public void AcceptItem(DragItemComponent item)
        {
            // Remove previous item if any
            if (currentItem != null)
            {
                RemoveItem(currentItem);
            }
            
            currentItem = item;
            
            // Position item in container
            item.transform.SetParent(itemContainer, false);
            
            RectTransform itemRect = item.GetComponent<RectTransform>();
            itemRect.anchoredPosition = Vector2.zero;
            itemRect.localScale = Vector3.one;
            
            // Visual feedback
            ShowItemAccepted();
        }

        public void RemoveItem(DragItemComponent item)
        {
            if (currentItem == item)
            {
                currentItem = null;
                ShowItemRemoved();
            }
        }

        private void ShowHighlight(bool show)
        {
            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
            }
            
            highlightCoroutine = StartCoroutine(AnimateHighlight(show));
        }

        private IEnumerator AnimateHighlight(bool show)
        {
            if (backgroundImage == null) yield break;
            
            Color startColor = backgroundImage.color;
            Color targetColor = show ? zoneData.highlightColor : originalBackgroundColor;
            
            float duration = 0.2f;
            float elapsedTime = 0f;
            
            // Show/hide highlight effect
            if (highlightEffect != null)
            {
                highlightEffect.SetActive(show);
            }
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                
                backgroundImage.color = Color.Lerp(startColor, targetColor, progress);
                yield return null;
            }
            
            backgroundImage.color = targetColor;
            isHighlighted = show;
        }

        private void ShowItemAccepted()
        {
            // Scale animation
            if (currentItem != null)
            {
                StartCoroutine(AnimateItemAccepted());
            }
        }

        private void ShowItemRemoved()
        {
            // Any cleanup animation can go here
        }

        private IEnumerator AnimateItemAccepted()
        {
            if (currentItem == null) yield break;
            
            RectTransform itemRect = currentItem.GetComponent<RectTransform>();
            Vector3 originalScale = itemRect.localScale;
            Vector3 targetScale = originalScale * 0.9f; // Slightly smaller in drop zone
            
            float duration = 0.3f;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                
                // Smooth scaling with bounce
                float bounceProgress = Mathf.Sin(progress * Mathf.PI);
                itemRect.localScale = Vector3.Lerp(originalScale, targetScale, bounceProgress);
                
                yield return null;
            }
            
            itemRect.localScale = targetScale;
        }

        private void PlayDropAnimation()
        {
            // Particle effect
            if (dropParticles != null)
            {
                dropParticles.Play();
            }
            
            // Screen shake effect could be added here for mobile
            StartCoroutine(DropPulseEffect());
        }

        private IEnumerator DropPulseEffect()
        {
            Vector3 originalScale = rectTransform.localScale;
            Vector3 pulseScale = originalScale * 1.05f;
            
            float duration = 0.15f;
            float elapsedTime = 0f;
            
            // Pulse out
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                rectTransform.localScale = Vector3.Lerp(originalScale, pulseScale, progress);
                yield return null;
            }
            
            // Pulse back
            elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                rectTransform.localScale = Vector3.Lerp(pulseScale, originalScale, progress);
                yield return null;
            }
            
            rectTransform.localScale = originalScale;
        }

        // Public getters
        public string GetZoneId() => zoneData.id;
        public DropZone GetZoneData() => zoneData;
        public DragItemComponent GetCurrentItem() => currentItem;
        public bool HasItem() => currentItem != null;
        public bool IsHighlighted() => isHighlighted;
        public bool IsDragOver() => isDragOver;

        // Utility methods
        public void ClearItem()
        {
            if (currentItem != null)
            {
                currentItem.ForceReturnToOriginal();
                currentItem = null;
            }
        }

        public void SetInteractable(bool interactable)
        {
            // Can be used to disable drop zone during certain states
            enabled = interactable;
            
            if (backgroundImage != null)
            {
                Color color = backgroundImage.color;
                color.a = interactable ? 1f : 0.5f;
                backgroundImage.color = color;
            }
        }

        // For debugging
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            
            // Auto-find components in editor
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            
            if (categoryNameText == null)
                categoryNameText = GetComponentInChildren<TextMeshProUGUI>();
            
            if (itemContainer == null)
            {
                Transform container = transform.Find("ItemContainer");
                if (container != null)
                    itemContainer = container;
            }
        }
    }
}