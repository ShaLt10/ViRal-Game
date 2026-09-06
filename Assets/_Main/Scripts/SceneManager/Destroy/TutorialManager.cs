using System.Collections;
using System.Collections.Generic;
using Game.Utility;
using TMPro;
using UnityEngine;

public class TutorialManager : SingletonDestroy<TutorialManager>
{
    [SerializeField]
    GameObject S;

    [SerializeField]
    GameObject I;

    [SerializeField]
    GameObject F;

    [SerializeField]
    GameObject T;

    [SerializeField]
    TMP_Text title;

    [SerializeField]
    int count = 0;

    private void Start()
    {
        ScreenRotateControl.Instance.SetLandscape();
        NextTutorial();
    }

    public void NextTutorial()
    {
        DisableAllText();
        string selectedCharacter = GetSelectedCharacterName();
        string dialogKey;

        switch (count)
        {
            case 0:
                title.SetText("<color=\"red\">S</color>IFT");
                dialogKey = selectedCharacter == "Gavi" ? DialoguesNames.Tutorial_S_Gavi : DialoguesNames.Tutorial_S_Raline;
                S.SetActive(true);
                break;
            case 1:
                title.SetText("S<color=\"red\">I</color>FT");
                dialogKey = selectedCharacter == "Gavi" ? DialoguesNames.Tutorial_I_Gavi : DialoguesNames.Tutorial_I_Raline;
                I.SetActive(true);
                break;
            case 2:
                title.SetText("SI<color=\"red\">F</color>T");
                dialogKey = selectedCharacter == "Gavi" ? DialoguesNames.Tutorial_F_Gavi : DialoguesNames.Tutorial_F_Raline;
                F.SetActive(true);
                break;
            case 3:
                title.SetText("SIF<color=\"red\">T</color>");
                dialogKey = selectedCharacter == "Gavi" ? DialoguesNames.Tutorial_T_Gavi : DialoguesNames.Tutorial_T_Raline;
                T.SetActive(true);
                break;
            default:
                return;
        }

        if (DialogManager.Instance != null)
            DialogManager.Instance.PlaySequenceThen(dialogKey, ChangeDialogue);
        else
        {
            Debug.LogWarning("[Tutorial] DialogManager tidak tersedia. Melanjutkan tutorial tanpa dialog.");
            ChangeDialogue();
        }
    }

    private string GetSelectedCharacterName()
    {
        if (CharacterManager.Instance != null)
        {
            return CharacterManager.Instance.GetPlayerName();
        }
        else
        {
            Debug.LogWarning("CharacterManager.Instance is null! Using default Raline.");
            return "Raline";
        }
    }

    private void ChangeDialogue()
    {
        count++;
        if (count >= 4)
        {
            SceneFlow.LoadScene("Map1");
        }
        else
        {
            NextTutorial();
        }
    }

    private void DisableAllText()
    { 
        S.SetActive(false);
        I.SetActive(false);
        F.SetActive(false);
        T.SetActive(false);
    }
}
