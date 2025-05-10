using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class JumbledLettersManager : MonoBehaviour
{
    private Trie wordTrie;
    public static JumbledLettersManager instance;

    [SerializeField]
    private Text questionText;

    [SerializeField]
    private GameObject gameOver;

    [SerializeField]
    private WordData[] answerWordArray;

    [SerializeField]
    private WordData[] optionWordArray;

    [SerializeField]
    private GridLayoutGroup optionGridLayout; // Reference to the GridLayoutGroup component

    [SerializeField]
    private RectTransform optionHolderRect; // Reference to the OptionHolder's RectTransform

    public TimerManager timerManager; // Assign in the Inspector

    [SerializeField]
    private Button passButton; // Assign the Pass button in the Inspector
    private List<int> skippedQuestions = new List<int>(); // Track skipped questions

    public Button hintButton; // Assign the hint button in the Inspector

    [SerializeField]
    private Text hintCounterText; // Assign the Text UI in the Inspector

    private int hintCounter = 3; // Maximum number of hints allowed

    private GameProgressHandler gameProgressHandler; // Added declaration

    private string apiUrl = $"{Web.BaseApiUrl}getJumbledLettersQuestions.php";

    private JLQuestionList questionData;
    private char[] charArray = new char[12];
    private int currentAnswerIndex = 0;
    private List<int> selectedWordIndex;
    private int currentQuestionIndex = 0;
    private GameStatus gameStatus = GameStatus.Playing;
    private string answerWord;
    private bool isLessonCompleted = false;
    private bool isRefreshing = false;

    private HashSet<int> correctlyAnsweredQuestions = new HashSet<int>(); // Track correctly answered questions

    private int correctAnswers = 0;
    private int totalAttempts = 0;
    private int totalSkipsUsed = 0; // Track the total number of skips used

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        selectedWordIndex = new List<int>();
        wordTrie = new Trie();

        if (passButton != null)
        {
            passButton.onClick.AddListener(PassQuestion);
        }

        if (hintButton != null)
        {
            hintButton.onClick.AddListener(RevealHint);
        }
        // Initialize GameProgressHandler reference
        gameProgressHandler = FindObjectOfType<GameProgressHandler>();
        if (gameProgressHandler == null)
        {
            Debug.LogError("GameProgressHandler not found in the scene.");
        }

        UpdateHintCounterUI();
        InitializeOptionGrid();
    }

    private void InitializeOptionGrid()
    {
        if (optionGridLayout == null)
        {
            Debug.LogError("GridLayoutGroup component not assigned!");
            return;
        }

        // Set initial grid properties
        //optionGridLayout.childAlignment = TextAnchor.MiddleCenter;
        optionGridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        optionGridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        optionGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        optionGridLayout.constraintCount = 6; // Default to 6 columns
        optionGridLayout.spacing = new Vector2(20f, 20f); // Add some spacing between cells
    }

    private void UpdateOptionGridLayout()
    {
        if (optionGridLayout == null || optionHolderRect == null)
            return;

        // Calculate the number of active options
        int activeOptions = optionWordArray.Count(option => option.gameObject.activeSelf);
        if (activeOptions == 0)
            return;

        // Set maximum columns to 6
        int maxColumns = 6;
        int columns = Mathf.Min(maxColumns, activeOptions);
        int rows = Mathf.CeilToInt((float)activeOptions / columns);

        // Update grid layout
        optionGridLayout.constraintCount = columns;

        // Calculate cell size based on container size and spacing
        float containerWidth = optionHolderRect.rect.width;
        float containerHeight = optionHolderRect.rect.height;
        float spacingX = optionGridLayout.spacing.x;
        float spacingY = optionGridLayout.spacing.y;

        float cellWidth = (containerWidth - (spacingX * (columns - 1))) / columns;
        float cellHeight = (containerHeight - (spacingY * (rows - 1))) / rows;

        // Set cell size (use the smaller dimension to maintain square cells)
        float cellSize = Mathf.Min(cellWidth, cellHeight);
        // Limit the maximum cell size to 100x100
        cellSize = Mathf.Min(cellSize, 100f);
        optionGridLayout.cellSize = new Vector2(cellSize, cellSize);

        // Calculate total width of all cells in a row
        float totalRowWidth = (cellSize * columns) + (spacingX * (columns - 1));

        // Calculate padding to center the grid
        float paddingX = (containerWidth - totalRowWidth) / 2;
        float paddingY = (containerHeight - (cellSize * rows + spacingY * (rows - 1))) / 2;

        // If we have more than 6 characters, we need to center the last row
        if (activeOptions > maxColumns)
        {
            int lastRowItems = activeOptions % maxColumns;
            if (lastRowItems == 0)
                lastRowItems = maxColumns;

            // Calculate the width of the last row
            float lastRowWidth = (cellSize * lastRowItems) + (spacingX * (lastRowItems - 1));

            // Calculate the padding needed to center the last row
            float lastRowPadding = (containerWidth - lastRowWidth) / 2;

            // Set the padding to center the last row
            optionGridLayout.padding = new RectOffset(
                Mathf.RoundToInt(lastRowPadding),
                Mathf.RoundToInt(lastRowPadding),
                Mathf.RoundToInt(paddingY),
                Mathf.RoundToInt(paddingY)
            );
        }
        else
        {
            // For 6 or fewer characters, center the entire grid
            optionGridLayout.padding = new RectOffset(
                Mathf.RoundToInt(paddingX),
                Mathf.RoundToInt(paddingX),
                Mathf.RoundToInt(paddingY),
                Mathf.RoundToInt(paddingY)
            );
        }
    }

    void OnEnable()
    {
        Debug.Log("Jumbled Letters game enabled.");

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

            int module_number = int.Parse(LessonsLoader.moduleNumber);
            int gameModeId = 2; // Assuming 2 is the ID for Jumbled Letters mode

            StartCoroutine(
                CheckLessonCompletion(
                    int.Parse(PlayerPrefs.GetString("User ID")),
                    module_number,
                    gameModeId,
                    subjectId
                )
            );
        }
    }

    private IEnumerator CheckLessonCompletion(
        int studentId,
        int module_number,
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
            $"CheckLessonCompletion called with studentId={studentId}, module_number={module_number}, gameModeId={gameModeId}, subjectId={subjectId}"
        );

        if (studentId <= 0 || module_number <= 0 || gameModeId <= 0)
        {
            Debug.LogError(
                $"Invalid parameters: studentId={studentId}, module_number={module_number}, gameModeId={gameModeId}, subjectId={subjectId}"
            );
            isRefreshing = false; // Reset flag
            yield break;
        }

        string url =
            $"{Web.BaseApiUrl}checkLessonCompletion.php?student_id={studentId}&module_number={module_number}&game_mode_id={gameModeId}&subject_id={subjectId}";
        Debug.Log("Checking lesson completion from URL: " + url);

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

        isRefreshing = false; // Reset flag

        if (isLessonCompleted)
        {
            HandleLessonState();
        }
        else
        {
            RefreshJumbledLettersData();
        }
    }

    private IEnumerator UpdateGameCompletionStatus(
        int studentId,
        int module_number,
        int gameModeId,
        int subjectId,
        float solveTime
    )
    {
        string url = $"{Web.BaseApiUrl}updateGameCompletion.php";
        WWWForm form = new WWWForm();
        form.AddField("student_id", studentId);
        form.AddField("module_number", module_number);
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

    private void HandleLessonState()
    {
        if (isLessonCompleted)
        {
            Debug.Log("Lesson is already completed.");
            questionText.text = "Lesson Completed!";
            timerManager?.StopTimer();
            gameOver.SetActive(true);
        }
        else
        {
            Debug.Log("Lesson not completed. Loading lesson data...");
            RefreshJumbledLettersData();
        }
        timerManager?.StopTimer(); // Stop the timer when lesson state is handled
    }

    public void RefreshJumbledLettersData()
    {
        Debug.Log("Refreshing Jumbled Letters data...");
        ResetGameState();
        StartCoroutine(
            LoadQuestionData(LessonsLoader.subjectId, int.Parse(LessonsLoader.moduleNumber))
        );
    }

    private void ResetGameState()
    {
        Debug.Log("Resetting Jumbled Letters game state...");
        currentQuestionIndex = 0;
        currentAnswerIndex = 0;
        hintCounter = 3; // Reset hint counter
        selectedWordIndex.Clear();
        skippedQuestions.Clear(); // Clear skipped questions
        correctlyAnsweredQuestions.Clear(); // Clear correctly answered questions
        gameStatus = GameStatus.Playing;

        // Reset GameProgressHandler counters
        if (gameProgressHandler != null)
        {
            gameProgressHandler.ResetVocabularyRangeCounters();
        }

        // Reset question text
        if (questionText != null)
        {
            questionText.text = "";
        }

        // Reset answer word array
        if (answerWordArray != null)
        {
            foreach (var word in answerWordArray)
            {
                if (word != null)
                {
                    word.SetChar('_');
                    word.gameObject.SetActive(true);
                }
            }
        }

        // Reset option word array
        if (optionWordArray != null)
        {
            foreach (var word in optionWordArray)
            {
                if (word != null)
                {
                    word.gameObject.SetActive(true);
                }
            }
        }

        // Reset game over panel
        if (gameOver != null)
        {
            gameOver.SetActive(false);
        }

        // Update hint counter UI
        UpdateHintCounterUI();
    }

    private IEnumerator LoadQuestionData(int subjectId, int module_number)
    {
        string url = $"{apiUrl}?subject_id={subjectId}&module_id={module_number}";
        Debug.Log("Fetching Jumbled Letters questions from URL: " + url);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string jsonText = www.downloadHandler.text;
                    Debug.Log("Raw JSON Response: " + jsonText);

                    questionData = JsonUtility.FromJson<JLQuestionList>(jsonText);

                    if (
                        questionData == null
                        || questionData.questions == null
                        || questionData.questions.Count == 0
                    )
                    {
                        Debug.LogWarning("No Jumbled Letters data received from the server.");
                        timerManager?.StopTimer();
                        gameOver.SetActive(true);
                        yield break;
                    }

                    foreach (var question in questionData.questions)
                    {
                        wordTrie.Insert(question.answer.ToUpper());
                    }

                    if (questionData != null && questionData.questions.Count > 0)
                    {
                        timerManager?.StartTimer(); // Start the timer when questions are loaded
                        SetQuestion();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error parsing JSON: " + e.Message);
                    timerManager?.StopTimer();
                    gameOver.SetActive(true);
                }
            }
            else
            {
                Debug.LogError("Failed to fetch Jumbled Letters data: " + www.error);
                timerManager?.StopTimer();
                gameOver.SetActive(true);
            }
        }
    }

    private void SetQuestion()
    {
        if (currentQuestionIndex >= questionData.questions.Count)
        {
            HandleSkippedQuestions(); // Handle skipped questions if all questions are traversed
            return;
        }

        currentAnswerIndex = 0;
        selectedWordIndex.Clear();

        JLQuestion currentQuestion = questionData.questions[currentQuestionIndex];
        questionText.text = currentQuestion.questionText;
        answerWord = currentQuestion.answer.ToUpper();

        ResetQuestion();

        // Only use letters from the answer word
        charArray = new char[answerWord.Length];
        for (int i = 0; i < answerWord.Length; i++)
        {
            charArray[i] = answerWord[i];
        }

        // Shuffle the characters
        charArray = ShuffleList.ShuffleListItems(charArray.ToList()).ToArray();

        // Only activate the number of option buttons needed for the answer
        for (int i = 0; i < optionWordArray.Length; i++)
        {
            if (i < charArray.Length)
            {
                optionWordArray[i].SetChar(charArray[i]);
                optionWordArray[i].gameObject.SetActive(true);
            }
            else
            {
                optionWordArray[i].gameObject.SetActive(false);
            }
        }

        // Update the grid layout after setting up the options
        UpdateOptionGridLayout();

        currentQuestionIndex++;
        gameStatus = GameStatus.Playing;
    }

    private void CheckIfAnswerComplete()
    {
        // Count the number of non-empty characters in the answerWordArray
        int filledCount = answerWordArray.Count(a => a.charValue != '_');

        // If the number of filled characters matches the answer length, check the answer
        if (filledCount == answerWord.Length)
        {
            CheckAnswer();
        }
    }

    public void SelectedOption(WordData wordData)
    {
        if (gameStatus == GameStatus.Next)
            return;

        // Find the first available (empty) index in answerWordArray
        int inputIndex = -1;
        for (int i = 0; i < answerWord.Length; i++)
        {
            if (answerWordArray[i].charValue == '_')
            {
                inputIndex = i;
                break;
            }
        }

        if (inputIndex == -1)
        {
            Debug.Log("No available indices to place the letter.");
            return;
        }

        // Insert the selectedWordIndex at the correct position
        if (inputIndex < selectedWordIndex.Count)
            selectedWordIndex.Insert(inputIndex, wordData.transform.GetSiblingIndex());
        else
            selectedWordIndex.Add(wordData.transform.GetSiblingIndex());

        wordData.gameObject.SetActive(false);
        answerWordArray[inputIndex].SetChar(wordData.charValue);

        // Update currentAnswerIndex to reflect the number of filled slots
        currentAnswerIndex = selectedWordIndex.Count;

        // Check if the answer is complete
        CheckIfAnswerComplete();
    }

    private void CheckAnswer()
    {
        string formedWord = string.Join(
                "",
                answerWordArray.Take(answerWord.Length).Select(a => a.charValue)
            )
            .ToUpper();
        string expectedAnswer = answerWord.ToUpper();

        Debug.Log($"Expected Answer: {expectedAnswer}");
        Debug.Log($"Formed Word: {formedWord}");

        totalAttempts++;
        if (formedWord.Equals(expectedAnswer, System.StringComparison.OrdinalIgnoreCase))
        {
            correctAnswers++;
            Debug.Log("Answer correct!");
            gameStatus = GameStatus.Next;

            // Mark the current question as correctly answered
            correctlyAnsweredQuestions.Add(currentQuestionIndex - 1);

            // Remove the question from the skipped list if it was skipped earlier
            skippedQuestions.Remove(currentQuestionIndex - 1);

            // Check if the game is complete
            if (correctlyAnsweredQuestions.Count == questionData.questions.Count)
            {
                CheckGameCompletion(); // Complete the game immediately
            }
            else
            {
                Debug.Log("Moving to the next unanswered or skipped question...");
                HandleSkippedQuestions(); // Continue to the next unanswered or skipped question
            }
        }
        else
        {
            gameProgressHandler.OnIncorrectAnswer(answerWord); // Call the incorrect answer method
            Debug.Log("Answer incorrect!" + answerWord);
            ResetCurrentInput();
        }
    }

    public void ShuffleOptions()
    {
        Debug.Log("Shuffling current options...");

        // Collect only the currently active option letters
        List<char> activeChars = new List<char>();
        for (int i = 0; i < optionWordArray.Length; i++)
        {
            if (optionWordArray[i].gameObject.activeSelf)
            {
                activeChars.Add(optionWordArray[i].charValue);
            }
        }

        // Shuffle the active characters
        activeChars = ShuffleList.ShuffleListItems(activeChars);

        // Reassign the shuffled characters back to the active options
        int activeIndex = 0;
        for (int i = 0; i < optionWordArray.Length; i++)
        {
            if (optionWordArray[i].gameObject.activeSelf)
            {
                optionWordArray[i].SetChar(activeChars[activeIndex]);
                activeIndex++;
            }
        }

        Debug.Log("Options shuffled successfully.");
    }

    public void ClearAnswer()
    {
        Debug.Log("Clearing current answer...");

        // Only clear user-inputted letters, not hints
        for (int i = 0; i < selectedWordIndex.Count; i++)
        {
            int originalIndex = selectedWordIndex[i];
            if (originalIndex >= 0 && originalIndex < optionWordArray.Length)
            {
                optionWordArray[originalIndex].gameObject.SetActive(true);
                answerWordArray[i].SetChar('_');
            }
        }

        // Remove only user-inputted indices from selectedWordIndex, keep hints (-1)
        for (int i = selectedWordIndex.Count - 1; i >= 0; i--)
        {
            if (selectedWordIndex[i] >= 0)
            {
                selectedWordIndex.RemoveAt(i);
            }
        }

        currentAnswerIndex = selectedWordIndex.Count;

        Debug.Log("Answer cleared successfully.");
    }

    public void ClearAnswerLetter(int answerIndex)
    {
        Debug.Log($"Clearing letter at answer index {answerIndex}...");

        if (answerIndex < 0 || answerIndex >= currentAnswerIndex)
        {
            Debug.LogWarning("Invalid answer index. Ignoring clear request.");
            return;
        }

        // Get the original index of the cleared letter in the options
        int originalIndex = selectedWordIndex[answerIndex];

        // Make the corresponding option visible again
        optionWordArray[originalIndex].gameObject.SetActive(true);

        // Shift the remaining letters in the answer to the left
        for (int i = answerIndex; i < currentAnswerIndex - 1; i++)
        {
            answerWordArray[i].SetChar(answerWordArray[i + 1].charValue);
            selectedWordIndex[i] = selectedWordIndex[i + 1];
        }

        // Clear the last letter in the answer
        answerWordArray[currentAnswerIndex - 1].SetChar('_');

        // Update the current answer index
        currentAnswerIndex--;

        Debug.Log("Letter cleared successfully.");
    }

    private void PassQuestion()
    {
        // Ensure the current question index is within bounds
        if (currentQuestionIndex < questionData.questions.Count)
        {
            Debug.Log($"Question {currentQuestionIndex} skipped.");
            gameProgressHandler.OnSkipUsed(answerWord); // Call the skip question method
            // Increment the skip counter
            totalSkipsUsed++;

            // Add the current question to the skipped list if not already answered or skipped
            if (
                !skippedQuestions.Contains(currentQuestionIndex)
                && !correctlyAnsweredQuestions.Contains(currentQuestionIndex)
            )
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
                Debug.Log("No more questions to display. Looping back to skipped questions...");
                HandleSkippedQuestions(); // Handle skipped questions if all questions are traversed
            }
        }
        else
        {
            Debug.Log("Looping back to skipped questions...");
            HandleSkippedQuestions(); // Handle skipped questions if already at the end
        }
    }

    private void HandleSkippedQuestions()
    {
        if (skippedQuestions.Count > 0)
        {
            Debug.Log($"Revisiting skipped questions. Remaining: {skippedQuestions.Count}");

            // Retrieve the first skipped question and remove it from the list
            currentQuestionIndex = skippedQuestions[0];
            skippedQuestions.RemoveAt(0);

            // Set the question for the skipped index
            SetQuestion();
        }
        else if (correctlyAnsweredQuestions.Count < questionData.questions.Count)
        {
            Debug.Log("No more skipped questions. Looping to unanswered questions...");
            // Find the next unanswered question
            for (int i = 0; i < questionData.questions.Count; i++)
            {
                if (!correctlyAnsweredQuestions.Contains(i))
                {
                    currentQuestionIndex = i;
                    SetQuestion();
                    return;
                }
            }

            Debug.Log("No more questions to revisit. Game over.");
        }
        else
        {
            Debug.Log("All questions answered correctly. Completing the game...");
            CheckGameCompletion();
        }
    }

    private void CheckGameCompletion()
    {
        Debug.Log("All questions answered correctly. Game over.");
        timerManager?.StopTimer();
        gameOver.SetActive(true);

        int studentId = int.Parse(PlayerPrefs.GetString("User ID"));
        int module_number = int.Parse(LessonsLoader.moduleNumber);
        int gameModeId = 2; // Jumbled Letters mode ID
        int subjectId = LessonsLoader.subjectId;
        float solveTime = timerManager?.elapsedTime ?? 0;

        StartCoroutine(
            UpdateGameCompletionStatus(studentId, module_number, gameModeId, subjectId, solveTime)
        );

        // Ensure all attributes are updated
        StartCoroutine(UpdateAttributes());
    }

    private IEnumerator UpdateAttributes()
    {
        int studentId = int.Parse(PlayerPrefs.GetString("User ID"));
        int module_number = int.Parse(LessonsLoader.moduleNumber);
        int gameModeId = 2; // Jumbled Letters mode ID
        int subjectId = LessonsLoader.subjectId;

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
            yield return gameProgressHandler.UpdateConsistency(studentId, 10);

            yield return gameProgressHandler.UpdateVocabularyRange(
                studentId,
                module_number,
                gameModeId,
                subjectId,
                gameProgressHandler.SkipUsageCount,
                gameProgressHandler.HintUsageCount,
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

    private void ResetCurrentInput()
    {
        // Only clear user-inputted letters, not hints
        for (int i = 0; i < selectedWordIndex.Count; i++)
        {
            int originalIndex = selectedWordIndex[i];
            if (originalIndex >= 0 && originalIndex < optionWordArray.Length)
            {
                optionWordArray[originalIndex].gameObject.SetActive(true);
                answerWordArray[i].SetChar('_');
            }
        }

        // Remove only user-inputted indices from selectedWordIndex, keep hints (-1)
        for (int i = selectedWordIndex.Count - 1; i >= 0; i--)
        {
            if (selectedWordIndex[i] >= 0)
            {
                selectedWordIndex.RemoveAt(i);
            }
        }

        // Update currentAnswerIndex to reflect only hints left
        currentAnswerIndex = selectedWordIndex.Count;

        // Do not call ResetQuestion() here, as it would clear hints as well
    }

    private void ResetQuestion()
    {
        for (int i = 0; i < answerWordArray.Length; i++)
        {
            answerWordArray[i].SetChar('_');
            answerWordArray[i].SetHintStyle(false); // Reset hint style
            answerWordArray[i].gameObject.SetActive(i < answerWord.Length);
        }

        foreach (var word in optionWordArray)
        {
            word.gameObject.SetActive(true);
        }
    }

    private void RevealHint()
    {
        if (hintCounter <= 0)
        {
            Debug.Log("No hints remaining.");
            return;
        }

        // Find all unrevealed indices
        List<int> unrevealedIndices = new List<int>();
        for (int i = 0; i < answerWord.Length; i++)
        {
            if (answerWordArray[i].charValue == '_')
            {
                unrevealedIndices.Add(i);
            }
        }

        if (unrevealedIndices.Count > 0)
        {
            int randomIndex = unrevealedIndices[
                UnityEngine.Random.Range(0, unrevealedIndices.Count)
            ];

            // Remove the character from the option buttons
            for (int i = 0; i < optionWordArray.Length; i++)
            {
                if (
                    optionWordArray[i].gameObject.activeSelf
                    && optionWordArray[i].charValue == answerWord[randomIndex]
                )
                {
                    optionWordArray[i].gameObject.SetActive(false);
                    break;
                }
            }

            answerWordArray[randomIndex].SetChar(answerWord[randomIndex]);
            answerWordArray[randomIndex].SetHintStyle(true); // Set hint style for the revealed letter
            // Insert -1 at the correct position in selectedWordIndex
            if (randomIndex < selectedWordIndex.Count)
                selectedWordIndex.Insert(randomIndex, -1);
            else
                selectedWordIndex.Add(-1);
            currentAnswerIndex = selectedWordIndex.Count;
            hintCounter--;
            gameProgressHandler.OnHintUsed(answerWord); // Call the hint used method
            UpdateHintCounterUI();
            Debug.Log($"Hint revealed at index {randomIndex}: {answerWord[randomIndex]}");

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

    public void LoadQuestionsOnButtonClick()
    {
        // Reset the question index and other related variables
        currentQuestionIndex = 0;
        currentAnswerIndex = 0;
        selectedWordIndex.Clear();
        gameStatus = GameStatus.Playing;

        StartCoroutine(
            LoadQuestionData(LessonsLoader.subjectId, int.Parse(LessonsLoader.moduleNumber))
        );
    }

    public void ReplayGame()
    {
        Debug.Log("Replay button clicked. Resetting Jumbled Letters game state...");

        // Reset game state
        ResetGameState();

        // Reload questions
        if (LessonsLoader.moduleNumber != null)
        {
            StartCoroutine(
                LoadQuestionData(LessonsLoader.subjectId, int.Parse(LessonsLoader.moduleNumber))
            );
        }
        else
        {
            Debug.LogError("LessonsLoader.moduleNumber is null. Cannot reload questions.");
        }
    }
}
