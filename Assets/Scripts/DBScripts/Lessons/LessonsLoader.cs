using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LessonsLoader : MonoBehaviour
{
    public static LessonsLoader Instance;

    private void Awake()
    {
        Instance = this;
    }

    public GameObject lessonPrefab;
    public GameObject englishLessonPrefab;
    public GameObject scienceLessonPrefab;
    public Transform parentTransform;
    public List<LessonData> lessons = new List<LessonData>();
    public static string moduleNumber = "1"; // Default to "1" to avoid null issues
    public static int subjectId;

    [SerializeField]
    public Image subjectImage;

    [SerializeField]
    public Image subjectArrow; // Add reference to arrow image

    [SerializeField]
    public Text subjectText;

    //    public Transform parentTransform;

    private void Start()
    {
        moduleNumber = ModuleManager.Instance.GetCurrentModule();

        LoadLessonsForSelectedModuleAndSubject();
    }

    //Load lessons for the selected modules based on module number and subject id
    public void LoadLessonsForSelectedModuleAndSubject()
    {
        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.ShowLoadingScreen(true);
        }
        // Reset the lessons and UI
        ResetLessons();
        //get the selected module_number and fk_subject_id from ButtonTracker
        subjectId = ButtonTracker.Instance.GetCurrentSubjectId();
        moduleNumber = ModuleManager.Instance.GetCurrentModule();

        // Save the current tracking IDs
        // Web.SetCurrentTrackingIds(subjectId, int.Parse(moduleNumber), 0); // Lesson ID is 0 initially

        // Debug.Log($"Loading lessons for Subject ID: {subjectId}, Module Number: {moduleNumber}");
        //start the coroutine to fetch and load modules
        StartCoroutine(LoadLessonsBySubject(subjectId, moduleNumber));
    }

    public void ResetLessons()
    {
        // Clear existing lesson prefabs in the parent transform
        foreach (Transform child in parentTransform)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("Lessons have been reset.");
    }

    private IEnumerator LoadLessonsBySubject(int subjectId, string moduleNumber)
    {
        int maxRetries = 3;
        int attempt = 0;
        float retryDelay = 2f; // seconds
        WWWForm form = new WWWForm();
        Debug.Log("subject id and module number = " + subjectId + " " + moduleNumber);

        form.AddField("module_number", moduleNumber);
        form.AddField("fk_subject_id", subjectId);
        while (attempt < maxRetries)
        {
            attempt++;
            using (
                UnityWebRequest www = UnityWebRequest.Post(Web.BaseApiUrl + "getLessons.php", form)
            )
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = www.downloadHandler.text;
                    Debug.Log("Lessons received: " + jsonResponse);
                    lessons.Clear();
                    lessons = JsonUtilityHelper.FromJsonList<LessonData>(jsonResponse);

                    if (lessons.Count == 0)
                    {
                        Debug.LogWarning("No lessons received for the selected module.");
                    }

                    string subjectName =
                        subjectId == 1 ? "English" : (subjectId == 2 ? "Science" : "Unknown");

                    string spriteName =
                        subjectId == 1
                            ? "EnglishLessonCard"
                            : (subjectId == 2 ? "ScienceLessonCard" : "LessonDefault");
                    Sprite subjectSprite = Resources.Load<Sprite>(spriteName);
                    string arrowName =
                        subjectId == 1
                            ? "EnglishArrow"
                            : (subjectId == 2 ? "ScienceArrow" : "LessonDefault");
                    Sprite arrowSprite = Resources.Load<Sprite>(arrowName);

                    if (subjectSprite != null && subjectImage != null)
                    {
                        subjectImage.sprite = subjectSprite;
                    }
                    else
                    {
                        Debug.LogError("Failed to load sprite: " + spriteName);
                    }

                    if (arrowSprite != null && subjectArrow != null)
                    {
                        subjectArrow.sprite = arrowSprite;
                    }
                    else
                    {
                        Debug.LogError("Failed to load arrow sprite: " + arrowName);
                    }

                    // Update subject name and color immediately
                    if (
                        MenuManager.InstanceMenu != null
                        && MenuManager.InstanceMenu.subjectName != null
                    )
                    {
                        MenuManager.InstanceMenu.subjectName.text = subjectName;
                        MenuManager.InstanceMenu.subjectName.color =
                            subjectId == 1
                                ? new Color(16f / 255f, 40f / 255f, 110f / 255f) // #10286e
                                : new Color(17f / 255f, 84f / 255f, 36f / 255f); // #115424

                        subjectText.color =
                            subjectId == 1
                                ? new Color(16f / 255f, 40f / 255f, 110f / 255f) // #10286e
                                : new Color(17f / 255f, 84f / 255f, 36f / 255f); // #115424
                    }

                    foreach (Transform child in parentTransform)
                    {
                        Destroy(child.gameObject);
                    }

                    foreach (var lesson in lessons)
                    {
                        Debug.Log("Lesson item: " + lesson);
                        string lessonName = lesson.lesson_name;
                        string lessonNumber = lesson.lesson_number; // Use lesson_number from the fetched data
                        string lessonLink = lesson.lesson_link;
                        GameObject prefabToUse = lessonPrefab;
                        if (subjectId == 1 && englishLessonPrefab != null)
                        {
                            prefabToUse = englishLessonPrefab;
                        }
                        else if (subjectId == 2 && scienceLessonPrefab != null)
                        {
                            prefabToUse = scienceLessonPrefab;
                        }

                        GameObject lessonInstance = Instantiate(prefabToUse, parentTransform);
                        LessonUI lessonUI = lessonInstance.GetComponent<LessonUI>();

                        if (lessonUI != null)
                        {
                            lessonUI.SetLessonData(
                                lessonName,
                                lessonNumber,
                                subjectName,
                                moduleNumber,
                                lessonLink
                            );

                            // Add a listener to track the clicked lesson
                            lessonUI.actionButton.onClick.AddListener(() =>
                            {
                                string clickedLessonId = lessonUI.GetLessonId(); // Retrieve the lesson ID from LessonUI
                                // Web.SetCurrentTrackingIds(subjectId, int.Parse(moduleNumber), int.Parse(clickedLessonId));
                                // Debug.Log(
                                //     $"Tracking updated: Subject ID: {subjectId}, Module Number: {moduleNumber}, Lesson ID: {clickedLessonId}"
                                // );
                            });
                        }
                        else
                        {
                            Debug.LogWarning("LessonUI component is missing on the lesson prefab.");
                        }
                    }

                    if (GameLoadingManager.Instance != null)
                    {
                        GameLoadingManager.Instance.HideLoadingScreen();
                    }
                    break; // Success, exit the loop
                }
                else
                {
                    Debug.LogError(
                        $"Failed to load lessons: {www.error} (Attempt {attempt}/{maxRetries})"
                    );
                    if (attempt < maxRetries)
                    {
                        Debug.LogWarning("Retrying to load lessons...");
                        yield return new WaitForSeconds(retryDelay);
                        continue;
                    }
                }
            }
        }
    }
}

[System.Serializable]
public class LessonData
{
    public string lesson_id;
    public string subject_id;
    public string lesson_number;
    public string lesson_name;
    public string lesson_link;
}

//utility class to parse JSON arrays into lists

public static class JsonUtilityHelper
{
    public static List<T> FromJsonList<T>(string json)
    {
        string newJson = "{\"Items\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.Items;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> Items;
    }
}
