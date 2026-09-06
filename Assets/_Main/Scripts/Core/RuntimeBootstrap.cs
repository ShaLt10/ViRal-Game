using UnityEngine;
using UnityEngine.UI;

public static class RuntimeBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (GameSession.Instance != null)
            return;

        GameObject root = new GameObject("[GameRoot]");
        Object.DontDestroyOnLoad(root);
        root.AddComponent<GameSession>();
        root.AddComponent<ObjectiveManager>();

        LoadPrefab("Managers/CharacterManager", root.transform);
        LoadPrefab("Managers/GeneratedDialog", root.transform);

        GameObject dialogUI = LoadPrefab("Managers/DialogUI", root.transform);
        EnsureDialogCanvas(dialogUI);

        LoadPrefab("Managers/DialogManager", root.transform);
    }

    private static GameObject LoadPrefab(string resourcePath, Transform parent)
    {
        GameObject prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab == null)
        {
            Debug.LogError($"[RuntimeBootstrap] Prefab Resources/{resourcePath} tidak ditemukan.");
            return null;
        }

        return Object.Instantiate(prefab, parent, false);
    }

    private static void EnsureDialogCanvas(GameObject dialogUI)
    {
        if (dialogUI == null || dialogUI.GetComponentInParent<Canvas>() != null)
            return;

        Canvas canvas = dialogUI.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        dialogUI.AddComponent<CanvasScaler>();
        dialogUI.AddComponent<GraphicRaycaster>();
    }

}
