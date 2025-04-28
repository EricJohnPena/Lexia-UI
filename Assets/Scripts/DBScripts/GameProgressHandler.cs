using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameProgressHandler : MonoBehaviour
{
    private const string UpdateSpeedUrl = "updateSpeedAttribute.php";
    private const string UpdateVocabularyRangeUrl = "updateVocabularyRangeAttribute.php";
    private const string UpdateRetentionUrl = "updateRetentionAttribute.php";
    private const int BaseTime = 30; // Base time in seconds

    private ComplexWordsHandler complexWordsHandler;
    private RepeatingWordsHandler repeatingWordsHandler;
    private int rareWordCount = 0;
    private int hintUsageCount = 0;
    private int wordDifficultyScore = 0;
    private int incorrectAnswerCount = 0;
    private int skipUsageCount = 0;
    private int complexWordAttemptCount = 0;

    public int WordDifficultyScore => wordDifficultyScore;
    public int RareWordCount => rareWordCount;
    public int HintUsageCount => hintUsageCount;
    public int IncorrectAnswerCount => incorrectAnswerCount;
    public int SkipUsageCount => skipUsageCount;
    public int ComplexWordAttemptCount => complexWordAttemptCount;

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
        wordDifficultyScore += difficulty;

        if (complexWordsHandler != null && complexWordsHandler.IsComplexWord(word))
        {
            rareWordCount++;
        }
    }

    public void OnHintUsed(string word)
    {
        if (complexWordsHandler != null && complexWordsHandler.IsComplexWord(word))
        {
            Debug.Log("Hint used on complex word!");
            hintUsageCount++;
        }
    }

    public void OnIncorrectAnswer(string word)
    {
        if (complexWordsHandler != null && complexWordsHandler.IsComplexWord(word))
        {
            Debug.Log("Incorrect answer on complex word!");
            incorrectAnswerCount++;
        }
        else
        {
            Debug.Log("Incorrect answer on non-complex word!");
            Debug.Log($"Word: {word}");
        }
    }

    public void OnSkipUsed(string word)
    {
        if (complexWordsHandler != null && complexWordsHandler.IsComplexWord(word))
        {
            Debug.Log("Skip used on complex word!");
            skipUsageCount++;
        }
    }

    public void ResetVocabularyRangeCounters()
    {
        rareWordCount = 0;
        hintUsageCount = 0;
        wordDifficultyScore = 0;
        incorrectAnswerCount = 0;
        skipUsageCount = 0;
    }

    public IEnumerator UpdateSpeed(
        int studentId,
        int lessonId,
        int gameModeId,
        int subjectId,
        float solveTime
    )
    {
        if (studentId <= 0 || lessonId <= 0 || gameModeId <= 0 || subjectId <= 0 || solveTime <= 0)
        {
            Debug.LogError(
                $"Invalid parameters for UpdateSpeed. Ensure all IDs and solveTime are valid. "
                    + $"studentId={studentId}, lessonId={lessonId}, gameModeId={gameModeId}, subjectId={subjectId}, solveTime={solveTime}"
            );
            yield break;
        }

        string url = $"{Web.BaseApiUrl}{UpdateSpeedUrl}";
        WWWForm form = new WWWForm();

        // Calculate speed as a whole number with a maximum value of 10
        int speed = Mathf.Clamp(Mathf.CeilToInt((BaseTime / solveTime) * 10), 0, 10);

        // Log the parameters being sent for debugging
        Debug.Log(
            $"Sending UpdateSpeed request with parameters: studentId={studentId}, lessonId={lessonId}, gameModeId={gameModeId}, subjectId={subjectId}, speed={speed}"
        );

        form.AddField("student_id", studentId);
        form.AddField("lesson_id", lessonId);
        form.AddField("game_mode_id", gameModeId);
        form.AddField("subject_id", subjectId);
        form.AddField("speed", speed); // Send calculated speed

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Speed attribute updated successfully.");
            }
            else
            {
                Debug.LogError(
                    $"Failed to update speed attribute: {www.error}. Response: {www.downloadHandler.text}"
                );
            }
        }
    }

    public IEnumerator UpdateVocabularyRange(
        int studentId,
        int lessonId,
        int gameModeId,
        int subjectId,
        int skipUsageCount,
        int hintUsageCount,
        int incorrectAnswerCount
    )
    {
        if (studentId <= 0 || lessonId <= 0 || gameModeId <= 0 || subjectId <= 0)
        {
            Debug.LogError(
                $"Invalid parameters for UpdateVocabularyRange. Ensure all IDs are valid. "
                    + $"studentId={studentId}, lessonId={lessonId}, gameModeId={gameModeId}, subjectId={subjectId}, wordDifficultyScore={wordDifficultyScore}, rareWordCount={rareWordCount}, hintUsageCount={hintUsageCount}"
            );
            yield break;
        }

        string url = $"{Web.BaseApiUrl}{UpdateVocabularyRangeUrl}";
        WWWForm form = new WWWForm();

        // Calculate vocabulary range score with default 10, decrease by incorrect answers, hints, and skips
        int vocabularyRangeScore = Mathf.Clamp(
            10 - ((incorrectAnswerCount / 2) + (hintUsageCount / 2) + (skipUsageCount / 2)),
            0,
            10
        );

        Debug.Log(
            $"Sending UpdateVocabularyRange request with parameters: studentId={studentId}, lessonId={lessonId}, gameModeId={gameModeId}, subjectId={subjectId}, vocabularyRangeScore={vocabularyRangeScore}"
        );

        form.AddField("student_id", studentId);
        form.AddField("lesson_id", lessonId);
        form.AddField("game_mode_id", gameModeId);
        form.AddField("subject_id", subjectId);
        form.AddField("vocabulary_range_score", vocabularyRangeScore);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Vocabulary range attribute updated successfully.");
            }
            else
            {
                Debug.LogError(
                    $"Failed to update vocabulary range attribute: {www.error}. Response: {www.downloadHandler.text}"
                );
            }
        }
    }

    public IEnumerator UpdateAccuracy(
        int studentId,
        int lessonId,
        int gameModeId,
        int subjectId,
        int correctAnswers,
        int totalAttempts
    )
    {
        if (
            studentId <= 0
            || lessonId <= 0
            || gameModeId <= 0
            || subjectId <= 0
            || totalAttempts <= 0
        )
        {
            Debug.LogError(
                $"Invalid parameters for UpdateAccuracy. Ensure all IDs and attempts are valid. "
                    + $"studentId={studentId}, lessonId={lessonId}, gameModeId={gameModeId}, subjectId={subjectId}, correctAnswers={correctAnswers}, totalAttempts={totalAttempts}"
            );
            yield break;
        }

        string url = $"{Web.BaseApiUrl}updateAccuracyAttribute.php";
        WWWForm form = new WWWForm();

        // Calculate accuracy as a percentage and round to the nearest integer
        int accuracy = Mathf.RoundToInt((float)(correctAnswers / 2) / (totalAttempts / 2) * 10);

        Debug.Log(
            $"Sending UpdateAccuracy request with parameters: studentId={studentId}, lessonId={lessonId}, gameModeId={gameModeId}, subjectId={subjectId}, accuracy={accuracy}"
        );

        form.AddField("student_id", studentId);
        form.AddField("lesson_id", lessonId);
        form.AddField("game_mode_id", gameModeId);
        form.AddField("subject_id", subjectId);
        form.AddField("accuracy", accuracy); // Use the rounded integer value

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Accuracy attribute updated successfully.");
            }
            else
            {
                Debug.LogError(
                    $"Failed to update accuracy attribute: {www.error}. Response: {www.downloadHandler.text}"
                );
            }
        }
    }

    public IEnumerator UpdateProblemSolving(
        int studentId,
        int lessonId,
        int gameModeId,
        int subjectId,
        int totalHintsUsed,
        int totalSkipsUsed
    )
    {
        if (studentId <= 0 || lessonId <= 0 || gameModeId <= 0 || subjectId <= 0)
        {
            Debug.LogError(
                $"Invalid parameters for UpdateProblemSolving. Ensure all IDs are valid. "
                    + $"studentId={studentId}, lessonId={lessonId}, gameModeId={gameModeId}, subjectId={subjectId}, totalHintsUsed={totalHintsUsed}, totalSkipsUsed={totalSkipsUsed}"
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
            $"Sending UpdateProblemSolving request with parameters: studentId={studentId}, lessonId={lessonId}, gameModeId={gameModeId}, subjectId={subjectId}, problemSolvingScore={problemSolvingScore}"
        );

        form.AddField("student_id", studentId);
        form.AddField("lesson_id", lessonId);
        form.AddField("game_mode_id", gameModeId);
        form.AddField("subject_id", subjectId);
        form.AddField("problem_solving", problemSolvingScore);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(
                    $"Problem-solving attribute updated successfully. Response: {www.downloadHandler.text}"
                );
            }
            else
            {
                Debug.LogError(
                    $"Failed to update problem-solving attribute: {www.error}. Response: {www.downloadHandler.text}"
                );
            }
        }
    }

    public IEnumerator UpdateConsistency(
        int studentId,
        int lessonId,
        int gameModeId,
        int subjectId,
        int currentScore
    )
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
        form.AddField("lesson_id", lessonId);
        form.AddField("game_mode_id", gameModeId);
        form.AddField("subject_id", subjectId);
        form.AddField("current_score", currentScore);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Consistency attribute updated successfully.");
            }
            else
            {
                Debug.LogError(
                    $"Failed to update consistency attribute: {www.error}. Response: {www.downloadHandler.text}"
                );
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
        int lessonId,
        int gameModeId,
        int subjectId,
        int skipUsageCount,
        int hintUsageCount,
        int incorrectAnswerCount
    )
    {
        if (studentId <= 0 || lessonId <= 0 || gameModeId <= 0 || subjectId <= 0)
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
            $"Sending UpdateRetention request with parameters: studentId={studentId}, lessonId={lessonId}, gameModeId={gameModeId}, subjectId={subjectId}, retentionScore={retentionScore}"
        );

        form.AddField("student_id", studentId);
        form.AddField("lesson_id", lessonId);
        form.AddField("game_mode_id", gameModeId);
        form.AddField("subject_id", subjectId);
        form.AddField("retention", retentionScore);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Retention attribute updated successfully.");
            }
            else
            {
                Debug.LogError(
                    $"Failed to update retention attribute: {www.error}. Response: {www.downloadHandler.text}"
                );
            }
        }
    }
}
