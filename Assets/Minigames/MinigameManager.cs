using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DigitalForensicsQuiz
{
    public class MinigameManager : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private GameObject instructionPanel;
        [SerializeField] private GameObject gamePanel;
        [SerializeField] private GameObject feedbackPanel;
        [SerializeField] private GameObject resultPanel;

        [Header("Dialog System")]
        [SerializeField] private MinigameDialogManager dialogManager;

        [Header("Instruction Panel")]
        [SerializeField] private Button confirmationButton;

        [Header("Game Panel Elements")]
        [SerializeField] private TextMeshProUGUI questionNumberText;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private TrackingBar trackingBar;
        [SerializeField] private Button globalSubmitButton;

        [Header("Multiple Choice")]
        [SerializeField] private GameObject multipleChoiceContainer;
        [SerializeField] private Button[] optionButtons;
        [SerializeField] private TextMeshProUGUI[] optionTexts;

        [Header("Drag and Drop")]
        [SerializeField] private GameObject dragDropContainer;
        [SerializeField] private Transform dragItemsParent;
        [SerializeField] private Transform dropZonesParent;
        [SerializeField] private GameObject dragItemPrefab;
        [SerializeField] private GameObject dropZonePrefab;

        [Header("Feedback Panel")]
        [SerializeField] private Image feedbackBG;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private TextMeshProUGUI explanationText;
        [SerializeField] private Button nextButton;

        [Header("Result Panel")]
        [SerializeField] private GameObject successResult;
        [SerializeField] private GameObject failedResult;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button exitButton;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip successAudio;
        [SerializeField] private AudioClip failureAudio;
        [SerializeField] private AudioClip hackingAudio;

        [Header("Colors")]
        [SerializeField] private Color correctColor = Color.green;
        [SerializeField] private Color incorrectColor = Color.red;
        [SerializeField] private Color defaultColor = Color.gray;

        // Game State
        private List<MinigameQuestionData> questions;
        private int currentQuestionIndex = 0;
        private MinigameQuestionData currentQuestion;
        private GameState gameState = GameState.Dialog;

        // Answer tracking
        private int selectedMultipleChoiceAnswer = -1;
        private Dictionary<string, string> dragDropAnswers = new Dictionary<string, string>();
        private List<bool> answerResults = new List<bool>();

        // Drag and Drop objects
        private List<DragItemComponent> activeDragItems = new List<DragItemComponent>();
        private List<DropZoneComponent> activeDropZones = new List<DropZoneComponent>();

        public enum GameState
        {
            Dialog,
            Instruction,
            Playing,
            Feedback,
            Result
        }

        private void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            // Load questions
            questions = QuestionProvider.GetDigitalForensicsQuestions();
            
            // Sort questions: MultipleChoice first, then DragAndDrop
            questions = questions.OrderBy(q => q.type == QuestionType.MultipleChoice ? 0 : 1).ToList();
            
            // Initialize tracking bar
            trackingBar.Initialize(questions.Count);
            
            // Setup button listeners
            confirmationButton.onClick.AddListener(StartGame);
            globalSubmitButton.onClick.AddListener(SubmitAnswer);
            nextButton.onClick.AddListener(NextQuestion);
            restartButton.onClick.AddListener(RestartGame);
            exitButton.onClick.AddListener(ExitGame);

            // Setup multiple choice buttons
            for (int i = 0; i < optionButtons.Length; i++)
            {
                int index = i; // Capture for closure
                optionButtons[i].onClick.AddListener(() => SelectMultipleChoiceOption(index));
            }

            // Start with dialog
            SetGameState(GameState.Dialog);
            StartDialog();
        }

        private void StartDialog()
        {
            if (dialogManager != null)
            {
                dialogManager.StartMinigameDialog(() => {
                    SetGameState(GameState.Instruction);
                });
            }
            else
            {
                SetGameState(GameState.Instruction);
            }
        }

        private void SetGameState(GameState newState)
        {
            gameState = newState;
            
            // Hide all panels
            dialogPanel.SetActive(false);
            instructionPanel.SetActive(false);
            gamePanel.SetActive(false);
            feedbackPanel.SetActive(false);
            resultPanel.SetActive(false);

            // Show current panel
            switch (gameState)
            {
                case GameState.Dialog:
                    dialogPanel.SetActive(true);
                    break;
                case GameState.Instruction:
                    instructionPanel.SetActive(true);
                    break;
                case GameState.Playing:
                    gamePanel.SetActive(true);
                    break;
                case GameState.Feedback:
                    feedbackPanel.SetActive(true);
                    break;
                case GameState.Result:
                    resultPanel.SetActive(true);
                    break;
            }
        }

        private void StartGame()
        {
            currentQuestionIndex = 0;
            answerResults.Clear();
            trackingBar.ResetAll();
            
            SetGameState(GameState.Playing);
            LoadCurrentQuestion();
        }

        private void LoadCurrentQuestion()
        {
            if (currentQuestionIndex >= questions.Count)
            {
                ShowResult();
                return;
            }

            currentQuestion = questions[currentQuestionIndex];
            
            // Update UI
            questionNumberText.text = $"Pertanyaan {currentQuestionIndex + 1}/{questions.Count}";
            questionText.text = currentQuestion.questionText;
            
            // Reset answers
            selectedMultipleChoiceAnswer = -1;
            dragDropAnswers.Clear();
            
            // Setup question type
            multipleChoiceContainer.SetActive(currentQuestion.type == QuestionType.MultipleChoice);
            dragDropContainer.SetActive(currentQuestion.type == QuestionType.DragAndDrop);
            
            if (currentQuestion.type == QuestionType.MultipleChoice)
            {
                SetupMultipleChoice();
            }
            else if (currentQuestion.type == QuestionType.DragAndDrop)
            {
                SetupDragAndDrop();
            }
            
            // Enable submit button
            globalSubmitButton.interactable = false;
            UpdateSubmitButtonState();
        }

        private void SetupMultipleChoice()
        {
            var randomizedOptions = new List<string>(currentQuestion.options);
            
            // Randomize options
            for (int i = randomizedOptions.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                string temp = randomizedOptions[i];
                randomizedOptions[i] = randomizedOptions[j];
                randomizedOptions[j] = temp;
            }
            
            // Update correct answer index after randomization
            string correctOption = currentQuestion.options[currentQuestion.correctAnswerIndex];
            int newCorrectIndex = randomizedOptions.IndexOf(correctOption);
            
            // Setup UI
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (i < randomizedOptions.Count)
                {
                    optionButtons[i].gameObject.SetActive(true);
                    optionTexts[i].text = randomizedOptions[i];
                    
                    // Reset button colors
                    var colors = optionButtons[i].colors;
                    colors.normalColor = Color.white;
                    colors.selectedColor = Color.cyan;
                    optionButtons[i].colors = colors;
                }
                else
                {
                    optionButtons[i].gameObject.SetActive(false);
                }
            }
            
            // Store the new correct index
            currentQuestion.correctAnswerIndex = newCorrectIndex;
        }

        private void SetupDragAndDrop()
        {
            // Clear existing items
            ClearDragDropItems();
            
            // Get randomized items
            var dragItems = currentQuestion.GetRandomizedDragItems();
            var dropZones = currentQuestion.GetRandomizedDropZones();
            
            // Create drag items
            foreach (var itemData in dragItems)
            {
                GameObject itemObj = Instantiate(dragItemPrefab, dragItemsParent);
                DragItemComponent dragItem = itemObj.GetComponent<DragItemComponent>();
                dragItem.Initialize(itemData, this);
                activeDragItems.Add(dragItem);
            }
            
            // Create drop zones
            foreach (var zoneData in dropZones)
            {
                GameObject zoneObj = Instantiate(dropZonePrefab, dropZonesParent);
                DropZoneComponent dropZone = zoneObj.GetComponent<DropZoneComponent>();
                dropZone.Initialize(zoneData);
                activeDropZones.Add(dropZone);
            }
        }

        private void ClearDragDropItems()
        {
            activeDragItems.Clear();
            activeDropZones.Clear();
            
            foreach (Transform child in dragItemsParent)
            {
                Destroy(child.gameObject);
            }
            
            foreach (Transform child in dropZonesParent)
            {
                Destroy(child.gameObject);
            }
        }

        private void SelectMultipleChoiceOption(int optionIndex)
        {
            selectedMultipleChoiceAnswer = optionIndex;
            
            // Update button visuals
            for (int i = 0; i < optionButtons.Length; i++)
            {
                var colors = optionButtons[i].colors;
                colors.normalColor = (i == optionIndex) ? Color.cyan : Color.white;
                optionButtons[i].colors = colors;
            }
            
            UpdateSubmitButtonState();
        }

        public void OnDragDropChanged(string dragItemId, string dropZoneId)
        {
            if (string.IsNullOrEmpty(dropZoneId))
            {
                dragDropAnswers.Remove(dragItemId);
            }
            else
            {
                dragDropAnswers[dragItemId] = dropZoneId;
            }
            
            UpdateSubmitButtonState();
        }

        private void UpdateSubmitButtonState()
        {
            bool canSubmit = false;
            
            if (currentQuestion.type == QuestionType.MultipleChoice)
            {
                canSubmit = selectedMultipleChoiceAnswer >= 0;
            }
            else if (currentQuestion.type == QuestionType.DragAndDrop)
            {
                canSubmit = dragDropAnswers.Count == currentQuestion.dragItems.Count;
            }
            
            globalSubmitButton.interactable = canSubmit;
        }

        private void SubmitAnswer()
        {
            bool isCorrect = false;
            
            if (currentQuestion.type == QuestionType.MultipleChoice)
            {
                isCorrect = AnswerValidator.ValidateMultipleChoice(currentQuestion, selectedMultipleChoiceAnswer);
            }
            else if (currentQuestion.type == QuestionType.DragAndDrop)
            {
                isCorrect = AnswerValidator.ValidateDragAndDrop(currentQuestion, dragDropAnswers);
            }
            
            answerResults.Add(isCorrect);
            trackingBar.SetResult(currentQuestionIndex, isCorrect);
            
            ShowFeedback(isCorrect);
        }

        private void ShowFeedback(bool isCorrect)
        {
            SetGameState(GameState.Feedback);
            
            if (isCorrect)
            {
                feedbackBG.color = correctColor;
                feedbackText.text = "BENAR!";
                feedbackText.color = Color.white;
            }
            else
            {
                feedbackBG.color = incorrectColor;
                feedbackText.text = "SALAH!";
                feedbackText.color = Color.white;
            }
            
            explanationText.text = currentQuestion.explanation;
        }

        private void NextQuestion()
        {
            currentQuestionIndex++;
            
            if (currentQuestionIndex >= questions.Count)
            {
                ShowResult();
            }
            else
            {
                SetGameState(GameState.Playing);
                LoadCurrentQuestion();
            }
        }

        private void ShowResult()
        {
            SetGameState(GameState.Result);
            
            bool allCorrect = answerResults.All(result => result);
            
            successResult.SetActive(allCorrect);
            failedResult.SetActive(!allCorrect);
            
            // Play audio
            if (allCorrect)
            {
                PlayAudio(successAudio);
                StartCoroutine(PlayHackingEffectDelayed());
            }
            else
            {
                PlayAudio(failureAudio);
            }
        }

        private IEnumerator PlayHackingEffectDelayed()
        {
            yield return new WaitForSeconds(1f);
            PlayAudio(hackingAudio);
        }

        private void PlayAudio(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }

        private void RestartGame()
        {
            // Clear caches to ensure randomization
            foreach (var question in questions)
            {
                question.ClearCache();
            }
            
            StartGame();
        }

        private void ExitGame()
        {
            // Implement exit logic (return to main menu, etc.)
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            // Clean up
            ClearDragDropItems();
        }
    }
}