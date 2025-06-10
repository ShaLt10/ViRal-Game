using System.Collections;
using System.Collections.Generic;
using Game.Utility;
using MoreMountains.TopDownEngine;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour
{
    /// <summary>
    /// SO menyimpan characterselector.
    /// </summary>
    [SerializeField] private CharacterSelected characterSelector;
    /// <summary>
    /// Button memilih Gavi.
    /// </summary>
    [SerializeField] private Button selectGavi;
    [SerializeField] private TMP_FontAsset gaviFont;
    /// <summary>
    /// Button memilihRaline
    /// </summary>
    [SerializeField] private Button selectRaline;
    [SerializeField] private TMP_FontAsset ralineFont;

    private void Awake()
    {
        selectGavi.onClick.AddListener(()=>SelectCharacter(gaviFont,StringContainer.Gavi));
        selectRaline.onClick.AddListener(() => SelectCharacter(ralineFont, StringContainer.Raline));
    }

    private void OnDestroy()
    {
        selectGavi.onClick.RemoveAllListeners();
        selectRaline.onClick.RemoveAllListeners();
    }

    private void SelectCharacter(TMP_FontAsset characterFont, string name)
    {
        characterSelector.setCharacter(name, characterFont);
    }
}
