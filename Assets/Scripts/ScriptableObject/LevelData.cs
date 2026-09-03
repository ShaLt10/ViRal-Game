// Assets/Scripts/SpotTheDifference/LevelData.cs
using UnityEngine;

[CreateAssetMenu(
    fileName = "LevelData",
    menuName  = "SpotDiff/LevelData")]   // ← ini yang munculkan menu Create
public class LevelData : ScriptableObject
{
    public Sprite  leftImage;
    public Sprite  rightImage;
    public Vector2[] differencePositions; // 3 titik, world space
    public float   circleRadius = 0.5f;
}