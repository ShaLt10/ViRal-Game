using System.Collections;
using System.Collections.Generic;
using Game.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Map5 : SingletonDestroy<Map5>
{
    private void Start()
    {
        ScreenRotateControl.Instance.SetLandscape();
        if (DialogueContainer.Instance.CharacterSelected.GetName() == StringContainer.Gavi)
        {
            EventManager.Publish(new OnDialogueRequestData($"{DialoguesNames.MapEpilogue_Gavi}", () => SceneManager.LoadScene(0)));
        }
        else
        {
            EventManager.Publish(new OnDialogueRequestData($"{DialoguesNames.MapEpilogue_Raline}", () => SceneManager.LoadScene(0)));
        }
    }
}
