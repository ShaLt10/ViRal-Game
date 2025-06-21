using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestionContoller : MonoBehaviour
{
    [SerializeField]
    private Button fakta;

    [SerializeField]
    private Button opini;

    InstagramQuestionData questionData;

    private void OnEnable()
    {
        fakta.onClick.AddListener(() => Answer(1));
        opini.onClick.AddListener(() => Answer(0));
        EventManager.Subscribe<QuestionData>(GetQuestion);
    }
    private void OnDisable()
    {
        fakta.onClick.RemoveAllListeners();
        opini.onClick.RemoveAllListeners();
        EventManager.Unsubscribe<QuestionData>(GetQuestion);
    }


    private void Answer(int i)
    { 
        
    }

    private void GetQuestion(QuestionData data)
    {
        questionData = new InstagramQuestionData(data.data);
    }
}
