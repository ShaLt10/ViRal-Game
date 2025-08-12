using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem
{
    // Main data structure untuk kirim dialogue ke DialogueBox
    [System.Serializable]
    public class DialogueSendData
    {
        public DialogueEntry[] dialogues;
        
        public DialogueSendData(DialogueEntry[] dialogues)
        {
            this.dialogues = dialogues;
        }
    }

    // Individual dialogue entry
    [System.Serializable]
    public class DialogueEntry
    {
        [Header("Speaker Info")]
        public string speaker;
        
        [Header("Dialogue Content")]
        [TextArea(3, 5)]
        public string dialogue;
        
        [Header("Character Portrait")]
        public CharacterPortraitData imagePotrait;
        
        [Header("Callback (Optional)")]
        [HideInInspector]
        public System.Action Doafter;
    }

    // Portrait data dengan multiple emotions
    [CreateAssetMenu(fileName = "CharacterPortrait", menuName = "DialogueSystem/Portrait")]
    public class CharacterPortraitData : ScriptableObject 
    {
        [Header("Portrait Settings")]
        public Sprite potraitImage;
        
        [Header("Character Info")]
        public string characterName;
        public string emotionState = "Innocent"; // Innocent, WideSmile, Sad, Angry, etc.
        
        [Header("Optional Settings")]
        public Color nameColor = Color.white;
    }

    // Dialogue sequence container
    [System.Serializable]
    public class DialogueSequence
    {
        [Header("Sequence Settings")]
        public string sequenceName;
        
        [Header("Dialogues")]
        public List<DialogueEntry> dialogues = new List<DialogueEntry>();
        
        public DialogueEntry[] GetDialogueArray()
        {
            return dialogues.ToArray();
        }
    }

    // Request data untuk dialogue
    [System.Serializable]
    public class OnDialogueRequestData
    {
        public string sequenceName;
        public System.Action action;
    }

    // Character emotions helper
    [System.Serializable]
    public class CharacterEmotions
    {
        [Header("Character Name")]
        public string characterName;
        
        [Header("Emotion Portraits")]
        public CharacterPortraitData Innocent;
        public CharacterPortraitData WideSmile;
        public CharacterPortraitData sad;
        public CharacterPortraitData angry;
        public CharacterPortraitData surprised;
        
        public CharacterPortraitData GetEmotion(EmotionType emotion)
        {
            switch (emotion)
            {
                case EmotionType.Innocent: return Innocent;
                case EmotionType.WideSmile: return WideSmile;
                case EmotionType.Sad: return sad;
                case EmotionType.Angry: return angry;
                case EmotionType.Surprised: return surprised;
                default: return Innocent;
            }
        }
    }

    public enum EmotionType
    {
        Innocent,
        WideSmile,
        Sad,
        Angry,
        Surprised
    }
}