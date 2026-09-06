using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DigitalForensicsQuiz
{
    /// <summary>
    /// Menyediakan data soal pilihan ganda untuk minigame Study Room.
    /// Soal dimuat dan di-cache dari Resources/{QuestionsPath}.
    /// </summary>
    public static class QuestionProvider
    {
        private const string QuestionsPath = "Minigame3QuestionData"; // Resources/Minigame3QuestionData

        private static List<MinigameQuestionData> _cachedQuestions;
        private static bool _isDataLoaded;

        public static List<MinigameQuestionData> GetAllQuestions()
        {
            if (_isDataLoaded && _cachedQuestions != null)
            {
                return new List<MinigameQuestionData>(_cachedQuestions);
            }

            LoadQuestionsFromResources();
            return new List<MinigameQuestionData>(_cachedQuestions ?? new List<MinigameQuestionData>());
        }

        public static List<MinigameQuestionData> GetQuestionsByType(QuestionType type)
        {
            return GetAllQuestions().Where(q => q.type == type).ToList();
        }

        public static MinigameQuestionData GetQuestionById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return GetAllQuestions().FirstOrDefault(q => q.id == id);
        }

        public static int GetTotalQuestionsCount() => GetAllQuestions().Count;

        public static int GetQuestionsCountByType(QuestionType type) => GetQuestionsByType(type).Count;

        private static void LoadQuestionsFromResources()
        {
            try
            {
                var loadedQuestions = Resources.LoadAll<MinigameQuestionData>(QuestionsPath);

                if (loadedQuestions.Length == 0)
                {
                    Debug.LogWarning($"[QuestionProvider] Tidak ada soal ditemukan di 'Resources/{QuestionsPath}'.");
                    _cachedQuestions = new List<MinigameQuestionData>();
                    _isDataLoaded = true;
                    return;
                }

                _cachedQuestions = loadedQuestions
                    .Where(q => q != null)
                    .ToList();

                _isDataLoaded = true;
                Debug.Log($"[QuestionProvider] Berhasil memuat {_cachedQuestions.Count} soal pilihan ganda.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[QuestionProvider] Gagal memuat soal: {e.Message}");
                _cachedQuestions = new List<MinigameQuestionData>();
                _isDataLoaded = true;
            }
        }

        public static bool ValidateAllQuestions()
        {
            var questions = GetAllQuestions();
            return questions.All(ValidateQuestion);
        }

        private static bool ValidateQuestion(MinigameQuestionData question)
        {
            if (question == null)
            {
                Debug.LogError("[QuestionProvider] Ditemukan soal null di resources!");
                return false;
            }

            if (string.IsNullOrEmpty(question.questionText))
            {
                Debug.LogError($"[QuestionProvider] Soal '{question.name}' punya questionText kosong!");
                return false;
            }

            if (question.options == null || question.options.Count < 2)
            {
                Debug.LogError($"[QuestionProvider] Soal '{question.name}' punya opsi jawaban kurang dari 2!");
                return false;
            }

            if (question.correctAnswerIndex < 0 || question.correctAnswerIndex >= question.options.Count)
            {
                Debug.LogError($"[QuestionProvider] Soal '{question.name}' punya correctAnswerIndex tidak valid!");
                return false;
            }

            return true;
        }

        public static void ClearCache()
        {
            if (_cachedQuestions != null)
            {
                foreach (var question in _cachedQuestions)
                {
                    question?.ClearCache();
                }
            }

            _cachedQuestions = null;
            _isDataLoaded = false;
        }

        public static void ReloadQuestions()
        {
            ClearCache();
            GetAllQuestions();
        }
    }
}