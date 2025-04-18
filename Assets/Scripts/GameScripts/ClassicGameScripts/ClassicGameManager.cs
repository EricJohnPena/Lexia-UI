using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ClassicGameManager : MonoBehaviour
{
    private Trie wordTrie;
    public static ClassicGameManager instance;

    [SerializeField]
    private Text questionText;

    [SerializeField]
    private Image questionImage; // New image component

    [SerializeField]
    private WordData[] answerWordArray;

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

    public TimerManager timerManager; // Assign in the Inspector

    private KeyboardQuestionList defaultQuestionData = new KeyboardQuestionList
    {
        questions = new List<KeyboardQuestion>
        {
            new KeyboardQuestion
            {
                questionText = "No Questions Loaded",
                answer = "ANSWER1",
                imagePath = "DefaultImages/default_image1",
            },
            new KeyboardQuestion
            {
                questionText = "No Questions Loaded",
                answer = "ANSWER2",
                imagePath = "DefaultImages/default_image2",
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
            hintButton.onClick.AddListener(RevealHint);
        }

        UpdateHintCounterUI();
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

            int lessonId = LessonUI.lesson_id;
            int gameModeId = 1; // Assuming 1 is the ID for Classic mode

            StartCoroutine(
                CheckLessonCompletion(
                    int.Parse(PlayerPrefs.GetString("User ID")),
                    lessonId,
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
        int lessonId,
        int gameModeId,
        int subjectId
    )
    {
        if (isRefreshing)
        {
            Debug.LogWarning("CheckLessonCompletion is already running. Skipping...");
            yield break;
        }

        isRefreshing = true; // Mark as running
        Debug.Log(
            $"CheckLessonCompletion called with studentId={studentId}, lessonId={lessonId}, gameModeId={gameModeId}, subjectId={subjectId}"
        );

        if (studentId <= 0 || lessonId <= 0 || gameModeId <= 0 || subjectId <= 0)
        {
            Debug.LogError(
                $"Invalid parameters: studentId={studentId}, lessonId={lessonId}, gameModeId={gameModeId}, subjectId={subjectId}"
            );
            isRefreshing = false; // Reset flag
            yield break;
        }

        string url =
            $"{Web.BaseApiUrl}checkLessonCompletion.php?student_id={studentId}&lesson_id={lessonId}&game_mode_id={gameModeId}&subject_id={subjectId}";
        Debug.Log("Checking lesson completion from URL: " + url);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            Debug.Log("Checking lesson completion from URL: " + url);

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string response = www.downloadHandler.text;
                    Debug.Log("Lesson Completion Response: " + response);

                    isLessonCompleted = response.Trim() == "true";
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
                isLessonCompleted = false;
            }
        }

        isLessonCheckCompleted = true; // Mark lesson check as completed
        isRefreshing = false; // Reset flag

        // Explicitly control the flow based on the lesson completion status
        if (isLessonCompleted)
        {
            HandleLessonCompleted();
        }
        else
        {
            StartCoroutine(
                LoadQuestionData(
                    LessonsLoader.subjectId,
                    int.Parse(LessonsLoader.moduleNumber),
                    LessonUI.lesson_id
                )
            );
        }
    }

    private IEnumerator UpdateGameCompletionStatus(
        int studentId,
        int lessonId,
        int gameModeId,
        int subjectId,
        float solveTime
    )
    {
        string url = $"{Web.BaseApiUrl}updateGameCompletion.php";
        WWWForm form = new WWWForm();
        form.AddField("student_id", studentId);
        form.AddField("lesson_id", lessonId);
        form.AddField("game_mode_id", gameModeId);
        form.AddField("subject_id", subjectId);
        form.AddField("solve_time", Mathf.FloorToInt(solveTime)); // Save solve time in seconds

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Game completion status updated successfully.");
            }
            else
            {
                Debug.LogError("Failed to update game completion status: " + www.error);
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
                LoadQuestionData(
                    LessonsLoader.subjectId,
                    int.Parse(LessonsLoader.moduleNumber),
                    LessonUI.lesson_id
                )
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
            LoadQuestionData(
                LessonsLoader.subjectId,
                int.Parse(LessonsLoader.moduleNumber),
                LessonUI.lesson_id
            )
        );
    }

    public void ResetGameInitialization()
    {
        // Method to reset the initialization flag if needed
        isGameInitialized = false;
    }

    private void ResetGameState()
    {
        isRefreshing = false; // Reset the refreshing flag
        isLessonCompleted = false; // Reset the lesson completion flag
        Debug.Log("Resetting classic game state...");

        // Reset variables
        currentQuestionIndex = 0;
        currentAnswerIndex = 0;
        selectedKeyIndex.Clear();
        gameStatus = GameStatus.Playing;

        // Reset UI
        if (questionText != null)
        {
            questionText.text = "";
        }

        foreach (var word in answerWordArray)
        {
            word.SetChar('_');
            word.gameObject.SetActive(true);
        }

        foreach (var button in keyboardButtons)
        {
            button.gameObject.SetActive(true);
        }

        if (gameOver != null)
        {

            gameOver.SetActive(false);
        }
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

    private IEnumerator LoadQuestionData(int subjectId, int moduleId, int lessonId)
    {
        Debug.Log("LoadQuestionData called.");
        Debug.Log($"{subjectId}, {moduleId}, {lessonId} from loadlessondata");
        Debug.Log(int.Parse(LessonsLoader.moduleNumber));

        string url = $"{apiUrl}?subject_id={subjectId}&module_id={moduleId}&lesson_id={lessonId}";
        Debug.Log("Fetching questions from URL: " + url);

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
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error parsing JSON: " + e.Message);
                    questionData = defaultQuestionData;
                }
            }
            else
            {
                Debug.LogWarning(
                    "Failed to fetch questions from the server. Using default questions."
                );
                questionData = defaultQuestionData;
            }
        }

        if (
            questionData == null
            || questionData.questions == null
            || questionData.questions.Count == 0
        )
        {
            Debug.LogWarning("No questions available. Using default questions.");
            questionData = defaultQuestionData;
        }

        if (questionData != null && questionData.questions.Count > 0)
        {
            timerManager?.StartTimer(); // Start the timer when questions are loaded
            SetQuestion();
        }
        isRefreshing = false; // Mark refresh as complete
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
        StartCoroutine(LoadQuestionImage(currentQuestion.imagePath));

        Debug.Log($"Current Question Index: {currentQuestionIndex}");
        Debug.Log($"Total Questions: {questionData.questions.Count}");
        Debug.Log($"Current Answer: {currentAnswer}");

        ResetQuestion();
        gameStatus = GameStatus.Playing;
    }

    private IEnumerator LoadQuestionImage(string imagePath)
    {
        questionImage.gameObject.SetActive(false);
        string fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, imagePath);

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(fullPath))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
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
                Debug.LogWarning($"Failed to load image: {imagePath}");
                questionImage.gameObject.SetActive(false);
            }
        }
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
        }
    }

    private void ResetQuestion()
    {
        // Reset all answerWordArray elements to inactive
        for (int i = 0; i < answerWordArray.Length; i++)
        {
            answerWordArray[i].gameObject.SetActive(false);
            answerWordArray[i].SetChar('_'); // Reset charValue to '_'
        }

        // Activate only the required number of elements based on currentAnswer length
        for (int i = 0; i < currentAnswer.Length; i++)
        {
            answerWordArray[i].gameObject.SetActive(true);
        }

        foreach (Button button in keyboardButtons)
        {
            button.gameObject.SetActive(true);
        }
    }

    private void OnKeyPressed(Button keyButton)
    {
        if (gameStatus == GameStatus.Next || currentAnswerIndex >= currentAnswer.Length)
            return;

        // Find the next available index that is not already answered
        while (currentAnswerIndex < currentAnswer.Length && answerWordArray[currentAnswerIndex].charValue != '_')
        {
            currentAnswerIndex++;
        }

        if (currentAnswerIndex >= currentAnswer.Length)
        {
            Debug.Log("No available indices to place the letter.");
            return;
        }

        string keyPressed = keyButton.GetComponentInChildren<Text>().text.ToUpper(); // Convert input to uppercase
        selectedKeyIndex.Add(System.Array.IndexOf(keyboardButtons, keyButton));

        answerWordArray[currentAnswerIndex].SetChar(keyPressed[0]);
        currentAnswerIndex++;

        // Check if the answer is complete
        CheckIfAnswerComplete();
    }

    private void ResetLastWord()
    {
        if (selectedKeyIndex.Count > 0)
        {
            int index = selectedKeyIndex[selectedKeyIndex.Count - 1];
            selectedKeyIndex.RemoveAt(selectedKeyIndex.Count - 1);

            currentAnswerIndex--;
            answerWordArray[currentAnswerIndex].SetChar('_');
        }
    }

    private void CheckAnswer()
    {
        // Form the user input from the answerWordArray
        string userInput = string.Join(
                "",
                answerWordArray.Take(currentAnswer.Length).Select(a => a.charValue)
            )
            .Trim()
            .ToUpper();
        string expectedAnswer = currentAnswer.Trim().ToUpper();

        Debug.Log($"Expected Answer: {expectedAnswer}");
        Debug.Log($"User Input: {userInput}");

        // Compare user input with the expected answer
        if (userInput.Equals(expectedAnswer, System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("Correct Answer!");
            gameStatus = GameStatus.Next;

            // Mark the current question as correctly answered
            correctlyAnsweredQuestions.Add(currentQuestionIndex);

            // Remove the question from the skipped list if it was skipped earlier
            skippedQuestions.Remove(currentQuestionIndex);

            // Check if the game is complete
            if (correctlyAnsweredQuestions.Count == questionData.questions.Count)
            {
                CheckGameCompletion(); // Complete the game immediately
            }
            else if (skippedQuestions.Count > 0)
            {
                HandleSkippedQuestions(); // Revisit skipped questions
            }
            else
            {
                // Move to the next question if no skipped questions remain
                currentQuestionIndex++;
                if (currentQuestionIndex < questionData.questions.Count)
                {
                    Invoke(nameof(SetQuestion), 0.5f);
                }
            }
        }
        else
        {
            Debug.Log("Incorrect Answer!");
            ResetCurrentInput();
        }
    }

    private void ResetCurrentInput()
    {
        // Reset the current input in the answerWordArray
        for (int i = 0; i < currentAnswerIndex; i++)
        {
            answerWordArray[i].SetChar('_');
        }

        currentAnswerIndex = 0;
        selectedKeyIndex.Clear();
    }

    private void PassQuestion()
    {
        if (currentQuestionIndex < questionData.questions.Count)
        {
            Debug.Log($"Question {currentQuestionIndex} skipped.");

            // Ensure the current question is added to the skipped list only once
            if (!skippedQuestions.Contains(currentQuestionIndex) && 
                !correctlyAnsweredQuestions.Contains(currentQuestionIndex))
            {
                skippedQuestions.Add(currentQuestionIndex);
            }

            // Move to the next question
            currentQuestionIndex++;

            // Check if there are more questions to display
            if (currentQuestionIndex < questionData.questions.Count)
            {
                SetQuestion();
            }
            else
            {
                HandleSkippedQuestions(); // Handle skipped questions if all questions are traversed
            }
        }
    }

    private void HandleSkippedQuestions()
    {
        if (skippedQuestions.Count > 0)
        {
            Debug.Log("Revisiting skipped questions...");

            // Retrieve the first skipped question and remove it from the list
            currentQuestionIndex = skippedQuestions[0];
            skippedQuestions.RemoveAt(0);

            // Set the question for the skipped index
            SetQuestion();
        }
        else if (correctlyAnsweredQuestions.Count == questionData.questions.Count)
        {
            CheckGameCompletion(); // Complete the game if all questions are answered correctly
        }
    }

    private void CheckGameCompletion()
    {
        Debug.Log("All questions answered correctly. Game over.");
        timerManager?.StopTimer();
        gameOver.SetActive(true);

        // Update game completion status
        int studentId = int.Parse(PlayerPrefs.GetString("User ID"));
        int lessonId = LessonUI.lesson_id;
        int gameModeId = 1; // Assuming 1 is the ID for Classic mode
        int subjectId = LessonsLoader.subjectId;
        float solveTime = timerManager?.elapsedTime ?? 0;

        StartCoroutine(
            UpdateGameCompletionStatus(studentId, lessonId, gameModeId, subjectId, solveTime)
        );
    }

    public void LoadQuestionsOnButtonClick()
    {
        // Reset the question index and other related variables
        currentQuestionIndex = 0;
        currentAnswerIndex = 0;
        selectedKeyIndex.Clear();
        gameStatus = GameStatus.Playing;

        StartCoroutine(
            LoadQuestionData(
                LessonsLoader.subjectId,
                int.Parse(LessonsLoader.moduleNumber),
                LessonUI.lesson_id
            )
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
            if (answerWordArray[i].charValue == '_')
            {
                unrevealedIndices.Add(i);
            }
        }

        if (unrevealedIndices.Count > 0)
        {
            int randomIndex = unrevealedIndices[UnityEngine.Random.Range(0, unrevealedIndices.Count)];
            answerWordArray[randomIndex].SetChar(currentAnswer[randomIndex]);
            hintCounter--;
            UpdateHintCounterUI();
            Debug.Log($"Hint revealed at index {randomIndex}: {currentAnswer[randomIndex]}");

            // Check if the answer is complete
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
            hintCounterText.text = $"Hints Remaining: {hintCounter}";
        }
    }

    private void CheckIfAnswerComplete()
    {
        // Count the number of non-empty characters in the answerWordArray
        int filledCount = answerWordArray.Count(a => a.charValue != '_');

        // If the number of filled characters matches the answer length, check the answer
        if (filledCount == currentAnswer.Length)
        {
            CheckAnswer();
        }
    }

    [System.Serializable]
    public class KeyboardQuestion
    {
        public string questionText;
        public string answer;
        public string imagePath;
    }

    [System.Serializable]
    public class KeyboardQuestionList
    {
        public List<KeyboardQuestion> questions;
    }
}
