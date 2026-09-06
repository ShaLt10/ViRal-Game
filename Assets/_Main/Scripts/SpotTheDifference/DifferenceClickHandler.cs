using UnityEngine;

public class DifferenceClickHandler : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject correctCirclePrefab;  // SpriteRenderer lingkaran hijau
    public GameObject wrongFlashPrefab;     // SpriteRenderer merah, auto-destroy

    private LevelData levelData;
    private bool[] found;
    private bool isEnabled;
    private Camera cam;

    void Awake() => cam = Camera.main;

    public void InitLevel(LevelData data)
    {
        levelData = data;
        found = new bool[data.differencePositions.Length];
        isEnabled = true;
    }

    public void SetEnabled(bool value) => isEnabled = value;

    void Update()
    {
        if (!isEnabled || levelData == null) return;

        bool clicked = Input.GetMouseButtonDown(0) ||
                       (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
        if (!clicked) return;

        Vector3 screenPos = Input.touchCount > 0
            ? (Vector3)Input.GetTouch(0).position
            : Input.mousePosition;
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z));

        CheckClick(world);
    }

    void CheckClick(Vector3 worldPos)
    {
        for (int i = 0; i < levelData.differencePositions.Length; i++)
        {
            if (found[i]) continue;
            float dist = Vector2.Distance(worldPos, levelData.differencePositions[i]);
            if (dist <= levelData.circleRadius)
            {
                found[i] = true;
                Instantiate(correctCirclePrefab, levelData.differencePositions[i], Quaternion.identity);
                SpotDifferenceManager.Instance.RegisterCorrectClick(i);
                return;
            }
        }
        // Tidak kena perbedaan → wrong click
        SpotDifferenceManager.Instance.RegisterWrongClick(worldPos);
        var fx = Instantiate(wrongFlashPrefab, worldPos, Quaternion.identity);
        Destroy(fx, 0.6f);
    }
}