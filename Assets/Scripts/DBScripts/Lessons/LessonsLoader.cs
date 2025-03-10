using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LessonsLoader : MonoBehaviour
{
    public static LessonsLoader Instance;
    private void Awake()
    {
        Instance = this;
    }
    public GameObject lessonPrefab;
    public Transform parentTransform;
    public List<LessonData> lessons = new List<LessonData>();
    string moduleNumber;

    //    public Transform parentTransform;

    private void Start()
    {

        moduleNumber = ModuleManager.Instance.GetCurrentModule();

        LoadLessonsForSelectedModuleAndSubject();

    }

    //Load lessons for the selected modules based on module number and subject id
    public void LoadLessonsForSelectedModuleAndSubject()
    {
        // Reset the lessons and UI
        ResetLessons();
        //get the selected module_number and fk_subject_id from ButtonTracker
        int subjectId = ButtonTracker.Instance.GetCurrentSubjectId();
        moduleNumber = ModuleManager.Instance.GetCurrentModule();
        //access module data from module loader

        Debug.Log($"Loading lessons for Subject ID: {subjectId}, Module Number: {moduleNumber}");
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


        WWWForm form = new WWWForm();
        Debug.Log("subject id and module number = " + subjectId + " " + moduleNumber);

        form.AddField("module_number", moduleNumber);
        form.AddField("fk_subject_id", subjectId);

        using (UnityWebRequest www = UnityWebRequest.Post("http://192.168.1.154/db_unity/getLessons.php", form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching modules: " + www.error);
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;
                Debug.Log("Lessons received: " + jsonResponse);
                // Clear the lessons list
                lessons.Clear();
                //Parse JSON response into a list of LessonDdata
                lessons = JsonUtilityHelper.FromJsonList<LessonData>(jsonResponse);
                if (lessons.Count == 0)
                {
                    Debug.LogWarning("No lessons received for the selected module.");
                }

                //determine lesson name dynamically
                string subjectName = subjectId == 1 ? "English" : (subjectId == 2 ? "Science" : "Unknown");
                //clear existing prefabs for each lessons
                foreach (Transform child in parentTransform)
                {
                    Destroy(child.gameObject);
                }

                //instantiate prefabs for each lesson
                foreach (var lesson in lessons)
                {
                    Debug.Log("Lessson items " + lesson);
                    string lessonName = lesson.lesson_name;
                    GameObject lessonInstance = Instantiate(lessonPrefab, parentTransform);
                    LessonUI lessonUI = lessonInstance.GetComponent<LessonUI>();

                    if (lessonUI != null)
                    {
                        //set lesson's data
                        lessonUI.SetLessonData(lessonName, lesson.lesson_number, subjectName);
                    }
                    else
                    {
                        Debug.Log("LessonUI component is missing on the lesson prefab/");
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