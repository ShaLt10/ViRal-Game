#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class DialogueCSVImporter : EditorWindow
{
    private string csvFilePath = "";
    private DialogueData targetDialogueData;
    
    [MenuItem("Tools/Dialogue CSV Importer")]
    public static void ShowWindow()
    {
        DialogueCSVImporter window = GetWindow<DialogueCSVImporter>();
        window.titleContent = new GUIContent("Dialogue CSV Importer");
        window.Show();
    }
    
    void OnGUI()
    {
        GUILayout.Label("Dialogue CSV Importer", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        // CSV File Selection
        EditorGUILayout.BeginHorizontal();
        csvFilePath = EditorGUILayout.TextField("CSV File Path:", csvFilePath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            csvFilePath = EditorUtility.OpenFilePanel("Select CSV File", "", "csv");
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Target DialogueData
        targetDialogueData = (DialogueData)EditorGUILayout.ObjectField("Target Dialogue Data:", targetDialogueData, typeof(DialogueData), false);
        
        EditorGUILayout.Space();
        
        // Import Button
        EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(csvFilePath) || targetDialogueData == null);
        if (GUILayout.Button("Import CSV Data", GUILayout.Height(30)))
        {
            ImportCSVData();
        }
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Space();
        
        // Instructions
        EditorGUILayout.HelpBox("CSV Format:\nNama NPC, Kategori Usia, Topik, Konteks, Dialog, Sentimen, Keyakinan, Kebenaran", MessageType.Info);
        
        EditorGUILayout.Space();
        
        // Quick create DialogueData button
        if (GUILayout.Button("Create New DialogueData Asset"))
        {
            CreateDialogueDataAsset();
        }
        
        EditorGUILayout.Space();
        
        // Test import button for debugging
        if (GUILayout.Button("Test Parse Single Line"))
        {
            TestParseLine();
        }
    }
    
    void TestParseLine()
    {
        string testLine = "\"John Doe\",\"Dewasa\",\"Greeting\",\"Meeting for first time\",\"Hello, how are you?\",\"Positif\",\"Tinggi\",\"Benar\"";
        DialogueEntry entry = ParseCSVLine(testLine);
        if (entry != null)
        {
            Debug.Log($"Test successful: {entry.npcName} - {entry.dialogue}");
        }
        else
        {
            Debug.LogError("Test failed to parse line");
        }
    }
    
    void CreateDialogueDataAsset()
    {
        DialogueData asset = CreateInstance<DialogueData>();
        
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path))
        {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(path), "");
        }
        
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New DialogueData.asset");
        
        AssetDatabase.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        
        // Auto-assign to target
        targetDialogueData = asset;
    }
    
    void ImportCSVData()
    {
        if (!File.Exists(csvFilePath))
        {
            EditorUtility.DisplayDialog("Error", "CSV file not found!", "OK");
            return;
        }
        
        try
        {
            string[] lines = File.ReadAllLines(csvFilePath);
            List<DialogueEntry> dialogueEntries = new List<DialogueEntry>();
            
            // Skip header row if exists
            int startIndex = 0;
            if (lines.Length > 0)
            {
                string firstLine = lines[0].ToLower();
                if (firstLine.Contains("nama") || firstLine.Contains("npc") || firstLine.Contains("kategori"))
                {
                    startIndex = 1;
                }
            }
            
            for (int i = startIndex; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;
                
                try
                {
                    DialogueEntry entry = ParseCSVLine(line);
                    if (entry != null)
                    {
                        dialogueEntries.Add(entry);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error parsing line {i + 1}: {line}\nError: {e.Message}");
                }
            }
            
            if (dialogueEntries.Count > 0)
            {
                // Create undo point
                Undo.RecordObject(targetDialogueData, "Import CSV Data");
                
                // Update the DialogueData asset
                targetDialogueData.dialogues = dialogueEntries.ToArray();
                
                // Mark as dirty and save
                EditorUtility.SetDirty(targetDialogueData);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                EditorUtility.DisplayDialog("Success", $"Imported {dialogueEntries.Count} dialogue entries!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Warning", "No valid dialogue entries found in CSV file!", "OK");
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to import CSV: {e.Message}", "OK");
            Debug.LogError($"CSV Import Error: {e.Message}");
        }
    }
    
    DialogueEntry ParseCSVLine(string line)
    {
        if (string.IsNullOrEmpty(line)) return null;
        
        string[] values = SplitCSVLine(line);
        
        if (values.Length < 8)
        {
            Debug.LogWarning($"Invalid CSV line format (expected 8 columns, got {values.Length}): {line}");
            return null;
        }
        
        try
        {
            DialogueEntry entry = new DialogueEntry();
            
            // Parse each field with error handling
            entry.npcName = CleanValue(values[0]);
            entry.ageCategory = ParseAgeCategory(values[1]);
            entry.topic = CleanValue(values[2]);
            entry.context = CleanValue(values[3]);
            entry.dialogue = CleanValue(values[4]);
            entry.sentiment = ParseSentiment(values[5]);
            entry.confidence = ParseConfidence(values[6]);
            entry.truthValue = ParseTruthValue(values[7]);
            
            return entry;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error parsing CSV line: {line}\nError: {e.Message}");
            return null;
        }
    }
    
    string CleanValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        
        value = value.Trim();
        
        // Remove surrounding quotes if present
        if (value.StartsWith("\"") && value.EndsWith("\""))
        {
            value = value.Substring(1, value.Length - 2);
        }
        
        // Replace escaped quotes
        value = value.Replace("\"\"", "\"");
        
        return value;
    }
    
    string[] SplitCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string currentValue = "";
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            
            if (c == '"')
            {
                // Handle escaped quotes
                if (i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentValue += '"';
                    i++; // Skip next quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentValue);
                currentValue = "";
            }
            else
            {
                currentValue += c;
            }
        }
        
        result.Add(currentValue);
        return result.ToArray();
    }
    
    AgeCategory ParseAgeCategory(string value)
    {
        value = CleanValue(value).ToLower();
        
        switch (value)
        {
            case "anak":
            case "anak-anak":
            case "child":
                return AgeCategory.Anak;
            case "remaja":
            case "teenager":
            case "teen":
                return AgeCategory.Remaja;
            case "dewasa":
            case "adult":
                return AgeCategory.Dewasa;
            case "lansia":
            case "lanjut usia":
            case "elderly":
                return AgeCategory.Lansia;
            default:
                Debug.LogWarning($"Unknown age category: '{value}', defaulting to Dewasa");
                return AgeCategory.Dewasa;
        }
    }
    
    Sentiment ParseSentiment(string value)
    {
        value = CleanValue(value).ToLower();
        
        switch (value)
        {
            case "positif":
            case "positive":
                return Sentiment.Positif;
            case "negatif":
            case "negative":
                return Sentiment.Negatif;
            case "netral":
            case "neutral":
                return Sentiment.Netral;
            default:
                Debug.LogWarning($"Unknown sentiment: '{value}', defaulting to Netral");
                return Sentiment.Netral;
        }
    }
    
    Confidence ParseConfidence(string value)
    {
        value = CleanValue(value).ToLower();
        
        switch (value)
        {
            case "tinggi":
            case "high":
                return Confidence.Tinggi;
            case "sedang":
            case "medium":
                return Confidence.Sedang;
            case "rendah":
            case "low":
                return Confidence.Rendah;
            default:
                Debug.LogWarning($"Unknown confidence: '{value}', defaulting to Sedang");
                return Confidence.Sedang;
        }
    }
    
    TruthValue ParseTruthValue(string value)
    {
        value = CleanValue(value).ToLower();
        
        switch (value)
        {
            case "benar":
            case "true":
                return TruthValue.Benar;
            case "salah":
            case "false":
                return TruthValue.Salah;
            case "sebagian benar":
            case "partially true":
            case "partial":
                return TruthValue.SebagianBenar;
            default:
                Debug.LogWarning($"Unknown truth value: '{value}', defaulting to Benar");
                return TruthValue.Benar;
        }
    }
}
#endif