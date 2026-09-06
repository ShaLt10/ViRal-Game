using System.Collections;
using System.Collections.Generic;
using Game.Utility;
using UnityEngine;

public class Map5 : SingletonDestroy<Map5>
{
    private void Start()
    {
        ScreenRotateControl.Instance.SetLandscape();
        
        string dialogKey = CharacterManager.Instance != null && CharacterManager.Instance.GetPlayerName() == "Gavi"
            ? DialoguesNames.MapEpilogue_Gavi
            : DialoguesNames.MapEpilogue_Raline;

        if (DialogManager.Instance != null)
            DialogManager.Instance.PlaySequenceThen(dialogKey, () => SceneFlow.LoadTitle());
        else
            SceneFlow.LoadTitle();
    }
}
