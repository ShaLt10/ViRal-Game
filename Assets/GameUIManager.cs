using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MoreMountains.TopDownEngine;
public class GameUIManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text ScoreText;
    public GameObject PausePanel;
    public GameObject HartaKarunPanel;
    public GameObject NextLevelButton;

    private void Start()
    {
        UpdateScore();
    }

    public void OpenHartaKarunPanel(bool toggle)
    {
        HartaKarunPanel.SetActive(toggle);
        NextLevelButton.SetActive(true);
        Debug.Log("Open Harta Karun");
    }

    public void OpenNextLevelPanel(bool toggle)
    {
        NextLevelButton.SetActive(toggle);
        Debug.Log("Open Next Level");
    }

    public void UpdateScore()
    {
        ScoreText.text = (GameManager.Instance.Points + LevelManager.Instance.Point).ToString() ;
    }

    public void NextLevel(string name)
    {
        LevelManager.Instance.StorePointToGameManager();
        LevelManager.Instance.GotoLevel(name);
    }

    public void AddScore()
    {
        LevelManager.Instance.AddPoint(5);
    }
}
