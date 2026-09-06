using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneFlow
{
    public const string TitleScene = "TitleScene";
    public const string FirstGameplayScene = "Map1";

    public static bool CanLoad(string sceneName)
    {
        return !string.IsNullOrWhiteSpace(sceneName)
            && Application.CanStreamedLevelBeLoaded(sceneName);
    }

    public static bool LoadScene(string sceneName, bool saveProgress = true, string checkpointId = "")
    {
        if (!CanLoad(sceneName))
        {
            Debug.LogError($"[SceneFlow] Scene '{sceneName}' tidak tersedia di Build Settings.");
            return false;
        }

        if (saveProgress && sceneName != TitleScene
            && (GameSession.Instance == null || !GameSession.Instance.SetLocation(sceneName, checkpointId)))
        {
            Debug.LogError($"[SceneFlow] Perpindahan ke '{sceneName}' dibatalkan karena progress gagal disimpan.");
            return false;
        }

        SceneManager.LoadScene(sceneName);
        return true;
    }

    public static bool LoadScene(int buildIndex, bool saveProgress = true)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(buildIndex);
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError($"[SceneFlow] Build index {buildIndex} tidak valid.");
            return false;
        }

        return LoadScene(Path.GetFileNameWithoutExtension(path), saveProgress);
    }

    public static bool ReloadCurrent()
    {
        return LoadScene(SceneManager.GetActiveScene().name, false);
    }

    public static bool LoadTitle()
    {
        if (GameSession.Instance != null && !GameSession.Instance.CaptureCurrentPosition())
        {
            Debug.LogError("[SceneFlow] Kembali ke title dibatalkan karena posisi gagal disimpan.");
            return false;
        }

        return LoadScene(TitleScene, false);
    }
}
