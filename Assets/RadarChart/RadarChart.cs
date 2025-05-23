using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace RadarChart
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class RadarChart : MonoBehaviour
    {
        public Text accuracyText;
        public Text speedText;
        public Text problemSolvingSkillsText;
        public Text vocabularyRangeText;
        public Text consistencyText;
        public Text retentionText;

        [SerializeField]
        private RadarStyle style;

        [SerializeField]
        public List<RadarItem> radarItems = new List<RadarItem>();

        private bool needsUpdate = true;
        private string lastFetchedUserId = null; // Track the last user we fetched items for

        private void Start()
        {
            // Initialize with empty values
            UpdateRadarValuesText();
        }

        private void OnEnable()
        {
            // Check if there's a pending radar fetch
            string pendingUserId = PlayerPrefs.GetString("PendingRadarFetch", "");
            if (!string.IsNullOrEmpty(pendingUserId))
            {
                Debug.Log($"Processing pending radar fetch for user: {pendingUserId}");
                // Clear the pending flag
                PlayerPrefs.DeleteKey("PendingRadarFetch");
                // Fetch the radar items
                StartCoroutine(FetchRadarItems(pendingUserId));
            }
        }

        // Public method to fetch radar items
        public void FetchItemsForCurrentUser()
        {
            string currentUserId = PlayerPrefs.GetString("User ID");
            Debug.Log("User ID: " + currentUserId);

            Debug.Log($"Fetching radar items for user: {currentUserId}");
            // Show loading screen at the start of lesson completion check
            if (GameLoadingManager.Instance != null)
            {
                GameLoadingManager.Instance.ShowLoadingScreen(true);
            }

            // Clear existing radar items and force a redraw to clear the mesh
            ClearRadarItems();
            ForceRedraw();

            lastFetchedUserId = currentUserId;

            // Check if the GameObject is active before starting the coroutine
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(FetchRadarItems(currentUserId));
            }
            else
            {
                Debug.LogWarning(
                    $"Cannot start radar chart coroutine - GameObject '{gameObject.name}' is inactive. Will fetch data when activated."
                );
                // Store the user ID to fetch data when activated
                PlayerPrefs.SetString("PendingRadarFetch", currentUserId);
            }
        }

        private void Update()
        {
            if (needsUpdate)
            {
                if (radarItems != null && radarItems.Count > 0)
                {
                    // Create a new mesh each time
                    Mesh newMesh = new Mesh();
                    newMesh.MarkDynamic();

                    RadarDrawer radarDrawer = new RadarDrawer(canvasRenderer, radarItems, style);
                    radarDrawer.Draw(newMesh);
                }
                else
                {
                    // Clear the mesh if there are no items
                    if (canvasRenderer != null)
                    {
                        canvasRenderer.SetMesh(null);
                    }
                }
                needsUpdate = false;
            }
        }

        public IEnumerator FetchRadarItems(string student_id)
        {
            // Clear existing radar items first
            radarItems.Clear();
            needsUpdate = true;

            if (string.IsNullOrEmpty(student_id))
            {
                // Hide loading screen if there's an error
                if (GameLoadingManager.Instance != null)
                {
                    GameLoadingManager.Instance.HideLoadingScreen();
                }
                Debug.LogWarning("User ID is null or empty. Cannot fetch radar items.");
                yield break;
            }

            // Force a fresh fetch from the server
            WWWForm form = new WWWForm();
            form.AddField("student_id", student_id);

            int maxRetries = 3;
            int attempt = 0;
            float retryDelay = 2f; // seconds
            while (attempt < maxRetries)
            {
                attempt++;

                using (
                    UnityWebRequest webRequest = UnityWebRequest.Post(
                        Web.BaseApiUrl + "getRadarItems.php",
                        form
                    )
                )
                {
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError("Error fetching radar items: " + webRequest.error);
                        yield return new WaitForSeconds(retryDelay); // Wait before retrying
                    }
                    else
                    {
                        string jsonResponse = webRequest.downloadHandler.text;
                        Debug.Log("Raw Radar Items Response: " + jsonResponse);

                        // Parse the JSON response
                        if (jsonResponse == null)
                        {
                            yield break;
                        }

                        try
                        {
                            // Parse as array of dictionaries
                            var radarArray = JsonConvert.DeserializeObject<
                                List<Dictionary<string, int>>
                            >(jsonResponse);
                            if (radarArray != null && radarArray.Count > 0)
                            {
                                Debug.Log($"Fetched {radarArray.Count} radar items from server.");

                                // Only use the first dictionary in the array
                                var radarValues = radarArray[0];
                                Debug.Log(
                                    $"Using first set of radar values: {JsonConvert.SerializeObject(radarValues)}"
                                );

                                // Define the expected radar items in order
                                string[] expectedItems = new string[]
                                {
                                    "accuracy",
                                    "speed",
                                    "problem_solving_skills",
                                    "vocabulary_range",
                                    "consistency",
                                    "retention",
                                };

                                // Create radar items in the correct order
                                radarItems.Clear(); // Ensure the list is cleared before adding new items
                                foreach (string itemName in expectedItems)
                                {
                                    if (radarValues.ContainsKey(itemName))
                                    {
                                        RadarItem radarItem = new RadarItem
                                        {
                                            Name = itemName,
                                            Value = Mathf.Clamp(radarValues[itemName], 0, 10),
                                        };
                                        radarItems.Add(radarItem);
                                        Debug.Log(
                                            $"Added radar item: {itemName} = {radarValues[itemName]}"
                                        );
                                    }
                                    else
                                    {
                                        Debug.LogWarning(
                                            $"Missing expected radar item: {itemName}"
                                        );
                                        RadarItem radarItem = new RadarItem
                                        {
                                            Name = itemName,
                                            Value = 0,
                                        };
                                        radarItems.Add(radarItem);
                                        Debug.Log($"Added default radar item: {itemName} = 0");
                                    }
                                }

                                // Ensure the radar items count does not exceed 6
                                if (radarItems.Count > 6)
                                {
                                    radarItems = radarItems.GetRange(0, 6);
                                    Debug.LogWarning(
                                        "Radar items count exceeded 6. Truncated to 6 items."
                                    );
                                }

                                // Save radar items to PlayerPrefs as JSON
                                string json = JsonConvert.SerializeObject(radarValues);
                                PlayerPrefs.SetString("RadarItems", json);
                                PlayerPrefs.Save();
                                UpdateRadarValuesText();
                                Debug.Log($"Final radar items count: {radarItems.Count}");
                                Debug.Log(
                                    $"Using {radarItems.Count} radar items for student ID {student_id}."
                                );
                                ForceRedraw();

                                if (GameLoadingManager.Instance != null)
                                {
                                    GameLoadingManager.Instance.HideLoadingScreen();
                                }
                                break; // Exit loop on success
                            }
                            else
                            {
                                Debug.Log("No radar items found for user ID " + student_id);
                                // Hide loading screen if there's an error
                                if (GameLoadingManager.Instance != null)
                                {
                                    GameLoadingManager.Instance.HideLoadingScreen();
                                }
                                yield break;
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Error parsing radar items: " + e.Message);
                            Debug.LogError("Stack trace: " + e.StackTrace);
                            yield break;
                        }
                    }
                }
            } // End of while loop
        }

        private void UpdateRadarValuesText()
        {
            if (radarItems.Count > 0)
            {
                accuracyText.text = radarItems[0].Value.ToString(); // Update accuracy text
                Debug.Log("Accuracy: " + radarItems[0].Value);
                speedText.text = radarItems[1].Value.ToString(); // Update speed text
                problemSolvingSkillsText.text = radarItems[2].Value.ToString(); // Update problem-solving skills text
                vocabularyRangeText.text = radarItems[3].Value.ToString(); // Update vocabulary range text
                consistencyText.text = radarItems[4].Value.ToString(); // Update consistency text
                retentionText.text = radarItems[5].Value.ToString(); // Update retention text
            }
            else
            {
                accuracyText.text = "0"; // Update accuracy text
                speedText.text = "0"; // Update speed text
                problemSolvingSkillsText.text = "0"; // Update problem-solving skills text
                vocabularyRangeText.text = "0"; // Update vocabulary range text
                consistencyText.text = "0"; // Update consistency text
                retentionText.text = "0"; // Update retention text
            }
        }

        [SerializeField, HideInInspector]
        private CanvasRenderer canvasRenderer;

        private void OnValidate()
        {
            canvasRenderer = GetComponent<CanvasRenderer>();
        }

        public void SetStat(string id, int val)
        {
            for (var i = 0; i < radarItems.Count; i++)
            {
                var radarItem = radarItems[i];
                if (radarItem.Name.Equals(id))
                    radarItem.Value = Mathf.Clamp(val, 0, 10);
            }
        }

        public void ClearRadarItems()
        {
            Debug.Log("Starting to clear radar items...");
            Debug.Log($"Current radar items count before clear: {radarItems.Count}");

            // Clear the radar items list
            radarItems.Clear();

            // Clear the mesh from the canvas renderer
            if (canvasRenderer != null)
            {
                canvasRenderer.SetMesh(null);
                Debug.Log("Cleared mesh from canvas renderer");
            }

            // Reset all text fields to 0
            if (accuracyText != null)
                accuracyText.text = "0";
            if (speedText != null)
                speedText.text = "0";
            if (problemSolvingSkillsText != null)
                problemSolvingSkillsText.text = "0";
            if (vocabularyRangeText != null)
                vocabularyRangeText.text = "0";
            if (consistencyText != null)
                consistencyText.text = "0";
            if (retentionText != null)
                retentionText.text = "0";

            // Force an update to ensure the chart is cleared
            needsUpdate = true;

            // Reset the last fetched user ID
            lastFetchedUserId = null;

            Debug.Log($"Final radar items count after clear: {radarItems.Count}");
            Debug.Log("Radar items cleared successfully");
        }

        public void ForceRedraw()
        {
            needsUpdate = true;
        }
    }

    [System.Serializable]
    public class RadarItem
    {
        public string Name { get; set; } // Add a setter to the Name property
        public int Value { get; set; }
    }
}
