using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DigitalForensicsQuiz
{
    public enum QuestionType
    {
        MultipleChoice,
        DragAndDrop
    }

    [System.Serializable]
    public class MinigameQuestionData
    {
        // Basic Info
        public string id;
        public QuestionType type;
        public string questionText;
        public string scenario;
        public string explanation;

        // Multiple Choice Data
        public List<string> options = new List<string>();
        public int correctAnswerIndex;

        // Drag and Drop Data
        public List<DragDropScenario> dragItems = new List<DragDropScenario>();
        public List<DropZone> dropZones = new List<DropZone>();
        public List<DragDropPair> correctPairs = new List<DragDropPair>();
        
        // Randomization settings
        public bool randomizeDragItems = true;
        public bool randomizeDropZones = false;

        // Performance cache
        [System.NonSerialized]
        private List<DragDropScenario> _cachedRandomizedDragItems;
        [System.NonSerialized]
        private List<DropZone> _cachedRandomizedDropZones;
        [System.NonSerialized]
        private System.Random _randomGenerator;

        // Validation
        public bool isValid => ValidateQuestion();

        bool ValidateQuestion()
        {
            if (string.IsNullOrEmpty(questionText)) return false;
            if (string.IsNullOrEmpty(explanation)) return false;

            switch (type)
            {
                case QuestionType.MultipleChoice:
                    return options.Count >= 2 && correctAnswerIndex >= 0 && correctAnswerIndex < options.Count;
                case QuestionType.DragAndDrop:
                    return dragItems.Count > 0 && dropZones.Count > 0 && 
                           correctPairs.Count > 0 &&
                           correctPairs.TrueForAll(pair => 
                               dragItems.Any(item => item.id == pair.dragItemId) &&
                               dropZones.Any(zone => zone.id == pair.dropZoneId));
                default:
                    return false;
            }
        }

        private void InitializeRandomGenerator()
        {
            if (_randomGenerator == null)
            {
                int seed = string.IsNullOrEmpty(id) ? UnityEngine.Random.Range(0, int.MaxValue) : id.GetHashCode();
                _randomGenerator = new System.Random(seed);
            }
        }

        public List<DragDropScenario> GetRandomizedDragItems()
        {
            if (_cachedRandomizedDragItems != null) return _cachedRandomizedDragItems;
            
            if (!randomizeDragItems) 
            {
                _cachedRandomizedDragItems = new List<DragDropScenario>(dragItems);
                return _cachedRandomizedDragItems;
            }
            
            InitializeRandomGenerator();
            
            // Fisher-Yates shuffle
            _cachedRandomizedDragItems = new List<DragDropScenario>(dragItems);
            for (int i = _cachedRandomizedDragItems.Count - 1; i > 0; i--)
            {
                int j = _randomGenerator.Next(0, i + 1);
                var temp = _cachedRandomizedDragItems[i];
                _cachedRandomizedDragItems[i] = _cachedRandomizedDragItems[j];
                _cachedRandomizedDragItems[j] = temp;
            }
            return _cachedRandomizedDragItems;
        }

        public List<DropZone> GetRandomizedDropZones()
        {
            if (_cachedRandomizedDropZones != null) return _cachedRandomizedDropZones;
            
            if (!randomizeDropZones)
            {
                _cachedRandomizedDropZones = new List<DropZone>(dropZones);
                return _cachedRandomizedDropZones;
            }
            
            InitializeRandomGenerator();
            
            // Fisher-Yates shuffle
            _cachedRandomizedDropZones = new List<DropZone>(dropZones);
            for (int i = _cachedRandomizedDropZones.Count - 1; i > 0; i--)
            {
                int j = _randomGenerator.Next(0, i + 1);
                var temp = _cachedRandomizedDropZones[i];
                _cachedRandomizedDropZones[i] = _cachedRandomizedDropZones[j];
                _cachedRandomizedDropZones[j] = temp;
            }
            return _cachedRandomizedDropZones;
        }

        public void ClearCache()
        {
            _cachedRandomizedDragItems = null;
            _cachedRandomizedDropZones = null;
            _randomGenerator = null;
        }

        public float GetDifficultyScore()
        {
            switch (type)
            {
                case QuestionType.MultipleChoice:
                    return Mathf.Clamp01((options.Count - 2) / 3f);
                case QuestionType.DragAndDrop:
                    return Mathf.Clamp01((dragItems.Count - 2) / 4f);
                default:
                    return 0.5f;
            }
        }

        public float GetEstimatedTime()
        {
            switch (type)
            {
                case QuestionType.MultipleChoice:
                    return 15f + (options.Count * 5f);
                case QuestionType.DragAndDrop:
                    return 30f + (dragItems.Count * 10f);
                default:
                    return 30f;
            }
        }
    }

    [System.Serializable]
    public class DragDropScenario
    {
        public string id;
        public string scenarioId;
        public string description;
        public string detailedInfo;
        
        [Header("Visual Settings")]
        public Sprite icon;
        public Color backgroundColor = Color.white;
        public Color textColor = Color.black;
        
        [Header("Mobile Optimization")]
        public bool allowTouchHighlight = true;
        public float minimumTouchSize = 80f;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(description);
        }

        public string GetFormattedDescription()
        {
            if (string.IsNullOrEmpty(scenarioId))
                return description;
            
            return $"{scenarioId}. {description}";
        }
    }

    [System.Serializable]
    public class DropZone
    {
        public string id;
        public string categoryName;
        public string description;
        
        [Header("Visual Settings")]
        public Sprite backgroundImage;
        public Color zoneColor = Color.gray;
        public Color textColor = Color.white;
        public Color highlightColor = Color.yellow;
        
        [Header("Mobile Optimization")]
        public float minimumSize = 100f;
        public bool enableHoverEffect = true;
        public bool enableDropAnimation = true;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(categoryName);
        }
    }

    [System.Serializable]
    public class DragDropPair
    {
        public string dragItemId;
        public string dropZoneId;
        public float confidence = 1f;
        
        public DragDropPair(string dragId, string dropId, float confidenceLevel = 1f)
        {
            dragItemId = dragId;
            dropZoneId = dropId;
            confidence = confidenceLevel;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(dragItemId) && !string.IsNullOrEmpty(dropZoneId);
        }
    }

    // Answer validation system
    public static class AnswerValidator
    {
        public static bool ValidateMultipleChoice(MinigameQuestionData question, int selectedAnswer)
        {
            if (question.type != QuestionType.MultipleChoice) return false;
            if (selectedAnswer < 0 || selectedAnswer >= question.options.Count) return false;
            return selectedAnswer == question.correctAnswerIndex;
        }

        public static bool ValidateDragAndDrop(MinigameQuestionData question, Dictionary<string, string> playerAnswers)
        {
            if (question.type != QuestionType.DragAndDrop) return false;
            if (playerAnswers.Count != question.correctPairs.Count) return false;

            foreach (var correctPair in question.correctPairs)
            {
                if (!playerAnswers.ContainsKey(correctPair.dragItemId)) return false;
                if (playerAnswers[correctPair.dragItemId] != correctPair.dropZoneId) return false;
            }

            return true;
        }

        public static float GetDragAndDropScore(MinigameQuestionData question, Dictionary<string, string> playerAnswers)
        {
            if (question.type != QuestionType.DragAndDrop) return 0f;
            
            float totalWeight = 0f;
            float correctWeight = 0f;
            
            foreach (var correctPair in question.correctPairs)
            {
                totalWeight += correctPair.confidence;
                
                if (playerAnswers.ContainsKey(correctPair.dragItemId) &&
                    playerAnswers[correctPair.dragItemId] == correctPair.dropZoneId)
                {
                    correctWeight += correctPair.confidence;
                }
            }

            return totalWeight > 0 ? correctWeight / totalWeight : 0f;
        }
    }

    // Question provider system
    public static class QuestionProvider
    {
        private static List<MinigameQuestionData> cachedQuestions;
        private static bool isDataLoaded = false;

        public static List<MinigameQuestionData> GetDigitalForensicsQuestions()
        {
            if (isDataLoaded && cachedQuestions != null)
                return cachedQuestions;

            cachedQuestions = new List<MinigameQuestionData>();

            try
            {
                cachedQuestions.Add(CreateDragAndDropQuestion());
                cachedQuestions.Add(CreateVideoContextQuestion());
                cachedQuestions.Add(CreateSIFTQuestion());
                cachedQuestions.Add(CreateMalinformationQuestion());
                cachedQuestions.Add(CreateSourceInvestigationQuestion());
                cachedQuestions.Add(CreateVisualDisinformationQuestion());

                ValidateQuestions();
                isDataLoaded = true;
                Debug.Log($"Successfully loaded {cachedQuestions.Count} questions");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load questions: {e.Message}");
                cachedQuestions = CreateFallbackQuestions();
                isDataLoaded = true;
            }

            return cachedQuestions;
        }

        private static void ValidateQuestions()
        {
            bool allValid = true;
            for (int i = 0; i < cachedQuestions.Count; i++)
            {
                if (!cachedQuestions[i].isValid)
                {
                    Debug.LogError($"Question {i + 1} (ID: {cachedQuestions[i].id}) is invalid!");
                    allValid = false;
                }
            }
            
            if (allValid)
            {
                Debug.Log($"All {cachedQuestions.Count} questions validated successfully!");
            }
        }

        static MinigameQuestionData CreateDragAndDropQuestion()
        {
            var question = new MinigameQuestionData
            {
                id = "Q001_DRAG_DROP",
                type = QuestionType.DragAndDrop,
                questionText = "Seret dan jatuhkan skenario ke klasifikasi yang tepat!",
                scenario = "Identifikasi jenis manipulasi informasi dengan menyeret setiap skenario ke kategori yang benar.",
                explanation = "DISINFORMASI = informasi palsu yang sengaja disebarkan untuk menipu. " +
                             "MISINFORMASI = informasi salah yang disebarkan tanpa niat jahat. " +
                             "MALINFORMASI = informasi benar yang disebarkan dengan niat jahat atau tanpa etika.",
                
                randomizeDragItems = true,
                randomizeDropZones = false,
                
                dragItems = new List<DragDropScenario>
                {
                    new DragDropScenario
                    {
                        id = "drag_scenario_A",
                        scenarioId = "A",
                        description = "Scammer sengaja menyamar sebagai @genz.berdampak",
                        detailedInfo = "Penyamaran identitas dengan tujuan menipu",
                        backgroundColor = new Color(0.9f, 0.9f, 1f, 1f),
                        textColor = Color.black,
                        minimumTouchSize = 90f
                    },
                    new DragDropScenario
                    {
                        id = "drag_scenario_B", 
                        scenarioId = "B",
                        description = "Aluna reshare tanpa cek keaslian",
                        detailedInfo = "Penyebaran informasi salah tanpa verifikasi",
                        backgroundColor = new Color(1f, 0.9f, 0.9f, 1f),
                        textColor = Color.black,
                        minimumTouchSize = 90f
                    },
                    new DragDropScenario
                    {
                        id = "drag_scenario_C",
                        scenarioId = "C",
                        description = "Kriminal mengancam menyalahgunakan data KTP asli Mbak Dyta",
                        detailedInfo = "Penggunaan data valid untuk tujuan jahat",
                        backgroundColor = new Color(0.9f, 1f, 0.9f, 1f),
                        textColor = Color.black,
                        minimumTouchSize = 90f
                    }
                },
                
                dropZones = new List<DropZone>
                {
                    new DropZone
                    {
                        id = "drop_disinformasi",
                        categoryName = "DISINFORMASI",
                        description = "Informasi palsu yang sengaja disebarkan untuk menipu",
                        zoneColor = new Color(1f, 0.2f, 0.2f, 0.3f),
                        highlightColor = new Color(1f, 0.2f, 0.2f, 0.6f),
                        textColor = Color.white,
                        minimumSize = 120f
                    },
                    new DropZone
                    {
                        id = "drop_misinformasi",
                        categoryName = "MISINFORMASI", 
                        description = "Informasi salah yang disebarkan tanpa niat jahat",
                        zoneColor = new Color(1f, 0.8f, 0.2f, 0.3f),
                        highlightColor = new Color(1f, 0.8f, 0.2f, 0.6f),
                        textColor = Color.black,
                        minimumSize = 120f
                    },
                    new DropZone
                    {
                        id = "drop_malinformasi",
                        categoryName = "MALINFORMASI",
                        description = "Informasi benar yang disebarkan dengan niat jahat",
                        zoneColor = new Color(0.8f, 0.2f, 1f, 0.3f),
                        highlightColor = new Color(0.8f, 0.2f, 1f, 0.6f),
                        textColor = Color.white,
                        minimumSize = 120f
                    }
                },
                
                correctPairs = new List<DragDropPair>
                {
                    new DragDropPair("drag_scenario_A", "drop_disinformasi", 1f),
                    new DragDropPair("drag_scenario_B", "drop_misinformasi", 1f),
                    new DragDropPair("drag_scenario_C", "drop_malinformasi", 1f)
                }
            };

            return question;
        }

        static MinigameQuestionData CreateVideoContextQuestion()
        {
            return new MinigameQuestionData
            {
                id = "Q002_VIDEO_CONTEXT",
                type = QuestionType.MultipleChoice,
                questionText = "Jenis informasi apa yang paling tepat menggambarkan konten tersebut?",
                scenario = "Sebuah akun publik membagikan ulang cuplikan video lama saat seorang dokter mengatakan bahwa \"vaksin tertentu masih dalam uji coba dan belum final.\" Potongan ini dipakai untuk memperkuat narasi anti-vaksin.",
                explanation = "Potongan informasi disengaja disebar ulang tanpa konteks waktu untuk menciptakan efek menyesatkan, padahal sumber awalnya benar. Ini adalah disinformasi karena ada niat untuk menyesatkan dengan menghilangkan konteks penting.",
                
                options = new List<string>
                {
                    "Misinformasi – kontennya tidak sengaja menyesatkan karena diambil dari sumber asli",
                    "Disinformasi – kontennya disengaja untuk menyesatkan, dengan menghilangkan konteks waktu",
                    "Malinformasi – informasinya benar, tapi dibagikan dengan tujuan edukatif",
                    "Disinformasi – kontennya salah dan tidak berasal dari sumber medis sama sekali"
                },
                correctAnswerIndex = 1
            };
        }

        static MinigameQuestionData CreateSIFTQuestion()
        {
            return new MinigameQuestionData
            {
                id = "Q003_SIFT_METHOD",
                type = QuestionType.MultipleChoice,
                questionText = "Apa langkah terbaik sesuai SIFT?",
                scenario = "Sebuah berita viral mengklaim bahwa \"air rebusan sereh dan jeruk nipis terbukti membunuh virus COVID-19.\" Kamu menemukan ada 3 sumber:\n" +
                          "A: Akun blog kesehatan pribadi\n" +
                          "B: Portal berita nasional tanpa referensi ilmiah\n" +
                          "C: Artikel jurnal kedokteran internasional",
                explanation = "Dalam kerangka SIFT, langkah tepat adalah 'trace claim' ke sumber primer yang kredibel, yaitu jurnal kedokteran. Popularitas atau opini publik bukan indikator kebenaran ilmiah.",
                
                options = new List<string>
                {
                    "Bandingkan ketiga sumber, lalu pilih yang paling meyakinkan dari bahasanya",
                    "Menggunakan artikel yang paling banyak dishare sebagai yang paling sah",
                    "Melacak ke jurnal kedokteran untuk melihat apakah ada studi pendukung",
                    "Menanyakan ke grup WhatsApp dan pilih jawaban terbanyak"
                },
                correctAnswerIndex = 2
            };
        }

        static MinigameQuestionData CreateMalinformationQuestion()
        {
            return new MinigameQuestionData
            {
                id = "Q004_MALINFORMATION",
                type = QuestionType.MultipleChoice,
                questionText = "Jenis manipulasi digital apa yang sedang terjadi?",
                scenario = "Seseorang membocorkan data pribadi pejabat publik yang valid, termasuk alamat rumah dan nama anak, lalu menambahkan caption bernada provokatif:\n" +
                          "\"Biar rakyat tahu siapa yang sebenarnya korup. Bagikan sebanyak mungkin!\"",
                explanation = "Malinformasi = informasi yang benar tapi disebar dengan niat jahat atau tanpa etika, seperti doxing atau memprovokasi. Ini yang paling berbahaya secara moral karena melanggar privasi dan keamanan.",
                
                options = new List<string>
                {
                    "Disinformasi – informasi palsu tentang pejabat disebarkan untuk menipu",
                    "Misinformasi – informasi benar tapi dibagikan secara keliru tanpa niat buruk",
                    "Malinformasi – informasi valid yang digunakan untuk menyakiti atau menyerang",
                    "Hoaks biasa – karena tidak ada unsur digital yang dimanipulasi"
                },
                correctAnswerIndex = 2
            };
        }

        static MinigameQuestionData CreateSourceInvestigationQuestion()
        {
            return new MinigameQuestionData
            {
                id = "Q005_SOURCE_INVESTIGATION",
                type = QuestionType.MultipleChoice,
                questionText = "Apa yang paling tepat kamu lakukan sebelum mempercayai isi kontennya?",
                scenario = "Sebuah akun TikTok sering membagikan \"fakta-fakta mengejutkan\" seputar konspirasi dunia. Profilnya tidak memuat identitas, dan klaimnya tidak pernah menyertakan sumber. Namun, akun itu punya jutaan follower dan videonya sering FYP.",
                explanation = "Langkah \"Investigate the source\" dari SIFT meminta kita melihat riwayat dan reputasi digital sang pembuat konten. Followers banyak bukan jaminan kebenaran. Kredibilitas harus dievaluasi berdasarkan transparansi dan akurasi historis.",
                
                options = new List<string>
                {
                    "Langsung berhenti menonton karena pasti hoaks",
                    "Telusuri kredibilitas akun dan cek apakah pernah dikoreksi oleh pemeriksa fakta",
                    "Bagikan dulu, lalu klarifikasi kalau ternyata salah",
                    "Anggap semua konspirasi menarik dan layak dipercaya sebagian"
                },
                correctAnswerIndex = 1
            };
        }

        static MinigameQuestionData CreateVisualDisinformationQuestion()
        {
            return new MinigameQuestionData
            {
                id = "Q006_VISUAL_DISINFORMATION",
                type = QuestionType.MultipleChoice,
                questionText = "Mengapa menyebarkan meme seperti ini sangat berbahaya?",
                scenario = "Sebuah meme menyebar luas berisi gambar seseorang yang tampak mabuk di jalanan, dengan caption:\n" +
                          "\"Beginilah kalau generasi milenial jadi pemimpin.\"\n" +
                          "Kamu melacak gambar tersebut ternyata dari video lawas tahun 2015 yang tidak ada kaitannya dengan politik.",
                explanation = "Meme disinformasi visual seperti ini menggunakan gambar nyata dengan framing menyesatkan, yang bisa mencemarkan nama baik dan memperkuat stigma palsu. Ini melanggar etika digital karena mencampur fakta dan opini tanpa konteks yang tepat.",
                
                options = new List<string>
                {
                    "Karena bisa menurunkan elektabilitas generasi muda",
                    "Karena kontennya lucu dan bisa disalahartikan sebagai humor",
                    "Karena mencampur fakta dan opini tanpa konteks melanggar etika digital",
                    "Karena tidak menyebutkan sumber gambar dan tidak menyebut tahun"
                },
                correctAnswerIndex = 2
            };
        }

        static List<MinigameQuestionData> CreateFallbackQuestions()
        {
            var fallbackQuestions = new List<MinigameQuestionData>();
            
            fallbackQuestions.Add(new MinigameQuestionData
            {
                id = "FALLBACK_001",
                type = QuestionType.MultipleChoice,
                questionText = "Apa yang dimaksud dengan disinformasi?",
                scenario = "Pertanyaan cadangan untuk memastikan aplikasi tetap berjalan.",
                explanation = "Disinformasi adalah informasi palsu yang sengaja disebarkan untuk menipu atau menyesatkan orang lain.",
                options = new List<string>
                {
                    "Informasi yang salah tanpa sengaja",
                    "Informasi palsu yang sengaja disebarkan untuk menipu",
                    "Informasi benar yang disebarkan dengan niat jahat",
                    "Informasi yang tidak jelas sumbernya"
                },
                correctAnswerIndex = 1
            });
            
            return fallbackQuestions;
        }

        public static MinigameQuestionData GetQuestionById(string id)
        {
            var questions = GetDigitalForensicsQuestions();
            return questions.Find(q => q.id == id);
        }

        public static List<MinigameQuestionData> GetQuestionsByType(QuestionType type)
        {
            var questions = GetDigitalForensicsQuestions();
            return questions.FindAll(q => q.type == type);
        }

        public static void ClearCache()
        {
            cachedQuestions = null;
            isDataLoaded = false;
        }
    }

    // Mobile optimization utilities
    public static class MobileOptimizationUtils
    {
        public enum HapticFeedbackType
        {
            Light,
            Medium,
            Heavy
        }

        public static void TriggerHapticFeedback(HapticFeedbackType type)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#elif UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        public static Vector2 GetResponsiveSize(float baseSize, float minimumSize)
        {
            float scaleFactor = 1f;
            
            if (Screen.dpi > 0)
            {
                scaleFactor = Screen.dpi / 160f;
            }
            
            float adjustedSize = Mathf.Max(minimumSize, baseSize * scaleFactor);
            return new Vector2(adjustedSize, adjustedSize);
        }
    }
}