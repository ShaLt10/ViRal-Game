using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSendData
{
    public Dialogue[] dialogues= Array.Empty<Dialogue>();

    public DialogueSendData(Dialogue[] dialogues)
    { 
        this.dialogues = new Dialogue[dialogues.Length];
        dialogues.CopyTo(this.dialogues, 0);
    }
}
