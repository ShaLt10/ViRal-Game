using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultButton : MonoBehaviour
{
    [SerializeField]
    Button button;

    public int win = 0;

    private void OnEnable()
    {
        button.onClick.AddListener(OnClickButton);
        EventManager.Subscribe<SpotTheDifferenceStatusData>(OnWin);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnClickButton);
        EventManager.Unsubscribe<SpotTheDifferenceStatusData>(OnWin);
    }

    private void OnClickButton()
    {
        SpotTheDifferenceSingleton.Instance.SetGame(win);
    }

    private void OnWin(SpotTheDifferenceStatusData data)
    {
        if (data.gameWin)
        {
            win = 1;
        }
        else
        {
            win = 0;
        }
    }
}
