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
        ControlMode? notifiedMode = null;
        System.Action<ControlMode> listener = mode => notifiedMode = mode;
        ControlSettings.ControlModeChanged += listener;

        try
        {
            PlayerPrefs.DeleteKey(keys[2]);
            Assert.AreEqual(UiLanguage.English, ControlSettings.Language);

            ControlSettings.Set(ControlMode.TapToMove);
            Assert.AreEqual(ControlMode.TapToMove, ControlSettings.Current);
            Assert.AreEqual(ControlMode.TapToMove, notifiedMode);

            ControlSettings.Set(ControlMode.Joystick);
            Assert.AreEqual(ControlMode.Joystick, ControlSettings.Current);

            ControlSettings.SetVolume(0.35f);
            Assert.AreEqual(0.35f, ControlSettings.Volume, 0.001f);

            ControlSettings.SetLanguage(UiLanguage.English);
            Assert.AreEqual(UiLanguage.English, ControlSettings.Language);
        }
        finally
        {
            ControlSettings.ControlModeChanged -= listener;
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

    [Test]
    public void JoystickPositionIncludesSafeAreaAndPadding()
    {
        MethodInfo getPosition = typeof(Analog)
            .GetMethod("GetSafeAreaPosition", BindingFlags.Static | BindingFlags.NonPublic);
        object[] args = { new Rect(40f, 20f, 1920f, 1080f), 2000, 2f, new Vector2(64f, 64f), false };
        Vector2 left = (Vector2)getPosition.Invoke(null, args);
        args[4] = true;
        Vector2 right = (Vector2)getPosition.Invoke(null, args);

        Assert.AreEqual(new Vector2(84f, 74f), left);
        Assert.AreEqual(new Vector2(-84f, 74f), right);
    }

    [Test]
    public void JoystickInputUsesRectCenterAndDeadZone()
    {
        MethodInfo getInput = typeof(Analog)
            .GetMethod("GetInputVector", BindingFlags.Static | BindingFlags.NonPublic);
        MethodInfo getHandleTravel = typeof(Analog)
            .GetMethod("GetHandleTravel", BindingFlags.Static | BindingFlags.NonPublic);
        Rect rect = new Rect(0f, 0f, 240f, 240f);

        Assert.AreEqual(Vector2.zero, getInput.Invoke(null, new object[] { rect.center, rect, 0.1f }));
        Assert.AreEqual(Vector2.zero, getInput.Invoke(null, new object[] { rect.center + Vector2.right * 5f, rect, 0.1f }));
        Assert.AreEqual(Vector2.right, getInput.Invoke(null, new object[] { new Vector2(rect.xMax, rect.center.y), rect, 0.1f }));
        Assert.AreEqual(57.5f, getHandleTravel.Invoke(null, new object[] { rect, new Rect(0f, 0f, 125f, 125f), 1f }));
    }
}
