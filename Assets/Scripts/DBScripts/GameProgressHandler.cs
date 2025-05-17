using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class GameProgressHandler : MonoBehaviour
{
    private const string UpdateSpeedUrl = "updateSpeedAttribute.php";
    private const string UpdateVocabularyRangeUrl = "updateVocabularyRangeAttribute.php";
    private const string UpdateRetentionUrl = "updateRetentionAttribute.php";
    private const int BaseTime = 30; // Base time in seconds
    private const int MaxRetries = 3; // Maximum number of retry attempts
    private const float RetryDelay = 1f; // Delay between retries in seconds

    private ComplexWordsHandler complexWordsHandler;
    private RepeatingWordsHandler repeatingWordsHandler;
    private int rareWordCount = 0;
    public int hintUsageCount = 0;
    private int hintOnRepeatingWordCount = 0;

    private int incorrectAnswerCount = 0;
    private int incorrectRepeatingAnswerCount = 0;
    private int skipUsageCount = 0;
    private int skipRepeatingUsageCount = 0;
    private int complexWordAttemptCount = 0;

    private HashSet<string> encounteredRepeatingWords = new HashSet<string>(); // Track encountered repeating words

    // Field declarations
    private int correctAnswers;
    private int totalAttempts;
    private TimerManager timerManager;
    private int hintCounter;
    private int totalSkipsUsed;

    public int HintOnRepeatingWordCount => hintOnRepeatingWordCount;
    public int IncorrectRepeatingAnswerCount => incorrectRepeatingAnswerCount;
    public int SkipRepeatingUsageCount => skipRepeatingUsageCount;
    public int IncorrectAnswerCount => incorrectAnswerCount;
    public int SkipUsageCount => skipUsageCount;
    public int ComplexWordAttemptCount => complexWordAttemptCount;

    private async Task<bool> RetryOperation(Func<Task<bool>> operation)
    {
        int attempts = 0;
        while (attempts < MaxRetries)
        {
            try
            {
                bool success = await operation();
                if (success)
                    return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Attempt {attempts + 1} failed: {e.Message}");
            }

            attempts++;
            if (attempts < MaxRetries)
            {
                await Task.Delay(TimeSpan.FromSeconds(RetryDelay));
            }
        }
        return false;
    }

    private void Awake()
    {
        complexWordsHandler = GetComponent<ComplexWordsHandler>();
        if (complexWordsHandler == null)
        {
            complexWordsHandler = gameObject.AddComponent<ComplexWordsHandler>();
        }
        complexWordsHandler.LoadComplexWords();

        repeatingWordsHandler = GetComponent<RepeatingWordsHandler>();
        if (repeatingWordsHandler == null)
        {
            repeatingWordsHandler = gameObject.AddComponent<RepeatingWordsHandler>();
        }
        repeatingWordsHandler.LoadRepeatingWords();
    }

    public void OnWordSolved(string word, int difficulty)
    {
        if (complexWordsHandler != null && complexWordsHandler.IsComplexWord(word.ToUpper()))
        {
            Debug.Log("Complex word solved!");
            complexWordAttemptCount++;
        }
        else
        {
            Debug.Log("Non-complex word solved!");
            Debug.Log($"Word: {word}");
        }
    }

    public void OnIncorrectAnswer(string word)
    {
        if (complexWordsHandler != null && complexWordsHandler.IsComplexWord(word.ToUpper()))
        {
            Debug.Log("Incorrect answer on complex word!");
            incorrectAnswerCount++;
        }
        else
        {
            Debug.Log("Incorrect answer on non-complex word!");
            Debug.Log($"Word: {word}");
        }
        if (repeatingWordsHandler != null && repeatingWordsHandler.IsRepeatingWord(word.ToUpper()))
        {
            if (!encounteredRepeatingWords.Contains(word.ToUpper()))
            {
                Debug.Log("First incorrect answer on repeating word. No retention penalty.");
                encounteredRepeatingWords.Add(word.ToUpper());
            }
            else
            {
                Debug.Log("Incorrect answer on repeating word!");
                incorrectRepeatingAnswerCount++;
            }
        }
    }

    public void OnHintUsed(string word)
    {
        if (complexWordsHandler != null && complexWordsHandler.IsComplexWord(word.ToUpper()))
        {
            Debug.Log("Hint used on complex word!");
            hintUsageCount++;
        }
        if (repeatingWordsHandler != null && repeatingWordsHandler.IsRepeatingWord(word.ToUpper()))
        {
            if (!encounteredRepeatingWords.Contains(word.ToUpper()))
            {
                Debug.Log("First hint used on repeating word. No retention penalty.");
                encounteredRepeatingWords.Add(word.ToUpper());
            }
            else
            {
                Debug.Log("Hint used on repeating word!");
                hintOnRepeatingWordCount++;
            }
        }
    }

    public void OnSkipUsed(string word)
    {
        if (complexWordsHandler != null && complexWordsHandler.IsComplexWord(word.ToUpper()))
        {
            Debug.Log("Skip used on complex word!");
            skipUsageCount++;
        }
        if (repeatingWordsHandler != null && repeatingWordsHandler.IsRepeatingWord(word.ToUpper()))
        {
            if (!encounteredRepeatingWords.Contains(word.ToUpper()))
            {
                Debug.Log("First skip used on repeating word. No retention penalty.");
                encounteredRepeatingWords.Add(word.ToUpper());
            }
            else
            {
                Debug.Log("Skip used on repeating word!");
                skipRepeatingUsageCount++;
            }
        }
    }

    public void ResetVocabularyRangeCounters()
    {
        rareWordCount = 0;
        hintUsageCount = 0;
        incorrectAnswerCount = 0;
        skipUsageCount = 0;
        encounteredRepeatingWords.Clear(); // Reset encountered repeating words
    }

    public IEnumerator UpdateSpeed(
        int studentId,
        int module_number,
        int gameModeId,
        int subjectId,
        float solveTime
    )
    {
        if (
            studentId <= 0
            || module_number <= 0
            || gameModeId <= 0
            || subjectId <= 0
            || solveTime <= 0
        )
        {
            Debug.LogError(
                $"Invalid parameters for UpdateSpeed. Ensure all IDs and solveTime are valid. "
                    + $"studentId={studentId}, module_number={module_number}, gameModeId={gameModeId}, subjectId={subjectId}, solveTime={solveTime}"
            );
            yield break;
        }

        string url = $"{Web.BaseApiUrl}{UpdateSpeedUrl}";
        WWWForm form = new WWWForm();

        // Calculate speed as a whole number with a maximum value of 10
        int speed = Mathf.Clamp(Mathf.CeilToInt((BaseTime / solveTime) * 10), 0, 10);

        // Log the parameters being sent for debugging
        Debug.Log(
            $"Sending UpdateSpeed request with parameters: studentId={studentId}, module_number={module_number}, gameModeId={gameModeId}, subjectId={subjectId}, speed={speed}"
        );

        form.AddField("student_id", studentId);
        form.AddField("module_number", module_number);
        form.AddField("game_mode_id", gameModeId);
        form.AddField("subject_id", subjectId);
        form.AddField("speed", speed); // Send calculated speed
        int maxRetries = 3; // Maximum number of retry attempts
        int attempt = 0;
        float retryDelay = 2f; // seconds

        while (attempt < maxRetries)
        {
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Speed attribute updated successfully.");
                    yield break; // Exit the coroutine on success
                }
                else
                {
                    Debug.LogError(
                        $"Failed to update speed attribute: {www.error}. Response: {www.downloadHandler.text}"
                    );
                    attempt++;
                    if (attempt < maxRetries)
                    {
                        Debug.LogWarning("Retrying to update speed attribute...");
                        yield return new WaitForSeconds(retryDelay);
                    }
                }
            }
        }
    }

    public IEnumerator UpdateVocabularyRange(
        int studentId,
        int module_number,
        int gameModeId,
        int subjectId,
        int complexWordAttemptCount,
        int hintUsageCount,
        int incorrectAnswerCount
    )
    {
        if (studentId <= 0 || module_number <= 0 || gameModeId <= 0 || subjectId <= 0)
        {
            Debug.LogError(
                $"Invalid parameters for UpdateVocabularyRange. Ensure all IDs are valid. "
                    + $"studentId={studentId}, module_number={module_number}, gameModeId={gameModeId}, subjectId={subjectId}, complexWordAttemptCount={complexWordAttemptCount}, hintUsageCount={hintUsageCount}, incorrectAnswerCount={incorrectAnswerCount}"
            );
            yield break;
        }

        string url = $"{Web.BaseApiUrl}{UpdateVocabularyRangeUrl}";
        WWWForm form = new WWWForm();

        // Calculate vocabulary range score using only the provided parameters
        // Start from 10, penalize for rareWordCount, hintUsageCount, and incorrectAnswerCount
        // (Assume rareWordCount is a positive metric, so it should increase the score)
        int baseScore = 10; // Start from a base, can be adjusted
        int score =
            baseScore - (rareWordCount / 2) - (hintUsageCount / 2) - (incorrectAnswerCount / 2);
        int vocabularyRangeScore = Mathf.Clamp(score, 0, 10);

        Debug.Log(
            $"Sending UpdateVocabularyRange request with parameters: studentId={studentId}, module_number={module_number}, gameModeId={gameModeId}, subjectId={subjectId}, vocabularyRangeScore={vocabularyRangeScore}, complexWordAttemptCount={complexWordAttemptCount}, hintUsageCount={hintUsageCount}, incorrectAnswerCount={incorrectAnswerCount}"
        );

        form.AddField("student_id", studentId);
        form.AddField("module_number", module_number);
        form.AddField("game_mode_id", gameModeId);
        form.AddField("subject_id", subjectId);
        form.AddField("vocabulary_range_score", (int)vocabularyRangeScore);
        int maxRetries = 3; // Maximum number of retry attempts
        int attempt = 0;
        float retryDelay = 2f; // seconds
        while (attempt < maxRetries)
        {
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Vocabulary range attribute updated successfully.");
                    yield break; // Exit the coroutine on success
                }
                else
                {
                    Debug.LogError(
                        $"Failed to update vocabulary range attribute: {www.error}. Response: {www.downloadHandler.text}"
                    );
                    attempt++;
                    if (attempt < maxRetries)
                    {
                        Debug.LogWarning("Retrying to update vocabulary range attribute...");
                        yield return new WaitForSeconds(retryDelay);
                    }
                }
            }
        }
    }

    public IEnumerator UpdateAccuracy(
        int studentId,
        int module_number,
        int gameModeId,
        int subjectId,
        int correctAnswers,
        int totalAttempts
    )
    {
        if (
            studentId <= 0
            || module_number <= 0
            || gameModeId <= 0
            || subjectId <= 0
            || totalAttempts <= 0
        )
        {
            Debug.LogError(
                $"Invalid parameters for UpdateAccuracy. Ensure all IDs and attempts are valid. "
                    + $"studentId={studentId}, module_number={module_number}, gameModeId={gameModeId}, subjectId={subjectId}, correctAnswers={correctAnswers}, totalAttempts={totalAttempts}"
            );
            yield break;
        }

        string url = $"{Web.BaseApiUrl}updateAccuracyAttribute.php";
        WWWForm form = new WWWForm();

        // Calculate accuracy as a percentage and round to the nearest integer
        int accuracy = Mathf.RoundToInt((float)(correctAnswers / 2) / (totalAttempts / 2) * 10);

        Debug.Log(
            $"Sending UpdateAccuracy request with parameters: studentId={studentId}, module_number={module_number}, gameModeId={gameModeId}, subjectId={subjectId}, accuracy={accuracy}"
        );

        form.AddField("student_id", studentId);
        form.AddField("module_number", module_number);
        form.AddField("game_mode_id", gameModeId);
        form.AddField("subject_id", subjectId);
        form.AddField("accuracy", accuracy); // Use the rounded integer value
        int maxRetries = 3; // Maximum number of retry attempts
        int attempt = 0;
        float retryDelay = 2f; // seconds
        while (attempt < maxRetries)
        {
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Accuracy attribute updated successfully.");
                    yield break; // Exit the coroutine on success
                }
                else
                {
                    Debug.LogError(
                        $"Failed to update accuracy attribute: {www.error}. Response: {www.downloadHandler.text}"
                    );
                    attempt++;
                    if (attempt < maxRetries)
                    {
                        Debug.LogWarning("Retrying to update accuracy attribute...");
                        yield return new WaitForSeconds(retryDelay);
                    }
                }
            }
        }
    }

    public IEnumerator UpdateProblemSolving(
        int studentId,
        int module_number,
        int gameModeId,
        int subjectId,
        int totalHintsUsed,
        int totalSkipsUsed
    )
    {
        if (studentId <= 0 || module_number <= 0 || gameModeId <= 0 || subjectId <= 0)
        {
            Debug.LogError(
                $"Invalid parameters for UpdateProblemSolving. Ensure all IDs are valid. "
                    + $"studentId={studentId}, module_number={module_number}, gameModeId={gameModeId}, subjectId={subjectId}, totalHintsUsed={totalHintsUsed}, totalSkipsUsed={totalSkipsUsed}"
            );
            yield break;
        }

        string url = $"{Web.BaseApiUrl}updateProblemSolvingAttribute.php";
        WWWForm form = new WWWForm();

        // Calculate problem-solving score (lower hints/skips = higher score, max 10)
        int problemSolvingScore = Mathf.Clamp(
            10 - ((totalHintsUsed / 2) + (totalSkipsUsed / 2)),
            0,
            10
        );

        Debug.Log(
            $"Sending UpdateProblemSolving request with parameters: studentId={studentId}, module_number={module_number}, gameModeId={gameModeId}, subjectId={subjectId}, problemSolvingScore={problemSolvingScore}"
        );

        form.AddField("student_id", studentId);
        form.AddField("module_number", module_number);
        form.AddField("game_mode_id", gameModeId);
        form.AddField("subject_id", subjectId);
        form.AddField("problem_solving", problemSolvingScore);
        int maxRetries = 3; // Maximum number of retry attempts
        int attempt = 0;
        float retryDelay = 2f; // seconds
        while (attempt < maxRetries)
        {
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Problem-solving attribute updated successfully.");
                    yield break; // Exit the coroutine on success
                }
                else
                {
                    Debug.LogError(
                        $"Failed to update problem-solving attribute: {www.error}. Response: {www.downloadHandler.text}"
                    );
                    attempt++;
                    if (attempt < maxRetries)
                    {
                        Debug.LogWarning("Retrying to update problem-solving attribute...");
                        yield return new WaitForSeconds(retryDelay);
                    }
                }
            }
        }
    }

    public IEnumerator UpdateConsistency(int studentId, int currentScore)
    {
        if (studentId <= 0)
        {
            Debug.LogError(
                $"Invalid parameters for UpdateConsistency. Ensure all IDs are valid. "
                    + $"studentId={studentId}, currentScore={currentScore}"
            );
            yield break;
        }

        string url = $"{Web.BaseApiUrl}updateConsistencyAttribute.php";
        WWWForm form = new WWWForm();

        // Log the parameters being sent for debugging
        Debug.Log(
            $"Sending UpdateConsistency request with parameters: studentId={studentId},currentScore={currentScore}"
        );

        form.AddField("student_id", studentId);
        form.AddField("current_score", currentScore);
        int maxRetries = 3; // Maximum number of retry attempts
        int attempt = 0;
        float retryDelay = 2f; // seconds
        while (attempt < maxRetries)
        {
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Consistency attribute updated successfully.");
                    yield break; // Exit the coroutine on success
                }
                else
                {
                    Debug.LogError(
                        $"Failed to update consistency attribute: {www.error}. Response: {www.downloadHandler.text}"
                    );
                    attempt++;
                    if (attempt < maxRetries)
                    {
                        Debug.LogWarning("Retrying to update consistency attribute...");
                        yield return new WaitForSeconds(retryDelay);
                    }
                }
            }
        }
    }

    private int CalculateRetentionScore(
        int incorrectAnswerCount,
        int hintUsageCount,
        int skipUsageCount
    )
    {
        return Mathf.Clamp(
            10 - ((incorrectAnswerCount / 2) + (hintUsageCount / 2) + (skipUsageCount / 2)),
            0,
            10
        );
    }

    public IEnumerator UpdateRetention(
        int studentId,
        int module_number,
        int gameModeId,
        int subjectId,
        int skipUsageCount,
        int hintUsageCount,
        int incorrectAnswerCount
    )
    {
        if (studentId <= 0 || module_number <= 0 || gameModeId <= 0 || subjectId <= 0)
        {
            Debug.LogError($"Invalid parameters for UpdateRetention. Ensure all IDs are valid. ");
            yield break;
        }

        int retentionScore = CalculateRetentionScore(
            incorrectAnswerCount,
            hintUsageCount,
            skipUsageCount
        );

        string url = $"{Web.BaseApiUrl}{UpdateRetentionUrl}";
        WWWForm form = new WWWForm();

        Debug.Log(
            $"Sending UpdateRetention request with parameters: studentId={studentId}, module_number={module_number}, gameModeId={gameModeId}, subjectId={subjectId}, retentionScore={retentionScore}"
        );

        form.AddField("student_id", studentId);
        form.AddField("module_number", module_number);
        form.AddField("game_mode_id", gameModeId);
        form.AddField("subject_id", subjectId);
        form.AddField("retention", retentionScore);
        int maxRetries = 3; // Maximum number of retry attempts
        int attempt = 0;
        float retryDelay = 2f; // seconds
        while (attempt < maxRetries)
        {
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Retention attribute updated successfully.");
                    yield break; // Exit the coroutine on success
                }
                else
                {
                    Debug.LogError(
                        $"Failed to update retention attribute: {www.error}. Response: {www.downloadHandler.text}"
                    );
                    attempt++;
                    if (attempt < maxRetries)
                    {
                        Debug.LogWarning("Retrying to update retention attribute...");
                        yield return new WaitForSeconds(retryDelay);
                    }
                }
            }
        }
    }

    public IEnumerator UpdateGameCompletionStatus(
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
        int maxRetries = 3; // Maximum number of retry attempts
        int attempt = 0;
        float retryDelay = 2f; // seconds
        while (attempt < maxRetries)
        {
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Game completion status updated successfully.");
                    yield break; // Exit the coroutine on success
                }
                else
                {
                    Debug.LogError(
                        $"Failed to update game completion status: {www.error}. Response: {www.downloadHandler.text}"
                    );
                    attempt++;
                    if (attempt < maxRetries)
                    {
                        Debug.LogWarning("Retrying to update game completion status...");
                        yield return new WaitForSeconds(retryDelay);
                    }
                }
            }
        }
    }

    private IEnumerator ExecuteWithRetry(IEnumerator operation)
    {
        int attempts = 0;
        bool success = false;
        string errorMessage = null;

        while (attempts < MaxRetries && !success)
        {
            // Create a wrapper coroutine to handle the operation
            var wrapper = new CoroutineWrapper(operation);
            yield return StartCoroutine(wrapper.Run());

            if (wrapper.Error != null)
            {
                attempts++;
                errorMessage = wrapper.Error;
                Debug.LogWarning($"Attempt {attempts} failed: {errorMessage}");
                if (attempts < MaxRetries)
                {
                    yield return new WaitForSeconds(RetryDelay);
                }
            }
            else
            {
                success = true;
            }
        }

        if (!success)
        {
            Debug.LogError(
                $"Failed to execute operation after {MaxRetries} attempts. Last error: {errorMessage}"
            );
        }
    }

    // Helper class to handle coroutine errors
    private class CoroutineWrapper
    {
        private readonly IEnumerator _operation;
        public string Error { get; private set; }

        public CoroutineWrapper(IEnumerator operation)
        {
            _operation = operation;
        }

        public IEnumerator Run()
        {
            bool hasError = false;
            while (!hasError && _operation.MoveNext())
            {
                object current = null;
                try
                {
                    current = _operation.Current;
                }
                catch (Exception e)
                {
                    Error = e.Message;
                    hasError = true;
                    yield break;
                }
                yield return current;
            }
        }
    }

    public IEnumerator UpdateAttributes()
    {
        int studentId = int.Parse(PlayerPrefs.GetString("User ID"));
        int module_number = int.Parse(LessonsLoader.moduleNumber);
        int gameModeId = 1; // Classic mode ID
        int subjectId = LessonsLoader.subjectId;
        Debug.Log(
            $"Updating attributes for studentId: {studentId}, module_number: {module_number}"
        );

        // Create a list of update operations
        var updateOperations = new List<IEnumerator>
        {
            UpdateAccuracy(
                studentId,
                module_number,
                gameModeId,
                subjectId,
                correctAnswers,
                totalAttempts
            ),
            UpdateSpeed(
                studentId,
                module_number,
                gameModeId,
                subjectId,
                timerManager?.elapsedTime ?? 0
            ),
            UpdateProblemSolving(
                studentId,
                module_number,
                gameModeId,
                subjectId,
                3 - hintCounter,
                totalSkipsUsed
            ),
            UpdateConsistency(studentId, 10),
            UpdateVocabularyRange(
                studentId,
                module_number,
                gameModeId,
                subjectId,
                SkipUsageCount,
                hintUsageCount,
                IncorrectAnswerCount
            ),
        };

        // Execute all operations in parallel
        var runningOperations = new List<Coroutine>();
        foreach (var operation in updateOperations)
        {
            var coroutine = StartCoroutine(ExecuteWithRetry(operation));
            runningOperations.Add(coroutine);
        }

        // Wait for all operations to complete
        foreach (var operation in runningOperations)
        {
            yield return operation;
        }
    }
}
