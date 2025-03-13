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
        private List<RadarItem> radarItems = new List<RadarItem>();

        private void Start()
        {
            Debug.Log("User ID: " + PlayerPrefs.GetString("User ID"));

            StartCoroutine(FetchRadarItems(PlayerPrefs.GetString("User ID")));
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

        private void Update()
        {
            if (radarItems.Count > 0)
            {
                RadarDrawer radarDrawer = new RadarDrawer(canvasRenderer, radarItems, style);
                radarDrawer.Draw();
            }
        }

        public IEnumerator FetchRadarItems(string student_id)
        {
            // Check if radar items are available in PlayerPrefs
            string radarItemsJson = PlayerPrefs.GetString("RadarItems", null);
            if (!string.IsNullOrEmpty(radarItemsJson))
            {
                // Deserialize JSON data into radarItems
                var radarValues = JsonConvert.DeserializeObject<Dictionary<string, int>>(
                    radarItemsJson
                );
                if (radarValues != null)
                {
                    radarItems.Clear();
                    foreach (var entry in radarValues)
                    {
                        RadarItem radarItem = new RadarItem
                        {
                            Name = entry.Key,
                            Value = Mathf.Max(0, entry.Value), // Ensure non-negative values
                        };
                        radarItems.Add(radarItem);
                    }
                    UpdateRadarValuesText(); // Update the UI text with fetched values
                    yield break; // Exit if items are loaded from PlayerPrefs
                }
            }

            if (string.IsNullOrEmpty(student_id))
            {
                Debug.LogWarning("User ID is null or empty. Cannot fetch radar items.");
                yield break; // Exit the coroutine if userId is not set
            }
            else
            {
                WWWForm form = new WWWForm();
                form.AddField("student_id", student_id);
                using (
                    UnityWebRequest webRequest = UnityWebRequest.Post(
                        "http://192.168.1.154/db_unity/getRadarItems.php?student_id=",
                        form
                    )
                )
                {
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError("Error fetching radar items: " + webRequest.error);
                    }
                    else
                    {
                        string jsonResponse = webRequest.downloadHandler.text;
                        Debug.Log("Radar Items Response: " + jsonResponse);

                        // Parse the JSON response
                        if (jsonResponse == null)
                        {
                            yield break;
                        }
                        var radarValues = JsonConvert.DeserializeObject<Dictionary<string, int>>(
                            jsonResponse.TrimStart('[').TrimEnd(']')
                        );
                        if (radarValues != null)
                        {
                            if (radarValues.Count > 0)
                            {
                                radarItems.Clear();
                                Debug.Log("Fetched " + radarValues.Count + " radar items.");
                                foreach (var entry in radarValues)
                                {
                                    RadarItem radarItem = new RadarItem
                                    {
                                        Name = entry.Key,
                                        Value = Mathf.Max(0, entry.Value), // Ensure non-negative values
                                    };
                                    radarItems.Add(radarItem);
                                }

                                // Save radar items to PlayerPrefs as JSON
                                string json = JsonConvert.SerializeObject(radarValues);
                                PlayerPrefs.SetString("RadarItems", json);
                                PlayerPrefs.Save();
                                UpdateRadarValuesText();
                                Debug.Log(
                                    $"Fetched {radarItems.Count} radar items for student ID {UserInfo.Instance.userId}."
                                );
                            }
                            else
                            {
                                Debug.Log("No radar items found for user ID " + student_id);
                                yield break;
                            }
                        }
                    }
                }
            }
        }

        public void SetStat(string id, int val)
        {
            for (var i = 0; i < radarItems.Count; i++)
            {
                var radarItem = radarItems[i];
                if (radarItem.Name.Equals(id))
                    radarItem.Value = val;
            }
        }

        public void ClearRadarItems()
        {
            radarItems.Clear();
        }
    }

    [System.Serializable]
    public class RadarItem
    {
        public string Name { get; set; } // Add a setter to the Name property
        public int Value { get; set; }
    }
}
