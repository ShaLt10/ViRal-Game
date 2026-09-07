using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenRotateControl : Singleton<ScreenRotateControl>
{

    protected override void OnDestroy()
    {
        base.OnDestroy(); // Important for cleanup!
    }
    public void SetPortrait()
    {
        Screen.orientation = ScreenOrientation.Portrait;
    }

    public void SetLandscape()
    {
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.orientation = ScreenOrientation.AutoRotation;
    }
}
