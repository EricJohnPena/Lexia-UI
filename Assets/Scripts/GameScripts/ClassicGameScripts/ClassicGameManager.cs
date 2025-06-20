using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ClassicGameManager : MonoBehaviour
{
    private Trie wordTrie;
    public static ClassicGameManager instance;

    private Texture2D currentTexture; // Added to hold the current texture

    [SerializeField]
    private Text questionText;

    [SerializeField]
    private Image questionImage;

    // Remove the fixed array and add prefab + parent for dynamic answer slots
    [SerializeField]
    private WordData answerWordPrefab; // Assign in Inspector

    [SerializeField]
    private RectTransform answerHolderRect; // Assign in Inspector (parent container)

    [SerializeField]
    private GridLayoutGroup answerGridLayout; // Assign in Inspector

    private List<WordData> answerWordList = new List<WordData>();

    [SerializeField]
    private GameObject gameOver;

    [SerializeField]
    private Button[] keyboardButtons;

    [SerializeField]
    private Button backspaceButton;

    [SerializeField]
    private Button passButton; // Assign the Pass button in the Inspector

    [SerializeField]
    private Button hintButton; // Assign the hint button in the Inspector

    [SerializeField]
    private Text hintCounterText; // Assign the Text UI in the Inspector

    private int hintCounter = 3; // Maximum number of hints allowed

    private List<int> skippedQuestions = new List<int>(); // Track skipped questions
    private int totalSkipsUsed = 0; // Track the total number of skips used

    public TimerManager timerManager; // Assign in the Inspector

    private GameProgressHandler gameProgressHandler; // Added declaration
    public GameObject englishAnswerWordPrefab; // Assign in Inspector
    public GameObject scienceAnswerWordPrefab; // Assign in Inspector

    private KeyboardQuestionList defaultQuestionData = new KeyboardQuestionList
    {
        questions = new List<KeyboardQuestion>
        {
            new KeyboardQuestion
            {
                questionText = "No Questions Loaded",
                answer = "ANSWER1",
                imageData = "DefaultImages/default_image1",
            },
            new KeyboardQuestion
            {
                questionText = "No Questions Loaded",
                answer = "ANSWER2",
                imageData = "DefaultImages/default_image2",
            },
        },
    };
    private KeyboardQuestionList questionData;
    private int currentQuestionIndex = 0;
    private string currentAnswer;
    private int currentAnswerIndex = 0;
    private GameStatus gameStatus = GameStatus.Playing;
    private List<int> selectedKeyIndex;
    public bool isGameInitialized = false; // Flag to track initialization
    private bool isRefreshing = false; // Prevent multiple refreshes
    private bool isLessonCompleted = false; // Track if the lesson is completed
    private List<string> availableGameModes = new List<string>();
    private bool isLessonCheckCompleted = false; // New flag to track lesson completion check
    private string apiUrl = $"{Web.BaseApiUrl}getClassicQuestions.php"; // Define API URL for fetching questions

    private HashSet<int> correctlyAnsweredQuestions = new HashSet<int>(); // Track correctly answered questions
    private int correctAnswers = 0;
    private int totalAttempts = 0;

    // New dictionary to store hinted indices per question index
    private Dictionary<int, HashSet<int>> questionHintedIndices =
        new Dictionary<int, HashSet<int>>();

    private HashSet<int> hintedIndices = new HashSet<int>(); // Add this field to track hinted letters

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        gameObject.SetActive(true); // Ensure the GameObject is active

        selectedKeyIndex = new List<int>();
        wordTrie = new Trie();

        LoadValidWords();

        currentQuestionIndex = 0; // Reset question index
        questionData = defaultQuestionData; // Reset question data to default

        SetupKeyboardListeners();

        if (passButton != null)
        {
            passButton.onClick.AddListener(PassQuestion);
        }

        if (hintButton != null)
        {
            // Set outline color based on subject id
            var outline = hintButton.GetComponent<UnityEngine.UI.Outline>();
            if (outline != null)
            {
                Color outlineColor = Color.white;
                if (LessonsLoader.subjectId == 1) // English
                {
                    outlineColor = new Color32(0, 102, 204, 255); // Example: blue
                }
                else if (LessonsLoader.subjectId == 2) // Science
                {
                    outlineColor = new Color32(0, 153, 0, 255); // Example: green
                }
                outline.effectColor = outlineColor;
            }
            hintButton.onClick.AddListener(RevealHint);
        }

        UpdateHintCounterUI();

        // Initialize GameProgressHandler reference
        gameProgressHandler = FindObjectOfType<GameProgressHandler>();
        if (gameProgressHandler == null)
        {
            Debug.LogError("GameProgressHandler not found in the scene.");
        }
    }

    private void OnDestroy()
    {
        if (passButton != null)
        {
            passButton.onClick.RemoveListener(PassQuestion);
        }

        if (hintButton != null)
        {
            hintButton.onClick.RemoveListener(RevealHint);
        }

        if (keyboardButtons != null)
        {
            for (int i = 0; i < keyboardButtons.Length; i++)
            {
                int index = i;
                keyboardButtons[i]
                    .onClick.RemoveListener(() => OnKeyPressed(keyboardButtons[index]));
            }
        }

        if (backspaceButton != null)
        {
            backspaceButton.onClick.RemoveListener(ResetLastWord);
        }

        // Destroy the current texture to prevent memory leaks
        if (currentTexture != null)
        {
            Destroy(currentTexture);
            currentTexture = null;
        }
    }

    void OnEnable()
    {
        Debug.Log("Classic game enabled.");

        if (!isRefreshing)
        {
            int subjectId = LessonsLoader.subjectId;
            int moduleId;

            if (string.IsNullOrEmpty(LessonsLoader.moduleNumber))
            {
                Debug.LogError(
                    "LessonsLoader.moduleNumber is null or empty. Cannot parse module number."
                );
                return;
            }

            try
            {
                moduleId = int.Parse(LessonsLoader.moduleNumber);
            }
            catch (System.Exception e)
            {
                Debug.LogError(
                    $"Failed to parse LessonsLoader.moduleNumber: {LessonsLoader.moduleNumber}. Error: {e.Message}"
                );
                return;
            }

            int moduleNumber = int.Parse(LessonsLoader.moduleNumber);
            int gameModeId = 1; // Assuming 1 is the ID for Classic mode

            StartCoroutine(
                CheckLessonCompletion(
                    int.Parse(PlayerPrefs.GetString("User ID")),
                    moduleNumber,
                    gameModeId,
                    subjectId
                )
            );
        }
    }

    private IEnumerator FetchGameModes(int subjectId, int moduleId, int lessonId)
    {
        string url =
            $"{Web.BaseApiUrl}getGameModeMappings.php?subject_id={subjectId}&module_id={moduleId}&lesson_id={lessonId}";
        Debug.Log("Fetching game modes from URL: " + url);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string jsonResponse = www.downloadHandler.text;
                    Debug.Log("Game Modes Response: " + jsonResponse);
                    availableGameModes = JsonUtility.FromJson<List<string>>(jsonResponse);
                    Debug.Log("Available Game Modes: " + string.Join(", ", availableGameModes));
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error parsing game modes response: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("Failed to fetch game modes: " + www.error);
            }
        }
    }

    private IEnumerator CheckLessonCompletion(
        int studentId,
        int moduleNumber,
        int gameModeId,
        int subjectId
    )
    {
        if (isRefreshing)
        {
            Debug.LogWarning("CheckLessonCompletion is already running. Skipping...");
            yield break;
        }

        // Show loading screen at the start of lesson completion check
        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.ShowLoadingScreen();
        }

        isRefreshing = true; // Mark as running
        Debug.Log(
            $"CheckLessonCompletion called with studentId={studentId}, moduleNumber={moduleNumber}, gameModeId={gameModeId}, subjectId={subjectId}"
        );

        if (studentId <= 0 || moduleNumber <= 0 || gameModeId <= 0 || subjectId <= 0)
        {
            Debug.LogError(
                $"Invalid parameters: studentId={studentId}, moduleNumber={moduleNumber}, gameModeId={gameModeId}, subjectId={subjectId}"
            );
            isRefreshing = false; // Reset flag

            yield break;
        }
        int maxRetries = 3;
        int attempt = 0;
        float retryDelay = 2f; // seconds
        string url =
            $"{Web.BaseApiUrl}checkLessonCompletion.php?student_id={studentId}&module_number={moduleNumber}&game_mode_id={gameModeId}&subject_id={subjectId}";
        Debug.Log("Checking lesson completion from URL: " + url);
        while (attempt < maxRetries)
        {
            attempt++;
            Debug.Log($"Attempt {attempt} to check lesson completion.");
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string response = www.downloadHandler.text;
                        Debug.Log("Lesson Completion Response: " + response);
                        isLessonCompleted = response.Trim() == "true";
                        if (GameLoadingManager.Instance != null)
                        {
                            GameLoadingManager.Instance.HideLoadingScreen();
                        }
                        break; // Exit loop on success
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Error parsing lesson completion response: " + e.Message);
                        isLessonCompleted = false;
                    }
                }
                else
                {
                    Debug.LogError("Failed to check lesson completion: " + www.error);
                    yield return new WaitForSeconds(retryDelay); // Wait before retrying
                }
            }
        }

        isLessonCheckCompleted = true; // Mark lesson check as completed
        isRefreshing = false; // Reset flag

        if (isLessonCompleted)
        {
            // Hide loading screen before handling lesson state
            if (GameLoadingManager.Instance != null)
            {
                GameLoadingManager.Instance.HideLoadingScreen();
            }
            HandleLessonCompleted();
        }
        else
        {
            // Don't hide loading screen here as LoadQuestionData will handle it
            StartCoroutine(
                LoadQuestionData(LessonsLoader.subjectId, int.Parse(LessonsLoader.moduleNumber))
            );
        }
    }

    private IEnumerator UpdateGameCompletionStatus(
        int studentId,
        int moduleNumber,
        int gameModeId,
        int subjectId,
        float solveTime
    )
    {
        int maxRetries = 3;
        int attempt = 0;
        float retryDelay = 2f; // seconds
        string url = $"{Web.BaseApiUrl}updateGameCompletion.php";
        WWWForm form = new WWWForm();
        form.AddField("student_id", studentId);
        form.AddField("module_number", moduleNumber);
        form.AddField("game_mode_id", gameModeId);
        form.AddField("subject_id", subjectId);
        form.AddField("solve_time", Mathf.FloorToInt(solveTime)); // Save solve time in seconds
        while (attempt < maxRetries)
        {
            attempt++;
            Debug.Log($"Attempt {attempt} to update game completion status.");
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Game completion status updated successfully.");
                    break; // Exit loop on success
                }
                else
                {
                    Debug.LogError("Failed to update game completion status: " + www.error);
                    yield return new WaitForSeconds(retryDelay); // Wait before retrying
                }
            }
        }
    }

    private void HandleLessonCompleted()
    {
        Debug.Log("Lesson is already completed.");
        questionText.text = "Lesson Completed!";
        timerManager?.StopTimer(); // Stop the timer when the lesson is completed
        gameOver.SetActive(true);
    }

    private void HandleLessonState()
    {
        if (!isLessonCheckCompleted)
        {
            Debug.LogWarning("Lesson completion check not finalized. Skipping HandleLessonState.");
            return; // Exit early if lesson completion check is not finalized
        }

        if (isLessonCompleted)
        {
            Debug.Log("Lesson is already completed.");
            questionText.text = "Lesson Completed!";
            timerManager?.StopTimer();
            gameOver.SetActive(true);

            return; // Exit early to prevent further execution
        }

        Debug.Log("Lesson not completed. Loading lesson data...");

        // Ensure LoadQuestionData is only called if the lesson is not completed
        if (!isRefreshing)
        {
            StartCoroutine(
                LoadQuestionData(LessonsLoader.subjectId, int.Parse(LessonsLoader.moduleNumber))
            );
        }
    }

    public void RefreshClassicGameData()
    {
        if (isRefreshing)
        {
            Debug.Log("Refresh already in progress. Skipping...");
            return;
        }

        Debug.Log("Refreshing classic game data...");
        isRefreshing = true; // Mark as refreshing

        // Reset game state
        ResetGameState();

        // Reload classic game data
        StartCoroutine(
            LoadQuestionData(LessonsLoader.subjectId, int.Parse(LessonsLoader.moduleNumber))
        );
    }

    public void ResetGameInitialization()
    {
        // Method to reset the initialization flag if needed
        isGameInitialized = false;
    }

    private void ResetGameState()
    {
        Debug.Log("Resetting Classic Game state...");
        isRefreshing = false;
        isLessonCompleted = false;
        currentQuestionIndex = 0; // Ensure we start from the first question
        currentAnswerIndex = 0;
        hintCounter = 3; // Reset hint counter
        selectedKeyIndex.Clear();
        skippedQuestions.Clear();
        correctlyAnsweredQuestions.Clear();
        questionHintedIndices.Clear(); // Clear hinted indices dictionary
        hintedIndices.Clear(); // Clear hinted indices set
        gameStatus = GameStatus.Playing;

        // Reset GameProgressHandler counters
        if (gameProgressHandler != null)
        {
            gameProgressHandler.ResetVocabularyRangeCounters();
        }

        // Reset UI
        if (questionText != null)
        {
            questionText.text = "";
        }

        // Clear answer slots
        foreach (var word in answerWordList)
        {
            if (word != null)
                Destroy(word.gameObject);
        }
        answerWordList.Clear();

        // Reset keyboard buttons
        foreach (var button in keyboardButtons)
        {
            if (button != null)
            {
                button.gameObject.SetActive(true);
            }
        }

        if (gameOver != null)
        {
            gameOver.SetActive(false);
        }

        // Update hint counter UI after reset
        UpdateHintCounterUI();
    }

    private void LoadValidWords()
    {
        // Insert valid words into the Trie
        foreach (var question in defaultQuestionData.questions)
        {
            wordTrie.Insert(question.answer.ToUpper());
        }

        if (questionData != null && questionData.questions != null)
        {
            foreach (var question in questionData.questions)
            {
                wordTrie.Insert(question.answer.ToUpper());
            }
        }

        Debug.Log("Valid words loaded into Trie.");
    }

    private IEnumerator LoadWordsFromJson(string path)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(path))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string jsonText = www.downloadHandler.text;
                    KeyboardQuestionList questionData = JsonUtility.FromJson<KeyboardQuestionList>(
                        jsonText
                    );

                    foreach (var question in questionData.questions)
                    {
                        wordTrie.Insert(question.answer.ToUpper());
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error parsing JSON: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("Failed to load words from file: " + www.error);
            }
        }
    }

    private IEnumerator LoadQuestionData(int subjectId, int moduleId)
    {
        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.ShowLoadingScreen();
        }

        Debug.Log("LoadQuestionData called.");
        Debug.Log($"{subjectId}, {moduleId} from loadlessondata");

        // Reset game state before loading new questions
        ResetGameState();

        int maxRetries = 3;
        int attempt = 0;
        float retryDelay = 2f; // seconds
        string url = $"{apiUrl}?subject_id={subjectId}&module_id={moduleId}";
        Debug.Log("Fetching questions from URL: " + url);

        while (attempt < maxRetries)
        {
            attempt++;
            Debug.Log($"Attempt {attempt} to load questions.");
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string jsonText = www.downloadHandler.text;
                        Debug.Log("Raw JSON Response: " + jsonText);
                        questionData = JsonUtility.FromJson<KeyboardQuestionList>(jsonText);

                        if (
                            questionData == null
                            || questionData.questions == null
                            || questionData.questions.Count == 0
                        )
                        {
                            Debug.LogWarning(
                                "Loaded JSON is empty or invalid. Using default questions."
                            );
                            questionData = defaultQuestionData;
                        }

                        // Shuffle the questions
                        questionData.questions = ShuffleList.ShuffleListItems(
                            questionData.questions
                        );

                        // Ensure we start from the first question
                        currentQuestionIndex = 0;

                        if (questionData != null && questionData.questions.Count > 0)
                        {
                            timerManager?.StartTimer(); // Start the timer when questions are loaded
                            SetQuestion();
                        }
                        break; // Exit loop on success
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Error parsing JSON: " + e.Message);
                        if (attempt >= maxRetries)
                        {
                            questionData = defaultQuestionData;
                            currentQuestionIndex = 0;
                            SetQuestion();
                        }
                    }
                }
                else
                {
                    Debug.LogError("Failed to load questions: " + www.error);
                    if (attempt >= maxRetries)
                    {
                        questionData = defaultQuestionData;
                        currentQuestionIndex = 0;
                        SetQuestion();
                    }
                    else
                    {
                        yield return new WaitForSeconds(retryDelay);
                    }
                }
            }
        }

        isRefreshing = false; // Mark refresh as complete

        // Hide loading screen
        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.HideLoadingScreen();
        }
    }

    private void SetQuestion()
    {
        if (currentQuestionIndex >= questionData.questions.Count)
        {
            timerManager?.StopTimer(); // Stop the timer when the game ends
            gameOver.SetActive(true);
            Debug.Log("No more questions available.");
            return;
        }

        currentAnswerIndex = 0;
        selectedKeyIndex.Clear();

        KeyboardQuestion currentQuestion = questionData.questions[currentQuestionIndex];
        questionText.text = currentQuestion.questionText;
        currentAnswer = currentQuestion.answer.ToUpper().Trim(); // Ensure answer is uppercase and trimmed
        StartCoroutine(LoadQuestionImage(currentQuestion.imageData));

        Debug.Log($"Current Question Index: {currentQuestionIndex}");
        Debug.Log($"Total Questions: {questionData.questions.Count}");
        Debug.Log($"Current Answer: {currentAnswer}");

        ResetQuestion();

        // --- FIX: Always clear all hint visuals and only apply hints for this question ---
        for (int i = 0; i < answerWordList.Count; i++)
        {
            answerWordList[i].SetHintStyle(false);
        }
        // Remove any previous hint indices
        hintedIndices.Clear();
        // Restore hints if any for this question
        if (questionHintedIndices.ContainsKey(currentQuestionIndex))
        {
            HashSet<int> hintedIndicesForQuestion = questionHintedIndices[currentQuestionIndex];
            foreach (int index in hintedIndicesForQuestion)
            {
                answerWordList[index].SetChar(currentAnswer[index]);
                answerWordList[index].SetHintStyle(true);
                hintedIndices.Add(index);
            }
            // Don't decrement hint counter here since these hints were already used
            UpdateHintCounterUI();
        }
        else
        {
            // No hints for this question, ensure hint visuals are off
            for (int i = 0; i < answerWordList.Count; i++)
            {
                answerWordList[i].SetHintStyle(false);
            }
        }

        gameStatus = GameStatus.Playing;
    }

    private IEnumerator LoadQuestionImage(string imageData)
    {
        questionImage.gameObject.SetActive(false);

        if (string.IsNullOrEmpty(imageData))
        {
            Debug.LogWarning("Image data is null or empty.");
            yield break;
        }

        // Destroy old sprite and texture to prevent memory leaks
        if (questionImage.sprite != null)
        {
            if (questionImage.sprite.texture != null)
            {
                Destroy(questionImage.sprite.texture);
            }
            Destroy(questionImage.sprite);
            questionImage.sprite = null;
        }

        // Destroy previously created texture to prevent memory leaks
        if (currentTexture != null)
        {
            Destroy(currentTexture);
            currentTexture = null;
        }

        // Remove the data URI scheme prefix if present
        string base64Data = imageData;
        int commaIndex = imageData.IndexOf(',');
        if (commaIndex >= 0)
        {
            base64Data = imageData.Substring(commaIndex + 1);
        }

        byte[] imageBytes = System.Convert.FromBase64String(base64Data);
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(imageBytes))
        {
            currentTexture = texture; // Store reference to destroy later

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            questionImage.sprite = sprite;
            questionImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Failed to load texture from image data.");
        }

        yield return null;
    }

    private void SetupKeyboardListeners()
    {
        for (int i = 0; i < keyboardButtons.Length; i++)
        {
            int index = i;
            keyboardButtons[i].onClick.AddListener(() => OnKeyPressed(keyboardButtons[index]));
        }

        if (backspaceButton != null)
        {
            backspaceButton.onClick.AddListener(ResetLastWord);
            // Set outline color based on subject id
            var outline = backspaceButton.GetComponent<UnityEngine.UI.Outline>();
            if (outline != null)
            {
                Color outlineColor = Color.white;
                if (LessonsLoader.subjectId == 1) // English
                {
                    outlineColor = new Color32(0, 102, 204, 255); // Example: blue
                }
                else if (LessonsLoader.subjectId == 2) // Science
                {
                    outlineColor = new Color32(0, 153, 0, 255); // Example: green
                }
                outline.effectColor = outlineColor;
            }
        }
    }

    private void ResetQuestion()
    {
        // Destroy any existing answerWord objects
        foreach (var word in answerWordList)
        {
            if (word != null)
                Destroy(word.gameObject);
        }
        answerWordList.Clear();

        // Clear the hinted indices when resetting the question
        hintedIndices.Clear();

        // Configure grid layout for single row and dynamic cell size
        if (answerGridLayout != null && answerHolderRect != null)
        {
            answerGridLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            answerGridLayout.constraintCount = 1;
            answerGridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            answerGridLayout.childAlignment = TextAnchor.MiddleCenter;

            // Calculate cell size based on container width and answer length
            float containerWidth = answerHolderRect.rect.width;
            float spacing = answerGridLayout.spacing.x;
            float minCellSize = 40f;
            float maxCellSize = 100f;
            float cellSize =
                (containerWidth - (spacing * (currentAnswer.Length - 1))) / currentAnswer.Length;
            cellSize = Mathf.Clamp(cellSize, minCellSize, maxCellSize);
            answerGridLayout.cellSize = new Vector2(cellSize, cellSize);
        }

        // Dynamically instantiate answerWord objects based on currentAnswer length
        WordData prefabToUse = answerWordPrefab;
        if (LessonsLoader.subjectId == 1 && englishAnswerWordPrefab != null)
        {
            prefabToUse = englishAnswerWordPrefab.GetComponent<WordData>();
        }
        else if (LessonsLoader.subjectId == 2 && scienceAnswerWordPrefab != null)
        {
            prefabToUse = scienceAnswerWordPrefab.GetComponent<WordData>();
        }
        for (int i = 0; i < currentAnswer.Length; i++)
        {
            WordData wordObj = Instantiate(prefabToUse, answerHolderRect);
            wordObj.SetChar('_');
            wordObj.SetHintStyle(false);
            wordObj.gameObject.SetActive(true);
            answerWordList.Add(wordObj);
        }

        // Set outline color of every button based on subject id
        Color outlineColor = Color.white;
        if (LessonsLoader.subjectId == 1) // English
        {
            outlineColor = new Color32(0, 102, 204, 255); // Example: blue
        }
        else if (LessonsLoader.subjectId == 2) // Science
        {
            outlineColor = new Color32(0, 153, 0, 255); // Example: green
        }
        foreach (var button in keyboardButtons)
        {
            var outline = button.GetComponent<UnityEngine.UI.Outline>();
            var textColor = button.GetComponentInChildren<Text>();
            if (textColor != null)
            {
                textColor.color = outlineColor;
            }
            if (outline != null)
            {
                outline.effectColor = outlineColor;
            }
            button.gameObject.SetActive(true);
        }
        // Update hint button outline and image
        if (hintButton != null)
        {
            var outline = hintButton.GetComponent<UnityEngine.UI.Outline>();
            if (outline != null)
            {
                outline.effectColor = outlineColor;
            }
        }
        // Update backspace button outline and image
        if (backspaceButton != null)
        {
            var outline = backspaceButton.GetComponent<UnityEngine.UI.Outline>();
            if (outline != null)
            {
                outline.effectColor = outlineColor;
            }
        }
    }

    private void OnKeyPressed(Button keyButton)
    {
        if (gameStatus == GameStatus.Next || currentAnswerIndex >= currentAnswer.Length)
            return;

        // Find the next available index that is not already answered
        while (
            currentAnswerIndex < currentAnswer.Length
            && answerWordList[currentAnswerIndex].charValue != '_'
        )
        {
            currentAnswerIndex++;
        }

        if (currentAnswerIndex >= currentAnswer.Length)
        {
            Debug.Log("No available indices to place the letter.");
            return;
        }

        string keyPressed = keyButton.GetComponentInChildren<Text>().text.ToUpper();
        selectedKeyIndex.Add(System.Array.IndexOf(keyboardButtons, keyButton));

        answerWordList[currentAnswerIndex].SetChar(keyPressed[0]);
        currentAnswerIndex++;

        // Check if the answer is complete
        CheckIfAnswerComplete();
    }

    private void ResetLastWord()
    {
        if (selectedKeyIndex.Count > 0)
        {
            int index = selectedKeyIndex[selectedKeyIndex.Count - 1];

            // Check if the letter being deleted is a hint
            if (hintedIndices.Contains(currentAnswerIndex - 1))
            {
                Debug.Log("Cannot delete a hinted letter.");
                currentAnswerIndex--;
                return;
            }

            selectedKeyIndex.RemoveAt(selectedKeyIndex.Count - 1);
            currentAnswerIndex--;
            answerWordList[currentAnswerIndex].SetChar('_');
        }
    }

    private void CheckAnswer()
    {
        // Form the user input from the answerWordList
        string userInput = string.Join(
                "",
                answerWordList.Take(currentAnswer.Length).Select(a => a.charValue)
            )
            .Trim()
            .ToUpper();
        string expectedAnswer = currentAnswer.Trim().ToUpper();

        Debug.Log($"Expected Answer: {expectedAnswer}");
        Debug.Log($"User Input: {userInput}");

        totalAttempts++;
        if (userInput.Equals(expectedAnswer, System.StringComparison.OrdinalIgnoreCase))
        {
            correctAnswers++;
            Debug.Log("Correct Answer!");
            gameStatus = GameStatus.Next;

            correctlyAnsweredQuestions.Add(currentQuestionIndex);
            skippedQuestions.Remove(currentQuestionIndex);

            if (correctlyAnsweredQuestions.Count == questionData.questions.Count)
            {
                HandleGameCompletion();
            }
            else if (skippedQuestions.Count > 0)
            {
                HandleSkippedQuestions();
            }
            else
            {
                currentQuestionIndex++;
                if (currentQuestionIndex < questionData.questions.Count)
                {
                    Invoke(nameof(SetQuestion), 0.5f);
                }
            }
        }
        else
        {
            gameProgressHandler?.OnIncorrectAnswer(expectedAnswer);
            Debug.Log("Incorrect Answer!" + currentAnswer);
            ResetCurrentInput();
        }
    }

    private void ResetCurrentInput()
    {
        for (int i = 0; i < currentAnswerIndex; i++)
        {
            if (!hintedIndices.Contains(i))
            {
                answerWordList[i].SetChar('_');
                answerWordList[i].SetHintStyle(false);
            }
        }

        currentAnswerIndex = 0;
        selectedKeyIndex.Clear();
    }

    private void PassQuestion()
    {
        if (currentQuestionIndex < questionData.questions.Count)
        {
            Debug.Log($"Question {currentQuestionIndex} skipped.");
            totalSkipsUsed++; // Increment the total skips used
            gameProgressHandler?.OnSkipUsed(currentAnswer);

            // Add the current question to skippedQuestions only if it is unanswered and not already skipped
            if (
                !skippedQuestions.Contains(currentQuestionIndex)
                && !correctlyAnsweredQuestions.Contains(currentQuestionIndex)
            )
            {
                skippedQuestions.Add(currentQuestionIndex);
            }

            // Move to the next unanswered question or stay if none found
            int nextIndex = currentQuestionIndex + 1;
            while (
                nextIndex < questionData.questions.Count
                && (
                    correctlyAnsweredQuestions.Contains(nextIndex)
                    || skippedQuestions.Contains(nextIndex)
                )
            )
            {
                nextIndex++;
            }

            if (nextIndex < questionData.questions.Count)
            {
                currentQuestionIndex = nextIndex;
                SetQuestion();
            }
            else
            {
                // If no next unanswered question, revisit skipped questions
                HandleSkippedQuestions();
            }
        }
    }

    private void HandleSkippedQuestions()
    {
        if (skippedQuestions.Count > 0)
        {
            Debug.Log("Revisiting skipped questions...");

            // Find the next skipped question that is not answered yet
            int nextSkippedIndex = -1;
            for (int i = 0; i < skippedQuestions.Count; i++)
            {
                int skippedIndex = skippedQuestions[i];
                if (!correctlyAnsweredQuestions.Contains(skippedIndex))
                {
                    nextSkippedIndex = skippedIndex;
                    skippedQuestions.RemoveAt(i);
                    break;
                }
            }

            if (nextSkippedIndex != -1)
            {
                currentQuestionIndex = nextSkippedIndex;
                SetQuestion();
            }
            else if (correctlyAnsweredQuestions.Count == questionData.questions.Count)
            {
                HandleGameCompletion(); // Complete the game if all questions are answered correctly
            }
        }
        else if (correctlyAnsweredQuestions.Count == questionData.questions.Count)
        {
            HandleGameCompletion(); // Complete the game if all questions are answered correctly
        }
    }

    private void HandleGameCompletion()
    {
        Debug.Log("All questions answered correctly. Game over.");
        timerManager?.StopTimer();
        gameOver.SetActive(true);

        int studentId = int.Parse(PlayerPrefs.GetString("User ID"));
        int gameModeId = 1; // Classic mode ID
        int subjectId = LessonsLoader.subjectId;
        float solveTime = timerManager?.elapsedTime ?? 0;
        int module_number = int.Parse(LessonsLoader.moduleNumber);

        // Show loading screen and update status/attributes, then hide loading screen when done
        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.ShowLoadingScreenUntilComplete(
                () =>
                {
                    StartCoroutine(
                        UpdateGameCompletionAndAttributes(
                            studentId,
                            module_number,
                            gameModeId,
                            subjectId,
                            solveTime
                        )
                    );
                }
            );
        }
        else
        {
            StartCoroutine(
                UpdateGameCompletionAndAttributes(
                    studentId,
                    module_number,
                    gameModeId,
                    subjectId,
                    solveTime
                )
            );
        }
    }

    private IEnumerator UpdateGameCompletionAndAttributes(
        int studentId,
        int module_number,
        int gameModeId,
        int subjectId,
        float solveTime
    )
    {
        // Update game completion status
        var completionCoroutine = StartCoroutine(
            UpdateGameCompletionStatus(studentId, module_number, gameModeId, subjectId, solveTime)
        );
        yield return completionCoroutine;

        if (completionCoroutine != null)
        {
            // Update attributes
            var attributesCoroutine = StartCoroutine(UpdateAttributes());
            yield return attributesCoroutine;
        }

        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.HideLoadingScreen();
        }
    }

    private IEnumerator UpdateAttributes()
    {
        int studentId = int.Parse(PlayerPrefs.GetString("User ID"));
        int module_number = int.Parse(LessonsLoader.moduleNumber);
        int gameModeId = 1; // Classic mode ID
        int subjectId = LessonsLoader.subjectId;
        Debug.Log(
            $"Updating attributes for studentId: {studentId}, module_number: {module_number}"
        );

        if (gameProgressHandler != null)
        {
            yield return gameProgressHandler.UpdateAccuracy(
                studentId,
                module_number,
                gameModeId,
                subjectId,
                correctAnswers,
                totalAttempts
            );

            yield return gameProgressHandler.UpdateSpeed(
                studentId,
                module_number,
                gameModeId,
                subjectId,
                timerManager?.elapsedTime ?? 0
            );

            yield return gameProgressHandler.UpdateProblemSolving(
                studentId,
                module_number,
                gameModeId,
                subjectId,
                3 - hintCounter, // Calculate total hints used
                totalSkipsUsed // Pass the total skips used
            );
            yield return gameProgressHandler.UpdateConsistency(
                studentId,
                10 // Use as the current score default value
            );

            // Update vocabulary range score
            yield return gameProgressHandler.UpdateVocabularyRange(
                studentId,
                module_number,
                gameModeId,
                subjectId,
                gameProgressHandler.ComplexWordAttemptCount,
                gameProgressHandler.hintUsageCount,
                gameProgressHandler.IncorrectAnswerCount
            );

            yield return gameProgressHandler.UpdateRetention(
                studentId,
                module_number,
                gameModeId,
                subjectId,
                gameProgressHandler.SkipRepeatingUsageCount,
                gameProgressHandler.HintOnRepeatingWordCount,
                gameProgressHandler.IncorrectRepeatingAnswerCount
            );
        }
    }

    public void LoadQuestionsOnButtonClick()
    {
        // Reset the question index and other related variables
        currentQuestionIndex = 0;
        currentAnswerIndex = 0;
        selectedKeyIndex.Clear();
        gameStatus = GameStatus.Playing;

        StartCoroutine(
            LoadQuestionData(LessonsLoader.subjectId, int.Parse(LessonsLoader.moduleNumber))
        );
    }

    private void RevealHint()
    {
        if (hintCounter <= 0)
        {
            Debug.Log("No hints remaining.");
            return;
        }

        if (currentAnswerIndex >= currentAnswer.Length)
        {
            Debug.Log("Answer is already complete. No hint needed.");
            return;
        }

        List<int> unrevealedIndices = new List<int>();
        for (int i = 0; i < currentAnswer.Length; i++)
        {
            if (answerWordList[i].charValue == '_')
            {
                unrevealedIndices.Add(i);
            }
        }

        if (unrevealedIndices.Count > 0)
        {
            int randomIndex = unrevealedIndices[
                UnityEngine.Random.Range(0, unrevealedIndices.Count)
            ];
            answerWordList[randomIndex].SetChar(currentAnswer[randomIndex]);
            answerWordList[randomIndex].SetHintStyle(true);
            hintedIndices.Add(randomIndex);
            // Store the hinted index for the current question only
            if (!questionHintedIndices.ContainsKey(currentQuestionIndex))
            {
                questionHintedIndices[currentQuestionIndex] = new HashSet<int>();
            }
            questionHintedIndices[currentQuestionIndex].Add(randomIndex);
            hintCounter--;
            UpdateHintCounterUI();
            gameProgressHandler?.OnHintUsed(currentAnswer);
            Debug.Log($"Hint revealed at index {randomIndex}: {currentAnswer[randomIndex]}");
            CheckIfAnswerComplete();
        }
        else
        {
            Debug.Log("No unrevealed letters remain.");
        }
    }

    private void UpdateHintCounterUI()
    {
        if (hintCounterText != null)
        {
            hintCounterText.text = $"Hints: {hintCounter}";
        }
    }

    private void CheckIfAnswerComplete()
    {
        int filledCount = answerWordList.Count(a => a.charValue != '_');
        if (filledCount == currentAnswer.Length)
        {
            CheckAnswer();
        }
    }

    public void ReplayGame()
    {
        Debug.Log("Replay button clicked. Resetting Classic Game state...");

        // Reset game state
        ResetGameState();

        // Reload questions
        StartCoroutine(
            LoadQuestionData(LessonsLoader.subjectId, int.Parse(LessonsLoader.moduleNumber))
        );
    }

    [System.Serializable]
    public class KeyboardQuestion
    {
        public string questionText;
        public string answer;
        public string imageData;
    }

    [System.Serializable]
    public class KeyboardQuestionList
    {
        public List<KeyboardQuestion> questions;
    }
}
