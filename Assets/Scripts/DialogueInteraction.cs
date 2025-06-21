using System;
using System.Collections;
using System.Collections.Generic;
using Game.Utility;
using UnityEngine;

public class DialogueInteraction : Interact
{
    [SerializeField]
    protected string id = string.Empty;
    protected Action action;
    public override void Interaction()
    {
        EventManager.Publish(new OnDialogueRequestData($"{id}{DialogueContainer.Instance.CharacterSelected.GetName()}",action));
    }
}
