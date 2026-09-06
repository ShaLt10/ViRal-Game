using UnityEngine;
using MoreMountains.TopDownEngine;

/// <summary>
/// Enhanced camera follow system with TDE integration, bounds, and mobile optimization
/// Attach to Main Camera
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("Target to follow. Leave empty for auto-detect player.")]
    public Transform target;
    
    [Tooltip("Camera offset from target")]
    public Vector3 offset = new Vector3(0, 0, -10);
    
    [Tooltip("Higher = smoother but slower. Lower = snappier but jittery.")]
    [Range(1f, 20f)]
    public float smoothSpeed = 5f;

    [Header("Auto Detection")]
    [Tooltip("Automatically find and follow the active player")]
    public bool autoDetectPlayer = true;
    
    [Tooltip("Re-check for player every X seconds if lost")]
    public float redetectInterval = 1f;

    [Header("Camera Bounds")]
    [Tooltip("Constrain camera within a bounded area")]
    public bool useBounds = false;
    
    [Tooltip("Collider2D that defines camera boundaries")]
    public Collider2D boundingBox;

    [Header("Mobile Optimization")]
    [Tooltip("Reduce update rate on mobile for better performance")]
    public bool mobileOptimization = true;
    
    [Tooltip("Update camera every N frames on mobile (1 = every frame)")]
    [Range(1, 3)]
    public int mobileUpdateInterval = 1;

    // Private variables
    private Camera cam;
    private float minX, maxX, minY, maxY;
    private float redetectTimer = 0f;
    private int frameCount = 0;
    private bool isMobile;

    void Start()
    {
        cam = GetComponent<Camera>();
        isMobile = Application.platform == RuntimePlatform.Android;

        // Auto-detect player on start
        if (autoDetectPlayer && target == null)
        {
            FindPlayer();
        }

        // Setup camera bounds
        if (useBounds && boundingBox != null)
        {
            SetupBounds();
        }
    }

    void FindPlayer()
    {
        // Find TDE Character component with Player type
        var characters = FindObjectsOfType<Character>();
        foreach (var character in characters)
        {
            if (character.CharacterType == Character.CharacterTypes.Player)
            {
                target = character.transform;
                Debug.Log("[CameraFollow] Auto-detected player: " + target.name);
                return;
            }
        }

        // Fallback: Find by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
            Debug.Log("[CameraFollow] Found player by tag: " + target.name);
        }
        else
        {
            Debug.LogWarning("[CameraFollow] No player found!");
        }
    }

    void SetupBounds()
    {
        if (cam.orthographic)
        {
            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            minX = boundingBox.bounds.min.x + camWidth;
            maxX = boundingBox.bounds.max.x - camWidth;
            minY = boundingBox.bounds.min.y + camHeight;
            maxY = boundingBox.bounds.max.y - camHeight;

            Debug.Log($"[CameraFollow] Bounds set: X({minX} to {maxX}), Y({minY} to {maxY})");
        }
        else
        {
            Debug.LogWarning("[CameraFollow] Bounds only work with Orthographic camera!");
        }
    }

    void LateUpdate()
    {
        // Mobile optimization: Skip frames
        if (isMobile && mobileOptimization)
        {
            frameCount++;
            if (frameCount < mobileUpdateInterval)
            {
                return;
            }
            frameCount = 0;
        }

        // Re-detect player if lost
        if (target == null)
        {
            if (autoDetectPlayer)
            {
                redetectTimer += Time.deltaTime;
                if (redetectTimer >= redetectInterval)
                {
                    FindPlayer();
                    redetectTimer = 0f;
                }
            }
            return;
        }

        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;
        
        // Smooth movement
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position, 
            desiredPosition, 
            smoothSpeed * Time.deltaTime
        );

        // Apply bounds if enabled
        if (useBounds && boundingBox != null)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX, maxX);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minY, maxY);
        }

        // Apply position
        transform.position = smoothedPosition;
    }

    /// <summary>
    /// Manually set camera target (called from MCSelector)
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        if (newTarget != null)
        {
            target = newTarget;
            Debug.Log("[CameraFollow] Target changed to: " + newTarget.name);
        }
    }

    /// <summary>
    /// Instantly snap camera to target (no smooth)
    /// </summary>
    public void SnapToTarget()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    /// <summary>
    /// Update bounds dynamically (e.g., when changing scenes)
    /// </summary>
    public void UpdateBounds(Collider2D newBounds)
    {
        boundingBox = newBounds;
        if (useBounds && boundingBox != null)
        {
            SetupBounds();
        }
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (useBounds && boundingBox != null && cam != null && cam.orthographic)
        {
            Gizmos.color = Color.yellow;
            
            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            float minX = boundingBox.bounds.min.x + camWidth;
            float maxX = boundingBox.bounds.max.x - camWidth;
            float minY = boundingBox.bounds.min.y + camHeight;
            float maxY = boundingBox.bounds.max.y - camHeight;

            // Draw bounds rectangle
            Vector3 bottomLeft = new Vector3(minX, minY, 0);
            Vector3 bottomRight = new Vector3(maxX, minY, 0);
            Vector3 topRight = new Vector3(maxX, maxY, 0);
            Vector3 topLeft = new Vector3(minX, maxY, 0);

            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
        }
    }
}