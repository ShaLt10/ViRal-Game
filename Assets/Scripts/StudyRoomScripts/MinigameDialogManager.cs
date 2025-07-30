using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace DigitalForensicsQuiz
{
    [System.Serializable]
    public class DialogData
    {
        public string characterName;
        public string dialogText;
        public Sprite characterSprite;
        public DialogPosition position = DialogPosition.Left;
        public float displayDuration = 3f;
        public bool waitForInput = true;
    }

    public enum DialogPosition
    {
        Left,
        Right
    }

    public class MinigameDialogManager : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI Components")]
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI dialogText;
        [SerializeField] private Button actionButton; // Continue/Start button
        [SerializeField] private TextMeshProUGUI actionButtonText;
        
        [Header("Character Display")]
        [SerializeField] private Image leftCharacterImage;
        [SerializeField] private Image rightCharacterImage;
        [SerializeField] private GameObject leftCharacterPanel;
        [SerializeField] private GameObject rightCharacterPanel;
        
        [Header("Dialog Box Click Area")]
        [SerializeField] private GameObject dialogClickArea; // Area yang bisa diklik untuk skip/continue
        
        [Header("Animation Settings")]
        [SerializeField] private float typewriterSpeed = 0.03f; // Lebih cepat
        [SerializeField] private float characterFadeSpeed = 0.3f;
        [SerializeField] private AnimationCurve characterScaleCurve = AnimationCurve.EaseInOut(0, 0.9f, 1, 1f);
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] characterVoices;
        [SerializeField] private AudioClip typewriterSound;
        
        [Header("Minigame Dialog Data")]
        [SerializeField] private List<DialogData> minigameIntroDialogs = new List<DialogData>();
        
        private Queue<DialogData> dialogQueue = new Queue<DialogData>();
        private Coroutine currentDialogCoroutine;
        private Action onDialogComplete;
        private bool isDialogActive = false;
        private bool isTypewriting = false;
        private int currentDialogIndex = 0;
        private string currentFullText = "";

        private void Awake()
        {
            // Setup action button
            if (actionButton != null)
            {
                actionButton.onClick.AddListener(HandleActionButton);
            }
            
            // Initialize dialog data with 2 characters
            if (minigameIntroDialogs.Count == 0)
            {
                CreateDefaultMinigameDialogs();
            }
            
            // Hide dialog panel initially
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(false);
            }
            
            // Setup click area for dialog box
            if (dialogClickArea != null)
            {
                // Add this script as click handler to the click area
                var clickHandler = dialogClickArea.GetComponent<DialogClickHandler>();
                if (clickHandler == null)
                {
                    clickHandler = dialogClickArea.AddComponent<DialogClickHandler>();
                }
                clickHandler.Initialize(this);
            }
        }

        public void StartMinigameDialog(Action onComplete = null)
        {
            onDialogComplete = onComplete;
            
            // Clear existing queue
            dialogQueue.Clear();
            
            // Add minigame dialogs to queue
            foreach (var dialog in minigameIntroDialogs)
            {
                dialogQueue.Enqueue(dialog);
            }
            
            // Start dialog sequence
            StartDialogSequence();
        }

        private void StartDialogSequence()
        {
            if (dialogQueue.Count == 0)
            {
                EndDialog();
                return;
            }
            
            isDialogActive = true;
            currentDialogIndex = 0;
            
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(true);
            }
            
            ShowNextDialog();
        }

        private void ShowNextDialog()
        {
            if (dialogQueue.Count == 0)
            {
                EndDialog();
                return;
            }
            
            DialogData currentDialog = dialogQueue.Dequeue();
            currentDialogIndex++;
            
            if (currentDialogCoroutine != null)
            {
                StopCoroutine(currentDialogCoroutine);
            }
            
            currentDialogCoroutine = StartCoroutine(DisplayDialog(currentDialog));
        }

        private IEnumerator DisplayDialog(DialogData dialog)
        {
            currentFullText = dialog.dialogText;
            
            // Set character name
            if (characterNameText != null)
            {
                characterNameText.text = dialog.characterName;
            }
            
            // Setup character display
            SetupCharacterDisplay(dialog);
            
            // Clear dialog text
            if (dialogText != null)
            {
                dialogText.text = "";
            }
            
            // Update action button
            UpdateActionButton();
            
            // Typewriter effect
            yield return StartCoroutine(TypewriterEffect(dialog.dialogText));
            
            // Update button after typewriting is done
            UpdateActionButton();
            
            // Wait for input if required
            if (dialog.waitForInput)
            {
                // Dialog will continue when user clicks or presses action button
                yield return new WaitUntil(() => !isDialogActive);
            }
            else
            {
                // Wait for specified duration
                yield return new WaitForSeconds(dialog.displayDuration);
                if (isDialogActive)
                {
                    ContinueToNext();
                }
            }
        }

        private IEnumerator TypewriterEffect(string text)
        {
            isTypewriting = true;
            
            for (int i = 0; i <= text.Length; i++)
            {
                if (!isDialogActive) break;
                
                dialogText.text = text.Substring(0, i);
                
                // Play typewriter sound occasionally
                if (typewriterSound != null && audioSource != null && i < text.Length && i % 3 == 0)
                {
                    audioSource.PlayOneShot(typewriterSound, 0.2f);
                }
                
                yield return new WaitForSeconds(typewriterSpeed);
            }
            
            isTypewriting = false;
            UpdateActionButton();
        }

        private void SetupCharacterDisplay(DialogData dialog)
        {
            // Reset both characters to inactive state
            SetCharacterActive(leftCharacterPanel, leftCharacterImage, false);
            SetCharacterActive(rightCharacterPanel, rightCharacterImage, false);
            
            // Show the appropriate character
            GameObject targetPanel = null;
            Image targetImage = null;
            
            switch (dialog.position)
            {
                case DialogPosition.Left:
                    targetPanel = leftCharacterPanel;
                    targetImage = leftCharacterImage;
                    break;
                case DialogPosition.Right:
                    targetPanel = rightCharacterPanel;
                    targetImage = rightCharacterImage;
                    break;
            }
            
            if (targetPanel != null && targetImage != null)
            {
                if (dialog.characterSprite != null)
                {
                    targetImage.sprite = dialog.characterSprite;
                }
                
                SetCharacterActive(targetPanel, targetImage, true);
                
                // Animate character appearance
                StartCoroutine(AnimateCharacterAppearance(targetPanel));
            }
        }

        private void SetCharacterActive(GameObject panel, Image image, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
            
            if (image != null)
            {
                Color color = image.color;
                color.a = active ? 1f : 0.3f; // Active character full opacity, inactive semi-transparent
                image.color = color;
            }
        }

        private IEnumerator AnimateCharacterAppearance(GameObject characterPanel)
        {
            RectTransform rectTransform = characterPanel.GetComponent<RectTransform>();
            CanvasGroup canvasGroup = characterPanel.GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
            {
                canvasGroup = characterPanel.AddComponent<CanvasGroup>();
            }
            
            // Start values
            Vector3 originalScale = rectTransform.localScale;
            canvasGroup.alpha = 0f;
            rectTransform.localScale = Vector3.one * 0.8f;
            
            float duration = characterFadeSpeed;
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                
                // Fade in
                canvasGroup.alpha = progress;
                
                // Scale up with curve
                float scaleProgress = characterScaleCurve.Evaluate(progress);
                rectTransform.localScale = Vector3.Lerp(Vector3.one * 0.8f, originalScale, scaleProgress);
                
                yield return null;
            }
            
            // Ensure final state
            canvasGroup.alpha = 1f;
            rectTransform.localScale = originalScale;
        }

        private void UpdateActionButton()
        {
            if (actionButton == null || actionButtonText == null) return;
            
            if (isTypewriting)
            {
                actionButtonText.text = "Skip";
                actionButton.gameObject.SetActive(true);
            }
            else if (dialogQueue.Count > 0)
            {
                actionButtonText.text = "Lanjutkan";
                actionButton.gameObject.SetActive(true);
            }
            else
            {
                actionButtonText.text = "Mulai";
                actionButton.gameObject.SetActive(true);
            }
        }

        public void HandleActionButton()
        {
            if (isTypewriting)
            {
                // Skip typewriter effect
                SkipTypewriter();
            }
            else
            {
                // Continue to next dialog or end
                ContinueToNext();
            }
        }

        public void OnDialogAreaClick()
        {
            // Same functionality as action button for tap anywhere
            HandleActionButton();
        }

        private void SkipTypewriter()
        {
            if (currentDialogCoroutine != null)
            {
                StopCoroutine(currentDialogCoroutine);
            }
            
            isTypewriting = false;
            
            if (dialogText != null)
            {
                dialogText.text = currentFullText;
            }
            
            UpdateActionButton();
        }

        private void ContinueToNext()
        {
            if (dialogQueue.Count > 0)
            {
                ShowNextDialog();
            }
            else
            {
                EndDialog();
            }
        }

        private void EndDialog()
        {
            isDialogActive = false;
            
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(false);
            }
            
            if (currentDialogCoroutine != null)
            {
                StopCoroutine(currentDialogCoroutine);
                currentDialogCoroutine = null;
            }
            
            // Invoke completion callback
            onDialogComplete?.Invoke();
        }

        private void CreateDefaultMinigameDialogs()
        {
            minigameIntroDialogs = new List<DialogData>
            {
                new DialogData
                {
                    characterName = "Gavi",
                    dialogText = "Baiklah tim, saatnya untuk investigasi digital tingkat lanjut! Kita akan trace bukti digital dan pahami cara kerja scam impersonation secara sistematis.",
                    position = DialogPosition.Left,
                    waitForInput = true
                }
            };
        }

        // IPointerClickHandler implementation for dialog box clicks
        public void OnPointerClick(PointerEventData eventData)
        {
            if (isDialogActive)
            {
                HandleActionButton();
            }
        }

        // Public methods for external control
        public void SkipDialog()
        {
            EndDialog();
        }

        public bool IsDialogActive()
        {
            return isDialogActive;
        }

        public void SetTypewriterSpeed(float speed)
        {
            typewriterSpeed = Mathf.Clamp(speed, 0.01f, 0.1f);
        }

        // Add custom dialog at runtime
        public void AddCustomDialog(DialogData dialog)
        {
            dialogQueue.Enqueue(dialog);
        }

        public void StartCustomDialogSequence(List<DialogData> dialogs, Action onComplete = null)
        {
            onDialogComplete = onComplete;
            
            dialogQueue.Clear();
            
            foreach (var dialog in dialogs)
            {
                dialogQueue.Enqueue(dialog);
            }
            
            StartDialogSequence();
        }

        private void Update()
        {
            // Handle keyboard input during dialog
            if (isDialogActive && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
            {
                HandleActionButton();
            }
        }

        private void OnDestroy()
        {
            if (currentDialogCoroutine != null)
            {
                StopCoroutine(currentDialogCoroutine);
            }
        }
    }

    // Helper component for dialog area clicks
    public class DialogClickHandler : MonoBehaviour, IPointerClickHandler
    {
        private MinigameDialogManager dialogManager;
        
        public void Initialize(MinigameDialogManager manager)
        {
            dialogManager = manager;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (dialogManager != null)
            {
                dialogManager.OnDialogAreaClick();
            }
        }
    }
}