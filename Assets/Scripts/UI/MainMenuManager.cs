using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Main Menu Manager with comprehensive debugging system
/// Debug methods can be safely removed after issues are fixed
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    #region Serialized Fields

    [Header("Panels")]
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject loadPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject settingsScreen;
    [SerializeField] private GameObject openingPanel;
    [SerializeField] private GameObject characterSelectPanel;

    [Header("First Selected (optional)")]
    [SerializeField] private Selectable firstOnTitle;
    [SerializeField] private Selectable firstOnLoad;
    [SerializeField] private Selectable firstOnCredits;
    [SerializeField] private Selectable firstOnSettings;
    [SerializeField] private Selectable firstOnSelect;

    [Header("Components")]
    [SerializeField] private OpeningAnimator openingAnimator;
    [SerializeField] private CharacterSelection characterSelection;

    [Header("Flow")]
    [SerializeField] private bool useOpening = true;
    [SerializeField] private bool narratorOnFirstEnter = true;
    [SerializeField] private bool resetSelectionOnEnter = true;
    [SerializeField] private string fallbackScene = "Map1";

    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool validateOnStart = true;
    [SerializeField] private bool showDebugUI = true;
    [SerializeField] private bool enableKeyboardShortcuts = true;

    #endregion

    #region Private Fields

    private bool hasValidatedReferences = false;
    private Dictionary<string, GameObject> panelRegistry;
    private List<string> debugLog = new List<string>();
    private const int MAX_DEBUG_LOGS = 50;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        InitializePanelRegistry();
        
        DebugLog("=== MAIN MENU MANAGER START ===", DebugLevel.Info);
        
        if (validateOnStart)
        {
            ValidateAllReferences();
        }

        ShowOnly(titleScreen);
        SelectFirst(firstOnTitle);

        if (openingAnimator != null)
        {
            openingAnimator.OnOpeningComplete.RemoveAllListeners();
            openingAnimator.OnOpeningComplete.AddListener(OnOpeningComplete);
            DebugLog("Opening animator event registered", DebugLevel.Success);
        }
        
        DebugLog("=== MAIN MENU READY ===", DebugLevel.Info);
    }

    private void Update()
    {
        // Keyboard shortcuts for testing (can be removed after debugging)
        if (enableKeyboardShortcuts)
        {
            HandleDebugKeyboardShortcuts();
        }
    }

    private void OnGUI()
    {
        // Debug UI overlay (can be removed after debugging)
        if (showDebugUI && enableDebugLogs)
        {
            DrawDebugUI();
        }
    }

    #endregion

    #region Public Methods - Title Screen Buttons

    /// <summary>
    /// Called when Play button is clicked
    /// </summary>
    public void OnPlay()
    {
        DebugLog("🎮 OnPlay() called", DebugLevel.Info);
        LogButtonClick("Play");
        
        if (useOpening && openingPanel != null && openingAnimator != null)
        {
            ShowOnly(openingPanel);
            openingAnimator.BeginOpening();
            DebugLog("Starting opening animation", DebugLevel.Success);
        }
        else
        {
            if (useOpening)
            {
                DebugLog("Opening enabled but panel/animator not assigned. Skipping to character select.", DebugLevel.Warning);
            }
            GoToCharacterSelectOrScene();
        }
    }

    /// <summary>
    /// Called when Load button is clicked
    /// </summary>
    public void OnOpenLoad()
    {
        DebugLog("💾 OnOpenLoad() called", DebugLevel.Info);
        LogButtonClick("Load");
        
        if (loadPanel == null)
        {
            DebugLog("Cannot open Load Panel - loadPanel is not assigned!", DebugLevel.Error);
            return;
        }
        
        ShowOnly(loadPanel);
        SelectFirst(firstOnLoad);
    }

    /// <summary>
    /// Called when Back button in Load panel is clicked
    /// </summary>
    public void OnBackFromLoad()
    {
        DebugLog("⬅️ OnBackFromLoad() called", DebugLevel.Info);
        LogButtonClick("Back from Load");
        ShowOnly(titleScreen);
        SelectFirst(firstOnTitle);
    }

    /// <summary>
    /// Called when Credits button is clicked
    /// </summary>
    public void OnOpenCredits()
    {
        DebugLog("🎬 OnOpenCredits() called", DebugLevel.Info);
        LogButtonClick("Credits");
        
        if (creditsPanel == null)
        {
            DebugLog("Cannot open Credits Panel - creditsPanel is not assigned!", DebugLevel.Error);
            return;
        }
        
        ShowOnly(creditsPanel);
        SelectFirst(firstOnCredits);
    }

    /// <summary>
    /// Called when Back button in Credits panel is clicked
    /// </summary>
    public void OnBackFromCredits()
    {
        DebugLog("⬅️ OnBackFromCredits() called", DebugLevel.Info);
        LogButtonClick("Back from Credits");
        ShowOnly(titleScreen);
        SelectFirst(firstOnTitle);
    }

    /// <summary>
    /// Called when Settings button is clicked
    /// </summary>
    public void OnOpenSettings()
    {
        DebugLog("⚙️ OnOpenSettings() called", DebugLevel.Info);
        LogButtonClick("Settings");
        
        if (settingsScreen == null)
        {
            DebugLog("Cannot open Settings Screen - settingsScreen is not assigned!", DebugLevel.Error);
            return;
        }
        
        ShowOnly(settingsScreen);
        SelectFirst(firstOnSettings);
    }

    /// <summary>
    /// Called when Back button in Settings screen is clicked
    /// </summary>
    public void OnBackFromSettings()
    {
        DebugLog("⬅️ OnBackFromSettings() called", DebugLevel.Info);
        LogButtonClick("Back from Settings");
        ShowOnly(titleScreen);
        SelectFirst(firstOnTitle);
    }

    /// <summary>
    /// Called when Back button in Character Select is clicked
    /// </summary>
    public void OnBackFromCharacterSelect()
    {
        DebugLog("⬅️ OnBackFromCharacterSelect() called", DebugLevel.Info);
        LogButtonClick("Back from Character Select");
        
        if (characterSelection != null)
        {
            characterSelection.ResetForMenu();
        }
        
        ShowOnly(titleScreen);
        SelectFirst(firstOnTitle);
    }

    #endregion

    #region Opening Animation

    private void OnOpeningComplete()
    {
        DebugLog("Opening animation complete → navigating to character select", DebugLevel.Success);
        GoToCharacterSelectOrScene();
    }

    #endregion

    #region Character Selection Flow

    private void GoToCharacterSelectOrScene()
    {
        if (characterSelectPanel != null && characterSelection != null)
        {
            DebugLog("Navigating to Character Select", DebugLevel.Info);
            
            if (resetSelectionOnEnter && CharacterManager.Instance != null)
            {
                CharacterManager.Instance.ResetCharacter();
                DebugLog("Character selection reset", DebugLevel.Success);
            }
            
            ShowOnly(characterSelectPanel);
            characterSelection.EnterFromMenu(narratorOnFirstEnter);
            SelectFirst(firstOnSelect);
        }
        else
        {
            if (characterSelectPanel == null)
            {
                DebugLog("Character Select Panel not assigned. Loading fallback scene.", DebugLevel.Warning);
            }
            if (characterSelection == null)
            {
                DebugLog("Character Selection component not assigned. Loading fallback scene.", DebugLevel.Warning);
            }
            
            if (!string.IsNullOrEmpty(fallbackScene))
            {
                DebugLog($"Loading fallback scene: {fallbackScene}", DebugLevel.Info);
                SceneManager.LoadScene(fallbackScene);
            }
            else
            {
                DebugLog("No character select setup and no fallback scene specified!", DebugLevel.Error);
            }
        }
    }

    #endregion

    #region Panel Management

    private void InitializePanelRegistry()
    {
        panelRegistry = new Dictionary<string, GameObject>
        {
            { "TitleScreen", titleScreen },
            { "LoadPanel", loadPanel },
            { "CreditsPanel", creditsPanel },
            { "SettingsScreen", settingsScreen },
            { "OpeningPanel", openingPanel },
            { "CharacterSelectPanel", characterSelectPanel }
        };
    }

    private void ShowOnly(GameObject target)
    {
        if (target == null)
        {
            DebugLog("ShowOnly called with NULL target!", DebugLevel.Error);
            return;
        }
        
        DebugLog($"Showing: {target.name}", DebugLevel.Info);
        
        SetPanelActive(titleScreen, target == titleScreen);
        SetPanelActive(loadPanel, target == loadPanel);
        SetPanelActive(creditsPanel, target == creditsPanel);
        SetPanelActive(settingsScreen, target == settingsScreen);
        SetPanelActive(openingPanel, target == openingPanel);
        SetPanelActive(characterSelectPanel, target == characterSelectPanel);
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel == null) return;
        if (panel.activeSelf == active) return;
        
        try
        {
            panel.SetActive(active);
            DebugLog($"  {(active ? "✅" : "⬜")} {panel.name}", DebugLevel.Success);
        }
        catch (System.Exception e)
        {
            DebugLog($"Failed to set {panel.name} active={active}: {e.Message}", DebugLevel.Error);
        }
    }

    #endregion

    #region UI Selection

    private void SelectFirst(Selectable selectable)
    {
        if (selectable == null)
        {
            DebugLog("  No first selectable specified (optional)", DebugLevel.Info);
            return;
        }
        
        if (EventSystem.current == null)
        {
            DebugLog("Cannot select UI - EventSystem.current is NULL!", DebugLevel.Error);
            return;
        }
        
        try
        {
            EventSystem.current.SetSelectedGameObject(null);
            selectable.Select();
            DebugLog($"  Selected: {selectable.gameObject.name}", DebugLevel.Success);
        }
        catch (System.Exception e)
        {
            DebugLog($"Failed to select {selectable.gameObject.name}: {e.Message}", DebugLevel.Error);
        }
    }

    #endregion

    #region Validation

    private void ValidateAllReferences()
    {
        if (hasValidatedReferences) return;
        
        DebugLog("=== VALIDATING REFERENCES ===", DebugLevel.Info);
        
        // Panels
        ValidateReference("titleScreen", titleScreen, true);
        ValidateReference("loadPanel", loadPanel, true);
        ValidateReference("creditsPanel", creditsPanel, true);
        ValidateReference("settingsScreen", settingsScreen, true);
        ValidateReference("openingPanel", openingPanel, false);
        ValidateReference("characterSelectPanel", characterSelectPanel, false);
        
        // First Selected
        ValidateReference("firstOnTitle", firstOnTitle, false);
        ValidateReference("firstOnLoad", firstOnLoad, false);
        ValidateReference("firstOnCredits", firstOnCredits, false);
        ValidateReference("firstOnSettings", firstOnSettings, false);
        ValidateReference("firstOnSelect", firstOnSelect, false);
        
        // Components
        ValidateReference("openingAnimator", openingAnimator, false);
        ValidateReference("characterSelection", characterSelection, false);
        
        // EventSystem
        ValidateEventSystem();
        
        // Canvas
        ValidateCanvas();
        
        DebugLog("=== VALIDATION COMPLETE ===", DebugLevel.Info);
        hasValidatedReferences = true;
    }

    private void ValidateReference(string name, Object obj, bool critical)
    {
        if (obj != null)
        {
            DebugLog($"✅ {name}: {obj.name}", DebugLevel.Success);
        }
        else
        {
            DebugLog($"❌ {name}: NOT ASSIGNED", critical ? DebugLevel.Error : DebugLevel.Warning);
        }
    }

    private void ValidateEventSystem()
    {
        if (EventSystem.current == null)
        {
            DebugLog("❌ EventSystem.current is NULL! Add EventSystem to scene.", DebugLevel.Error);
        }
        else
        {
            DebugLog($"✅ EventSystem: {EventSystem.current.gameObject.name}", DebugLevel.Success);
        }
    }

    private void ValidateCanvas()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        if (canvases.Length == 0)
        {
            DebugLog("❌ No Canvas found in scene!", DebugLevel.Error);
            return;
        }

        foreach (Canvas canvas in canvases)
        {
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                DebugLog($"⚠️ Canvas '{canvas.gameObject.name}' missing GraphicRaycaster!", DebugLevel.Warning);
            }
            else
            {
                DebugLog($"✅ Canvas '{canvas.gameObject.name}' has GraphicRaycaster", DebugLevel.Success);
            }
        }
    }

    #endregion

    // ============================================================================
    // DEBUG METHODS BELOW - CAN BE SAFELY REMOVED AFTER DEBUGGING
    // ============================================================================

    #region Debug System (REMOVABLE)

    private enum DebugLevel { Info, Success, Warning, Error }

    private void DebugLog(string message, DebugLevel level = DebugLevel.Info)
    {
        if (!enableDebugLogs) return;

        string prefix = level switch
        {
            DebugLevel.Success => "✅",
            DebugLevel.Warning => "⚠️",
            DebugLevel.Error => "❌",
            _ => "ℹ️"
        };

        string formattedMessage = $"{prefix} [MainMenu] {message}";
        
        // Log to console
        switch (level)
        {
            case DebugLevel.Error:
                Debug.LogError(formattedMessage, this);
                break;
            case DebugLevel.Warning:
                Debug.LogWarning(formattedMessage, this);
                break;
            default:
                Debug.Log(formattedMessage, this);
                break;
        }

        // Store for UI display
        debugLog.Add($"{Time.time:F2}s: {formattedMessage}");
        if (debugLog.Count > MAX_DEBUG_LOGS)
        {
            debugLog.RemoveAt(0);
        }
    }

    private void LogButtonClick(string buttonName)
    {
        DebugLog($"🖱️ Button Clicked: {buttonName}", DebugLevel.Info);
    }

    #endregion

    #region Debug UI Overlay (REMOVABLE)

    private Vector2 debugScrollPosition;
    private bool showDebugPanel = true;
    private Rect debugWindowRect = new Rect(10, 10, 400, 300);

    private void DrawDebugUI()
    {
        if (!showDebugPanel) return;

        debugWindowRect = GUI.Window(0, debugWindowRect, DrawDebugWindow, "🔧 Main Menu Debug");
    }

    private void DrawDebugWindow(int windowID)
    {
        GUILayout.BeginVertical();

        // Status Section
        GUILayout.Label("=== STATUS ===", GUI.skin.box);
        DrawStatusInfo();

        GUILayout.Space(10);

        // Quick Actions
        GUILayout.Label("=== QUICK ACTIONS ===", GUI.skin.box);
        DrawQuickActions();

        GUILayout.Space(10);

        // Recent Logs
        GUILayout.Label("=== RECENT LOGS ===", GUI.skin.box);
        DrawRecentLogs();

        GUILayout.EndVertical();

        GUI.DragWindow();
    }

    private void DrawStatusInfo()
    {
        GUILayout.BeginVertical(GUI.skin.box);
        
        // Current panel
        string currentPanel = "None";
        foreach (var kvp in panelRegistry)
        {
            if (kvp.Value != null && kvp.Value.activeSelf)
            {
                currentPanel = kvp.Key;
                break;
            }
        }
        GUILayout.Label($"Current Panel: {currentPanel}");
        
        // EventSystem
        GUILayout.Label($"EventSystem: {(EventSystem.current != null ? "✅ OK" : "❌ MISSING")}");
        
        // Selected GameObject
        GameObject selected = EventSystem.current?.currentSelectedGameObject;
        GUILayout.Label($"Selected: {(selected != null ? selected.name : "None")}");
        
        GUILayout.EndVertical();
    }

    private void DrawQuickActions()
    {
        GUILayout.BeginVertical(GUI.skin.box);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Show Title")) ShowOnly(titleScreen);
        if (GUILayout.Button("Show Load")) OnOpenLoad();
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Show Credits")) OnOpenCredits();
        if (GUILayout.Button("Show Settings")) OnOpenSettings();
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Validate Refs")) ValidateAllReferences();
        if (GUILayout.Button("Clear Logs")) debugLog.Clear();
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
    }

    private void DrawRecentLogs()
    {
        GUILayout.BeginVertical(GUI.skin.box);
        
        debugScrollPosition = GUILayout.BeginScrollView(debugScrollPosition, GUILayout.Height(150));
        
        for (int i = debugLog.Count - 1; i >= 0 && i >= debugLog.Count - 10; i--)
        {
            GUILayout.Label(debugLog[i]);
        }
        
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    #endregion

    #region Debug Keyboard Shortcuts (REMOVABLE)

    private void HandleDebugKeyboardShortcuts()
    {
        // F1-F6 for quick panel switching
        if (Input.GetKeyDown(KeyCode.F1))
        {
            DebugLog("🎹 F1: Showing Title Screen", DebugLevel.Info);
            ShowOnly(titleScreen);
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            DebugLog("🎹 F2: Opening Load Panel", DebugLevel.Info);
            OnOpenLoad();
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            DebugLog("🎹 F3: Opening Credits Panel", DebugLevel.Info);
            OnOpenCredits();
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            DebugLog("🎹 F4: Opening Settings", DebugLevel.Info);
            OnOpenSettings();
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            DebugLog("🎹 F5: Revalidating References", DebugLevel.Info);
            hasValidatedReferences = false;
            ValidateAllReferences();
        }
        else if (Input.GetKeyDown(KeyCode.F6))
        {
            DebugLog("🎹 F6: Opening Character Select", DebugLevel.Info);
            GoToCharacterSelectOrScene();
        }
        
        // Toggle debug UI
        if (Input.GetKeyDown(KeyCode.F12))
        {
            showDebugPanel = !showDebugPanel;
            DebugLog($"🎹 F12: Debug Panel {(showDebugPanel ? "Shown" : "Hidden")}", DebugLevel.Info);
        }
    }

    #endregion

    #region Context Menu Debug Commands (REMOVABLE)

    [ContextMenu("Debug/Test: Open Load Panel")]
    private void DebugTestOpenLoad()
    {
        DebugLog("=== TESTING: Open Load Panel ===", DebugLevel.Info);
        OnOpenLoad();
    }

    [ContextMenu("Debug/Test: Open Credits Panel")]
    private void DebugTestOpenCredits()
    {
        DebugLog("=== TESTING: Open Credits Panel ===", DebugLevel.Info);
        OnOpenCredits();
    }

    [ContextMenu("Debug/Test: Open Settings")]
    private void DebugTestOpenSettings()
    {
        DebugLog("=== TESTING: Open Settings ===", DebugLevel.Info);
        OnOpenSettings();
    }

    [ContextMenu("Debug/Validate All References")]
    private void DebugForceValidation()
    {
        hasValidatedReferences = false;
        ValidateAllReferences();
    }

    [ContextMenu("Debug/Show Current Panel States")]
    private void DebugShowPanelStates()
    {
        DebugLog("=== CURRENT PANEL STATES ===", DebugLevel.Info);
        foreach (var kvp in panelRegistry)
        {
            string state = kvp.Value == null ? "NULL" : (kvp.Value.activeSelf ? "ACTIVE ✅" : "inactive ⬜");
            DebugLog($"{kvp.Key}: {state}", DebugLevel.Info);
        }
    }

    [ContextMenu("Debug/List All Buttons in Scene")]
    private void DebugListAllButtons()
    {
        DebugLog("=== ALL BUTTONS IN SCENE ===", DebugLevel.Info);
        Button[] buttons = FindObjectsOfType<Button>(true);
        
        foreach (Button button in buttons)
        {
            int eventCount = button.onClick.GetPersistentEventCount();
            string status = eventCount > 0 ? $"✅ {eventCount} events" : "❌ No events";
            DebugLog($"Button: {button.gameObject.name} - {status}", eventCount > 0 ? DebugLevel.Success : DebugLevel.Warning);
            
            // List events
            for (int i = 0; i < eventCount; i++)
            {
                string targetName = button.onClick.GetPersistentTarget(i)?.name ?? "NULL";
                string methodName = button.onClick.GetPersistentMethodName(i);
                DebugLog($"  → {targetName}.{methodName}()", DebugLevel.Info);
            }
        }
    }

    [ContextMenu("Debug/Check EventSystem")]
    private void DebugCheckEventSystem()
    {
        DebugLog("=== EVENT SYSTEM CHECK ===", DebugLevel.Info);
        
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
        DebugLog($"Found {eventSystems.Length} EventSystem(s)", eventSystems.Length == 1 ? DebugLevel.Success : DebugLevel.Warning);
        
        if (EventSystem.current != null)
        {
            DebugLog($"Current EventSystem: {EventSystem.current.gameObject.name}", DebugLevel.Success);
            DebugLog($"Currently Selected: {EventSystem.current.currentSelectedGameObject?.name ?? "None"}", DebugLevel.Info);
        }
        else
        {
            DebugLog("No active EventSystem found!", DebugLevel.Error);
        }
    }

    [ContextMenu("Debug/Check All Canvas Raycasters")]
    private void DebugCheckCanvasRaycasters()
    {
        DebugLog("=== CANVAS RAYCASTER CHECK ===", DebugLevel.Info);
        
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        DebugLog($"Found {canvases.Length} Canvas(es)", DebugLevel.Info);
        
        foreach (Canvas canvas in canvases)
        {
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                DebugLog($"✅ {canvas.gameObject.name} has GraphicRaycaster", DebugLevel.Success);
            }
            else
            {
                DebugLog($"❌ {canvas.gameObject.name} MISSING GraphicRaycaster!", DebugLevel.Error);
            }
        }
    }

    [ContextMenu("Debug/Generate Full Diagnostic Report")]
    private void DebugGenerateFullReport()
    {
        DebugLog("=== FULL DIAGNOSTIC REPORT ===", DebugLevel.Info);
        
        // References
        hasValidatedReferences = false;
        ValidateAllReferences();
        
        // Panel states
        DebugShowPanelStates();
        
        // Buttons
        DebugListAllButtons();
        
        // EventSystem
        DebugCheckEventSystem();
        
        // Canvas
        DebugCheckCanvasRaycasters();
        
        DebugLog("=== REPORT COMPLETE ===", DebugLevel.Info);
    }

    #endregion

    // ============================================================================
    // END OF REMOVABLE DEBUG METHODS
    // To remove all debug functionality after fixing issues:
    // 1. Set enableDebugLogs = false
    // 2. Set showDebugUI = false  
    // 3. Set enableKeyboardShortcuts = false
    // 4. Delete everything between the "DEBUG METHODS" markers above
    // ============================================================================
}