using System.Collections;
using System.Collections.Generic;
using MenuScripts;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SubjectProgressManager : MonoBehaviour
{
    public static SubjectProgressManager Instance { get; private set; }

    [SerializeField]
    private GameObject subjectProgressModalObject;

    [SerializeField]
    private GameObject englishModuleProgressPrefab;

    [SerializeField]
    private GameObject scienceModuleProgressPrefab;

    [SerializeField]
    private Transform contentParent;

    [SerializeField]
    private Text subjectNameText;

    [SerializeField]
    private Button closeButton;

    [SerializeField]
    private Button englishButton;

    [SerializeField]
    private Button scienceButton;

    [SerializeField]
    private Text generatedCommentText;

    [SerializeField]
    private Text teacherCommentText;

    [SerializeField]
    private RadialProgressIndicator englishProgressIndicator;

    [SerializeField]
    private RadialProgressIndicator scienceProgressIndicator;

    // Subject-specific UI elements
    [Header("Subject-specific UI Elements")]
    [SerializeField]
    private Image subjectIconImage; // Main subject icon

    [SerializeField]
    private Image subjectBackgroundImage; // Subject background image

    [SerializeField]
    private Text[] subjectTexts; // Array of texts to change color based on subject

    // Subject-specific colors and sprites
    [Header("Subject-specific Colors and Sprites")]
    [SerializeField]
    private Color[] subjectColors = new Color[2]
    {
        new Color32(0, 102, 204, 255), // English blue
        new Color32(0, 153, 0, 255), // Science green
    };

    [SerializeField]
    private Sprite[] subjectIcons; // Array of subject icons

    [SerializeField]
    private Sprite[] subjectBackgrounds; // Array of subject background images

    private List<ModuleProgressData> moduleProgressList = new List<ModuleProgressData>();

    private int currentSubjectId = 1; // Default to English

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        closeButton.onClick.AddListener(() => subjectProgressModalObject.SetActive(false));

        // Add button listeners
        if (englishButton != null)
        {
            englishButton.onClick.AddListener(() => SwitchSubject(1));
        }

        if (scienceButton != null)
        {
            scienceButton.onClick.AddListener(() => SwitchSubject(2));
        }

        // Set initial button states
        if (englishButton != null && scienceButton != null)
        {
            englishButton.interactable = false; // English selected by default
            scienceButton.interactable = true;
        }

        // Load initial progress for both subjects
        StartCoroutine(LoadSubjectProgress(1)); // English
        StartCoroutine(LoadSubjectProgress(2)); // Science
    }

  

    public void ShowSubjectProgress(int subjectId)
    {
        subjectNameText.text =
            subjectId == 1 ? "English Progress" : (subjectId == 2 ? "Science Progress" : "Unknown");
        subjectProgressModalObject.SetActive(true);

        // Set initial button states
        if (englishButton != null && scienceButton != null)
        {
            englishButton.interactable = subjectId != 1;
            scienceButton.interactable = subjectId != 2;
        }

        currentSubjectId = subjectId;
        UpdateSubjectUI(subjectId);

        // Load progress and comments only for the selected subject
        StartCoroutine(LoadSubjectProgress(subjectId));
        
        // Load and display comments for the selected subject
        RadialProgressIndicator selectedIndicator =
            subjectId == 1 ? englishProgressIndicator : scienceProgressIndicator;
        if (selectedIndicator != null)
        {
            StartCoroutine(LoadAndDisplayComments(subjectId, selectedIndicator));
        }
    }

    private void SwitchSubject(int subjectId)
    {
        if (currentSubjectId == subjectId)
            return; // Don't switch if already on this subject

        currentSubjectId = subjectId;

        // Update button states
        if (englishButton != null && scienceButton != null)
        {
            englishButton.interactable = subjectId != 1;
            scienceButton.interactable = subjectId != 2;
        }

        // Update UI
        UpdateSubjectUI(subjectId);

        // Load progress and comments for the newly selected subject
        StartCoroutine(LoadSubjectProgress(subjectId));
        
        // Load and display comments for the selected subject
        RadialProgressIndicator selectedIndicator =
            subjectId == 1 ? englishProgressIndicator : scienceProgressIndicator;
        if (selectedIndicator != null)
        {
            StartCoroutine(LoadAndDisplayComments(subjectId, selectedIndicator));
        }
    }

    private void UpdateSubjectUI(int subjectId)
    {
        if (subjectId < 1 || subjectId > 2)
            return;

        int index = subjectId - 1; // Convert to 0-based index

        // Update subject icon
        if (subjectIconImage != null && index < subjectIcons.Length)
        {
            subjectIconImage.sprite = subjectIcons[index];
        }

        // Update subject background
        if (subjectBackgroundImage != null && index < subjectBackgrounds.Length)
        {
            subjectBackgroundImage.sprite = subjectBackgrounds[index];
        }

        // Update text colors
        if (subjectTexts != null)
        {
            foreach (Text text in subjectTexts)
            {
                if (text != null && index < subjectColors.Length)
                {
                    text.color = subjectColors[index];
                }
            }
        }
    }

    public IEnumerator LoadSubjectProgress(int subjectId)
    {
        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.ShowLoadingScreen(true);
        }

        // Only clear and create prefabs if this is the currently selected subject
        if (subjectProgressModalObject.activeSelf && subjectId == currentSubjectId)
        {
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }
        }

        int maxRetries = 3;
        int attempt = 0;
        float retryDelay = 2f; // seconds

        while (attempt < maxRetries)
        {
            attempt++;
            Debug.Log($"Attempt {attempt} to load subject progress.");

            WWWForm form = new WWWForm();
            form.AddField("subject_id", subjectId);
            form.AddField("student_id", PlayerPrefs.GetString("User ID"));

            using (
                UnityWebRequest www = UnityWebRequest.Post(
                    Web.BaseApiUrl + "getSubjectProgress.php",
                    form
                )
            )
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = www.downloadHandler.text;
                    try
                    {
                        moduleProgressList = JsonConvert.DeserializeObject<
                            List<ModuleProgressData>
                        >(jsonResponse);
                        if (moduleProgressList != null)
                        {
                            // Only display progress if this is the currently selected subject
                            if (subjectProgressModalObject.activeSelf && subjectId == currentSubjectId)
                            {
                                DisplayModuleProgress(subjectId);
                            }
                            UpdateRadialProgress(subjectId);
                            break; // Exit loop on success
                        }
                    }
                    catch (JsonException)
                    {
                        try
                        {
                            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(
                                jsonResponse
                            );
                            if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.error))
                            {
                                Debug.LogError(
                                    $"Server error (Attempt {attempt}/{maxRetries}): {errorResponse.error}"
                                );
                            }
                        }
                        catch (JsonException)
                        {
                            Debug.LogError(
                                $"Failed to parse server response (Attempt {attempt}/{maxRetries}): {jsonResponse}"
                            );
                        }
                    }
                }
                else
                {
                    Debug.LogError(
                        $"Error fetching subject progress (Attempt {attempt}/{maxRetries}): {www.error}"
                    );
                }

                if (attempt < maxRetries)
                {
                    Debug.Log($"Retrying in {retryDelay} seconds...");
                    yield return new WaitForSeconds(retryDelay);
                }
            }
        }

        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.HideLoadingScreen();
        }
    }

    private void UpdateRadialProgress(int subjectId)
    {
        if (moduleProgressList == null || moduleProgressList.Count == 0)
            return;

        float totalProgress = 0f;
        foreach (var module in moduleProgressList)
        {
            // Calculate progress as a percentage of 3 attempts per module
            float moduleProgress = Mathf.Clamp01(module.completed_count / 3f);
            totalProgress += moduleProgress;
        }

        // Calculate average progress across all modules
        float averageProgress = totalProgress / moduleProgressList.Count;

        // Update the appropriate radial indicator
        if (subjectId == 1 && englishProgressIndicator != null)
        {
            englishProgressIndicator.SetProgress(averageProgress, "English");
            // Only load comments if this is the currently selected subject
            if (subjectId == currentSubjectId)
            {
                StartCoroutine(LoadAndDisplayComments(subjectId, englishProgressIndicator));
            }
        }
        else if (subjectId == 2 && scienceProgressIndicator != null)
        {
            scienceProgressIndicator.SetProgress(averageProgress, "Science");
            // Only load comments if this is the currently selected subject
            if (subjectId == currentSubjectId)
            {
                StartCoroutine(LoadAndDisplayComments(subjectId, scienceProgressIndicator));
            }
        }
    }

    private void DisplayModuleProgress(int subjectId)
    {
        if (moduleProgressList == null || moduleProgressList.Count == 0)
        {
            Debug.LogWarning("No module progress data available to display.");
            return;
        }

        // Validate subject ID and get corresponding prefab
        GameObject prefabToUse = null;
        switch (subjectId)
        {
            case 1: // English
                if (englishModuleProgressPrefab == null)
                {
                    Debug.LogError("English module progress prefab is not assigned!");
                    return;
                }
                prefabToUse = englishModuleProgressPrefab;
                break;
            case 2: // Science
                if (scienceModuleProgressPrefab == null)
                {
                    Debug.LogError("Science module progress prefab is not assigned!");
                    return;
                }
                prefabToUse = scienceModuleProgressPrefab;
                break;
            default:
                Debug.LogError($"Invalid subject ID: {subjectId}. Must be 1 (English) or 2 (Science).");
                return;
        }

        int index = subjectId - 1; // Convert to 0-based index
        Color completeColor = subjectColors[index];
        Color incompleteColor = new Color32(204, 0, 0, 255); // Red color for incomplete status

        // Clear existing content
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Create progress items using the subject-specific prefab
        foreach (var moduleProgress in moduleProgressList)
        {
            GameObject progressItem = Instantiate(prefabToUse, contentParent);
            Text[] texts = progressItem.GetComponentsInChildren<Text>();
            
            if (texts == null || texts.Length < 2)
            {
                Debug.LogError($"Module progress prefab for subject {subjectId} is missing required Text components");
                continue;
            }

            texts[0].text = "Week " + moduleProgress.module_number;

            if (moduleProgress.completed_count >= 3)
            {
                texts[1].text = "Complete";
                texts[1].color = completeColor;
            }
            else
            {
                texts[1].text = $"{moduleProgress.completed_count}/3";
                texts[1].color = incompleteColor;
            }
        }
    }

    private IEnumerator LoadAndDisplayComments(int subjectId, RadialProgressIndicator indicator)
    {
        int maxRetries = 3;
        int attempt = 0;
        float retryDelay = 2f;

        while (attempt < maxRetries)
        {
            attempt++;
            Debug.Log($"Attempt {attempt} to load comments for subject {subjectId}");

            WWWForm form = new WWWForm();
            form.AddField("student_id", PlayerPrefs.GetString("User ID"));
            form.AddField("subject_id", subjectId);

            using (
                UnityWebRequest www = UnityWebRequest.Post(
                    Web.BaseApiUrl + "getStudentComments.php",
                    form
                )
            )
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string responseText = www.downloadHandler.text;
                    CommentsResponse response = null;
                    bool shouldRetry = false;

                    try
                    {
                        response = JsonConvert.DeserializeObject<CommentsResponse>(responseText);
                    }
                    catch (JsonException)
                    {
                        Debug.LogError(
                            $"Failed to parse comments response (Attempt {attempt}/{maxRetries})"
                        );
                        shouldRetry = true;
                    }

                    if (shouldRetry)
                    {
                        if (attempt < maxRetries)
                        {
                            yield return new WaitForSeconds(retryDelay);
                            continue;
                        }
                    }
                    else if (response != null)
                    {
                        if (response.error != null)
                        {
                            Debug.LogError($"Server error: {response.error}");
                            if (attempt < maxRetries)
                            {
                                yield return new WaitForSeconds(retryDelay);
                                continue;
                            }
                        }
                        else
                        {
                            string generatedComment = !string.IsNullOrEmpty(
                                response.generated_comment
                            )
                                ? response.generated_comment
                                : "There are no comments yet.";
                            string teacherComment = !string.IsNullOrEmpty(response.teacher_comment)
                                ? response.teacher_comment
                                : "There are no comments yet.";

                            yield return new WaitForEndOfFrame();

                            // Update indicator (if used visually)
                            if (indicator != null)
                            {
                                indicator.SetComments(generatedComment, teacherComment);
                            }

                            // Update actual UI Text fields so user sees the new content
                            if (generatedCommentText != null)
                            {
                                generatedCommentText.text = generatedComment;
                            }
                            if (teacherCommentText != null)
                            {
                                teacherCommentText.text = teacherComment;
                            }

                            Debug.Log(
                                $"Updated comments for subject {subjectId}: Generated={generatedComment}, Teacher={teacherComment}"
                            );

                            break; // Success
                        }
                    }
                }
                else
                {
                    Debug.LogError(
                        $"Error fetching comments (Attempt {attempt}/{maxRetries}): {www.error}"
                    );
                    if (attempt < maxRetries)
                    {
                        yield return new WaitForSeconds(retryDelay);
                        continue;
                    }
                }
            }
        }

        // If all retries failed
        if (attempt >= maxRetries)
        {
            if (indicator != null)
            {
                indicator.SetComments("Unable to load comments.", "Unable to load comments.");
            }

            if (generatedCommentText != null)
            {
                generatedCommentText.text = "Unable to load comments.";
            }

            if (teacherCommentText != null)
            {
                teacherCommentText.text = "Unable to load comments.";
            }
        }
    }

    [System.Serializable]
    private class ErrorResponse
    {
        public string error;
    }

    [System.Serializable]
    private class CommentsResponse
    {
        public string generated_comment;
        public string teacher_comment;
        public string error;
    }
}
