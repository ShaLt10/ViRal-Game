using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpotTheDifferenceTime : MonoBehaviour
{
    [SerializeField]
    private Image image;

    private bool isGameFinished;
    
    [SerializeField]
    private float time;

    private float timeLeft = 1;

    private void OnEnable()
    {
        EventManager.Subscribe<SpotTheDifferenceStatusData>(GameStart);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<SpotTheDifferenceStatusData>(GameStart);
    }

    public void Update()
    {
        if (!isGameFinished)
        {
            timeLeft -= Time.deltaTime / time;
            Debug.Log($"time {timeLeft}");
            image.fillAmount = timeLeft;
            if (timeLeft <= 0)
            {
                EventManager.Publish(new SpotTheDifferenceStatusData(false,false,false));
            }
        }
    }


    public void GameStart(SpotTheDifferenceStatusData data)
    {
        if (data.gameFinished == true)
        {
            isGameFinished = data.gameFinished;
        }
        else if (data.gameStart == true)
        {
            isGameFinished = false;
        }
    }
}
