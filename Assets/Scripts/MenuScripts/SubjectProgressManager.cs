using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;
using MenuScripts;

public class SubjectProgressManager : MonoBehaviour
{
    public static SubjectProgressManager Instance { get; private set; }

    [SerializeField] private GameObject subjectProgressModalObject;
    [SerializeField] private GameObject moduleProgressPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private Text subjectNameText;
    [SerializeField] private Button closeButton;

    [SerializeField] private RadialProgressIndicator englishProgressIndicator;
    [SerializeField] private RadialProgressIndicator scienceProgressIndicator;

    private List<ModuleProgressData> moduleProgressList = new List<ModuleProgressData>();

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
        // Load initial progress for both subjects
        StartCoroutine(LoadSubjectProgress(1)); // English
        StartCoroutine(LoadSubjectProgress(2)); // Science
    }

    public void ShowSubjectProgress(int subjectId)
    {
        subjectNameText.text = subjectId == 1 ? "English Progress" : (subjectId == 2 ? "Science Progress" : "Unknown");
        subjectProgressModalObject.SetActive(true);
        StartCoroutine(LoadSubjectProgress(subjectId));
    }

    public IEnumerator LoadSubjectProgress(int subjectId)
    {
        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.ShowLoadingScreen(true);
        }

        // Clear existing progress items if showing modal
        if (subjectProgressModalObject.activeSelf)
        {
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }
        }

        WWWForm form = new WWWForm();
        form.AddField("subject_id", subjectId);
        form.AddField("student_id", PlayerPrefs.GetString("User ID"));

        using (UnityWebRequest www = UnityWebRequest.Post(Web.BaseApiUrl + "getSubjectProgress.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = www.downloadHandler.text;
                try
                {
                    moduleProgressList = JsonConvert.DeserializeObject<List<ModuleProgressData>>(jsonResponse);
                    if (moduleProgressList != null)
                    {
                        if (subjectProgressModalObject.activeSelf)
                        {
                            DisplayModuleProgress();
                        }
                        UpdateRadialProgress(subjectId);
                    }
                }
                catch (JsonException)
                {
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(jsonResponse);
                        if (errorResponse != null && !string.IsNullOrEmpty(errorResponse.error))
                        {
                            Debug.LogError("Server error: " + errorResponse.error);
                        }
                    }
                    catch (JsonException)
                    {
                        Debug.LogError("Failed to parse server response: " + jsonResponse);
                    }
                }
            }
            else
            {
                Debug.LogError("Error fetching subject progress: " + www.error);
            }
        }

        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.HideLoadingScreen();
        }
    }

    private void UpdateRadialProgress(int subjectId)
    {
        if (moduleProgressList == null || moduleProgressList.Count == 0) return;

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
        }
        else if (subjectId == 2 && scienceProgressIndicator != null)
        {
            scienceProgressIndicator.SetProgress(averageProgress, "Science");
        }
    }

    private void DisplayModuleProgress()
    {
        foreach (var moduleProgress in moduleProgressList)
        {
            Debug.Log(moduleProgress);
            GameObject progressItem = Instantiate(moduleProgressPrefab, contentParent);
            Text[] texts = progressItem.GetComponentsInChildren<Text>();
            
            texts[0].text = "Week " + moduleProgress.module_number;
            
            if (moduleProgress.completed_count >= 3)
            {
                texts[1].text = "Complete";
                texts[1].color = Color.green;
            }
            else
            {
                texts[1].text = $"{moduleProgress.completed_count}/3";
                texts[1].color = Color.red;
            }
        }
    }

    [System.Serializable]
    private class ErrorResponse
    {
        public string error;
    }
} 