using UnityEngine;
using System;

// Enums for dialogue system
public enum AgeCategory
{
    Anak,
    Remaja,
    Dewasa,
    Lansia
}

public enum Sentiment
{
    Positif,
    Negatif,
    Netral
}

public enum Confidence
{
    Tinggi,
    Sedang,
    Rendah
}

public enum TruthValue
{
    Benar,
    Salah,
    SebagianBenar
}

// Individual dialogue entry
[System.Serializable]
public class DialogueEntry
{
    [Header("NPC Information")]
    public string npcName;
    public AgeCategory ageCategory;
    
    [Header("Dialogue Content")]
    public string topic;
    [TextArea(3, 5)]
    public string context;
    [TextArea(3, 8)]
    public string dialogue;
    
    [Header("Dialogue Properties")]
    public Sentiment sentiment;
    public Confidence confidence;
    public TruthValue truthValue;
    
    // Constructor
    public DialogueEntry()
    {
        npcName = "";
        ageCategory = AgeCategory.Dewasa;
        topic = "";
        context = "";
        dialogue = "";
        sentiment = Sentiment.Netral;
        confidence = Confidence.Sedang;
        truthValue = TruthValue.Benar;
    }
    
    // Helper method to get localized age category string
    public string GetAgeCategoryString()
    {
        switch (ageCategory)
        {
            case AgeCategory.Anak: return "Anak";
            case AgeCategory.Remaja: return "Remaja";
            case AgeCategory.Dewasa: return "Dewasa";
            case AgeCategory.Lansia: return "Lansia";
            default: return "Dewasa";
        }
    }
    
    // Helper method to get localized sentiment string
    public string GetSentimentString()
    {
        switch (sentiment)
        {
            case Sentiment.Positif: return "Positif";
            case Sentiment.Negatif: return "Negatif";
            case Sentiment.Netral: return "Netral";
            default: return "Netral";
        }
    }
    
    // Helper method to get localized confidence string
    public string GetConfidenceString()
    {
        switch (confidence)
        {
            case Confidence.Tinggi: return "Tinggi";
            case Confidence.Sedang: return "Sedang";
            case Confidence.Rendah: return "Rendah";
            default: return "Sedang";
        }
    }
    
    // Helper method to get localized truth value string
    public string GetTruthValueString()
    {
        switch (truthValue)
        {
            case TruthValue.Benar: return "Benar";
            case TruthValue.Salah: return "Salah";
            case TruthValue.SebagianBenar: return "Sebagian Benar";
            default: return "Benar";
        }
    }
}

// ScriptableObject to hold dialogue data
[CreateAssetMenu(fileName = "New DialogueData", menuName = "Dialogue System/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Dialogue Collection")]
    public DialogueEntry[] dialogues = new DialogueEntry[0];
    
    [Header("Metadata")]
    public string datasetName = "Default Dataset";
    public string description = "";
    public int version = 1;
    
    // Get dialogue count
    public int GetDialogueCount()
    {
        return dialogues != null ? dialogues.Length : 0;
    }
    
    // Get dialogues by NPC name
    public DialogueEntry[] GetDialoguesByNPC(string npcName)
    {
        if (dialogues == null) return new DialogueEntry[0];
        
        var result = new System.Collections.Generic.List<DialogueEntry>();
        foreach (var dialogue in dialogues)
        {
            if (dialogue.npcName.Equals(npcName, System.StringComparison.OrdinalIgnoreCase))
            {
                result.Add(dialogue);
            }
        }
        return result.ToArray();
    }
    
    // Get dialogues by age category
    public DialogueEntry[] GetDialoguesByAgeCategory(AgeCategory category)
    {
        if (dialogues == null) return new DialogueEntry[0];
        
        var result = new System.Collections.Generic.List<DialogueEntry>();
        foreach (var dialogue in dialogues)
        {
            if (dialogue.ageCategory == category)
            {
                result.Add(dialogue);
            }
        }
        return result.ToArray();
    }
    
    // Get dialogues by topic
    public DialogueEntry[] GetDialoguesByTopic(string topic)
    {
        if (dialogues == null) return new DialogueEntry[0];
        
        var result = new System.Collections.Generic.List<DialogueEntry>();
        foreach (var dialogue in dialogues)
        {
            if (dialogue.topic.IndexOf(topic, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result.Add(dialogue);
            }
        }
        return result.ToArray();
    }
    
    // Get dialogues by sentiment
    public DialogueEntry[] GetDialoguesBySentiment(Sentiment sentiment)
    {
        if (dialogues == null) return new DialogueEntry[0];
        
        var result = new System.Collections.Generic.List<DialogueEntry>();
        foreach (var dialogue in dialogues)
        {
            if (dialogue.sentiment == sentiment)
            {
                result.Add(dialogue);
            }
        }
        return result.ToArray();
    }
    
    // Get dialogues by truth value
    public DialogueEntry[] GetDialoguesByTruthValue(TruthValue truthValue)
    {
        if (dialogues == null) return new DialogueEntry[0];
        
        var result = new System.Collections.Generic.List<DialogueEntry>();
        foreach (var dialogue in dialogues)
        {
            if (dialogue.truthValue == truthValue)
            {
                result.Add(dialogue);
            }
        }
        return result.ToArray();
    }
    
    // Get random dialogue
    public DialogueEntry GetRandomDialogue()
    {
        if (dialogues == null || dialogues.Length == 0) return null;
        
        int randomIndex = UnityEngine.Random.Range(0, dialogues.Length);
        return dialogues[randomIndex];
    }
    
    // Get random dialogue by criteria
    public DialogueEntry GetRandomDialogue(AgeCategory? ageCategory = null, Sentiment? sentiment = null, TruthValue? truthValue = null)
    {
        if (dialogues == null || dialogues.Length == 0) return null;
        
        var filteredDialogues = new System.Collections.Generic.List<DialogueEntry>();
        
        foreach (var dialogue in dialogues)
        {
            bool matches = true;
            
            if (ageCategory.HasValue && dialogue.ageCategory != ageCategory.Value)
                matches = false;
            
            if (sentiment.HasValue && dialogue.sentiment != sentiment.Value)
                matches = false;
            
            if (truthValue.HasValue && dialogue.truthValue != truthValue.Value)
                matches = false;
            
            if (matches)
                filteredDialogues.Add(dialogue);
        }
        
        if (filteredDialogues.Count == 0) return null;
        
        int randomIndex = UnityEngine.Random.Range(0, filteredDialogues.Count);
        return filteredDialogues[randomIndex];
    }
}