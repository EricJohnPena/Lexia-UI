using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GameProgressHandler : MonoBehaviour
{
    private const string UpdateSpeedUrl = "updateSpeedAttribute.php";
    private const int BaseTime = 30; // Base time in seconds

    public IEnumerator UpdateSpeed(int studentId, int lessonId, int gameModeId, int subjectId, float solveTime)
    {
        if (studentId <= 0 || lessonId <= 0 || gameModeId <= 0 || subjectId <= 0 || solveTime <= 0)
        {
            Debug.LogError($"Invalid parameters for UpdateSpeed. Ensure all IDs and solveTime are valid. " +
                           $"studentId={studentId}, lessonId={lessonId}, gameModeId={gameModeId}, subjectId={subjectId}, solveTime={solveTime}");
            yield break;
        }

        string url = $"{Web.BaseApiUrl}{UpdateSpeedUrl}";
        WWWForm form = new WWWForm();

        // Calculate speed as a whole number with a maximum value of 10
        int speed = Mathf.Clamp(Mathf.CeilToInt((BaseTime / solveTime) * 10), 0, 10);

        // Log the parameters being sent for debugging
        Debug.Log($"Sending UpdateSpeed request with parameters: studentId={studentId}, lessonId={lessonId}, gameModeId={gameModeId}, subjectId={subjectId}, speed={speed}");

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
                Debug.LogError($"Failed to update speed attribute: {www.error}. Response: {www.downloadHandler.text}");
            }
        }
    }

    public IEnumerator UpdateAccuracy(int studentId, int lessonId, int gameModeId, int subjectId, int correctAnswers, int totalAttempts)
    {
        if (studentId <= 0 || lessonId <= 0 || gameModeId <= 0 || subjectId <= 0 || totalAttempts <= 0)
        {
            Debug.LogError($"Invalid parameters for UpdateAccuracy. Ensure all IDs and attempts are valid. " +
                           $"studentId={studentId}, lessonId={lessonId}, gameModeId={gameModeId}, subjectId={subjectId}, correctAnswers={correctAnswers}, totalAttempts={totalAttempts}");
            yield break;
        }

        string url = $"{Web.BaseApiUrl}updateAccuracyAttribute.php";
        WWWForm form = new WWWForm();

        // Calculate accuracy as a percentage and round to the nearest integer
        int accuracy = Mathf.RoundToInt((float)correctAnswers / totalAttempts * 10);

        Debug.Log($"Sending UpdateAccuracy request with parameters: studentId={studentId}, lessonId={lessonId}, gameModeId={gameModeId}, subjectId={subjectId}, accuracy={accuracy}");

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
                Debug.LogError($"Failed to update accuracy attribute: {www.error}. Response: {www.downloadHandler.text}");
            }
        }
    }
}
