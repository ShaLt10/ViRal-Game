using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueBox : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    GameObject dialogBox;

    [SerializeField] Image potrait;

    [SerializeField] TMP_Text Narrator;

    [SerializeField] TMP_Text Text;

    [SerializeField]
    Dialogue[] dialogues = Array.Empty<Dialogue>();

    int count;

    private void OnEnable()
    {
        EventManager.Subscribe<DialogueSendData>(ShowDialogue);
    }

    private void OnDisable()
    {
        EventManager.Subscribe<DialogueSendData>(ShowDialogue);
    }

    public void ShowDialogue(DialogueSendData data)
    {
        Debug.Log("ShowAh");
        count = 0;
        dialogBox.SetActive(true);
        dialogues = new Dialogue[data.dialogues.Length];
        data.dialogues.CopyTo(dialogues,0);

        potrait.sprite = dialogues[count].imagePotrait.potraitImage;
        Narrator.SetText($"{dialogues[count].speaker}");
        Text.SetText($"{dialogues[count].dialogue}");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        dialogBox.SetActive(false);
    }
}
