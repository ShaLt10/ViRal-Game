using System.Collections;
using System.Collections.Generic;
using Game.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        switch (count)
        { 
            case 0:
                title.SetText("<color=\"red\">S</color>IFT");
                if (DialogueContainer.Instance.CharacterSelected.GetName() == StringContainer.Gavi)
                {
                    EventManager.Publish(new OnDialogueRequestData(DialoguesNames.Tutorial_S_Gavi, ChangeDialogue));
                }
                else
                {
                    EventManager.Publish(new OnDialogueRequestData(DialoguesNames.Tutorial_S_Raline, ChangeDialogue));
                }
                S.SetActive(true);
                break;
            case 1:
                title.SetText("S<color=\"red\">I</color>FT");
                if (DialogueContainer.Instance.CharacterSelected.GetName() == StringContainer.Gavi)
                {
                    EventManager.Publish(new OnDialogueRequestData(DialoguesNames.Tutorial_I_Gavi, ChangeDialogue));
                }
                else
                {
                    EventManager.Publish(new OnDialogueRequestData(DialoguesNames.Tutorial_I_Raline, ChangeDialogue));
                }
                I.SetActive(true);
                break;
            case 2:
                title.SetText("SI<color=\"red\">F</color>T");
                if (DialogueContainer.Instance.CharacterSelected.GetName() == StringContainer.Gavi)
                {
                    EventManager.Publish(new OnDialogueRequestData(DialoguesNames.Tutorial_F_Gavi, ChangeDialogue));
                }
                else
                {
                    EventManager.Publish(new OnDialogueRequestData(DialoguesNames.Tutorial_F_Raline, ChangeDialogue));
                }
                F.SetActive(true);
                break;
            case 3:
                title.SetText("SIF<color=\"red\">T</color>");
                if (DialogueContainer.Instance.CharacterSelected.GetName() == StringContainer.Gavi)
                {
                    EventManager.Publish(new OnDialogueRequestData(DialoguesNames.Tutorial_T_Gavi, ChangeDialogue));
                }
                else
                {
                    EventManager.Publish(new OnDialogueRequestData(DialoguesNames.Tutorial_T_Raline, ChangeDialogue));
                }
                T.SetActive(true);
                break;
        }
    }

    private void ChangeDialogue()
    {
        count ++;
        NextTutorial();
        if (count >= 4)
        {
            SceneManager.LoadScene(3);
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
