using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Eventdata;
using UnityEngine;

public class SpotTheDifferenceSingleton : Singleton<SpotTheDifferenceSingleton>
{

    private int gameType = 0;
    
    [SerializeField]
    private GameObject game1;

    [SerializeField]
    private GameObject game2;

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        EventManager.Publish<SpotTheDifferenceStatusData>(new SpotTheDifferenceStatusData(true,false,false));
    }
    public void StopGame()
    {
        EventManager.Publish<SpotTheDifferenceStatusData>(new SpotTheDifferenceStatusData(false,false,false));
    }

    public void SetGame(int add)
    {
        gameType+= add ;
        DisableType();
        if (gameType == 0)
        {
            game1.SetActive(true);
            EventManager.Publish(new ResetGameData());
        }
        if (gameType == 1)
        {
            game2.SetActive(true);
            EventManager.Publish(new ResetGameData());
        }
        else if (gameType >1) 
        {
            SceneLoaderSingleton.Instance.LoadSceneMode(0);       
        }
    }

    public void DisableType()
    { 
        game1?.SetActive(false);
        game2?.SetActive(false);
    }
}

