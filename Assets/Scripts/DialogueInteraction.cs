using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueInteraction : Interact
{
    [SerializeField]
    private string id = string.Empty;
    Action action;
    public override void Interaction()
    {
        EventManager.Publish(new OnDialogueRequestData(id,action));
    }
}
