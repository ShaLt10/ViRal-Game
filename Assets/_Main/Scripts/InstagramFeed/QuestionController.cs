using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionController : MonoBehaviour
{
    [SerializeField] private Button fakta;
    [SerializeField] private Button opini;

    private InstagramQuestionData questionData;

    [SerializeField] private TMP_Text question;
    [SerializeField] private Image potraitImage;
    [SerializeField] private string nextScene = "MainGame";

    private int score = 0;
    private int count = 0;

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
        count++;

        // 0 = opini, 1 = fakta
        if (i == 0)
        {
            if (!questionData.Fact) score += 2000;
        }
        else // i == 1
        {
            if (questionData.Fact) score += 2000;
        }

        if (count < 6)
        {
            EventManager.Publish(new GetQuestion(count));
            return;
        }

        // Selesai 6 soal → tentukan dialog & lanjutannya
        string dialogKey = score >= 12000
            ? DialoguesNames.InstagramFeed_Win
            : DialoguesNames.InstagramFeed_Lost;

        // Mainkan dialog dulu, lalu callback pindah scene
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.PlaySequenceThen(dialogKey, () =>
            {
                if (score >= 12000)
                {
                    if (SaveCompletion())
                        SceneFlow.LoadScene(nextScene);
                }
                else
                    SceneFlow.ReloadCurrent();
            });
        }
        else
        {
            // Fallback kalau DialogManager belum ada (biar nggak buntu)
            Debug.LogWarning("DialogManager.Instance null. Melanjutkan tanpa dialog.");
            if (score >= 12000)
            {
                if (SaveCompletion())
                    SceneFlow.LoadScene(nextScene);
            }
            else
                SceneFlow.ReloadCurrent();
        }
    }

    private void GetQuestion(QuestionData data)
    {
        questionData = new InstagramQuestionData(data.data);
        if (question != null) question.SetText(questionData.Question);
        if (potraitImage != null) potraitImage.sprite = questionData.image;
    }

    private static bool SaveCompletion()
    {
        if (GameSession.Instance != null
            && GameSession.Instance.CompleteMinigame("InstagramLive"))
            return true;

        Debug.LogError("[QuestionController] Progress gagal disimpan; perpindahan scene dibatalkan.");
        return false;
    }
}
