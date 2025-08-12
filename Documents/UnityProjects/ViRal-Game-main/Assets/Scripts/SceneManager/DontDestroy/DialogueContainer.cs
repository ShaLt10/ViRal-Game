using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueSystem; // Import namespace

public class DialogueContainer : PreDetermindSingleton<DialogueContainer>
{
    [Header("Dialogue Sequences")]
    [SerializeField]
    List<DialogueSequence> sequences = new();

    [Header("Character Emotions")]
    [SerializeField]
    List<CharacterEmotions> characterEmotions = new();

    [Header("Legacy Support")]
    [SerializeField]
    public CharacterSelected CharacterSelected;

    // Method dengan callback action
    private void Sequences(string id, Action action = null)
    { 
        var dialogueSequence = sequences.Find(x => x.sequenceName == id);
        Debug.Log($"Requesting dialogue: {id}");
        
        if (dialogueSequence == null) 
        {
            Debug.LogWarning($"Dialogue sequence '{id}' not found!");
            return;
        }
        
        Debug.Log("Sending dialogue data");
        
        // Convert DialogueSequence ke format DialogueSendData
        DialogueEntry[] dialogueEntries = dialogueSequence.GetDialogueArray();
        
        // Tambah callback ke dialogue terakhir jika ada action
        if (action != null && dialogueEntries.Length > 0)
        {
            // Clone array untuk avoid reference issues
            DialogueEntry[] entriesWithCallback = new DialogueEntry[dialogueEntries.Length];
            for (int i = 0; i < dialogueEntries.Length; i++)
            {
                entriesWithCallback[i] = new DialogueEntry
                {
                    speaker = dialogueEntries[i].speaker,
                    dialogue = dialogueEntries[i].dialogue,
                    imagePotrait = dialogueEntries[i].imagePotrait,
                    Doafter = (i == dialogueEntries.Length - 1) ? action : dialogueEntries[i].Doafter
                };
            }
            dialogueEntries = entriesWithCallback;
        }
        
        // Kirim ke DialogueBox
        DialogueSendData dialogueData = new DialogueSendData(dialogueEntries);
        EventManager.Publish<DialogueSendData>(dialogueData);
    }
    
    // Method tanpa callback
    public void Sequences(string id)
    {
        Sequences(id, null);
    }

    // Helper method untuk get portrait berdasarkan emotion
    public CharacterPortraitData GetCharacterPortrait(string characterName, EmotionType emotion = EmotionType.Innocent)
    {
        var characterEmotion = characterEmotions.Find(x => x.characterName == characterName);
        if (characterEmotion != null)
        {
            return characterEmotion.GetEmotion(emotion);
        }
        
        Debug.LogWarning($"Character emotions for '{characterName}' not found!");
        return null;
    }

    // Quick dialogue creation for runtime
    public void ShowQuickDialogue(string speaker, string text, string characterName, EmotionType emotion = EmotionType.Innocent, Action callback = null)
    {
        var portrait = GetCharacterPortrait(characterName, emotion);
        
        DialogueEntry[] quickDialogue = new DialogueEntry[]
        {
            new DialogueEntry
            {
                speaker = speaker,
                dialogue = text,
                imagePotrait = portrait,
                Doafter = callback
            }
        };
        
        DialogueSendData dialogueData = new DialogueSendData(quickDialogue);
        EventManager.Publish<DialogueSendData>(dialogueData);
    }

    public void Nothing()
    {
       // Debug.Log("nothing");
    }

    private void OnEnable()
    {
        EventManager.Subscribe<OnDialogueRequestData>(CallDialogue);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<OnDialogueRequestData>(CallDialogue);
    }

    private void CallDialogue(OnDialogueRequestData data)
    {
        Debug.Log("Received dialogue request");
        Sequences(data.sequenceName, data.action);
    }
}