using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueBox : MonoBehaviour, IPointerDownHandler
{
    [Header("UI Components")]
    [SerializeField] GameObject dialogBox;
    [SerializeField] Image portrait;
    [SerializeField] TMP_Text speakerName;
    [SerializeField] TMP_Text dialogueText;
    [SerializeField] Image dialogBG;
    
    [Header("Typewriter Settings")]
    [SerializeField] float typewriterSpeed = 0.05f;
    [SerializeField] AudioClip typewriterSound;
    [SerializeField] AudioSource audioSource;
    [SerializeField] bool playTypewriterSound = true;
    
    [Header("Continue Indicator")]
    [SerializeField] GameObject continueIndicator; // Arrow atau icon untuk menunjukkan bisa lanjut
    [SerializeField] float blinkSpeed = 1f;
    [SerializeField] float bounceHeight = 10f;
    [SerializeField] float bounceSpeed = 2f;
    [SerializeField] AnimationCurve bounceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [SerializeField] Dialogue[] dialogues = Array.Empty<Dialogue>();
    
    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private bool canContinue = false;
    private Coroutine typewriterCoroutine;
    private Coroutine blinkCoroutine;
    private string fullText = "";
    private Vector3 originalIndicatorPosition;
    private RectTransform indicatorRectTransform;

    private void OnEnable()
    {
        EventManager.Subscribe<DialogueSendData>(ShowDialogue);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<DialogueSendData>(ShowDialogue);
        
        // Stop all coroutines when disabled
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
    }

    private void Start()
    {
        DialogueContainer.Instance.Nothing();
        
        // Setup audio source if not assigned
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        // Setup continue indicator animation
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
            indicatorRectTransform = continueIndicator.GetComponent<RectTransform>();
            if (indicatorRectTransform != null)
                originalIndicatorPosition = indicatorRectTransform.anchoredPosition;
        }
    }

    public void ShowDialogue(DialogueSendData data)
    {
        Debug.Log("ShowDialogue");
        currentDialogueIndex = 0;
        dialogBox.SetActive(true);
        dialogBG.raycastTarget = true;
        
        // Copy dialogue data
        dialogues = new Dialogue[data.dialogues.Length];
        data.dialogues.CopyTo(dialogues, 0);
        
        UpdateDialogue();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isTyping)
        {
            // If currently typing, complete the text immediately
            CompleteCurrentText();
        }
        else if (canContinue)
        {
            // Move to next dialogue or close
            if (currentDialogueIndex + 1 < dialogues.Length)
            {
                currentDialogueIndex++;
                UpdateDialogue();
            }
            else
            {
                CloseDialogue();
            }
        }
    }

    private void UpdateDialogue()
    {
        var currentDialogue = dialogues[currentDialogueIndex];
        
        // Update portrait and speaker name
        portrait.sprite = currentDialogue.imagePotrait.potraitImage;
        speakerName.SetText(currentDialogue.speaker);
        
        // Start typewriter effect for dialogue text
        fullText = currentDialogue.dialogue;
        StartTypewriter();
        
        // Hide continue indicator while typing
        if (continueIndicator != null)
            continueIndicator.SetActive(false);
        
        // Stop previous blink coroutine
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
    }

    private void StartTypewriter()
    {
        isTyping = true;
        canContinue = false;
        
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);
        
        typewriterCoroutine = StartCoroutine(TypewriterEffect());
    }

    private IEnumerator TypewriterEffect()
    {
        dialogueText.text = "";
        
        for (int i = 0; i <= fullText.Length; i++)
        {
            dialogueText.text = fullText.Substring(0, i);
            
            // Play typewriter sound for visible characters (not spaces)
            if (playTypewriterSound && i < fullText.Length && !char.IsWhiteSpace(fullText[i]))
            {
                if (audioSource != null && typewriterSound != null)
                {
                    audioSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f); // Slight pitch variation
                    audioSource.PlayOneShot(typewriterSound, 0.3f);
                }
            }
            
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        OnTypewriterComplete();
    }

    private void CompleteCurrentText()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }
        
        dialogueText.text = fullText;
        OnTypewriterComplete();
    }

    private void OnTypewriterComplete()
    {
        isTyping = false;
        canContinue = true;
        
        // Show and start blinking continue indicator
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(true);
            blinkCoroutine = StartCoroutine(BlinkContinueIndicator());
        }
    }

    private IEnumerator BlinkContinueIndicator()
    {
        while (canContinue && continueIndicator != null)
        {
            continueIndicator.SetActive(true);
            
            // Animate bouncing effect
            if (indicatorRectTransform != null)
            {
                float elapsedTime = 0f;
                while (elapsedTime < blinkSpeed && canContinue)
                {
                    float bounce = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
                    bounce = bounceCurve.Evaluate((bounce + bounceHeight) / (bounceHeight * 2)) * bounceHeight - (bounceHeight * 0.5f);
                    
                    indicatorRectTransform.anchoredPosition = originalIndicatorPosition + Vector3.up * bounce;
                    
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(blinkSpeed);
            }
            
            continueIndicator.SetActive(false);
            yield return new WaitForSeconds(blinkSpeed);
        }
        
        // Reset position when done
        if (indicatorRectTransform != null)
            indicatorRectTransform.anchoredPosition = originalIndicatorPosition;
    }

    private void CloseDialogue()
    {
        dialogBG.raycastTarget = false;
        dialogBox.SetActive(false);
        
        // Stop all coroutines
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
        
        // Hide continue indicator and reset position
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
            if (indicatorRectTransform != null)
                indicatorRectTransform.anchoredPosition = originalIndicatorPosition;
        }
        
        Debug.Log("Dialogue Complete");
        
        // Execute callback if exists
        dialogues.Last().Doafter?.Invoke();
    }

    // Public method to set typewriter speed (useful for different characters)
    public void SetTypewriterSpeed(float speed)
    {
        typewriterSpeed = speed;
    }
    
    // Public method to skip current dialogue (for skip button)
    public void SkipCurrentDialogue()
    {
        if (isTyping)
        {
            CompleteCurrentText();
        }
        else
        {
            OnPointerDown(null);
        }
    }
    
    // Public method to skip all remaining dialogues
    public void SkipAllDialogues()
    {
        CloseDialogue();
    }
}