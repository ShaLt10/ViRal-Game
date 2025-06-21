using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstagramFeedManager : SingletonDestroy<InstagramFeedManager>
{
    [SerializeField]
    IntagramFeedQuestionData questionDatas;

    private void Awake()
    {
        ScreenRotateControl.Instance.SetPortrait();
    }

    public IntagramFeedQuestionData GetDataQuestion()
    { 
        return questionDatas;
    }
}
