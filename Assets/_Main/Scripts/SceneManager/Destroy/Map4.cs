using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Map4 : SingletonDestroy<Map4>
{
    [SerializeField]
    Image BgBlack;
    [SerializeField] private string nextScene = "MapEpilogue";

    private void Start()
    {
        ScreenRotateControl.Instance.SetLandscape();
    }


    public void FadeOut()
    {
        ColorAlphaChange(BgBlack.color);
    }

    public void ColorAlphaChange(Color color)
    {
        BgBlack.raycastTarget = true;
        StartCoroutine(Change(color, PlayClosingDialog));
    }

    private void PlayClosingDialog()
    {
        if (DialogManager.Instance != null)
            DialogManager.Instance.PlaySequenceThen(DialoguesNames.Map4_Dialogue2, () => SceneFlow.LoadScene(nextScene));
        else
            SceneFlow.LoadScene(nextScene);
    }


    public IEnumerator Change(Color color, Action action = null)
    {
        var a = color;
        while (color.a < 1)
        { 
            color.a += Time.deltaTime;
            BgBlack.color = color;
            yield return null;
        }
        action?.Invoke();
    }
}
