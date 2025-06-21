using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AlunaMap2 : DialogueInteraction
{
    public override void Interaction()
    {
        EventManager.Publish(new OnDialogueRequestData($"{id}{DialogueContainer.Instance.CharacterSelected.GetName()}", () => SceneManager.LoadScene(5)));
    }
}
