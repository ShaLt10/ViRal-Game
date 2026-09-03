// Assets/Scripts/SpotTheDifference/SpotDifferenceManager.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// GameManager khusus section Spot The Difference.
/// Single scene, 2 level, timer penalty, win/lose via dialog system.
/// </summary>
public class SpotDifferenceManager : MonoBehaviour
{
    public static SpotDifferenceManager Instance { get; private set; }

    // ── Config ─────────────────────────────────────────────────
    [Header("Level Config")]
    public LevelData[] levels;
    public float timePerLevel = 30f;
    public float wrongClickPenalty = 5f;

    [Header("Scene References")]
    public SpriteRenderer leftRenderer;
    public SpriteRenderer rightRenderer;
    public DifferenceClickHandler clickHandler;
    public TimerManager timerManager;
    public SpotDiffUIManager uiManager;   // ← pakai SpotDiffUIManager

    [Header("Dialog / Cutscene (opsional)")]
    [Tooltip("Isi jika win/lose trigger dialog. Kosongkan bila dialog belum siap.")]
    public GameObject winDialogTrigger;
    public GameObject loseDialogTrigger;

    // ── State ──────────────────────────────────────────────────
    public enum GameState { MainMenu, Playing, LevelComplete, GameOver }
    public GameState State { get; private set; }

    int currentLevelIndex;
    int foundCount;
    const int DiffsPerLevel = 3;

    // ── Events (untuk sistem lain yang subscribe) ──────────────
    public UnityEvent<int> onLevelStart;
    public UnityEvent<int> onDiffFound;
    public UnityEvent      onLevelComplete;
    public UnityEvent      onGameOver;

    // ══════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() => ShowMainMenu();

    // ── Public API (dipanggil tombol UI / dialog) ──────────────

    public void StartGame()
    {
        currentLevelIndex = 0;
        LoadLevel(currentLevelIndex);
    }

    /// <summary>Dipanggil DifferenceClickHandler saat klik benar.</summary>
    public void RegisterCorrectClick(int diffIndex)
    {
        foundCount++;
        onDiffFound?.Invoke(foundCount);
        uiManager.ShowFoundFeedback(foundCount, DiffsPerLevel);

        if (foundCount >= DiffsPerLevel)
            StartCoroutine(CompleteLevelRoutine());
    }

    /// <summary>Dipanggil DifferenceClickHandler saat klik salah.</summary>
    public void RegisterWrongClick(Vector3 worldPos)
    {
        timerManager.ApplyPenalty(wrongClickPenalty);
        uiManager.ShowPenaltyFeedback(wrongClickPenalty);
    }

    /// <summary>Dipanggil TimerManager saat timer habis.</summary>
    public void OnTimerExpired()
    {
        if (State != GameState.Playing) return;
        State = GameState.GameOver;
        clickHandler.SetEnabled(false);
        onGameOver?.Invoke();
        uiManager.ShowGameOver();
        TriggerLoseDialog();
    }

    public void RestartCurrentLevel() => LoadLevel(currentLevelIndex);

    /// <summary>Lanjut ke level berikutnya. Jika sudah habis → victory.</summary>
    public void NextLevel()
    {
        currentLevelIndex++;
        if (currentLevelIndex < levels.Length)
            LoadLevel(currentLevelIndex);
        else
        {
            uiManager.ShowVictory();
            TriggerWinDialog();
        }
    }

    // ── Private ────────────────────────────────────────────────

    void ShowMainMenu()
    {
        State = GameState.MainMenu;
        uiManager.ShowMainMenu();
    }

    void LoadLevel(int index)
    {
        if (index < 0 || index >= levels.Length)
        {
            Debug.LogError($"[SpotDiff] Level index {index} out of range!");
            return;
        }

        State    = GameState.Playing;
        foundCount = 0;

        LevelData data = levels[index];
        leftRenderer.sprite  = data.leftImage;
        rightRenderer.sprite = data.rightImage;

        clickHandler.InitLevel(data);
        timerManager.StartTimer(timePerLevel);
        uiManager.OnLevelStart(index + 1, DiffsPerLevel);
        onLevelStart?.Invoke(index);
    }

    IEnumerator CompleteLevelRoutine()
    {
        State = GameState.LevelComplete;
        clickHandler.SetEnabled(false);
        timerManager.StopTimer();
        onLevelComplete?.Invoke();
        uiManager.ShowLevelComplete(currentLevelIndex + 1);
        yield return new WaitForSeconds(2f);
        // Panel "Next" muncul → player klik sendiri via tombol
    }

    void TriggerWinDialog()
    {
        if (winDialogTrigger != null)
            winDialogTrigger.SetActive(true);
    }

    void TriggerLoseDialog()
    {
        if (loseDialogTrigger != null)
            loseDialogTrigger.SetActive(true);
    }
}