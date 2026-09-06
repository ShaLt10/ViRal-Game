using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace DigitalForensicsQuiz
{
    public class StudyRoomMinigameManager : MonoBehaviour
    {
        #region === UI: Panels & Backgrounds ===
        public enum UIPanel { Dialog, Instruction, Minigame, Feedback, Result, Completion }

        [Header("Panel Objects (urutkan sesuai enum: Dialog, Instruction, Minigame, Feedback, Result, Completion)")]
        [SerializeField] private GameObject[] panelObjects;
        private readonly Dictionary<UIPanel, GameObject> _panels = new();

        [Header("Panel Backgrounds")]
        [SerializeField] private GameObject feedbackBackground;
        [SerializeField] private GameObject resultBackground;
        [SerializeField] private GameObject completionBackground;

        [Tooltip("Background yang dipakai bareng oleh panel Dialog, Instruction, dan Minigame.")]
        [SerializeField] private GameObject gameplayBackground;

        private static readonly UIPanel[] GameplayPanels = { UIPanel.Dialog, UIPanel.Instruction, UIPanel.Minigame };
        #endregion

        #region === Core UI ===
        [Header("Core UI")]
        [SerializeField] private Transform trackingBar;
        [SerializeField] private GameObject trackingCirclePrefab;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private Button submitButton;
        #endregion

        #region === Multiple Choice ===
        [Header("Multiple Choice")]
        [SerializeField] private Transform optionsContainer;
        [SerializeField] private Button optionButtonPrefab;

        private const int TotalQuestionCount = 10;
        #endregion

        #region === Feedback & Result ===
        [Header("Feedback UI")]
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private TextMeshProUGUI explanationText;
        [SerializeField] private Button nextQuestionButton;

        [Header("Result UI")]
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button resultNextButton;

        [Header("Completion UI - Success")]
        [SerializeField] private GameObject successElements;
        [SerializeField] private TextMeshProUGUI successHeaderText;
        [SerializeField] private TextMeshProUGUI trackingText;
        [SerializeField] private TextMeshProUGUI fileMetadataText;
        [SerializeField] private TextMeshProUGUI misuseDescriptionText;
        [SerializeField] private Button closeButton;

        [Header("Completion UI - Failed")]
        [SerializeField] private GameObject failedElements;
        [SerializeField] private TextMeshProUGUI failedText;
        [SerializeField] private Button restartButton;
        #endregion

        #region === Dialog & Instruction ===
        [Header("Universal Dialog (DialogUI)")]
        [Tooltip("Referensi ke DialogUI universal yang dipakai di seluruh game.")]
        [SerializeField] private DialogUI dialogUI;

        [Tooltip("Sequence dialog pembuka Study Room (System/Gavi ngobrol sebelum instruksi).")]
        [SerializeField] private DialogSequence openingDialogSequence;

        [Header("Instruction UI")]
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private Image glitchImage;
        [SerializeField] private Button confirmationButton;
        #endregion

        #region === Completion Screen (Root) ===
        [Header("Completion Root")]
        [SerializeField] private GameObject completionScreen;
        #endregion

        #region === Narrative Text ===
        [Header("Dialog & Instruction Text")]
        // Nama karakter, portrait, dan urutan dialog pembuka sekarang
        // sepenuhnya dikelola oleh DialogUI + CharacterManager (openingDialogSequence).

        [Header("Feedback Text")]
        [SerializeField] private string correctFeedback = "Benar!";
        [SerializeField] private string incorrectFeedback = "Salah!";

        [Header("Result Text")]
        [SerializeField] private string successMessage = "PERFECT! Semua jawaban benar!";
        [SerializeField] private string failureMessage = "GAGAL! Skor: {0}/{1}";

        [Header("Completion Text - Success")]
        [SerializeField] private string successHeader = "INVESTIGASI BERHASIL!";
        [SerializeField] private string trackingMessage = "Jejak digital berhasil dilacak...";

        [TextArea(2, 4)]
        [SerializeField]
        private string fileMetadata =
            "File: SaveInta.com_AQOH...jITBIjog8.mp4\n" +
            "Ukuran: 720x1280 (9:16)  |  Durasi: 00:13\n" +
            "Encoder: AVC Coding (software render)\n" +
            "Create/Modify Date: KOSONG (0000:00:00)";

        [TextArea(3, 6)]
        [SerializeField]
        private string misuseDescription =
            "Nama file diawali tag situs downloader reels pihak ketiga — video ini BUKAN " +
            "file asli dari akun resmi, tapi hasil unduh ulang dari reel orang lain.\n\n" +
            "Tanggal pembuatan & encoder-nya menunjukkan file sudah di-render ulang " +
            "sebelum disebar, bukan rekaman mentah dari kamera. Pola ini khas video " +
            "\"curian\" yang di-download, di-edit ulang (audio & teks ajakan disisipkan), " +
            "lalu disebar sebagai iklan lowongan kerja palsu yang meminta data pribadi.";

        [Header("Completion Text - Failed")]
        [SerializeField] private string failedMessage = "Investigasi gagal! Tim forensik harus mengulang analisis.";
        #endregion

        #region === Color Settings ===
        [Header("Color Settings")]
        [SerializeField] private Color correctColor = Color.green;
        [SerializeField] private Color incorrectColor = Color.red;
        [SerializeField] private Color selectedButtonColor = Color.cyan;
        [SerializeField] private Color defaultButtonColor = Color.white;
        [SerializeField] private Color trackingDefaultColor = Color.gray;
        #endregion

        #region === Animation Settings ===
        [Header("Animation Settings")]
        [SerializeField] private float typewriterSpeed = 0.05f;
        [SerializeField] private float glitchInterval = 0.1f;
        [SerializeField] private float glitchIntensity = 5f;
        #endregion

        [Header("Canvas")]
        [SerializeField] private CanvasScaler canvasScaler;

        [Header("Optional HUD")]
        [SerializeField] private TextMeshProUGUI scoreText;

        #region === State ===
        private List<MinigameQuestionData> _questions;
        private int _currentQuestionIndex;
        private readonly List<bool> _questionResults = new();
        private readonly List<Image> _trackingCircles = new();

        private int _selectedAnswerIndex = -1;
        private int _shuffledCorrectIndex = -1;
        private readonly List<Button> _spawnedOptionButtons = new();

        private Coroutine _typewriterCoroutine;
        private Coroutine _glitchCoroutine;
        private Vector3 _glitchOriginalPosition;

        private MinigameAudioManager _audioManager;
        private bool _trackingBarInitialized;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const bool Verbose = true;
#else
        private const bool Verbose = false;
#endif
        #endregion

        #region === Unity Lifecycle ===
        private void Awake()
        {
            RegisterPanels();
            SetupButtonListeners();
            InitializeBackgrounds();
        }

        private void Start()
        {
            _audioManager = FindObjectOfType<MinigameAudioManager>();
            _questions = LoadQuestions();

            if (_questions == null || _questions.Count == 0)
            {
                Debug.LogError("[StudyRoomMinigameManager] Tidak ada soal ditemukan. Pastikan QuestionProvider siap.");
                ShowPanel(UIPanel.Instruction);
                if (instructionText != null) instructionText.text = "Data soal tidak ditemukan.";
                return;
            }

            if (submitButton == null)
            {
                Debug.LogError("[StudyRoomMinigameManager] Submit button belum di-assign!");
                return;
            }

            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnSubmitButtonClicked);
            submitButton.gameObject.SetActive(false);

            PlayOpeningDialog();
        }
        #endregion

        #region === Public API (Scene Buttons) ===
        public void ExitToMenu() => SceneFlow.LoadTitle();
        public void RestartGame() => SceneFlow.ReloadCurrent();
        #endregion

        #region === Setup ===
        /// <summary>
        /// Memicu background gameplay lalu memutar dialog pembuka lewat
        /// DialogUI universal. Setelah sequence selesai, otomatis lanjut
        /// ke panel Instruction. Kalau DialogUI/sequence belum di-assign,
        /// langsung skip ke Instruction supaya game tidak macet.
        /// </summary>
        private void PlayOpeningDialog()
        {
            ShowPanel(UIPanel.Dialog); // cuma buat toggle gameplayBackground, panelObjects[Dialog] boleh kosong

            if (dialogUI == null || openingDialogSequence == null)
            {
                Debug.LogWarning("[StudyRoomMinigameManager] DialogUI atau openingDialogSequence belum di-assign, skip ke Instruction.");
                ShowPanel(UIPanel.Instruction);
                return;
            }

            dialogUI.OnSequenceComplete = () => ShowPanel(UIPanel.Instruction);
            dialogUI.PlaySequence(openingDialogSequence);
        }
        #endregion

        #region === Panel Setup ===
        private void RegisterPanels()
        {
            for (int i = 0; i < panelObjects.Length; i++)
            {
                var panel = panelObjects[i];
                if (panel == null) continue;

                _panels[(UIPanel)i] = panel;
                panel.SetActive(false);
            }
        }

        /// <summary>
        /// Mengambil soal dari provider dan membatasi jumlahnya menjadi
        /// tepat <see cref="TotalQuestionCount"/> soal pilihan ganda,
        /// sesuai desain minigame ini.
        /// </summary>
        private List<MinigameQuestionData> LoadQuestions()
        {
            var allQuestions = QuestionProvider.GetAllQuestions();
            if (allQuestions == null) return null;

            var multipleChoiceOnly = allQuestions
                .Where(q => q.type == QuestionType.MultipleChoice)
                .Take(TotalQuestionCount)
                .ToList();

            if (multipleChoiceOnly.Count < TotalQuestionCount)
            {
                Debug.LogWarning(
                    $"[StudyRoomMinigameManager] Hanya ditemukan {multipleChoiceOnly.Count} soal pilihan ganda, " +
                    $"target {TotalQuestionCount}.");
            }

            return multipleChoiceOnly;
        }
        #endregion

        #region === Flow ===
        private void StartMinigame()
        {
            _currentQuestionIndex = 0;
            _questionResults.Clear();
            _trackingBarInitialized = false;
            CreateTrackingBar();

            ShowPanel(UIPanel.Minigame);
            DisplayCurrentQuestion();
        }

        private void DisplayCurrentQuestion()
        {
            if (_currentQuestionIndex >= _questions.Count) return;

            if (Verbose) Debug.Log($"=== Menampilkan soal #{_currentQuestionIndex + 1}/{_questions.Count} ===");
            StartCoroutine(DisplayQuestionAfterCleanup());
        }

        private IEnumerator DisplayQuestionAfterCleanup()
        {
            ForceCleanupPreviousQuestion();
            yield return null; // satu frame untuk bersih

            var question = _questions[_currentQuestionIndex];
            if (questionText != null) questionText.text = question.questionText;

            ResetSubmitButton();
            yield return StartCoroutine(DisplayOptionsCoroutine(question));
        }

        private void GoToNextQuestion()
        {
            _currentQuestionIndex++;
            if (_currentQuestionIndex < _questions.Count)
            {
                ShowPanel(UIPanel.Minigame);
                DisplayCurrentQuestion();
            }
            else
            {
                ShowResultPanel();
            }
        }

        private void ShowResultPanel()
        {
            ShowPanel(UIPanel.Result);
            if (resultBackground != null) resultBackground.SetActive(true);

            int correctCount = _questionResults.Count(isCorrect => isCorrect);
            bool allCorrect = correctCount == _questions.Count;
            string message = allCorrect
                ? successMessage
                : string.Format(failureMessage, correctCount, _questions.Count);

            if (resultText != null)
            {
                StartTypewriterEffect(resultText, message);
                resultText.color = allCorrect ? correctColor : incorrectColor;
            }

            if (scoreText != null) scoreText.text = $"Score: {correctCount}/{_questions.Count}";

            _audioManager?.OnGameEnd(allCorrect);
        }

        private void ShowCompletionScreen()
        {
            // Root completion panel sudah diaktifkan lewat ShowPanel(UIPanel.Completion)
            if (completionBackground != null) completionBackground.SetActive(true);

            int correctCount = _questionResults.Count(isCorrect => isCorrect);
            bool allCorrect = correctCount == _questions.Count;

            if (allCorrect) ShowSuccessCompletion();
            else ShowFailedCompletion();
        }

        private void ShowSuccessCompletion()
        {
            if (GameSession.Instance == null
                || !GameSession.Instance.CompleteMinigame("StudyRoom"))
            {
                Debug.LogError("[StudyRoom] Progress minigame gagal disimpan.");
                if (successElements != null) successElements.SetActive(false);
                if (failedElements != null) failedElements.SetActive(true);
                if (failedText != null) StartTypewriterEffect(failedText, "Progress gagal disimpan. Silakan coba lagi.");
                return;
            }
            if (successElements != null) successElements.SetActive(true);
            if (failedElements != null) failedElements.SetActive(false);
            StartCoroutine(ShowSuccessElementsSequentially());
        }

        private void ShowFailedCompletion()
        {
            if (successElements != null) successElements.SetActive(false);
            if (failedElements != null) failedElements.SetActive(true);
            if (failedText != null) StartTypewriterEffect(failedText, failedMessage);
        }
        #endregion

        #region === Panel Ops ===
        private void ShowPanel(UIPanel target)
        {
            foreach (var panel in _panels.Values) panel.SetActive(false);

            if (feedbackBackground != null) feedbackBackground.SetActive(false);
            if (resultBackground != null) resultBackground.SetActive(false);
            if (completionBackground != null) completionBackground.SetActive(false);
            if (gameplayBackground != null) gameplayBackground.SetActive(System.Array.IndexOf(GameplayPanels, target) >= 0);

            if (!_panels.TryGetValue(target, out var targetPanel)) return;

            targetPanel.SetActive(true);
            switch (target)
            {
                case UIPanel.Instruction:
                    SetupInstructionPanel();
                    break;
                case UIPanel.Completion:
                    ShowCompletionScreen();
                    break;
            }
        }

        private void InitializeBackgrounds()
        {
            if (feedbackBackground != null) feedbackBackground.SetActive(false);
            if (resultBackground != null) resultBackground.SetActive(false);
            if (completionBackground != null) completionBackground.SetActive(false);
            if (glitchImage != null) _glitchOriginalPosition = glitchImage.transform.localPosition;

            if (successElements != null) successElements.SetActive(false);
            if (failedElements != null) failedElements.SetActive(false);
            if (completionScreen != null) completionScreen.SetActive(false);
        }
        #endregion

        #region === Button Listeners ===
        private void SetupButtonListeners()
        {
            ClearAllButtonListeners();

            if (confirmationButton != null) confirmationButton.onClick.AddListener(StartMinigame);
            if (nextQuestionButton != null) nextQuestionButton.onClick.AddListener(GoToNextQuestion);
            if (resultNextButton != null) resultNextButton.onClick.AddListener(() => ShowPanel(UIPanel.Completion));
            if (closeButton != null) closeButton.onClick.AddListener(QuitGame);
            if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        }

        private void ClearAllButtonListeners()
        {
            confirmationButton?.onClick.RemoveAllListeners();
            nextQuestionButton?.onClick.RemoveAllListeners();
            resultNextButton?.onClick.RemoveAllListeners();
            closeButton?.onClick.RemoveAllListeners();
            restartButton?.onClick.RemoveAllListeners();
        }
        #endregion

        #region === Instruction ===
        private void SetupInstructionPanel()
        {
            if (instructionText != null && !string.IsNullOrEmpty(instructionText.text))
            {
                StartTypewriterEffect(instructionText, instructionText.text);
            }

            if (glitchImage != null) StartGlitchAnimation();
        }
        #endregion

        #region === Submit & Feedback ===
        private void ResetSubmitButton()
        {
            if (submitButton == null) return;

            submitButton.gameObject.SetActive(false);
            submitButton.interactable = false;

            var buttonImage = submitButton.GetComponent<Image>();
            if (buttonImage != null) buttonImage.raycastTarget = true;
        }

        private void EnableSubmitButton(string reason)
        {
            if (submitButton == null)
            {
                Debug.LogError("[StudyRoomMinigameManager] Tidak bisa enable submit: referensi kosong.");
                return;
            }
            if (Verbose) Debug.Log($"Enable submit: {reason}");

            submitButton.gameObject.SetActive(true);
            submitButton.interactable = true;

            var buttonImage = submitButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.raycastTarget = true;
                if (buttonImage.color.a < 0.95f)
                {
                    var color = buttonImage.color;
                    color.a = 1f;
                    buttonImage.color = color;
                }
            }

            foreach (var canvasGroup in submitButton.GetComponentsInParent<CanvasGroup>())
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = Mathf.Max(1f, canvasGroup.alpha);
            }

            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnSubmitButtonClicked);
            submitButton.transform.SetAsLastSibling();
            Canvas.ForceUpdateCanvases();
        }

        private void OnSubmitButtonClicked() => SubmitAnswer();

        private void SubmitAnswer()
        {
            if (_currentQuestionIndex >= _questions.Count) return;

            var question = _questions[_currentQuestionIndex];
            bool isCorrect = AnswerValidator.ValidateMultipleChoice(question, _selectedAnswerIndex, _shuffledCorrectIndex);

            _questionResults.Add(isCorrect);
            UpdateTrackingCircle(_currentQuestionIndex, isCorrect);

            string explanation = isCorrect ? question.correctExplanation : question.incorrectExplanation;
            ShowFeedbackPanel(isCorrect, explanation);
        }

        private void ShowFeedbackPanel(bool isCorrect, string explanation)
        {
            ShowPanel(UIPanel.Feedback);
            if (feedbackBackground != null) feedbackBackground.SetActive(true);

            if (feedbackText != null)
            {
                feedbackText.text = isCorrect ? correctFeedback : incorrectFeedback;
                feedbackText.color = isCorrect ? correctColor : incorrectColor;
            }

            if (explanationText != null) explanationText.text = explanation;

            if (nextQuestionButton != null)
            {
                var buttonLabel = nextQuestionButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonLabel != null)
                {
                    bool isLastQuestion = _currentQuestionIndex >= _questions.Count - 1;
                    buttonLabel.text = isLastQuestion ? "Lihat Hasil" : "Lanjut";
                }
            }

            if (isCorrect) _audioManager?.OnQuestionCorrect();
            else _audioManager?.OnQuestionIncorrect();
        }
        #endregion

        #region === Tracking Bar ===
        private void CreateTrackingBar()
        {
            if (_trackingBarInitialized || trackingBar == null || trackingCirclePrefab == null) return;

            foreach (Transform child in trackingBar) SafeDestroy(child.gameObject);
            _trackingCircles.Clear();

            for (int i = 0; i < _questions.Count; i++)
            {
                var circle = Instantiate(trackingCirclePrefab, trackingBar);
                var circleImage = circle.GetComponent<Image>();
                if (circleImage == null) continue;

                circleImage.color = trackingDefaultColor;
                _trackingCircles.Add(circleImage);
            }

            _trackingBarInitialized = true;
            if (Verbose) Debug.Log($"Tracking bar dibuat dengan {_trackingCircles.Count} lingkaran.");
        }

        private void UpdateTrackingCircle(int questionIndex, bool isCorrect)
        {
            if (questionIndex < 0 || questionIndex >= _trackingCircles.Count) return;
            _trackingCircles[questionIndex].color = isCorrect ? correctColor : incorrectColor;
        }
        #endregion

        #region === Multiple Choice Rendering ===
        private IEnumerator DisplayOptionsCoroutine(MinigameQuestionData question)
        {
            if (optionsContainer == null || optionButtonPrefab == null)
            {
                Debug.LogError("[StudyRoomMinigameManager] Options container/prefab belum di-assign!");
                yield break;
            }

            optionsContainer.gameObject.SetActive(true);

            var shuffledOptions = question.GetShuffledOptions(out _shuffledCorrectIndex);
            _selectedAnswerIndex = -1;

            foreach (var optionLabel in shuffledOptions)
            {
                var button = Instantiate(optionButtonPrefab, optionsContainer);
                button.gameObject.SetActive(true);
                button.interactable = true;

                var buttonLabel = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonLabel != null) buttonLabel.text = optionLabel;

                int optionIndex = _spawnedOptionButtons.Count;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SelectOption(optionIndex));

                _spawnedOptionButtons.Add(button);
                yield return null; // beri waktu per spawn
            }

            yield return new WaitForEndOfFrame();

            var containerRect = optionsContainer.GetComponent<RectTransform>();
            if (containerRect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
        }

        public void SelectOption(int index)
        {
            _selectedAnswerIndex = index;
            _audioManager?.PlayButtonClickSFX();

            for (int i = 0; i < _spawnedOptionButtons.Count; i++)
            {
                var buttonImage = _spawnedOptionButtons[i]?.GetComponent<Image>();
                if (buttonImage != null) buttonImage.color = (i == index) ? selectedButtonColor : defaultButtonColor;
            }

            EnableSubmitButton("Opsi dipilih");
        }
        #endregion

        #region === Cleanup ===
        private void ForceCleanupPreviousQuestion()
        {
            for (int i = _spawnedOptionButtons.Count - 1; i >= 0; i--)
            {
                if (_spawnedOptionButtons[i] != null) SafeDestroy(_spawnedOptionButtons[i].gameObject);
            }
            _spawnedOptionButtons.Clear();

            if (optionsContainer != null)
            {
                for (int i = optionsContainer.childCount - 1; i >= 0; i--)
                    SafeDestroy(optionsContainer.GetChild(i).gameObject);
                optionsContainer.gameObject.SetActive(false);
            }

            _selectedAnswerIndex = -1;
            _shuffledCorrectIndex = -1;
        }

        private void SafeDestroy(GameObject target)
        {
            if (target == null) return;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(target);
                return;
            }
#endif
            Destroy(target);
        }
        #endregion

        #region === Completion: Success Sequence ===
        /// <summary>
        /// Menampilkan hasil investigasi secara berurutan: header sukses,
        /// pesan tracking, lalu metadata file video yang jadi bahan
        /// misinformasi beserta penjelasan bagaimana file itu disalahgunakan.
        /// </summary>
        private IEnumerator ShowSuccessElementsSequentially()
        {
            if (successHeaderText != null)
            {
                yield return StartCoroutine(TypewriterCoroutine(successHeaderText, successHeader));
                yield return new WaitForSeconds(0.5f);
            }

            if (trackingText != null)
            {
                yield return StartCoroutine(TypewriterCoroutine(trackingText, trackingMessage));
                yield return new WaitForSeconds(0.5f);
            }

            if (fileMetadataText != null)
            {
                yield return StartCoroutine(TypewriterCoroutine(fileMetadataText, fileMetadata));
                yield return new WaitForSeconds(0.5f);
            }

            if (misuseDescriptionText != null)
            {
                yield return StartCoroutine(TypewriterCoroutine(misuseDescriptionText, misuseDescription));
            }
        }
        #endregion

        #region === Typewriter & Glitch ===
        private void StartTypewriterEffect(TextMeshProUGUI textComponent, string fullText)
        {
            if (textComponent == null || string.IsNullOrEmpty(fullText)) return;
            if (_typewriterCoroutine != null) StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = StartCoroutine(TypewriterCoroutine(textComponent, fullText));
        }

        private IEnumerator TypewriterCoroutine(TextMeshProUGUI textComponent, string fullText)
        {
            textComponent.text = "";
            for (int i = 0; i <= fullText.Length; i++)
            {
                if (textComponent == null) yield break;
                textComponent.text = fullText.Substring(0, i);
                yield return new WaitForSeconds(typewriterSpeed);
            }
        }

        private void StartGlitchAnimation()
        {
            if (_glitchCoroutine != null) StopCoroutine(_glitchCoroutine);
            _glitchCoroutine = StartCoroutine(GlitchCoroutine());
        }

        private IEnumerator GlitchCoroutine()
        {
            while (glitchImage != null && glitchImage.gameObject.activeInHierarchy)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-glitchIntensity, glitchIntensity),
                    Random.Range(-glitchIntensity, glitchIntensity),
                    0f);

                glitchImage.transform.localPosition = _glitchOriginalPosition + offset;
                yield return new WaitForSeconds(glitchInterval);

                glitchImage.transform.localPosition = _glitchOriginalPosition;
                yield return new WaitForSeconds(glitchInterval * 0.1f);
            }
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion
    }
}
