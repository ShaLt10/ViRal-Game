using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public sealed class GameSaveData
{
    public const int CurrentVersion = 2;

    public int version = CurrentVersion;
    public int selectedCharacter;
    public string currentScene = "Map1";
    public string checkpointId = "";
    public bool hasPlayerPosition;
    public float playerX;
    public float playerY;
    public float playerZ;
    public string activeObjectiveId = "";
    public int activeObjectiveCount;
    public List<string> completedObjectives = new List<string>();
    public List<string> completedMinigames = new List<string>();

    public bool TryUpgrade()
    {
        if (version < 1 || version > CurrentVersion)
            return false;

        if (version == 1)
        {
            hasPlayerPosition = false;
            version = 2;
        }

        completedObjectives ??= new List<string>();
        completedMinigames ??= new List<string>();
        checkpointId ??= "";
        activeObjectiveId ??= "";
        return IsValid();
    }

    public bool IsValid()
    {
        return version == CurrentVersion
            && selectedCharacter >= 0
            && selectedCharacter <= 2
            && !string.IsNullOrWhiteSpace(currentScene)
            && IsFinite(playerX)
            && IsFinite(playerY)
            && IsFinite(playerZ)
            && activeObjectiveCount >= 0
            && completedObjectives != null
            && completedMinigames != null;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}

public sealed class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }
    public GameSaveData Data { get; private set; }
    public bool HasSave => SaveSystem.HasSave;

    private bool restorePlayerOnSceneLoad;
    private bool suppressAutomaticPositionSave;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Data = SaveSystem.TryLoad(out GameSaveData loaded) ? loaded : new GameSaveData();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            CaptureCurrentPosition();
    }

    private void OnApplicationQuit()
    {
        CaptureCurrentPosition();
    }

    public bool StartNewGame()
    {
        if (!SaveSystem.Delete())
            return false;

        Data = new GameSaveData();
        suppressAutomaticPositionSave = false;
        CharacterManager.Instance?.ResetCharacter(false);
        ObjectiveManager.Instance?.ResetAllObjectives(false);
        return true;
    }

    public bool ContinueGame()
    {
        if (!SaveSystem.TryLoad(out GameSaveData loaded))
            return false;

        string targetScene = loaded.currentScene;
        if (targetScene == SceneFlow.TitleScene || !SceneFlow.CanLoad(targetScene))
        {
            Debug.LogWarning($"[GameSession] Scene save '{targetScene}' tidak tersedia. Kembali ke {SceneFlow.FirstGameplayScene}.");
            loaded.currentScene = SceneFlow.FirstGameplayScene;
            loaded.checkpointId = "";
            loaded.hasPlayerPosition = false;
            if (!SaveSystem.Save(loaded))
                return false;
            targetScene = loaded.currentScene;
        }

        Data = loaded;
        suppressAutomaticPositionSave = false;
        CharacterManager.Instance?.ApplySavedSelection(Data.selectedCharacter);
        ObjectiveManager.Instance?.RestoreFromSession();
        restorePlayerOnSceneLoad = Data.hasPlayerPosition || !string.IsNullOrEmpty(Data.checkpointId);
        if (SceneFlow.LoadScene(targetScene, false))
            return true;

        restorePlayerOnSceneLoad = false;
        return false;
    }

    public bool DeleteSave()
    {
        if (!SaveSystem.Delete())
            return false;

        Data = new GameSaveData();
        suppressAutomaticPositionSave = true;
        CharacterManager.Instance?.ResetCharacter(false);
        ObjectiveManager.Instance?.ResetAllObjectives(false);
        return true;
    }

    public bool SetSelectedCharacter(int selectedCharacter)
    {
        int previous = Data.selectedCharacter;
        Data.selectedCharacter = Mathf.Clamp(selectedCharacter, 0, 2);
        if (Save())
        {
            suppressAutomaticPositionSave = false;
            return true;
        }

        Data.selectedCharacter = previous;
        return false;
    }

    public bool SetLocation(string sceneName, string checkpointId = "")
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return false;

        string previousScene = Data.currentScene;
        string previousCheckpoint = Data.checkpointId;
        bool previousHasPosition = Data.hasPlayerPosition;
        float previousX = Data.playerX;
        float previousY = Data.playerY;
        float previousZ = Data.playerZ;
        Data.currentScene = sceneName;
        Data.checkpointId = checkpointId ?? "";
        Data.hasPlayerPosition = false;

        if (Save())
        {
            suppressAutomaticPositionSave = false;
            return true;
        }

        Data.currentScene = previousScene;
        Data.checkpointId = previousCheckpoint;
        Data.hasPlayerPosition = previousHasPosition;
        Data.playerX = previousX;
        Data.playerY = previousY;
        Data.playerZ = previousZ;
        return false;
    }

    public bool CompleteObjective(string objectiveId)
    {
        if (string.IsNullOrWhiteSpace(objectiveId))
            return false;

        bool alreadyCompleted = Data.completedObjectives.Contains(objectiveId);
        string previousActiveId = Data.activeObjectiveId;
        int previousActiveCount = Data.activeObjectiveCount;

        if (!alreadyCompleted)
            Data.completedObjectives.Add(objectiveId);
        if (Data.activeObjectiveId == objectiveId)
        {
            Data.activeObjectiveId = "";
            Data.activeObjectiveCount = 0;
        }

        if (Save())
        {
            suppressAutomaticPositionSave = false;
            return true;
        }

        if (!alreadyCompleted)
            Data.completedObjectives.Remove(objectiveId);
        Data.activeObjectiveId = previousActiveId;
        Data.activeObjectiveCount = previousActiveCount;
        return false;
    }

    public bool CompleteMinigame(string minigameId)
    {
        return AddUnique(Data.completedMinigames, minigameId);
    }

    public bool IsObjectiveCompleted(string objectiveId)
    {
        return !string.IsNullOrWhiteSpace(objectiveId)
            && Data.completedObjectives.Contains(objectiveId);
    }

    public bool IsMinigameCompleted(string minigameId)
    {
        return !string.IsNullOrWhiteSpace(minigameId)
            && Data.completedMinigames.Contains(minigameId);
    }

    public bool SetActiveObjective(string objectiveId, int currentCount)
    {
        string previousId = Data.activeObjectiveId;
        int previousCount = Data.activeObjectiveCount;
        Data.activeObjectiveId = objectiveId ?? "";
        Data.activeObjectiveCount = Mathf.Max(0, currentCount);

        if (Save())
        {
            suppressAutomaticPositionSave = false;
            return true;
        }

        Data.activeObjectiveId = previousId;
        Data.activeObjectiveCount = previousCount;
        return false;
    }

    public bool ClearObjectives()
    {
        var previous = new List<string>(Data.completedObjectives);
        string previousActiveId = Data.activeObjectiveId;
        int previousActiveCount = Data.activeObjectiveCount;
        Data.completedObjectives.Clear();
        Data.activeObjectiveId = "";
        Data.activeObjectiveCount = 0;
        if (Save())
        {
            suppressAutomaticPositionSave = false;
            return true;
        }

        Data.completedObjectives.AddRange(previous);
        Data.activeObjectiveId = previousActiveId;
        Data.activeObjectiveCount = previousActiveCount;
        return false;
    }

    public bool Save()
    {
        return SaveSystem.Save(Data);
    }

    public bool CaptureCurrentPosition(string checkpointId = null)
    {
        if (suppressAutomaticPositionSave)
            return true;

        Scene scene = SceneManager.GetActiveScene();
        Transform player = FindPlayer();
        if (!scene.isLoaded || scene.name == SceneFlow.TitleScene || player == null)
            return true;

        string previousScene = Data.currentScene;
        string previousCheckpoint = Data.checkpointId;
        bool previousHasPosition = Data.hasPlayerPosition;
        float previousX = Data.playerX;
        float previousY = Data.playerY;
        float previousZ = Data.playerZ;

        string resolvedCheckpoint = checkpointId ?? CurrentCheckpointName();
        if (string.IsNullOrEmpty(resolvedCheckpoint) && Data.currentScene == scene.name)
            resolvedCheckpoint = Data.checkpointId;

        Data.currentScene = scene.name;
        Data.checkpointId = resolvedCheckpoint;
        Data.hasPlayerPosition = true;
        Data.playerX = player.position.x;
        Data.playerY = player.position.y;
        Data.playerZ = player.position.z;

        if (Save())
        {
            suppressAutomaticPositionSave = false;
            return true;
        }

        Data.currentScene = previousScene;
        Data.checkpointId = previousCheckpoint;
        Data.hasPlayerPosition = previousHasPosition;
        Data.playerX = previousX;
        Data.playerY = previousY;
        Data.playerZ = previousZ;
        return false;
    }

    private bool AddUnique(List<string> values, string value)
    {
        if (string.IsNullOrWhiteSpace(value) || values.Contains(value))
            return true;

        values.Add(value);
        if (Save())
        {
            suppressAutomaticPositionSave = false;
            return true;
        }

        values.Remove(value);
        return false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!restorePlayerOnSceneLoad || scene.name != Data.currentScene)
            return;

        restorePlayerOnSceneLoad = false;
        StartCoroutine(RestorePlayerPosition());
    }

    private IEnumerator RestorePlayerPosition()
    {
        yield return null;

        Transform player = FindPlayer();
        if (player == null)
        {
            Debug.LogWarning("[GameSession] Player belum tersedia; posisi save tidak dapat dipulihkan.");
            yield break;
        }

        Vector3 target = Data.hasPlayerPosition
            ? new Vector3(Data.playerX, Data.playerY, Data.playerZ)
            : player.position;

        var checkpoints = FindObjectsOfType<MoreMountains.TopDownEngine.CheckPoint>();
        foreach (var checkpoint in checkpoints)
        {
            if (checkpoint.name != Data.checkpointId)
                continue;

            target = checkpoint.transform.position;
            if (MoreMountains.TopDownEngine.LevelManager.HasInstance)
                MoreMountains.TopDownEngine.LevelManager.Instance.SetCurrentCheckpoint(checkpoint);
            break;
        }

        player.position = target;
        if (player.TryGetComponent(out Rigidbody2D body2D))
            body2D.position = target;
        if (player.TryGetComponent(out Rigidbody body3D))
            body3D.position = target;
        Physics2D.SyncTransforms();
        Physics.SyncTransforms();
    }

    private static Transform FindPlayer()
    {
        foreach (var character in FindObjectsOfType<MoreMountains.TopDownEngine.Character>())
        {
            if (character.CharacterType == MoreMountains.TopDownEngine.Character.CharacterTypes.Player)
                return character.transform;
        }

        GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
        return taggedPlayer != null ? taggedPlayer.transform : null;
    }

    private static string CurrentCheckpointName()
    {
        if (!MoreMountains.TopDownEngine.LevelManager.HasInstance
            || MoreMountains.TopDownEngine.LevelManager.Instance.CurrentCheckpoint == null)
            return "";

        return MoreMountains.TopDownEngine.LevelManager.Instance.CurrentCheckpoint.name;
    }
}
