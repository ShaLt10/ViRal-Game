using System.Collections;
using System.Collections.Generic;
using Game.Utility;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Map4Npc : DialogueInteraction
{
    [SerializeField]
    Sprite lookDown;

    [SerializeField]
    SpriteRenderer image;

    [SerializeField]
    Map4Npc friend;

    public override void Interaction()
    {
        if (DialogueContainer.Instance.CharacterSelected.GetName() == StringContainer.Gavi)
        {
            EventManager.Publish(new OnDialogueRequestData($"{DialoguesNames.Map4_dialogue1_Gavi}",() => Map4.Instance.FadeOut()));
        }
        else
        {
            EventManager.Publish(new OnDialogueRequestData($"{DialoguesNames.Map4_dialogue1_Raline}", () => Map4.Instance.FadeOut()));
        }
        LookDown();
        friend.LookDown();
    }

    public void LookDown()
    {

        image.sprite = lookDown;
    }
}
