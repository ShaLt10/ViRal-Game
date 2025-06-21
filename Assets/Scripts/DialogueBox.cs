using System;
using System.Linq;
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

    int count = 0;

    [SerializeField]
    Image bigButton;

    private void OnEnable()
    {
        EventManager.Subscribe<DialogueSendData>(ShowDialogue);
    }

    private void OnDisable()
    {
        EventManager.Subscribe<DialogueSendData>(ShowDialogue);
    }

    private void Start()
    {
        DialogueContainer.Instance.Nothing();
    }

    public void ShowDialogue(DialogueSendData data)
    {
        Debug.Log("ShowAh");
        count = 0;
        dialogBox.SetActive(true);
        bigButton.raycastTarget = true;
        dialogues = new Dialogue[data.dialogues.Length];
        data.dialogues.CopyTo(dialogues,0);
        UpdateDialogue();

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (count + 1 < dialogues.Length)
        {
            count++;
            UpdateDialogue();
        }
        else
        {
            bigButton.raycastTarget = false;
            dialogBox.SetActive(false);
            Debug.Log("hayo");
            dialogues.Last().Doafter?.Invoke();
        }
    }

    private void UpdateDialogue()
    {
        potrait.sprite = dialogues[count].imagePotrait.potraitImage;
        Narrator.SetText($"{dialogues[count].speaker}");
        Text.SetText($"{dialogues[count].dialogue}");
    }
}
