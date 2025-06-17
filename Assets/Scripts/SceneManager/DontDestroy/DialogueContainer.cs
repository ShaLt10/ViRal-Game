using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueContainer : Singleton<DialogueContainer>
{

    [SerializeField]
    List<DialogueSequence> sequences = new();

    public void Sequences(string id)
    { 
        var dialogue = sequences.Find(x => x.sequenceName == id);
        if (dialogue == null) return;
        Debug.Log("Kiriman");
        EventManager.Publish<DialogueSendData>(new DialogueSendData(dialogue.GetDialogue()));
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    private void OnEnable()
    {
        EventManager.Subscribe<OnDialogueRequestData>(CallDialogue);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<OnDialogueRequestData>(CallDialogue);
    }


    private void CallDialogue(OnDialogueRequestData data)
    {
        Sequences(data.sequenceName);
    }
}
