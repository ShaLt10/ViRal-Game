using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Difference : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Image image;
    [SerializeField]
    private Difference link;
    [SerializeField]
    private DifferenceController controller;
    public bool isClicked =false;


    public void OnPointerClick(PointerEventData eventData)
    {
        if (isClicked) return;
        OnClick();
        link.OnClick();
    }

    public void OnClick()
    { 
        isClicked = true;
        image.color = ChangeAlpha(image.color);
        EventManager.Publish(new Win("Ente buta"));
    }

    public Color ChangeAlpha(Color color)
    {
            color.a = 1;
        return color;
    }
}
