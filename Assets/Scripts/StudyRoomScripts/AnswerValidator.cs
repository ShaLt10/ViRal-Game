using System.Collections.Generic;
using System.Linq;

namespace DigitalForensicsQuiz
{
    /// <summary>
    /// Utility validasi jawaban untuk minigame Study Room.
    /// Hanya menangani soal pilihan ganda (multiple choice).
    /// </summary>
    public static class AnswerValidator
    {
        public static bool ValidateMultipleChoice(MinigameQuestionData question, int selectedAnswer, int shuffledCorrectIndex)
        {
            if (question.type != QuestionType.MultipleChoice) return false;
            if (selectedAnswer < 0 || shuffledCorrectIndex < 0) return false;

            return selectedAnswer == shuffledCorrectIndex;
        }

        public static bool ValidateAllAnswers(List<MinigameQuestionData> questions, List<bool> results)
        {
            if (questions == null || results == null) return false;
            if (questions.Count != results.Count) return false;

            return results.All(result => result);
        }

        public static int GetCorrectAnswersCount(List<bool> results)
        {
            return results?.Count(result => result) ?? 0;
        }

        public static float GetOverallAccuracy(List<bool> results)
        {
            if (results == null || results.Count == 0) return 0f;
            return (float)GetCorrectAnswersCount(results) / results.Count;
        }
    }
}