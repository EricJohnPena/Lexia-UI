using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

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
        // Get the selected fk_subject_id from ButtonTracker
        int subjectId = ButtonTracker.Instance.GetCurrentSubjectId();

        // Start the coroutine to fetch and load modules
        StartCoroutine(LoadModulesBySubject(subjectId));
    }

    private IEnumerator LoadModulesBySubject(int subjectId)
    {
        WWWForm form = new WWWForm();
        form.AddField("fk_subject_id", subjectId);

        using (UnityWebRequest www = UnityWebRequest.Post("http://192.168.1.154/db_unity/getModules.php", form))
        {
            //SetDefaultHeaders(www);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching modules: " + www.error);
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;
                Debug.Log("Modules received: " + jsonResponse);
                modules.Clear();
                // Parse JSON response into a list of ModuleData
                modules = JsonConvert.DeserializeObject<List<ModuleData>>(jsonResponse);

                // Determine subject name dynamically
                string subjectName = subjectId == 1 ? "English" : (subjectId == 2 ? "Science" : "Unknown");

                // Clear existing module prefabs
                foreach (Transform child in parentTransform)
                {
                    Destroy(child.gameObject);
                }

                // Instantiate prefabs for each module
                foreach (var module in modules)
                {
                    GameObject moduleInstance = Instantiate(modulePrefab, parentTransform);
                    ModuleUI moduleUI = moduleInstance.GetComponent<ModuleUI>();

                    if (moduleUI != null)
                    {
                        // Set the module's data (e.g., name and number)
                        moduleUI.SetModuleData(module.module_number, subjectName);
                    }
                    else
                    {
                        Debug.LogWarning("ModuleUI component is missing on the module prefab.");
                    }
                }
            }
        }
    }


    private void SetDefaultHeaders(UnityWebRequest www)
    {
        //www.SetRequestHeader("Accept", "application/json");
        www.SetRequestHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        www.SetRequestHeader("Accept-Encoding", "gzip, deflate");
        www.SetRequestHeader("Accept-Language", "en-US,en;q=0.5");
        www.SetRequestHeader("Cache-Control", "max-age=0");
        www.SetRequestHeader("Cookie", "__test=a179c2531f72ee6d3d402e20c138e870");
        www.SetRequestHeader("Upgrade-Insecure-Requests", "1");
        www.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:134.0) Gecko/20100101 Firefox/134.0");
    }
}


[System.Serializable]
public class ModuleData
{
    public string module_id;
    public string module_number;
    public string fk_subject_id;
}

