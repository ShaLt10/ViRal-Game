// Assets/Scripts/SpotTheDifference/SpotDiffUIManager.cs
using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// UI manager khusus section Spot The Difference.
/// Sengaja dipisah dari UIManager global supaya tidak bentrok.
/// </summary>
public class SpotDiffUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject gamePanel;
    public GameObject levelCompletePanel;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    [Header("HUD - Game Panel")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI foundText;       // "0 / 3"
    public TextMeshProUGUI penaltyPopup;    // "-5s", aktif sebentar

    [Header("Level Complete Panel")]
    public TextMeshProUGUI levelCompleteText;

    [Header("Game Over / Victory")]
    public TextMeshProUGUI gameOverText;

    // ── Panel control ──────────────────────────────────────────

    public void ShowMainMenu()
    {
        SetActivePanel(mainMenuPanel);
    }

    public void OnLevelStart(int levelNum, int total)
    {
        SetActivePanel(gamePanel);
        levelText.text = $"Level {levelNum}";
        foundText.text = $"0 / {total}";
        penaltyPopup.gameObject.SetActive(false);
    }

    public void ShowFoundFeedback(int found, int total)
    {
        foundText.text = $"{found} / {total}";
    }

    public void ShowPenaltyFeedback(float seconds)
    {
        StartCoroutine(PenaltyPopupRoutine(seconds));
    }

    public void ShowLevelComplete(int levelNum)
    {
        levelCompletePanel.SetActive(true);
        if (levelCompleteText != null)
            levelCompleteText.text = $"Level {levelNum} Clear!";
    }

    public void ShowGameOver()
    {
        SetActivePanel(gameOverPanel);
    }

    public void ShowVictory()
    {
        SetActivePanel(victoryPanel);
    }

    // ── Internal ───────────────────────────────────────────────

    IEnumerator PenaltyPopupRoutine(float seconds)
    {
        penaltyPopup.text = $"-{seconds}s";
        penaltyPopup.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.2f);
        penaltyPopup.gameObject.SetActive(false);
    }

    /// <summary>
    /// Aktifkan satu panel, nonaktifkan sisanya.
    /// LevelCompletePanel dikontrol terpisah (overlay, bukan replace).
    /// </summary>
    void SetActivePanel(GameObject target)
    {
        if (mainMenuPanel     != null) mainMenuPanel.SetActive(mainMenuPanel == target);
        if (gamePanel         != null) gamePanel.SetActive(gamePanel == target);
        if (gameOverPanel     != null) gameOverPanel.SetActive(gameOverPanel == target);
        if (victoryPanel      != null) victoryPanel.SetActive(victoryPanel == target);
        // levelCompletePanel tidak dimatikan di sini (overlay di atas gamePanel)
    }
}