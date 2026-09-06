using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class GameSaveDataTests
{
    [Test]
    public void JsonRoundTripPreservesProgress()
    {
        var original = new GameSaveData
        {
            selectedCharacter = 2,
            currentScene = "Map3",
            checkpointId = "after-dialog",
            hasPlayerPosition = true,
            playerX = 12.5f,
            playerY = -3f,
            playerZ = 1f,
            activeObjectiveId = "objective-active",
            activeObjectiveCount = 3
        };
        original.completedObjectives.Add("objective-1");
        original.completedMinigames.Add("InstagramLive");

        GameSaveData restored = JsonUtility.FromJson<GameSaveData>(JsonUtility.ToJson(original));

        Assert.That(restored.IsValid(), Is.True);
        Assert.That(restored.selectedCharacter, Is.EqualTo(2));
        Assert.That(restored.currentScene, Is.EqualTo("Map3"));
        Assert.That(restored.checkpointId, Is.EqualTo("after-dialog"));
        Assert.That(restored.hasPlayerPosition, Is.True);
        Assert.That(restored.playerX, Is.EqualTo(12.5f));
        Assert.That(restored.activeObjectiveId, Is.EqualTo("objective-active"));
        Assert.That(restored.activeObjectiveCount, Is.EqualTo(3));
        CollectionAssert.Contains(restored.completedObjectives, "objective-1");
        CollectionAssert.Contains(restored.completedMinigames, "InstagramLive");
    }

    [Test]
    public void InvalidVersionIsRejected()
    {
        var data = new GameSaveData { version = GameSaveData.CurrentVersion + 1 };

        Assert.That(data.IsValid(), Is.False);
    }

    [Test]
    public void VersionOneSaveUpgradesToCurrentVersion()
    {
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(
            "{\"version\":1,\"selectedCharacter\":1,\"currentScene\":\"Map2\"}");

        Assert.That(data.TryUpgrade(), Is.True);
        Assert.That(data.version, Is.EqualTo(GameSaveData.CurrentVersion));
        Assert.That(data.hasPlayerPosition, Is.False);
        Assert.That(data.completedObjectives, Is.Not.Null);
        Assert.That(data.completedMinigames, Is.Not.Null);
    }

    [Test]
    public void InvalidPlayerPositionIsRejected()
    {
        var data = new GameSaveData { hasPlayerPosition = true, playerX = float.NaN };

        Assert.That(data.IsValid(), Is.False);
    }

    [Test]
    public void BuildStartsAtTitleScene()
    {
        EditorBuildSettingsScene firstEnabled = System.Array.Find(
            EditorBuildSettings.scenes, scene => scene.enabled);

        Assert.That(firstEnabled, Is.Not.Null);
        Assert.That(firstEnabled.path, Is.EqualTo("Assets/Scenes/TitleScene.unity"));
    }
}
