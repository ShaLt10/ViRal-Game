using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstagramFeedManager : SingletonDestroy<InstagramFeedManager>
{
    private void Awake()
    {
        ScreenRotateControl.Instance.SetPortrait();
    }
}
