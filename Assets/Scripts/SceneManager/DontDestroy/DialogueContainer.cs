using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueContainer : PreDetermindSingleton<DialogueContainer>
{

    [SerializeField]
    List<DialogueSequence> sequences = new();

    [SerializeField]
    public CharacterSelected CharacterSelected;

    private void Sequences(string id, Action action = null)
    { 
        var dialogue = sequences.Find(x => x.sequenceName == id);
        Debug.Log($"hahahihi{id}");
        if (dialogue == null) return;
        Debug.Log("Kiriman");
        EventManager.Publish<DialogueSendData>(new DialogueSendData(dialogue.GetDialogue(),action));
    }
    public void Sequences(string id)
    {
        var dialogue = sequences.Find(x => x.sequenceName == id);
        Debug.Log($"hahahihi{id}");
        if (dialogue == null) return;
        Debug.Log("Kiriman");
        EventManager.Publish<DialogueSendData>(new DialogueSendData(dialogue.GetDialogue()));
    }

    public void Nothing()
    {
       // Debug.Log("nothing");
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
        Debug.Log("data request dialogue");
        Sequences(data.sequenceName,data.action);
    }
}
