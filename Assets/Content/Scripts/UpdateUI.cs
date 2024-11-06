using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateUI : MonoBehaviour
{
    public RectTransform uiElement;
    public RectTransform uiElement2;

    public void ForceUpdate()
    {
        // Force the UI to update its layout immediately
        uiElement.ForceUpdateRectTransforms();
        uiElement2.ForceUpdateRectTransforms();
        Debug.Log("UPDATE");
    }
}
