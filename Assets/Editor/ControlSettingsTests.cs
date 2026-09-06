using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class ControlSettingsTests
{
    [Test]
    public void PersistsSettings()
    {
        string[] keys = { "ControlMode", "MasterVolume", "Language" };
        bool[] hadValues = { PlayerPrefs.HasKey(keys[0]), PlayerPrefs.HasKey(keys[1]), PlayerPrefs.HasKey(keys[2]) };
        int previousControl = PlayerPrefs.GetInt(keys[0]);
        float previousVolume = PlayerPrefs.GetFloat(keys[1]);
        int previousLanguage = PlayerPrefs.GetInt(keys[2]);

        try
        {
            PlayerPrefs.DeleteKey(keys[2]);
            Assert.AreEqual(UiLanguage.English, ControlSettings.Language);

            ControlSettings.Set(ControlMode.TapToMove);
            Assert.AreEqual(ControlMode.TapToMove, ControlSettings.Current);

            ControlSettings.Set(ControlMode.Joystick);
            Assert.AreEqual(ControlMode.Joystick, ControlSettings.Current);

            ControlSettings.SetVolume(0.35f);
            Assert.AreEqual(0.35f, ControlSettings.Volume, 0.001f);

            ControlSettings.SetLanguage(UiLanguage.English);
            Assert.AreEqual(UiLanguage.English, ControlSettings.Language);
        }
        finally
        {
            if (hadValues[0]) PlayerPrefs.SetInt(keys[0], previousControl); else PlayerPrefs.DeleteKey(keys[0]);
            if (hadValues[1]) PlayerPrefs.SetFloat(keys[1], previousVolume); else PlayerPrefs.DeleteKey(keys[1]);
            if (hadValues[2]) PlayerPrefs.SetInt(keys[2], previousLanguage); else PlayerPrefs.DeleteKey(keys[2]);
            AudioListener.volume = hadValues[1] ? previousVolume : 1f;
            PlayerPrefs.Save();
        }
    }

    [Test]
    public void TapMovementSkipsReachedWaypoints()
    {
        GameObject playerObject = new GameObject("TapMovementTest");
        try
        {
            PlayerController controller = playerObject.AddComponent<PlayerController>();
            CapsuleCollider2D navigationCollider = playerObject.AddComponent<CapsuleCollider2D>();
            navigationCollider.offset = Vector2.down;
            typeof(PlayerController).GetField("navigationCollider", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(controller, navigationCollider);
            typeof(PlayerController).GetField("path", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(controller, new List<Vector3> {
                    Vector3.down, Vector3.down + Vector3.right * 0.1f, Vector3.down + Vector3.right
                });

            Vector2 direction = (Vector2)typeof(PlayerController)
                .GetMethod("FollowPath", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(controller, null);

            Assert.AreEqual(Vector2.right, direction);
            Assert.IsNotNull(playerObject.GetComponent<Pathfinding.Seeker>());
        }
        finally
        {
            Object.DestroyImmediate(playerObject);
        }
    }
}
