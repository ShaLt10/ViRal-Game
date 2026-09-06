using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuController : MonoBehaviour
{
    public GameObject HighscorePanel;
    public TMP_Text ScoreText;
    [SerializeField] private string firstScene = SceneFlow.FirstGameplayScene;
    public void ExitGame()
    {
        Application.Quit();
    }

    public void PlayLevel()
    {
        SceneFlow.LoadScene(firstScene);
    }

    public void ShowHighscore(bool toggle)
    {
        int score = PlayerPrefs.GetInt("Highscore", 0);
        ScoreText.text = score.ToString();
        HighscorePanel.SetActive(toggle);
    }
}
