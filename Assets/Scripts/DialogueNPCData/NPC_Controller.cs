using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPC_Controller : MonoBehaviour
{
    [Header("NPC Settings")]
    public string npcName;
    public DialogueData dialogueData;
    
    [Header("UI References")]
    public GameObject dialoguePanel;
    public Text npcNameText;
    public Text dialogueText;
    public Text topicText;
    public Text sentimentText;
    public Button nextButton;
    public Button closeButton;
    
    [Header("CGAN Integration")]
    public bool useGeneratedDialogue = false;
    public string cganModelPath; // Path to your .pth file
    
    private DialogueEntry[] npcDialogues;
    private int currentDialogueIndex = 0;
    private bool isInDialogue = false;
    
    void Start()
    {
        // Load NPC specific dialogues
        npcDialogues = dialogueData.GetDialoguesByNPC(npcName);
        
        // Setup UI
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
            
        if (nextButton != null)
            nextButton.onClick.AddListener(NextDialogue);
            
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseDialogue);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartDialogue();
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CloseDialogue();
        }
    }
    
    public void StartDialogue()
    {
        if (isInDialogue) return;
        
        isInDialogue = true;
        currentDialogueIndex = 0;
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
            
        DisplayCurrentDialogue();
    }
    
    public void DisplayCurrentDialogue()
    {
        if (npcDialogues == null || npcDialogues.Length == 0)
        {
            Debug.LogWarning($"No dialogues found for NPC: {npcName}");
            return;
        }
        
        DialogueEntry currentDialogue = npcDialogues[currentDialogueIndex];
        
        if (npcNameText != null)
            npcNameText.text = currentDialogue.npcName;
            
        if (dialogueText != null)
            dialogueText.text = currentDialogue.dialogue;
            
        if (topicText != null)
            topicText.text = $"Topik: {currentDialogue.topic}";
            
        if (sentimentText != null)
            sentimentText.text = $"Sentimen: {currentDialogue.sentiment}";
            
        // Update button states
        if (nextButton != null)
            nextButton.interactable = currentDialogueIndex < npcDialogues.Length - 1;
    }
    
    public void NextDialogue()
    {
        if (currentDialogueIndex < npcDialogues.Length - 1)
        {
            currentDialogueIndex++;
            DisplayCurrentDialogue();
        }
    }
    
    public void CloseDialogue()
    {
        isInDialogue = false;
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
    
    // Method to integrate with CGAN generated dialogue
    public void SetGeneratedDialogue(string generatedText)
    {
        if (useGeneratedDialogue && dialogueText != null)
        {
            dialogueText.text = generatedText;
        }
    }
    
    // Get random dialogue by topic (useful for CGAN training context)
    public DialogueEntry GetRandomDialogueByTopic(string topic)
    {
        DialogueEntry[] topicDialogues = dialogueData.GetDialoguesByTopic(topic);
        if (topicDialogues.Length > 0)
        {
            return topicDialogues[Random.Range(0, topicDialogues.Length)];
        }
        return null;
    }
}