using System.Collections.Generic;
using Game.Utility;
using Pathfinding;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PlayerInteract), typeof(Seeker))]
public class PlayerController : MonoBehaviour
{
    public Analog joystick;
    public float moveSpeed = 5f;
    public bool is3D = false; // Set true if 3D
    [SerializeField]
    private Animator animator;
    [SerializeField]
    Transform graph;
    [SerializeField]
    private Rigidbody2D rb;
    private static int Movex = Animator.StringToHash("MoveX");
    private static int Movey = Animator.StringToHash("MoveY");
    private static int Speed = Animator.StringToHash("Speed");
    private static int Facex = Animator.StringToHash("FaceX");
    private static int Facey = Animator.StringToHash("FaceY");
    private PlayerInteract interact;
    private Seeker seeker;
    private Collider2D navigationCollider;
    private List<Vector3> path;
    private int waypointIndex;
    private ControlMode controlMode;

    [SerializeField] private float waypointDistance = 0.15f;

    [SerializeField]
    RuntimeAnimatorController Raline;

    [SerializeField]
    RuntimeAnimatorController Gavi;

    private void OnEnable() => ControlSettings.ControlModeChanged += ApplyControlMode;

    private void OnDisable() => ControlSettings.ControlModeChanged -= ApplyControlMode;
    
    private void Start()
    {
        interact = GetComponent<PlayerInteract>();
        seeker = GetComponent<Seeker>();
        navigationCollider = GetComponent<Collider2D>();
        if (AstarPath.active != null && AstarPath.active.data != null
            && AstarPath.active.data.gridGraph != null)
        {
            GridGraph grid = AstarPath.active.data.gridGraph;
            grid.collision.mask = Physics2D.GetLayerCollisionMask(gameObject.layer) & ~(1 << gameObject.layer);
            if (navigationCollider != null)
                grid.collision.diameter = Mathf.Max(navigationCollider.bounds.size.x,
                    navigationCollider.bounds.size.y) / grid.nodeSize;
            grid.collision.Initialize(grid.transform, grid.nodeSize);
        }
        ApplyControlMode(ControlSettings.Current);
        
        // FIXED: Gunakan CharacterManager singleton yang sudah ada
        if (CharacterManager.Instance != null)
        {
            if (CharacterManager.Instance.GetPlayerName() == StringContainer.Raline)
            {
                animator.runtimeAnimatorController = Raline;
            }
            else
            {
                animator.runtimeAnimatorController = Gavi;
            }
        }
        else
        {
            Debug.LogWarning("CharacterManager.Instance is null! Using default Raline animator.");
            animator.runtimeAnimatorController = Raline;
        }
    }

    void Update()
    {
        if (joystick != null && !joystick.isActiveAndEnabled)
            joystick.ApplySafeArea();

        if (controlMode == ControlMode.TapToMove)
            HandleTap();

        Vector2 input = controlMode == ControlMode.Joystick
            ? (joystick != null ? joystick.Direction : Vector2.zero)
            : FollowPath();
        Vector3 move = is3D ? new Vector3(input.x, 0, input.y) : new Vector3(input.x, input.y, 0);
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
        animator.SetFloat(Movex, move.x);
        animator.SetFloat(Movey, move.y);
        animator.SetFloat(Speed, Vector3.Magnitude(move)); 

        if (move.sqrMagnitude > 0.001f)
        {
            ResetFace();
            if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
            {
                float x = Mathf.Sign(move.x);
                interact.SetFace(x < 0 ? Face.Left : Face.Right);
                animator.SetFloat(Facex, x);
                graph.rotation = Quaternion.Euler(0, x < 0 ? 0 : 180, 0);
            }
            else
            {
                float y = Mathf.Sign(move.y);
                interact.SetFace(y < 0 ? Face.Down : Face.Up);
                animator.SetFloat(Facey, y);
            }
        }
    }

    private void HandleTap()
    {
        Vector2 screenPosition;
        int pointerId;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);
            screenPosition = touch.position;
            pointerId = touch.fingerId;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            screenPosition = Input.mousePosition;
            pointerId = -1;
        }
        else
        {
            return;
        }

        if (EventSystem.current != null && (pointerId < 0
                ? EventSystem.current.IsPointerOverGameObject()
                : EventSystem.current.IsPointerOverGameObject(pointerId)))
            return;

        Camera camera = Camera.main;
        if (camera == null || seeker == null || AstarPath.active == null)
            return;
        // ponytail: ignore taps while A* is busy; queue the latest tap only if single taps are ever lost.
        if (!seeker.IsDone())
            return;

        Vector3 target = camera.ScreenToWorldPoint(screenPosition);
        target.z = transform.position.z;
        Vector3 navigationPosition = NavigationPosition;
        Vector3 navigationTarget = target + (navigationPosition - transform.position);
        // ponytail: refresh on tap; add periodic replanning only for obstacles that move mid-route.
        Physics2D.SyncTransforms();
        Bounds updateBounds = new Bounds((navigationPosition + navigationTarget) * 0.5f,
            new Vector3(Mathf.Abs(navigationTarget.x - navigationPosition.x),
                Mathf.Abs(navigationTarget.y - navigationPosition.y), 1f));
        updateBounds.Expand(2f);
        AstarPath.active.UpdateGraphs(updateBounds);
        AstarPath.active.FlushGraphUpdates();
        seeker.StartPath(navigationPosition, navigationTarget, OnPathComplete);
    }

    private void OnPathComplete(Pathfinding.Path result)
    {
        if (result.error)
            return;

        path = result.vectorPath;
        waypointIndex = 0;
    }

    private Vector2 FollowPath()
    {
        if (path == null)
            return Vector2.zero;

        Vector3 navigationPosition = NavigationPosition;
        float reachDistance = Mathf.Max(waypointDistance, moveSpeed * Time.deltaTime);
        while (waypointIndex < path.Count
               && Vector2.Distance(navigationPosition, path[waypointIndex]) <= reachDistance)
            waypointIndex++;

        if (waypointIndex >= path.Count)
        {
            path = null;
            return Vector2.zero;
        }

        return ((Vector2)(path[waypointIndex] - navigationPosition)).normalized;
    }

    private Vector3 NavigationPosition => navigationCollider != null
        ? navigationCollider.bounds.center
        : transform.position;

    private void ApplyControlMode(ControlMode mode)
    {
        controlMode = mode;
        path = null;
        if (joystick != null && joystick.background != null)
            joystick.background.gameObject.SetActive(mode == ControlMode.Joystick);
    }

    private void ResetFace()
    {
        animator.SetFloat(Facex, 0);
        animator.SetFloat(Facey, 0);
    }
}
