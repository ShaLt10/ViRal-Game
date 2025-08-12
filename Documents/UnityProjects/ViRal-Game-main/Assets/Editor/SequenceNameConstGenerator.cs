using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class SequenceNameConstGenerator : EditorWindow
{
    string targetFolder = "Assets/ScriptableObject/DialogueSequence";
    string outputPath = "Assets/Scripts/Utility/DialoguesNames.cs";
    string className = "DialoguesNames";

    [MenuItem("Tools/Generate Sequence Name Constants")]
    public static void ShowWindow()
    {
        GetWindow<SequenceNameConstGenerator>("Sequence Const Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Sequence Name Const Generator", EditorStyles.boldLabel);
        targetFolder = EditorGUILayout.TextField("Source Folder", targetFolder);
        outputPath = EditorGUILayout.TextField("Output File", outputPath);
        className = EditorGUILayout.TextField("Class Name", className);

        if (GUILayout.Button("Generate"))
        {
            GenerateConstants();
        }
    }

    void GenerateConstants()
    {
        string[] guids = AssetDatabase.FindAssets("t:DialogueSequence", new[] { targetFolder });

        if (guids.Length == 0)
        {
            Debug.LogWarning("No MySO ScriptableObjects found in folder: " + targetFolder);
            return;
        }

        HashSet<string> usedKeys = new();
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("// Auto-generated file. Do not edit manually.");
        sb.AppendLine($"public static class {className}");
        sb.AppendLine("{");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DialogueSequence so = AssetDatabase.LoadAssetAtPath<DialogueSequence>(path);

            if (so != null && !string.IsNullOrEmpty(so.sequenceName))
            {
                string constName = MakeValidIdentifier(so.sequenceName);

                // Avoid duplicate keys
                if (usedKeys.Contains(constName))
                    continue;

                usedKeys.Add(constName);
                sb.AppendLine($"\tpublic const string {constName} = \"{so.sequenceName}\";");
            }
        }

        sb.AppendLine("}");

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        File.WriteAllText(outputPath, sb.ToString());
        AssetDatabase.Refresh();

        Debug.Log($"Generated constants from {guids.Length} assets to: {outputPath}");
    }

    string MakeValidIdentifier(string input)
    {
        var valid = new StringBuilder();
        if (!char.IsLetter(input[0]) && input[0] != '_')
            valid.Append('_');

        foreach (char c in input)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
                valid.Append(c);
            else
                valid.Append('_');
        }

        return valid.ToString();
    }
}
