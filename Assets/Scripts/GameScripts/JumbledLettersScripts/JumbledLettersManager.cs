using System.Collections;
using System.Collections.Generic;
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

    public TimerManager timerManager; // Assign in the Inspector

    [SerializeField]
    private Button passButton; // Assign the Pass button in the Inspector
    private List<int> skippedQuestions = new List<int>(); // Track skipped questions

    public Button hintButton; // Assign the hint button in the Inspector

    [SerializeField]
    private Text hintCounterText; // Assign the Text UI in the Inspector

    private int hintCounter = 3; // Maximum number of hints allowed

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

        UpdateHintCounterUI();
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

            int lessonId = LessonUI.lesson_id;
            int gameModeId = 2; // Assuming 2 is the ID for Jumbled Letters mode

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

        if (studentId <= 0 || lessonId <= 0 || gameModeId <= 0)
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
            LoadQuestionData(
                LessonsLoader.subjectId,
                int.Parse(LessonsLoader.moduleNumber),
                LessonUI.lesson_id
            )
        );
    }

    private void ResetGameState()
    {
        Debug.Log("Resetting Jumbled Letters game state...");
        currentQuestionIndex = 0;
        currentAnswerIndex = 0;
        selectedWordIndex.Clear();
        skippedQuestions.Clear(); // Clear skipped questions
        correctlyAnsweredQuestions.Clear(); // Clear correctly answered questions
        gameStatus = GameStatus.Playing;

        if (questionText != null)
            questionText.text = "";

        foreach (var word in answerWordArray)
        {
            word.SetChar('_');
            word.gameObject.SetActive(true);
        }

        foreach (var word in optionWordArray)
        {
            word.gameObject.SetActive(true);
        }

        if (gameOver != null)
            gameOver.SetActive(false);
    }

    private IEnumerator LoadQuestionData(int subjectId, int moduleId, int lessonId)
    {
        string url = $"{apiUrl}?subject_id={subjectId}&module_id={moduleId}&lesson_id={lessonId}";
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

        for (int i = 0; i < answerWord.Length; i++)
        {
            charArray[i] = answerWord[i];
        }

        for (int i = answerWord.Length; i < optionWordArray.Length; i++)
        {
            charArray[i] = (char)UnityEngine.Random.Range(65, 91); // Random uppercase letters
        }

        // Shuffle the characters
        charArray = ShuffleList
            .ShuffleListItems(charArray.Take(optionWordArray.Length).ToList())
            .ToArray();

        for (int i = 0; i < optionWordArray.Length; i++)
        {
            optionWordArray[i].SetChar(charArray[i]);
        }

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
        if (gameStatus == GameStatus.Next || currentAnswerIndex >= answerWord.Length)
            return;

        // Find the next available index that is not already answered
        while (currentAnswerIndex < answerWord.Length && answerWordArray[currentAnswerIndex].charValue != '_')
        {
            currentAnswerIndex++;
        }

        if (currentAnswerIndex >= answerWord.Length)
        {
            Debug.Log("No available indices to place the letter.");
            return;
        }

        selectedWordIndex.Add(wordData.transform.GetSiblingIndex());
        wordData.gameObject.SetActive(false);
        answerWordArray[currentAnswerIndex].SetChar(wordData.charValue);
        currentAnswerIndex++;

        // Check if the answer is complete
        CheckIfAnswerComplete();
    }

    private void CheckAnswer()
    {
        string formedWord = string.Join("", answerWordArray.Take(answerWord.Length).Select(a => a.charValue)).ToUpper();
        string expectedAnswer = answerWord.ToUpper();

        Debug.Log($"Expected Answer: {expectedAnswer}");
        Debug.Log($"Formed Word: {formedWord}");

        if (formedWord.Equals(expectedAnswer, System.StringComparison.OrdinalIgnoreCase))
        {
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
            Debug.Log("Answer incorrect!");
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

        // Reset the current input
        for (int i = 0; i < currentAnswerIndex; i++)
        {
            int originalIndex = selectedWordIndex[i];
            optionWordArray[originalIndex].gameObject.SetActive(true); // Make options visible again
        }

        selectedWordIndex.Clear();
        currentAnswerIndex = 0;

        // Reset the answerWordArray display
        for (int i = 0; i < answerWordArray.Length; i++)
        {
            answerWordArray[i].SetChar('_');
        }

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

            // Add the current question to the skipped list if not already answered or skipped
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

        // Update game completion status
        int studentId = int.Parse(PlayerPrefs.GetString("User ID"));
        int lessonId = LessonUI.lesson_id;
        int gameModeId = 2; // Assuming 2 is the ID for Jumbled Letters mode
        int subjectId = LessonsLoader.subjectId;
        float solveTime = timerManager?.elapsedTime ?? 0;

        StartCoroutine(
            UpdateGameCompletionStatus(studentId, lessonId, gameModeId, subjectId, solveTime)
        );
    }

    private void ResetCurrentInput()
    {
        for (int i = 0; i < currentAnswerIndex; i++)
        {
            int originalIndex = selectedWordIndex[i];
            optionWordArray[originalIndex].gameObject.SetActive(true);
        }

        selectedWordIndex.Clear();
        currentAnswerIndex = 0;
        ResetQuestion();
    }

    private void ResetQuestion()
    {
        for (int i = 0; i < answerWordArray.Length; i++)
        {
            answerWordArray[i].SetChar('_');
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

        if (currentAnswerIndex >= answerWord.Length)
        {
            Debug.Log("Answer is already complete. No hint needed.");
            return;
        }

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
            int randomIndex = unrevealedIndices[UnityEngine.Random.Range(0, unrevealedIndices.Count)];
            answerWordArray[randomIndex].SetChar(answerWord[randomIndex]);
            hintCounter--;
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
}
