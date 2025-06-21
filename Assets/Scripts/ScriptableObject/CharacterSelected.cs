using TMPro;
using UnityEngine;

/// <summary>
/// Menyimpan karakter yang disimpan.
/// </summary>
[CreateAssetMenu(fileName ="StringSO", menuName = "Game/VariableSO/StringSO")]
public class CharacterSelected : ScriptableObject
{
    /// <summary>
    /// Id dari SO ini
    /// </summary>
    public string id = "";
    /// <summary>
    /// value dari So ini
    /// </summary>
    [SerializeField]
    private string characterName = "";

    [SerializeField]
    private TMP_FontAsset characterFont;

    public void setCharacter(string characterName, TMP_FontAsset characterFont)
    { 
        this.characterName = characterName;
        this.characterFont = characterFont;
    }

    public string GetName()
    { 
        return characterName;
    }
}
