using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map1 : SingletonDestroy<Map1>
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"asasasasas Area1-PlayerOpening-{DialogueContainer.Instance.CharacterSelected.GetName()}");
        EventManager.Publish(new OnDialogueRequestData($"Area1-PlayerOpening-{DialogueContainer.Instance.CharacterSelected.GetName()}"));
    }
}
