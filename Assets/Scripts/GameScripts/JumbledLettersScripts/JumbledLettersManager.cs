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
    private WordData optionWordPrefab; // Assign in Inspector (prefab for option holder)

    [SerializeField]
    private RectTransform optionHolderRect; // Assign in Inspector (parent container for options)

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

    // Modify dictionary to track hinted indices per word
    private Dictionary<string, HashSet<int>> wordHintedIndices =
        new Dictionary<string, HashSet<int>>();

    private List<WordData> answerWordList = new List<WordData>();

    private List<WordData> optionWordList = new List<WordData>(); // Dynamically created option holders

    [SerializeField]
    private WordData answerWordPrefab; // Assign in Inspector

    [SerializeField]
    private RectTransform answerHolderRect; // Assign in Inspector (parent container for answer slots)

    [SerializeField]
    private GridLayoutGroup answerGridLayout; // Assign in Inspector

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
        if (optionHolderRect == null)
        {
            Debug.LogError("OptionHolder RectTransform not assigned!");
            return;
        }
        // Destroy any existing option holders
        foreach (var word in optionWordList)
        {
            if (word != null)
                Destroy(word.gameObject);
        }
        optionWordList.Clear();
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

        // Show loading screen at the start of lesson completion check
        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.ShowLoadingScreen();
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
            // Hide loading screen if there's an error
            if (GameLoadingManager.Instance != null)
            {
                GameLoadingManager.Instance.HideLoadingScreen();
            }
            yield break;
        }

        string url =
            $"{Web.BaseApiUrl}checkLessonCompletion.php?student_id={studentId}&module_number={module_number}&game_mode_id={gameModeId}&subject_id={subjectId}";
        Debug.Log("Checking lesson completion from URL: " + url);
        int maxRetries = 3;
        int attempts = 0;
        float retryDelay = 2f; // seconds

        while (attempts < maxRetries)
        {
            attempts++;
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
                    Debug.LogError(
                        $"Failed to check lesson completion (Attempt {attempts}/{maxRetries}): {www.error}"
                    );
                    if (attempts >= maxRetries)
                    {
                        isLessonCompleted = false;
                    }
                    else
                    {
                        yield return new WaitForSeconds(retryDelay);
                    }
                }
            }
        }

        isRefreshing = false; // Reset flag

        if (isLessonCompleted)
        {
            // Hide loading screen before handling lesson state
            if (GameLoadingManager.Instance != null)
            {
                GameLoadingManager.Instance.HideLoadingScreen();
            }
            HandleLessonState();
        }
        else
        {
            // Don't hide loading screen here as RefreshJumbledLettersData will handle it
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
        hintCounter = 3;
        selectedWordIndex.Clear();
        skippedQuestions.Clear();
        correctlyAnsweredQuestions.Clear();
        wordHintedIndices.Clear(); // Clear hinted indices dictionary
        gameStatus = GameStatus.Playing;
        if (gameProgressHandler != null)
        {
            gameProgressHandler.ResetVocabularyRangeCounters();
        }
        if (questionText != null)
        {
            questionText.text = "";
        }
        if (answerWordPrefab != null && answerHolderRect != null)
        {
            foreach (var word in answerWordList)
            {
                if (word != null)
                    Destroy(word.gameObject);
            }
            answerWordList.Clear();
        }
        // Destroy all option holders
        foreach (var word in optionWordList)
        {
            if (word != null)
                Destroy(word.gameObject);
        }
        optionWordList.Clear();
        if (gameOver != null)
        {
            gameOver.SetActive(false);
        }
        UpdateHintCounterUI();
    }

    private IEnumerator LoadQuestionData(int subjectId, int module_number)
    {
        // Show loading screen
        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.ShowLoadingScreen();
        }

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
                Debug.LogError("Failed to fetch questions: " + www.error);
                timerManager?.StopTimer();
                gameOver.SetActive(true);
            }
        }

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
            HandleSkippedQuestions();
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
        // Destroy any existing option holders
        foreach (var word in optionWordList)
        {
            if (word != null)
                Destroy(word.gameObject);
        }
        optionWordList.Clear();
        // Layout: max 8 per row, center both rows
        int maxPerRow = 8;
        int optionCount = charArray.Length;
        int rowCount = (optionCount + maxPerRow - 1) / maxPerRow;
        float cellSize = 100f;
        float spacing = 15f;
        float containerWidth = optionHolderRect.rect.width;
        float containerHeight = optionHolderRect.rect.height;
        for (int row = 0; row < rowCount; row++)
        {
            int startIdx = row * maxPerRow;
            int countThisRow = Mathf.Min(maxPerRow, optionCount - startIdx);
            float totalRowWidth = (cellSize * countThisRow) + (spacing * (countThisRow - 1));
            float startX = (containerWidth - totalRowWidth) / 2f;
            float y = 0f;
            if (rowCount == 2)
            {
                // Vertically center two rows
                float totalHeight = cellSize + spacing;
                y = (containerHeight - totalHeight) / 2f + row * (cellSize + spacing);
            }
            else
            {
                // Single row, center vertically
                y = (containerHeight - cellSize) / 2f;
            }
            for (int i = 0; i < countThisRow; i++)
            {
                int charIdx = startIdx + i;
                WordData wordObj = Instantiate(optionWordPrefab, optionHolderRect);
                wordObj.SetChar(charArray[charIdx]);
                wordObj.gameObject.SetActive(true);
                RectTransform rt = wordObj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(startX + i * (cellSize + spacing), -y);
                    rt.sizeDelta = new Vector2(cellSize, cellSize);
                }
                // Set outline color based on subject
                var outline = wordObj.GetComponent<UnityEngine.UI.Outline>();
                var text = wordObj.GetComponentInChildren<UnityEngine.UI.Text>();
                if (outline != null)
                {
                    Color outlineColor = Color.white;
                    Color textColor = Color.black;
                    if (LessonsLoader.subjectId == 1) // English
                    {
                        outlineColor = new Color32(0, 102, 204, 255); // Blue
                        textColor = new Color32(0, 51, 102, 255); // Darker Blue
                    }
                    else if (LessonsLoader.subjectId == 2) // Science
                    {
                        outlineColor = new Color32(0, 153, 0, 255); // Green
                        textColor = new Color32(0, 102, 0, 255); // Darker Green
                    }
                    outline.effectColor = outlineColor;
                    if (text != null)
                    {
                        text.color = textColor;
                    }
                }
                optionWordList.Add(wordObj);
            }
        }

        // Restore hints if any for this word
        if (wordHintedIndices.ContainsKey(answerWord))
        {
            HashSet<int> hintedIndicesForWord = wordHintedIndices[answerWord];
            foreach (int index in hintedIndicesForWord)
            {
                // Remove the character from the option buttons
                for (int i = 0; i < optionWordList.Count; i++)
                {
                    if (
                        optionWordList[i].gameObject.activeSelf
                        && optionWordList[i].charValue == answerWord[index]
                    )
                    {
                        optionWordList[i].gameObject.SetActive(false);
                        break;
                    }
                }
                answerWordList[index].SetChar(answerWord[index]);
                answerWordList[index].SetHintStyle(true);
                if (index < selectedWordIndex.Count)
                    selectedWordIndex.Insert(index, -1);
                else
                    selectedWordIndex.Add(-1);
            }
            currentAnswerIndex = selectedWordIndex.Count;
            hintCounter -= hintedIndicesForWord.Count;
            UpdateHintCounterUI();
        }

        currentQuestionIndex++;
        gameStatus = GameStatus.Playing;
    }

    private void CheckIfAnswerComplete()
    {
        int filledCount = answerWordList.Count(a => a.charValue != '_');
        if (filledCount == answerWord.Length)
        {
            CheckAnswer();
        }
    }

    public void SelectedOption(WordData wordData)
    {
        if (gameStatus == GameStatus.Next)
            return;

        // Find the first available (empty) index in answerWordList
        int inputIndex = -1;
        for (int i = 0; i < answerWord.Length; i++)
        {
            if (answerWordList[i].charValue == '_')
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
        int optionIndex = optionWordList.IndexOf(wordData);
        if (inputIndex < selectedWordIndex.Count)
            selectedWordIndex.Insert(inputIndex, optionIndex);
        else
            selectedWordIndex.Add(optionIndex);

        wordData.gameObject.SetActive(false);
        answerWordList[inputIndex].SetChar(wordData.charValue);

        // Update currentAnswerIndex to reflect the number of filled slots
        currentAnswerIndex = selectedWordIndex.Count;

        // Check if the answer is complete
        CheckIfAnswerComplete();
    }

    private void CheckAnswer()
    {
        string formedWord = string.Join(
                "",
                answerWordList.Take(answerWord.Length).Select(a => a.charValue)
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
        for (int i = 0; i < optionWordList.Count; i++)
        {
            if (optionWordList[i].gameObject.activeSelf)
            {
                activeChars.Add(optionWordList[i].charValue);
            }
        }
        // Shuffle the active characters
        activeChars = ShuffleList.ShuffleListItems(activeChars);
        // Reassign the shuffled characters back to the active options
        int activeIndex = 0;
        for (int i = 0; i < optionWordList.Count; i++)
        {
            if (optionWordList[i].gameObject.activeSelf)
            {
                optionWordList[i].SetChar(activeChars[activeIndex]);
                activeIndex++;
            }
        }
        Debug.Log("Options shuffled successfully.");
    }

    // Update ClearAnswerLetter and ClearAnswer to use optionWordList instead of optionWordArray
    public void ClearAnswerLetter(int answerIndex)
    {
        Debug.Log($"Clearing letter at answer index {answerIndex}...");

        if (answerIndex < 0 || answerIndex >= currentAnswerIndex)
        {
            Debug.LogWarning("Invalid answer index. Ignoring clear request.");
            return;
        }

        // Check if the letter is a hint (index -1 in selectedWordIndex)
        if (selectedWordIndex[answerIndex] == -1)
        {
            Debug.Log("Cannot clear a hinted letter.");
            return;
        }

        // Get the original index of the cleared letter in the options
        int originalIndex = selectedWordIndex[answerIndex];

        // Make the corresponding option visible again
        if (originalIndex >= 0 && originalIndex < optionWordList.Count)
            optionWordList[originalIndex].gameObject.SetActive(true);

        // Shift the remaining letters in the answer to the left
        for (int i = answerIndex; i < currentAnswerIndex - 1; i++)
        {
            answerWordList[i].SetChar(answerWordList[i + 1].charValue);
            selectedWordIndex[i] = selectedWordIndex[i + 1];
        }

        // Clear the last letter in the answer
        answerWordList[currentAnswerIndex - 1].SetChar('_');

        // Update the current answer index
        currentAnswerIndex--;

        Debug.Log("Letter cleared successfully.");
    }

    public void ClearAnswer()
    {
        Debug.Log("Clearing one character from current answer...");

        // Find the last user-inputted letter (not a hint, i.e., index >= 0)
        int lastInputIndex = -1;
        for (int i = selectedWordIndex.Count - 1; i >= 0; i--)
        {
            if (selectedWordIndex[i] >= 0)
            {
                lastInputIndex = i;
                break;
            }
        }

        if (lastInputIndex == -1)
        {
            Debug.Log("No user-inputted letters to clear.");
            return;
        }

        int originalIndex = selectedWordIndex[lastInputIndex];
        if (originalIndex >= 0 && originalIndex < optionWordList.Count)
        {
            optionWordList[originalIndex].gameObject.SetActive(true);
            answerWordList[lastInputIndex].SetChar('_');
        }

        selectedWordIndex.RemoveAt(lastInputIndex);
        currentAnswerIndex = selectedWordIndex.Count;

        Debug.Log("One character cleared successfully.");
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
        int gameModeId = 2; // Jumbled Letters mode ID (adjust as needed)
        int subjectId = LessonsLoader.subjectId;
        float solveTime = timerManager?.elapsedTime ?? 0;
        int module_number = int.Parse(LessonsLoader.moduleNumber);

        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.ShowLoadingScreenWithDelay(
                0.5f,
                false,
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
        yield return StartCoroutine(
            UpdateGameCompletionStatus(studentId, module_number, gameModeId, subjectId, solveTime)
        );
        yield return StartCoroutine(UpdateAttributes());
        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.HideLoadingScreen();
        }
    }

    private IEnumerator UpdateAttributes()
    {
        int studentId = int.Parse(PlayerPrefs.GetString("User ID"));
        int module_number = int.Parse(LessonsLoader.moduleNumber);
        int gameModeId = 2; // Jumbled Letters mode ID (adjust as needed)
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
            yield return gameProgressHandler.UpdateConsistency(studentId, 10);

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

        // Hide loading screen after all updates are complete
        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.HideLoadingScreen();
        }
    }

    private void ResetCurrentInput()
    {
        // Only clear user-inputted letters, not hints
        for (int i = 0; i < selectedWordIndex.Count; i++)
        {
            int originalIndex = selectedWordIndex[i];
            if (originalIndex >= 0 && originalIndex < optionWordList.Count)
            {
                optionWordList[originalIndex].gameObject.SetActive(true);
                answerWordList[i].SetChar('_');
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
    }

    private void ResetQuestion()
    {
        foreach (var word in answerWordList)
        {
            if (word != null)
                Destroy(word.gameObject);
        }
        answerWordList.Clear();
        if (answerGridLayout != null && answerHolderRect != null)
        {
            answerGridLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            answerGridLayout.constraintCount = 1;
            answerGridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            answerGridLayout.childAlignment = TextAnchor.MiddleCenter;
            float containerWidth = answerHolderRect.rect.width;
            float spacing = answerGridLayout.spacing.x;
            float minCellSize = 40f;
            float maxCellSize = 100f;
            float cellSize =
                (containerWidth - (spacing * (answerWord.Length - 1))) / answerWord.Length;
            cellSize = Mathf.Clamp(cellSize, minCellSize, maxCellSize);
            answerGridLayout.cellSize = new Vector2(cellSize, cellSize);
        }
        for (int i = 0; i < answerWord.Length; i++)
        {
            WordData wordObj = Instantiate(answerWordPrefab, answerHolderRect);
            wordObj.SetChar('_');
            wordObj.SetHintStyle(false);
            wordObj.gameObject.SetActive(true);
            // Set outline color based on subject
            var outline = wordObj.GetComponent<UnityEngine.UI.Outline>();
            var text = wordObj.GetComponentInChildren<UnityEngine.UI.Text>();
            if (outline != null)
            {
                Color outlineColor = Color.white;
                Color textColor = Color.black;
                if (LessonsLoader.subjectId == 1) // English
                {
                    outlineColor = new Color32(0, 102, 204, 255); // Blue
                    textColor = new Color32(0, 51, 102, 255); // Darker Blue
                }
                else if (LessonsLoader.subjectId == 2) // Science
                {
                    outlineColor = new Color32(0, 153, 0, 255); // Green
                    textColor = new Color32(0, 102, 0, 255); // Darker Green
                }
                outline.effectColor = outlineColor;
                if (text != null)
                {
                    text.color = textColor;
                }
            }
            answerWordList.Add(wordObj);
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
            // Remove the character from the option buttons
            for (int i = 0; i < optionWordList.Count; i++)
            {
                if (
                    optionWordList[i].gameObject.activeSelf
                    && optionWordList[i].charValue == answerWord[randomIndex]
                )
                {
                    optionWordList[i].gameObject.SetActive(false);
                    break;
                }
            }
            answerWordList[randomIndex].SetChar(answerWord[randomIndex]);
            answerWordList[randomIndex].SetHintStyle(true);
            if (randomIndex < selectedWordIndex.Count)
                selectedWordIndex.Insert(randomIndex, -1);
            else
                selectedWordIndex.Add(-1);
            currentAnswerIndex = selectedWordIndex.Count;
            hintCounter--;
            gameProgressHandler.OnHintUsed(answerWord);
            UpdateHintCounterUI();
            Debug.Log($"Hint revealed at index {randomIndex}: {answerWord[randomIndex]}");

            // Store the hinted index for the current word
            if (!wordHintedIndices.ContainsKey(answerWord))
            {
                wordHintedIndices[answerWord] = new HashSet<int>();
            }
            wordHintedIndices[answerWord].Add(randomIndex);

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
