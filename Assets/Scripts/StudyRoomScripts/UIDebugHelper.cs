using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class EnhancedMinigame3Debug : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool enableAutoFix = true;
    public bool showDetailedLogs = true;
    public bool createVisualIndicators = true;

    [Header("Manual Testing")]
    public GameObject testUIPrefab;
    public Sprite testSprite;

    void Start()
    {
        Invoke(nameof(ComprehensiveMinigame3Debug), 1f);
    }

    [ContextMenu("Comprehensive Debug")]
    public void ComprehensiveMinigame3Debug()
    {
        Debug.Log("🔍 ===== COMPREHENSIVE MINIGAME3 DEBUG ===== 🔍");
        
        Canvas minigame3Canvas = FindMinigame3Canvas();
        
        if (minigame3Canvas == null)
        {
            Debug.LogError("❌ MINIGAME3 CANVAS NOT FOUND!");
            SearchAllMinigame3Objects();
            return;
        }

        // Step 1: Basic Canvas Analysis
        AnalyzeCanvasBasics(minigame3Canvas);
        
        // Step 2: Deep Content Analysis
        AnalyzeCanvasContent(minigame3Canvas);
        
        // Step 3: Render Pipeline Analysis
        AnalyzeRenderPipeline(minigame3Canvas);
        
        // Step 4: Visibility Issues Analysis
        AnalyzeVisibilityIssues(minigame3Canvas);
        
        // Step 5: Provide Solutions
        ProvideSolutions(minigame3Canvas);
        
        // Step 6: Auto-fix if enabled
        if (enableAutoFix)
        {
            ApplyAutoFixes(minigame3Canvas);
        }
        
        Debug.Log("✅ ===== DEBUG COMPLETE ===== ✅");
    }

    private Canvas FindMinigame3Canvas()
    {
        // Try multiple search strategies
        Canvas foundCanvas = null;
        
        // Strategy 1: Active objects with "minigame3" in name
        Canvas[] activeCanvases = FindObjectsOfType<Canvas>();
        foreach (var canvas in activeCanvases)
        {
            if (canvas.name.ToLower().Contains("minigame3"))
            {
                Debug.Log($"🎯 Found active Minigame3 canvas: {canvas.name}");
                return canvas;
            }
        }
        
        // Strategy 2: All objects including inactive
        Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (var canvas in allCanvases)
        {
            if (string.IsNullOrEmpty(canvas.gameObject.scene.name)) continue;
            
            if (canvas.name.ToLower().Contains("minigame3"))
            {
                Debug.Log($"🎯 Found inactive Minigame3 canvas: {canvas.name}");
                return canvas;
            }
        }
        
        // Strategy 3: Search by tag
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (var obj in taggedObjects)
        {
            if (obj.name.ToLower().Contains("minigame3"))
            {
                Canvas canvas = obj.GetComponent<Canvas>();
                if (canvas != null)
                {
                    Debug.Log($"🎯 Found Minigame3 canvas by tag search: {canvas.name}");
                    return canvas;
                }
            }
        }
        
        return null;
    }

    private void SearchAllMinigame3Objects()
    {
        Debug.Log("🔍 Searching for ANY Minigame3 objects...");
        
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        List<GameObject> minigame3Objects = new List<GameObject>();
        
        foreach (var obj in allObjects)
        {
            if (string.IsNullOrEmpty(obj.scene.name)) continue;
            
            if (obj.name.ToLower().Contains("minigame") || 
                obj.name.ToLower().Contains("mini") ||
                obj.name.ToLower().Contains("game3"))
            {
                minigame3Objects.Add(obj);
            }
        }
        
        Debug.Log($"📋 Found {minigame3Objects.Count} potential Minigame3 objects:");
        foreach (var obj in minigame3Objects)
        {
            string components = "";
            Component[] comps = obj.GetComponents<Component>();
            foreach (var comp in comps)
            {
                if (comp != null) components += comp.GetType().Name + " ";
            }
            
            Debug.Log($"  📦 {obj.name} - Active: {obj.activeInHierarchy} - Components: {components}");
        }
        
        // Suggest creating a canvas if none found
        if (minigame3Objects.Count == 0)
        {
            Debug.LogError("❌ NO MINIGAME3 OBJECTS FOUND AT ALL!");
            Debug.Log("💡 SUGGESTION: Create a new Canvas named 'Minigame3Canvas'");
        }
    }

    private void AnalyzeCanvasBasics(Canvas canvas)
    {
        Debug.Log("📊 === CANVAS BASICS ===");
        
        GameObject canvasObj = canvas.gameObject;
        Debug.Log($"Canvas Name: {canvas.name}");
        Debug.Log($"GameObject Active: {canvasObj.activeInHierarchy}");
        Debug.Log($"Component Enabled: {canvas.enabled}");
        Debug.Log($"Render Mode: {canvas.renderMode}");
        Debug.Log($"Sort Order: {canvas.sortingOrder}");
        Debug.Log($"Sorting Layer: {canvas.sortingLayerName}");
        Debug.Log($"Pixel Perfect: {canvas.pixelPerfect}");
        
        // Check hierarchy path
        string hierarchyPath = GetFullHierarchyPath(canvas.transform);
        Debug.Log($"Hierarchy Path: {hierarchyPath}");
        
        // Check camera assignment
        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            if (canvas.worldCamera != null)
            {
                Debug.Log($"✅ World Camera: {canvas.worldCamera.name}");
            }
            else
            {
                Debug.LogError("❌ NO WORLD CAMERA ASSIGNED!");
            }
        }
        
        // Check canvas components
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
        
        Debug.Log($"Canvas Scaler: {(scaler != null ? "✅ Present" : "❌ Missing")}");
        Debug.Log($"Graphic Raycaster: {(raycaster != null ? "✅ Present" : "❌ Missing")}");
    }

    private void AnalyzeCanvasContent(Canvas canvas)
    {
        Debug.Log("📋 === CANVAS CONTENT ANALYSIS ===");
        
        Transform[] allChildren = canvas.GetComponentsInChildren<Transform>(true);
        Debug.Log($"Total child objects: {allChildren.Length - 1}");
        
        // Categorize objects
        List<GameObject> activeObjects = new List<GameObject>();
        List<GameObject> inactiveObjects = new List<GameObject>();
        List<GameObject> objectsWithGraphics = new List<GameObject>();
        List<GameObject> objectsWithoutGraphics = new List<GameObject>();
        List<GameObject> emptyObjects = new List<GameObject>();
        
        foreach (Transform child in allChildren)
        {
            if (child == canvas.transform) continue;
            
            GameObject obj = child.gameObject;
            
            if (obj.activeInHierarchy)
                activeObjects.Add(obj);
            else
                inactiveObjects.Add(obj);
            
            // Check for graphics components
            bool hasGraphics = obj.GetComponent<Graphic>() != null;
            if (hasGraphics)
                objectsWithGraphics.Add(obj);
            else
                objectsWithoutGraphics.Add(obj);
            
            // Check if object is essentially empty
            Component[] components = obj.GetComponents<Component>();
            if (components.Length <= 2) // Only Transform and RectTransform
                emptyObjects.Add(obj);
        }
        
        Debug.Log($"📊 BREAKDOWN:");
        Debug.Log($"  Active Objects: {activeObjects.Count}");
        Debug.Log($"  Inactive Objects: {inactiveObjects.Count}");
        Debug.Log($"  Objects with Graphics: {objectsWithGraphics.Count}");
        Debug.Log($"  Objects without Graphics: {objectsWithoutGraphics.Count}");
        Debug.Log($"  Empty Objects: {emptyObjects.Count}");
        
        // List all objects with details
        Debug.Log($"📝 DETAILED OBJECT LIST:");
        foreach (Transform child in allChildren)
        {
            if (child == canvas.transform) continue;
            
            AnalyzeSingleObject(child.gameObject, canvas.transform);
        }
        
        // Analyze graphics specifically
        AnalyzeGraphicsComponents(canvas);
    }

    private void AnalyzeSingleObject(GameObject obj, Transform canvasRoot)
    {
        string path = GetChildPath(obj.transform, canvasRoot);
        string status = obj.activeInHierarchy ? "🟢" : "🔴";
        
        // Get components summary
        Component[] components = obj.GetComponents<Component>();
        List<string> componentNames = new List<string>();
        bool hasGraphic = false;
        bool hasButton = false;
        
        foreach (var comp in components)
        {
            if (comp == null) continue;
            
            string compName = comp.GetType().Name;
            componentNames.Add(compName);
            
            if (comp is Graphic) hasGraphic = true;
            if (comp is Button) hasButton = true;
        }
        
        string compList = string.Join(", ", componentNames);
        
        // Check RectTransform
        RectTransform rt = obj.GetComponent<RectTransform>();
        Vector3 scale = rt != null ? rt.localScale : Vector3.one;
        Vector2 size = rt != null ? rt.sizeDelta : Vector2.zero;
        Vector2 position = rt != null ? rt.anchoredPosition : Vector2.zero;
        
        Debug.Log($"{status} {path}");
        Debug.Log($"    Components: {compList}");
        Debug.Log($"    Scale: {scale}, Size: {size}, Position: {position}");
        
        // Check for issues
        List<string> issues = new List<string>();
        
        if (!obj.activeInHierarchy)
            issues.Add("INACTIVE");
        
        if (scale == Vector3.zero)
            issues.Add("ZERO SCALE");
        
        if (hasButton && !hasGraphic)
            issues.Add("BUTTON WITHOUT IMAGE");
        
        if (components.Length <= 2)
            issues.Add("EMPTY OBJECT");
        
        if (hasGraphic)
        {
            Graphic graphic = obj.GetComponent<Graphic>();
            if (graphic.color.a == 0)
                issues.Add("ZERO ALPHA");
        }
        
        if (issues.Count > 0)
        {
            Debug.LogWarning($"    ⚠️ ISSUES: {string.Join(", ", issues)}");
        }
        
        // If object has children, analyze them too
        if (obj.transform.childCount > 0)
        {
            Debug.Log($"    👥 Has {obj.transform.childCount} children");
        }
    }

    private void AnalyzeGraphicsComponents(Canvas canvas)
    {
        Debug.Log("🎨 === GRAPHICS COMPONENTS ANALYSIS ===");
        
        Graphic[] allGraphics = canvas.GetComponentsInChildren<Graphic>(true);
        Debug.Log($"Total Graphics found: {allGraphics.Length}");
        
        if (allGraphics.Length == 0)
        {
            Debug.LogError("❌ NO GRAPHICS COMPONENTS FOUND!");
            Debug.Log("💡 This is why your canvas is invisible - it has no visual elements!");
            return;
        }
        
        int visibleGraphics = 0;
        int invisibleGraphics = 0;
        
        foreach (var graphic in allGraphics)
        {
            bool isVisible = graphic.gameObject.activeInHierarchy && graphic.color.a > 0;
            
            if (isVisible)
            {
                visibleGraphics++;
                Debug.Log($"✅ VISIBLE: {graphic.name} ({graphic.GetType().Name}) - Alpha: {graphic.color.a}");
            }
            else
            {
                invisibleGraphics++;
                string reason = !graphic.gameObject.activeInHierarchy ? "INACTIVE" : "ZERO ALPHA";
                Debug.LogWarning($"❌ INVISIBLE: {graphic.name} ({graphic.GetType().Name}) - Reason: {reason}");
            }
            
            // Check for culling
            CanvasRenderer renderer = graphic.GetComponent<CanvasRenderer>();
            if (renderer != null && renderer.cull)
            {
                Debug.LogWarning($"⚠️ CULLED: {graphic.name}");
            }
        }
        
        Debug.Log($"📊 GRAPHICS SUMMARY:");
        Debug.Log($"  Visible Graphics: {visibleGraphics}");
        Debug.Log($"  Invisible Graphics: {invisibleGraphics}");
        
        if (visibleGraphics == 0)
        {
            Debug.LogError("❌ NO VISIBLE GRAPHICS! Canvas will appear empty!");
        }
    }

    private void AnalyzeRenderPipeline(Canvas canvas)
    {
        Debug.Log("🖼️ === RENDER PIPELINE ANALYSIS ===");
        
        // Check Canvas Renderers
        CanvasRenderer[] renderers = canvas.GetComponentsInChildren<CanvasRenderer>(true);
        Debug.Log($"Canvas Renderers found: {renderers.Length}");
        
        int activeRenderers = 0;
        int culledRenderers = 0;
        
        foreach (var renderer in renderers)
        {
            if (renderer.gameObject.activeInHierarchy && !renderer.cull)
            {
                activeRenderers++;
            }
            else
            {
                culledRenderers++;
            }
        }
        
        Debug.Log($"Active Renderers: {activeRenderers}");
        Debug.Log($"Culled/Inactive Renderers: {culledRenderers}");
        
        // Check materials and shaders
        Graphic[] graphics = canvas.GetComponentsInChildren<Graphic>(true);
        foreach (var graphic in graphics)
        {
            if (!graphic.gameObject.activeInHierarchy) continue;
            
            Material mat = graphic.material;
            if (mat != null)
            {
                Debug.Log($"Material on {graphic.name}: {mat.name} (Shader: {mat.shader.name})");
            }
        }
    }

    private void AnalyzeVisibilityIssues(Canvas canvas)
    {
        Debug.Log("👁️ === VISIBILITY ISSUES ANALYSIS ===");
        
        // Check Canvas Groups
        CanvasGroup[] canvasGroups = canvas.GetComponentsInChildren<CanvasGroup>(true);
        if (canvasGroups.Length > 0)
        {
            Debug.Log($"Canvas Groups found: {canvasGroups.Length}");
            foreach (var group in canvasGroups)
            {
                Debug.Log($"  {group.name}: Alpha={group.alpha}, Interactable={group.interactable}");
                if (group.alpha == 0)
                {
                    Debug.LogError($"❌ ZERO ALPHA Canvas Group: {group.name}");
                }
            }
        }
        
        // Check occlusion by other canvases
        CheckCanvasOcclusion(canvas);
        
        // Check camera issues
        CheckCameraIssues(canvas);
        
        // Check screen bounds
        CheckScreenBounds(canvas);
    }

    private void CheckCanvasOcclusion(Canvas targetCanvas)
    {
        Debug.Log("🔍 Checking for canvas occlusion...");
        
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        List<Canvas> higherOrderCanvases = new List<Canvas>();
        
        foreach (var canvas in allCanvases)
        {
            if (canvas == targetCanvas) continue;
            if (!canvas.gameObject.activeInHierarchy) continue;
            
            if (canvas.sortingOrder > targetCanvas.sortingOrder)
            {
                higherOrderCanvases.Add(canvas);
            }
        }
        
        if (higherOrderCanvases.Count > 0)
        {
            Debug.LogWarning($"⚠️ {higherOrderCanvases.Count} canvases with higher sort order found:");
            foreach (var canvas in higherOrderCanvases)
            {
                Debug.LogWarning($"  - {canvas.name} (Sort Order: {canvas.sortingOrder})");
                
                // Check if it has opaque backgrounds
                Image[] images = canvas.GetComponentsInChildren<Image>();
                foreach (var img in images)
                {
                    if (img.color.a > 0.8f)
                    {
                        Debug.LogWarning($"    Potentially blocking image: {img.name} (Alpha: {img.color.a})");
                    }
                }
            }
        }
    }

    private void CheckCameraIssues(Canvas canvas)
    {
        Debug.Log("📷 Checking camera setup...");
        
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Debug.Log("✅ Render mode is Screen Space Overlay - no camera needed");
            return;
        }
        
        if (canvas.worldCamera == null)
        {
            Debug.LogError("❌ No world camera assigned!");
            
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                Debug.Log($"Main camera found: {mainCam.name}");
            }
            else
            {
                Debug.LogError("❌ No main camera in scene!");
            }
        }
        else
        {
            Camera cam = canvas.worldCamera;
            Debug.Log($"✅ World camera: {cam.name}");
            Debug.Log($"  Active: {cam.gameObject.activeInHierarchy}");
            Debug.Log($"  Enabled: {cam.enabled}");
            Debug.Log($"  Culling Mask: {cam.cullingMask}");
            Debug.Log($"  Depth: {cam.depth}");
        }
    }

    private void CheckScreenBounds(Canvas canvas)
    {
        Debug.Log("📐 Checking screen bounds...");
        
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect == null) return;
        
        Vector3[] corners = new Vector3[4];
        canvasRect.GetWorldCorners(corners);
        
        Debug.Log($"Canvas corners in world space:");
        for (int i = 0; i < corners.Length; i++)
        {
            Debug.Log($"  Corner {i}: {corners[i]}");
        }
        
        // Check if canvas is visible on screen
        Camera cam = canvas.worldCamera ?? Camera.main;
        if (cam != null)
        {
            bool anyCornerVisible = false;
            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 screenPoint = cam.WorldToScreenPoint(corners[i]);
                bool visible = screenPoint.x >= 0 && screenPoint.x <= Screen.width &&
                              screenPoint.y >= 0 && screenPoint.y <= Screen.height &&
                              screenPoint.z > 0;
                
                if (visible)
                {
                    anyCornerVisible = true;
                    break;
                }
            }
            
            Debug.Log($"Canvas visible on screen: {anyCornerVisible}");
            if (!anyCornerVisible)
            {
                Debug.LogError("❌ CANVAS IS NOT VISIBLE ON SCREEN!");
            }
        }
    }

    private void ProvideSolutions(Canvas canvas)
    {
        Debug.Log("💡 === SUGGESTED SOLUTIONS ===");
        
        // Analyze issues and provide specific solutions
        Transform[] children = canvas.GetComponentsInChildren<Transform>(true);
        Graphic[] graphics = canvas.GetComponentsInChildren<Graphic>(true);
        
        List<string> solutions = new List<string>();
        
        if (graphics.Length == 0)
        {
            solutions.Add("❗ ADD VISUAL COMPONENTS: Your canvas has no Image, Text, or other visual components!");
            solutions.Add("  → Add Image components to create backgrounds, icons, etc.");
            solutions.Add("  → Add Text components to display text");
            solutions.Add("  → Add Button components for interactive elements");
        }
        
        int inactiveCount = 0;
        int zeroAlphaCount = 0;
        int zeroScaleCount = 0;
        
        foreach (var graphic in graphics)
        {
            if (!graphic.gameObject.activeInHierarchy) inactiveCount++;
            if (graphic.color.a == 0) zeroAlphaCount++;
            
            RectTransform rt = graphic.GetComponent<RectTransform>();
            if (rt != null && rt.localScale == Vector3.zero) zeroScaleCount++;
        }
        
        if (inactiveCount > 0)
        {
            solutions.Add($"❗ ACTIVATE OBJECTS: {inactiveCount} graphics are inactive");
            solutions.Add("  → Set gameObject.SetActive(true) on inactive UI elements");
        }
        
        if (zeroAlphaCount > 0)
        {
            solutions.Add($"❗ FIX TRANSPARENCY: {zeroAlphaCount} graphics have zero alpha");
            solutions.Add("  → Set graphic.color with alpha > 0 (e.g., Color.white)");
        }
        
        if (zeroScaleCount > 0)
        {
            solutions.Add($"❗ FIX SCALE: {zeroScaleCount} objects have zero scale");
            solutions.Add("  → Set transform.localScale = Vector3.one");
        }
        
        // Check canvas setup issues
        if (canvas.sortingOrder < 0)
        {
            solutions.Add("❗ INCREASE SORT ORDER: Canvas sort order is negative");
            solutions.Add("  → Set canvas.sortingOrder to a positive value (e.g., 100)");
        }
        
        if (!canvas.gameObject.activeInHierarchy)
        {
            solutions.Add("❗ ACTIVATE CANVAS: Canvas GameObject is inactive");
            solutions.Add("  → Set canvas.gameObject.SetActive(true)");
        }
        
        if (!canvas.enabled)
        {
            solutions.Add("❗ ENABLE CANVAS: Canvas component is disabled");
            solutions.Add("  → Set canvas.enabled = true");
        }
        
        if (solutions.Count == 0)
        {
            solutions.Add("✅ No obvious issues found - canvas should be visible!");
            solutions.Add("💡 If still not visible, try:");
            solutions.Add("  → Check if canvas is behind other UI elements");
            solutions.Add("  → Verify camera setup");
            solutions.Add("  → Look for Canvas Groups with zero alpha");
        }
        
        foreach (string solution in solutions)
        {
            Debug.Log(solution);
        }
    }

    private void ApplyAutoFixes(Canvas canvas)
    {
        Debug.Log("🔧 === APPLYING AUTO FIXES ===");
        
        int fixesApplied = 0;
        
        // Fix 1: Activate canvas and parents
        Transform current = canvas.transform;
        while (current != null)
        {
            if (!current.gameObject.activeInHierarchy)
            {
                current.gameObject.SetActive(true);
                Debug.Log($"🔧 Activated: {current.name}");
                fixesApplied++;
            }
            current = current.parent;
        }
        
        // Fix 2: Enable canvas component
        if (!canvas.enabled)
        {
            canvas.enabled = true;
            Debug.Log("🔧 Enabled canvas component");
            fixesApplied++;
        }
        
        // Fix 3: Set reasonable sort order
        if (canvas.sortingOrder < 10)
        {
            canvas.sortingOrder = 100;
            Debug.Log("🔧 Set canvas sort order to 100");
            fixesApplied++;
        }
        
        // Fix 4: Add missing components
        if (canvas.GetComponent<CanvasScaler>() == null)
        {
            CanvasScaler scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            Debug.Log("🔧 Added CanvasScaler");
            fixesApplied++;
        }
        
        if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
            Debug.Log("🔧 Added GraphicRaycaster");
            fixesApplied++;
        }
        
        // Fix 5: Fix graphics issues
        Graphic[] graphics = canvas.GetComponentsInChildren<Graphic>(true);
        foreach (var graphic in graphics)
        {
            // Fix zero alpha
            if (graphic.color.a == 0)
            {
                Color color = graphic.color;
                color.a = 1f;
                graphic.color = color;
                Debug.Log($"🔧 Fixed zero alpha on: {graphic.name}");
                fixesApplied++;
            }
            
            // Activate inactive graphics
            if (!graphic.gameObject.activeInHierarchy)
            {
                graphic.gameObject.SetActive(true);
                Debug.Log($"🔧 Activated: {graphic.name}");
                fixesApplied++;
            }
            
            // Fix zero scale
            RectTransform rt = graphic.GetComponent<RectTransform>();
            if (rt != null && rt.localScale == Vector3.zero)
            {
                rt.localScale = Vector3.one;
                Debug.Log($"🔧 Fixed zero scale on: {graphic.name}");
                fixesApplied++;
            }
        }
        
        // Fix 6: Assign camera if needed
        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvas.worldCamera == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                canvas.worldCamera = mainCam;
                Debug.Log("🔧 Assigned main camera to canvas");
                fixesApplied++;
            }
        }
        
        Debug.Log($"✅ Applied {fixesApplied} fixes");
        
        // Force canvas update
        Canvas.ForceUpdateCanvases();
    }

    [ContextMenu("Create Test Content")]
    public void CreateTestContent()
    {
        Debug.Log("🎨 === CREATING TEST CONTENT ===");
        
        Canvas canvas = FindMinigame3Canvas();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }
        
        // Create a test panel
        GameObject testPanel = new GameObject("TestPanel");
        testPanel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = testPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.25f, 0.25f);
        panelRect.anchorMax = new Vector2(0.75f, 0.75f);
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;
        
        Image panelImage = testPanel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0.5f, 1f, 0.8f); // Blue semi-transparent
        
        // Create test text
        GameObject testText = new GameObject("TestText");
        testText.transform.SetParent(testPanel.transform, false);
        
        RectTransform textRect = testText.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        Text text = testText.AddComponent<Text>();
        text.text = "MINIGAME3 IS NOW VISIBLE!\n\nThis test content proves your canvas is working.";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        
        // Create test button
        GameObject testButton = new GameObject("TestButton");
        testButton.transform.SetParent(testPanel.transform, false);
        
        RectTransform buttonRect = testButton.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.3f, 0.1f);
        buttonRect.anchorMax = new Vector2(0.7f, 0.3f);
        buttonRect.sizeDelta = Vector2.zero;
        buttonRect.anchoredPosition = Vector2.zero;
        
        Image buttonImage = testButton.AddComponent<Image>();
        buttonImage.color = new Color(0f, 0.8f, 0f, 1f); // Green
        
        Button button = testButton.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        
        // Create button text
        GameObject buttonText = new GameObject("ButtonText");
        buttonText.transform.SetParent(testButton.transform, false);
        
        RectTransform buttonTextRect = buttonText.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;
        buttonTextRect.anchoredPosition = Vector2.zero;
        
        Text buttonTextComp = buttonText.AddComponent<Text>();
        buttonTextComp.text = "TEST BUTTON";
        buttonTextComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonTextComp.fontSize = 18;
        buttonTextComp.color = Color.white;
        buttonTextComp.alignment = TextAnchor.MiddleCenter;
        
        // Add button functionality
        button.onClick.AddListener(() => {
            Debug.Log("🎉 TEST BUTTON CLICKED! Canvas is fully functional!");
            text.text = "BUTTON CLICKED!\n\nCanvas is working perfectly!";
        });
        
        Debug.Log("✅ Test content created successfully!");
        Debug.Log("If you can see a blue panel with text and a green button, your canvas is working!");
    }

    [ContextMenu("Emergency Canvas Creation")]
    public void EmergencyCanvasCreation()
    {
        Debug.Log("🚨 === EMERGENCY CANVAS CREATION ===");
        
        // Create a new Minigame3 canvas from scratch
        GameObject canvasObj = new GameObject("Minigame3Canvas_Emergency");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
        
        Debug.Log("✅ Emergency canvas created with proper setup");
        
        // Add immediate test content
        CreateTestContent();
        
        Debug.Log("🎯 Emergency canvas should now be visible with test content!");
    }

    [ContextMenu("Force Canvas to Front")]
    public void ForceCanvasToFront()
    {
        Canvas canvas = FindMinigame3Canvas();
        if (canvas == null) return;
        
        // Set highest sort order
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        int highestOrder = 0;
        foreach (var c in allCanvases)
        {
            if (c.sortingOrder > highestOrder)
                highestOrder = c.sortingOrder;
        }
        
        canvas.sortingOrder = highestOrder + 100;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        Debug.Log($"🚀 Forced canvas to front with sort order: {canvas.sortingOrder}");
    }

    [ContextMenu("Analyze All Scene Canvases")]
    public void AnalyzeAllSceneCanvases()
    {
        Debug.Log("🔍 === ALL SCENE CANVASES ===");
        
        Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
        List<Canvas> sceneCanvases = new List<Canvas>();
        
        foreach (var canvas in allCanvases)
        {
            if (!string.IsNullOrEmpty(canvas.gameObject.scene.name))
            {
                sceneCanvases.Add(canvas);
            }
        }
        
        // Sort by sort order
        sceneCanvases.Sort((a, b) => a.sortingOrder.CompareTo(b.sortingOrder));
        
        Debug.Log($"Found {sceneCanvases.Count} canvases in scene:");
        
        for (int i = 0; i < sceneCanvases.Count; i++)
        {
            Canvas canvas = sceneCanvases[i];
            string activeStatus = canvas.gameObject.activeInHierarchy ? "🟢" : "🔴";
            string enabledStatus = canvas.enabled ? "✅" : "❌";
            
            // Count graphics
            Graphic[] graphics = canvas.GetComponentsInChildren<Graphic>(true);
            int visibleGraphics = 0;
            foreach (var g in graphics)
            {
                if (g.gameObject.activeInHierarchy && g.color.a > 0) visibleGraphics++;
            }
            
            Debug.Log($"{i + 1}. {activeStatus}{enabledStatus} '{canvas.name}'");
            Debug.Log($"   Sort Order: {canvas.sortingOrder}, Render Mode: {canvas.renderMode}");
            Debug.Log($"   Graphics: {graphics.Length} total, {visibleGraphics} visible");
            Debug.Log($"   Hierarchy: {GetFullHierarchyPath(canvas.transform)}");
            
            if (canvas.name.ToLower().Contains("minigame3"))
            {
                Debug.Log($"   🎯 ← THIS IS YOUR MINIGAME3 CANVAS!");
            }
            
            Debug.Log(""); // Empty line for readability
        }
    }

    [ContextMenu("Quick Visual Test")]
    public void QuickVisualTest()
    {
        Canvas canvas = FindMinigame3Canvas();
        if (canvas == null)
        {
            Debug.LogError("No Minigame3 canvas found for visual test!");
            return;
        }
        
        Debug.Log("🎨 === QUICK VISUAL TEST ===");
        
        // Clear any existing test objects
        Transform[] children = canvas.GetComponentsInChildren<Transform>();
        foreach (var child in children)
        {
            if (child.name.StartsWith("QuickTest"))
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
        
        // Create a simple full-screen colored panel
        GameObject testOverlay = new GameObject("QuickTestOverlay");
        testOverlay.transform.SetParent(canvas.transform, false);
        
        RectTransform overlayRect = testOverlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;
        overlayRect.anchoredPosition = Vector2.zero;
        
        Image overlayImage = testOverlay.AddComponent<Image>();
        overlayImage.color = new Color(1f, 0f, 1f, 0.5f); // Bright magenta, semi-transparent
        
        // Add large text
        GameObject testText = new GameObject("QuickTestText");
        testText.transform.SetParent(testOverlay.transform, false);
        
        RectTransform textRect = testText.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        Text text = testText.AddComponent<Text>();
        text.text = "MINIGAME3 CANVAS\nVISUAL TEST\n\nIf you can see this bright magenta overlay,\nyour canvas is working!";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 48;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontStyle = FontStyle.Bold;
        
        // Force to very high sort order
        canvas.sortingOrder = 9999;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        Debug.Log("🎯 Created bright magenta overlay test!");
        Debug.Log("If you can see it, canvas is working. If not, there's a fundamental issue.");
        
        // Auto-remove after 5 seconds if in play mode
        if (Application.isPlaying)
        {
            Invoke(nameof(RemoveQuickTest), 5f);
        }
    }

    private void RemoveQuickTest()
    {
        Canvas canvas = FindMinigame3Canvas();
        if (canvas == null) return;
        
        Transform[] children = canvas.GetComponentsInChildren<Transform>();
        foreach (var child in children)
        {
            if (child.name.StartsWith("QuickTest"))
            {
                Destroy(child.gameObject);
            }
        }
        
        Debug.Log("🧹 Removed quick test overlay");
    }

    // Helper methods
    private string GetChildPath(Transform child, Transform root)
    {
        if (child == root) return "";
        
        string path = child.name;
        Transform current = child.parent;
        
        while (current != null && current != root)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        
        return path;
    }

    private string GetFullHierarchyPath(Transform transform)
    {
        string path = transform.name;
        Transform current = transform.parent;
        
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        
        return path;
    }

    [ContextMenu("Generate Detailed Report")]
    public void GenerateDetailedReport()
    {
        Debug.Log("📋 === DETAILED MINIGAME3 REPORT ===");
        
        Canvas canvas = FindMinigame3Canvas();
        if (canvas == null)
        {
            Debug.LogError("❌ CRITICAL: No Minigame3 canvas found!");
            Debug.Log("🔧 SOLUTION: Use 'Emergency Canvas Creation' to create one");
            return;
        }
        
        // Generate comprehensive report
        System.Text.StringBuilder report = new System.Text.StringBuilder();
        
        report.AppendLine("=== MINIGAME3 CANVAS DIAGNOSTIC REPORT ===");
        report.AppendLine($"Generated at: {System.DateTime.Now}");
        report.AppendLine("");
        
        // Canvas basics
        report.AppendLine("CANVAS INFORMATION:");
        report.AppendLine($"  Name: {canvas.name}");
        report.AppendLine($"  Active: {canvas.gameObject.activeInHierarchy}");
        report.AppendLine($"  Enabled: {canvas.enabled}");
        report.AppendLine($"  Render Mode: {canvas.renderMode}");
        report.AppendLine($"  Sort Order: {canvas.sortingOrder}");
        report.AppendLine($"  Hierarchy Path: {GetFullHierarchyPath(canvas.transform)}");
        report.AppendLine("");
        
        // Content analysis
        Transform[] children = canvas.GetComponentsInChildren<Transform>(true);
        Graphic[] graphics = canvas.GetComponentsInChildren<Graphic>(true);
        
        report.AppendLine("CONTENT ANALYSIS:");
        report.AppendLine($"  Total Child Objects: {children.Length - 1}");
        report.AppendLine($"  Graphics Components: {graphics.Length}");
        
        int activeGraphics = 0;
        int visibleGraphics = 0;
        
        foreach (var graphic in graphics)
        {
            if (graphic.gameObject.activeInHierarchy) activeGraphics++;
            if (graphic.gameObject.activeInHierarchy && graphic.color.a > 0) visibleGraphics++;
        }
        
        report.AppendLine($"  Active Graphics: {activeGraphics}");
        report.AppendLine($"  Visible Graphics: {visibleGraphics}");
        report.AppendLine("");
        
        // Issues identification
        report.AppendLine("IDENTIFIED ISSUES:");
        List<string> issues = new List<string>();
        
        if (!canvas.gameObject.activeInHierarchy)
            issues.Add("Canvas GameObject is inactive");
        
        if (!canvas.enabled)
            issues.Add("Canvas component is disabled");
        
        if (graphics.Length == 0)
            issues.Add("No graphics components found - canvas will be invisible");
        
        if (visibleGraphics == 0 && graphics.Length > 0)
            issues.Add("All graphics are either inactive or transparent");
        
        if (canvas.sortingOrder < 0)
            issues.Add("Canvas sort order is negative");
        
        if (issues.Count == 0)
        {
            report.AppendLine("  ✅ No major issues detected");
        }
        else
        {
            foreach (string issue in issues)
            {
                report.AppendLine($"  ❌ {issue}");
            }
        }
        
        report.AppendLine("");
        
        // Recommendations
        report.AppendLine("RECOMMENDATIONS:");
        if (graphics.Length == 0)
        {
            report.AppendLine("  1. Add Image, Text, or Button components to make canvas visible");
            report.AppendLine("  2. Use 'Create Test Content' to add sample UI elements");
        }
        if (visibleGraphics == 0 && graphics.Length > 0)
        {
            report.AppendLine("  1. Activate inactive UI objects");
            report.AppendLine("  2. Set alpha values > 0 on transparent graphics");
        }
        if (canvas.sortingOrder < 100)
        {
            report.AppendLine("  1. Increase canvas sort order to ensure it appears on top");
        }
        
        report.AppendLine("");
        report.AppendLine("=== END REPORT ===");
        
        Debug.Log(report.ToString());
    }

    [ContextMenu("Reset Canvas to Defaults")]
    public void ResetCanvasToDefaults()
    {
        Canvas canvas = FindMinigame3Canvas();
        if (canvas == null) return;
        
        Debug.Log("🔄 === RESETTING CANVAS TO DEFAULTS ===");
        
        // Reset canvas settings
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        canvas.pixelPerfect = false;
        canvas.enabled = true;
        canvas.gameObject.SetActive(true);
        
        // Reset Canvas Scaler
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;
        }
        
        // Reset all RectTransforms
        RectTransform[] rectTransforms = canvas.GetComponentsInChildren<RectTransform>(true);
        foreach (var rt in rectTransforms)
        {
            if (rt == canvas.transform) continue; // Skip canvas itself
            
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
        }
        
        // Reset all Graphics alpha
        Graphic[] allGraphics = canvas.GetComponentsInChildren<Graphic>(true);
        foreach (var graphic in allGraphics)
        {
            Color color = graphic.color;
            if (color.a == 0) color.a = 1f;
            graphic.color = color;
        }
        
        Debug.Log("✅ Canvas reset to default settings");
    }
}