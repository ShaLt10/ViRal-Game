using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DigitalForensicsQuiz
{
    public class TrackingBar : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private Color defaultColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Dark gray
        [SerializeField] private Color correctColor = Color.green;
        [SerializeField] private Color incorrectColor = Color.red;
        
        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private List<Image> circleImages = new List<Image>();
        private List<Color> targetColors = new List<Color>();
        
        private void Awake()
        {
            // Get all child circle images
            foreach (Transform child in transform)
            {
                Image circleImage = child.GetComponent<Image>();
                if (circleImage != null)
                {
                    circleImages.Add(circleImage);
                    targetColors.Add(defaultColor);
                }
            }
        }

        public void Initialize(int questionCount)
        {
            // Ensure we have the right number of circles
            if (circleImages.Count != questionCount)
            {
                Debug.LogWarning($"TrackingBar has {circleImages.Count} circles but needs {questionCount}. Please adjust the prefab.");
            }
            
            // Set all to default color
            ResetAll();
        }

        public void ResetAll()
        {
            for (int i = 0; i < circleImages.Count; i++)
            {
                SetCircleColor(i, defaultColor, false);
            }
        }

        public void SetResult(int questionIndex, bool isCorrect)
        {
            if (questionIndex < 0 || questionIndex >= circleImages.Count)
            {
                Debug.LogError($"Invalid question index: {questionIndex}. Valid range: 0-{circleImages.Count - 1}");
                return;
            }

            Color targetColor = isCorrect ? correctColor : incorrectColor;
            SetCircleColor(questionIndex, targetColor, true);
        }

        private void SetCircleColor(int index, Color color, bool animate)
        {
            if (index < 0 || index >= circleImages.Count) return;

            targetColors[index] = color;

            if (animate)
            {
                StartCoroutine(AnimateColorChange(index, color));
            }
            else
            {
                circleImages[index].color = color;
            }
        }

        private System.Collections.IEnumerator AnimateColorChange(int index, Color targetColor)
        {
            Image targetImage = circleImages[index];
            Color startColor = targetImage.color;
            float elapsedTime = 0f;

            // Scale animation for feedback
            Vector3 originalScale = targetImage.transform.localScale;
            Vector3 targetScale = originalScale * 1.2f;

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / animationDuration;
                float curveValue = animationCurve.Evaluate(progress);

                // Color lerp
                targetImage.color = Color.Lerp(startColor, targetColor, curveValue);

                // Scale animation (pop effect)
                if (progress < 0.5f)
                {
                    targetImage.transform.localScale = Vector3.Lerp(originalScale, targetScale, progress * 2f);
                }
                else
                {
                    targetImage.transform.localScale = Vector3.Lerp(targetScale, originalScale, (progress - 0.5f) * 2f);
                }

                yield return null;
            }

            // Ensure final state
            targetImage.color = targetColor;
            targetImage.transform.localScale = originalScale;
        }

        public bool AreAllCorrect()
        {
            for (int i = 0; i < circleImages.Count; i++)
            {
                if (targetColors[i] != correctColor)
                {
                    return false;
                }
            }
            return true;
        }

        public int GetCorrectCount()
        {
            int count = 0;
            for (int i = 0; i < circleImages.Count; i++)
            {
                if (targetColors[i] == correctColor)
                {
                    count++;
                }
            }
            return count;
        }

        public int GetIncorrectCount()
        {
            int count = 0;
            for (int i = 0; i < circleImages.Count; i++)
            {
                if (targetColors[i] == incorrectColor)
                {
                    count++;
                }
            }
            return count;
        }

        // Utility method for external status checking
        public TrackingStatus GetStatus()
        {
            return new TrackingStatus
            {
                TotalQuestions = circleImages.Count,
                CorrectAnswers = GetCorrectCount(),
                IncorrectAnswers = GetIncorrectCount(),
                AllCorrect = AreAllCorrect()
            };
        }
    }

    [System.Serializable]
    public struct TrackingStatus
    {
        public int TotalQuestions;
        public int CorrectAnswers;
        public int IncorrectAnswers;
        public bool AllCorrect;
        
        public float SuccessRate => TotalQuestions > 0 ? (float)CorrectAnswers / TotalQuestions : 0f;
    }
}