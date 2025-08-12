using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogueSendData
{
    public Dialogue[] dialogues= Array.Empty<Dialogue>();
    public Action OnDone;

    public DialogueSendData(Dialogue[] dialogues, Action Ondone = null)
    { 
        this.dialogues = new Dialogue[dialogues.Length];
        dialogues.CopyTo(this.dialogues, 0);
       dialogues.Last().Doafter = Ondone;
    }
}
