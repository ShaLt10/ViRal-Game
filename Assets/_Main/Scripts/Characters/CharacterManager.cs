using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public enum SelectedMC { None = 0, Raline = 1, Gavi = 2 }

    [Header("Character Selection")]
    public SelectedMC selectedCharacter = SelectedMC.None;

    [System.Serializable]
    public class CharacterPortrait
    {
        public string portraitName; // "Raline_Happy", "Gavi_Default", ...
        public Sprite sprite;
    }

    [Header("Character Portraits")]
    public CharacterPortrait[] allPortraits;

    // PlayerPrefs keys (baru + legacy)
    private const string KEY_INT_NEW   = "SelectedCharacterInt";
    private const string KEY_STR_NEW   = "SelectedCharacterName";
    private const string KEY_STR_OLD   = "SelectedCharacter";   // legacy (beberapa skrip lama pakai ini)

    // Singleton
    public static CharacterManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);
            LoadSelectedCharacter();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // === Public API ===
    public void SelectCharacter(int index)
    {
        if (index < 0 || index > 1)
        {
            Debug.LogError($"[CM] Character index tidak valid: {index}");
            return;
        }

        SelectedMC previous = selectedCharacter;
        selectedCharacter = index == 0 ? SelectedMC.Raline : SelectedMC.Gavi;
        if (!SaveSelectedCharacter())
        {
            selectedCharacter = previous;
            Debug.LogError("[CM] Pemilihan karakter dibatalkan karena save gagal.");
            return;
        }
        Debug.Log($"[CM] Selected => {GetPlayerName()} (enum {(int)selectedCharacter})");
    }

    public string GetPlayerName()
    {
        return selectedCharacter switch
        {
            SelectedMC.Raline => "Raline",
            SelectedMC.Gavi   => "Gavi",
            _                 => "None"
        };
    }

    public string GetSupportingName()
    {
        return selectedCharacter == SelectedMC.Raline ? "Gavi" : "Raline";
    }

    public bool HasSelectedCharacter() => selectedCharacter != SelectedMC.None;

    // === Portrait helpers ===
    public Sprite GetPortrait(string characterName, string expression = "Default")
    {
        if (string.IsNullOrEmpty(characterName)) return null;
        string fullName = $"{characterName}_{expression}";

        // exact
        foreach (var p in allPortraits)
            if (p.portraitName == fullName) return p.sprite;

        // fallback default
        if (expression != "Default") return GetPortrait(characterName, "Default");

        // fallback prefix
        foreach (var p in allPortraits)
            if (p.portraitName.StartsWith(characterName + "_")) return p.sprite;

        Debug.LogWarning($"[CM] Portrait not found: {characterName}_{expression}");
        return null;
    }

    public Sprite GetPortrait(string full) // accepts "Name_Expression"
    {
        if (!string.IsNullOrEmpty(full) && full.Contains("_"))
        {
            foreach (var p in allPortraits)
                if (p.portraitName == full) return p.sprite;
        }
        return null;
    }

    public Sprite GetPortraitWithExpression(string characterName, string expression)
        => GetPortrait($"{characterName}_{expression}");

    public bool IsMainCharacter(string name) => name == "Raline" || name == "Gavi";

    public bool IsSystem(string name)
    {
        string s = (name ?? "").Trim().ToLowerInvariant();
        return string.IsNullOrEmpty(s) || s == "system" || s == "[system]";
    }

    // Keep legacy method for backward compatibility
    public bool IsNarrator(string name) => IsSystem(name);

    // === Persistence (INT new + STRING compat) ===
    bool SaveSelectedCharacter()
    {
        return GameSession.Instance != null
            && GameSession.Instance.SetSelectedCharacter((int)selectedCharacter);
    }

    void LoadSelectedCharacter()
    {
        if (GameSession.Instance != null && GameSession.Instance.Data.selectedCharacter != 0)
        {
            ApplySavedSelection(GameSession.Instance.Data.selectedCharacter);
            ClearLegacySelection();
            return;
        }

        // Migrasi sekali dari PlayerPrefs versi prototype lama.
        bool hasLegacySelection = PlayerPrefs.HasKey(KEY_INT_NEW)
            || PlayerPrefs.HasKey(KEY_STR_NEW)
            || PlayerPrefs.HasKey(KEY_STR_OLD);
        if (PlayerPrefs.HasKey(KEY_INT_NEW))
        {
            selectedCharacter = (SelectedMC)PlayerPrefs.GetInt(KEY_INT_NEW);
            if (selectedCharacter == 0) // None? coba fallback ke string
            {
                selectedCharacter = ReadFromStrings();
            }
        }
        else
        {
            selectedCharacter = ReadFromStrings();
        }

        if (hasLegacySelection
            && (selectedCharacter == SelectedMC.None || SaveSelectedCharacter()))
            ClearLegacySelection();

        Debug.Log($"[CM] Loaded => {GetPlayerName()} (enum {(int)selectedCharacter})");
    }

    public void ApplySavedSelection(int value)
    {
        selectedCharacter = value >= 1 && value <= 2
            ? (SelectedMC)value
            : SelectedMC.None;
    }

    private SelectedMC ReadFromStrings()
    {
        string name =
            PlayerPrefs.HasKey(KEY_STR_NEW) ? PlayerPrefs.GetString(KEY_STR_NEW) :
            PlayerPrefs.HasKey(KEY_STR_OLD) ? PlayerPrefs.GetString(KEY_STR_OLD) :
            "None";

        return name switch
        {
            "Raline" => SelectedMC.Raline,
            "Gavi"   => SelectedMC.Gavi,
            _        => SelectedMC.None
        };
    }

    public void ResetCharacter()
    {
        ResetCharacter(true);
    }

    public void ResetCharacter(bool updateSave)
    {
        selectedCharacter = SelectedMC.None;
        ClearLegacySelection();
        if (updateSave)
            GameSession.Instance?.SetSelectedCharacter(0);
        Debug.Log("[CM] ResetCharacter -> None (all keys cleared)");
    }

    private static void ClearLegacySelection()
    {
        if (!PlayerPrefs.HasKey(KEY_INT_NEW)
            && !PlayerPrefs.HasKey(KEY_STR_NEW)
            && !PlayerPrefs.HasKey(KEY_STR_OLD))
            return;

        PlayerPrefs.DeleteKey(KEY_INT_NEW);
        PlayerPrefs.DeleteKey(KEY_STR_NEW);
        PlayerPrefs.DeleteKey(KEY_STR_OLD);
        PlayerPrefs.Save();
    }

    // debug util
    [ContextMenu("Print Debug")]
    public void PrintDebug()
    {
        var i = PlayerPrefs.HasKey(KEY_INT_NEW) ? PlayerPrefs.GetInt(KEY_INT_NEW) : -999;
        var sN= PlayerPrefs.HasKey(KEY_STR_NEW) ? PlayerPrefs.GetString(KEY_STR_NEW) : "(null)";
        var sO= PlayerPrefs.HasKey(KEY_STR_OLD) ? PlayerPrefs.GetString(KEY_STR_OLD) : "(null)";
        Debug.Log($"[CM] enum={selectedCharacter} name={GetPlayerName()} | int={i} strNew={sN} strOld={sO}");
    }
}
