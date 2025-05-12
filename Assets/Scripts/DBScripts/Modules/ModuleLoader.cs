using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class ModuleLoader : MonoBehaviour
{
    public GameObject modulePrefab; // Prefab for modules
    public Transform parentTransform; // Parent transform for module instantiation
    public List<ModuleData> modules = new List<ModuleData>();

    private void Start()
    {
        // Automatically load modules for the default selected button when the scene starts
        LoadModulesForSelectedSubject();
    }

    // Load modules for the selected subject based on the button clicked
    public void LoadModulesForSelectedSubject()
    {
        // Show loading screen at the start of lesson completion check
        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.ShowLoadingScreen(true);
        }
        // Get the selected fk_subject_id from ButtonTracker
        int subjectId = ButtonTracker.Instance.GetCurrentSubjectId();

        // Start the coroutine to fetch and load modules
        StartCoroutine(LoadModulesBySubject(subjectId));
    }

    private IEnumerator LoadModulesBySubject(int subjectId)
    {
        int maxRetries = 3;
        int attempt = 0;
        float retryDelay = 2f; // seconds
        WWWForm form = new WWWForm();
        form.AddField("fk_subject_id", subjectId);
        form.AddField("student_id", PlayerPrefs.GetString("User ID")); // Pass the student ID

        while (attempt < maxRetries)
        {
            attempt++;
            using (
                UnityWebRequest www = UnityWebRequest.Post(
                    Web.BaseApiUrl + "getModulesWithProgress.php",
                    form
                )
            )
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = www.downloadHandler.text;
                    Debug.Log("Modules received: " + jsonResponse);
                    modules.Clear();
                    modules = JsonConvert.DeserializeObject<List<ModuleData>>(jsonResponse);
                    string subjectName =
                        subjectId == 1 ? "English" : (subjectId == 2 ? "Science" : "Unknown");

                    foreach (Transform child in parentTransform)
                    {
                        Destroy(child.gameObject);
                    }

                    bool previousModuleCompleted = true; // Start with the first module unlocked

                    foreach (var module in modules)
                    {
                        GameObject moduleInstance = Instantiate(modulePrefab, parentTransform);
                        ModuleUI moduleUI = moduleInstance.GetComponent<ModuleUI>();

                        if (moduleUI != null)
                        {
                            bool isCompleted = module.is_completed; // Check if the module is completed
                            moduleUI.SetModuleData(
                                module.module_number,
                                subjectName,
                                previousModuleCompleted
                            );
                            previousModuleCompleted = isCompleted; // Unlock the next module only if the current one is completed
                        }
                    }

                    if (GameLoadingManager.Instance != null)
                    {
                        GameLoadingManager.Instance.HideLoadingScreen();
                    }
                    break; // Exit the loop if successful
                }
                else
                {
                    Debug.LogError("Error fetching modules: " + www.error);
                    if (attempt < maxRetries)
                    {
                        Debug.LogWarning("Retrying to fetch modules...");
                        yield return new WaitForSeconds(retryDelay);
                    }
                }
            }
        }
    }
}

[System.Serializable]
public class ModuleData
{
    public string module_id;
    public string module_number;
    public string fk_subject_id;
    public bool is_completed; // Add completion status field
}
