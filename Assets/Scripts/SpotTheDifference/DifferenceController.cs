using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Eventdata;
using UnityEngine;
using UnityEngine.UI;

public class DifferenceController : MonoBehaviour
{
    private int diffCount;

    private int allDiff = 6;

    [SerializeField]
    GameObject result;
    private void OnEnable()
    {
        EventManager.Subscribe<DifferenceSpotted>(GameStart);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<DifferenceSpotted>(GameStart);
    }

    private void GameStart(DifferenceSpotted data)
    {
        diffCount++;
        CheckAllDiff();
    }

    private void CheckAllDiff()
    {
        if (diffCount >= allDiff)
        {
            result.SetActive(true);
            EventManager.Publish(new SpotTheDifferenceStatusData(false, true, true));

        }
    }
}
