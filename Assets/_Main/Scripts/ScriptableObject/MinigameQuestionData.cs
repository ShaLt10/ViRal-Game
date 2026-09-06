using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DigitalForensicsQuiz
{
    public enum QuestionType
    {
        MultipleChoice = 0
    }

    /// <summary>
    /// Data soal pilihan ganda untuk minigame Study Room.
    /// Setiap asset merepresentasikan satu soal beserta opsi jawaban,
    /// jawaban benar, dan penjelasan untuk masing-masing hasil.
    /// </summary>
    [CreateAssetMenu(fileName = "NewQuestion", menuName = "Digital Forensics Quiz/Minigame Question")]
    public class MinigameQuestionData : ScriptableObject
    {
        [Header("Question Identity")]
        public string id;
        public QuestionType type = QuestionType.MultipleChoice;

        [TextArea(3, 6)]
        public string questionText;

        [Header("Explanations")]
        [TextArea(2, 4)]
        [Tooltip("Explanation shown when answer is correct")]
        public string correctExplanation;

        [TextArea(2, 4)]
        [Tooltip("Explanation shown when answer is incorrect")]
        public string incorrectExplanation;

        [Header("Answer Options")]
        public List<string> options = new List<string>();
        public int correctAnswerIndex;

        private System.Random _randomGenerator;
        private List<string> _cachedShuffledOptions;
        private int _cachedCorrectIndex = -1;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = name;
            }

            if (correctAnswerIndex < 0 || correctAnswerIndex >= options.Count)
            {
                Debug.LogWarning($"Question {name}: Correct answer index is out of range!");
            }
        }

        /// <summary>
        /// Mengacak urutan opsi jawaban dan mengembalikan index jawaban benar
        /// yang baru sesuai urutan hasil acak. Hasil di-cache per instance
        /// sampai <see cref="ClearCache"/> dipanggil.
        /// </summary>
        public List<string> GetShuffledOptions(out int newCorrectIndex)
        {
            newCorrectIndex = correctAnswerIndex;

            if (options == null || options.Count <= 1)
            {
                return new List<string>(options ?? new List<string>());
            }

            if (_cachedShuffledOptions != null && _cachedCorrectIndex != -1)
            {
                newCorrectIndex = _cachedCorrectIndex;
                return new List<string>(_cachedShuffledOptions);
            }

            InitializeRandom();
            var indexedOptions = options
                .Select((option, index) => new { option, index })
                .ToList();

            // Fisher-Yates shuffle
            for (int i = indexedOptions.Count - 1; i > 0; i--)
            {
                int j = _randomGenerator.Next(0, i + 1);
                (indexedOptions[i], indexedOptions[j]) = (indexedOptions[j], indexedOptions[i]);
            }

            _cachedCorrectIndex = indexedOptions.FindIndex(item => item.index == correctAnswerIndex);
            _cachedShuffledOptions = indexedOptions.Select(x => x.option).ToList();

            newCorrectIndex = _cachedCorrectIndex;
            return new List<string>(_cachedShuffledOptions);
        }

        public bool IsValidQuestion()
        {
            if (string.IsNullOrEmpty(questionText)) return false;
            if (string.IsNullOrEmpty(correctExplanation) || string.IsNullOrEmpty(incorrectExplanation)) return false;

            return options != null && options.Count >= 2 &&
                   correctAnswerIndex >= 0 && correctAnswerIndex < options.Count;
        }

        public int GetEstimatedTimeToComplete()
        {
            const int baseTime = 30;
            int complexityBonus = (options?.Count ?? 0) * 5;
            return baseTime + complexityBonus;
        }

        public void ClearCache()
        {
            _randomGenerator = null;
            _cachedShuffledOptions = null;
            _cachedCorrectIndex = -1;
        }

        public void RegenerateShuffles()
        {
            ClearCache();
            GetShuffledOptions(out _);
        }

        private void InitializeRandom()
        {
            if (_randomGenerator == null)
            {
                int seed = string.IsNullOrEmpty(id) ? Random.Range(0, int.MaxValue) : id.GetHashCode();
                _randomGenerator = new System.Random(seed);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Validate Question")]
        public void ValidateQuestionData()
        {
            if (IsValidQuestion())
            {
                Debug.Log($"Question '{name}' is valid!");
            }
            else
            {
                Debug.LogError($"Question '{name}' has validation errors!");
            }
        }

        [ContextMenu("Clear Shuffle Cache")]
        public void ClearShuffleCache()
        {
            ClearCache();
            Debug.Log($"Shuffle cache cleared for question '{name}'");
        }

        [ContextMenu("Test Shuffle")]
        public void TestShuffle()
        {
            ClearCache();
            var shuffled = GetShuffledOptions(out int newIndex);
            Debug.Log($"Original correct index: {correctAnswerIndex}, New index: {newIndex}");
            Debug.Log($"Shuffled options: {string.Join(", ", shuffled)}");
        }
#endif
    }
}