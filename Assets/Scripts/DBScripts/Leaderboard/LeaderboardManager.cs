using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;

    [Header("Subject Filter")]
    public Button englishButton;
    public Button scienceButton;
    private int currentSubjectId = 1; // Default to English

    [Header("Top 3 Players")]
    public Transform podiumContainer;
    public GameObject podiumEntryPrefab; // Default/fallback
    public GameObject podiumEntryEnglishPrefab;
    public GameObject podiumEntrySciencePrefab;

    [Header("Other Players")]
    public GameObject leaderboardEntryPrefab;
    public Transform listContainer;
    public List<LeaderboardData> leaderboardEntries = new List<LeaderboardData>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetupSubjectButtons();
        ValidatePrefabs();
        // LoadLeaderboard(); // Commented out to prevent loading leaderboard data on start
    }

    private void SetupSubjectButtons()
    {
        // Remove previous listeners to avoid duplicate calls
        englishButton.onClick.RemoveAllListeners();
        scienceButton.onClick.RemoveAllListeners();

        englishButton.onClick.AddListener(() =>
        {
            FilterBySubject(1);
            HighlightButton(englishButton, scienceButton);
        });

        scienceButton.onClick.AddListener(() =>
        {
            FilterBySubject(2);
            HighlightButton(scienceButton, englishButton);
        });

        // Set default subject and highlight
        currentSubjectId = 1;
        HighlightButton(englishButton, scienceButton);
        LoadLeaderboard();
    }

    private void HighlightButton(Button selectedButton, Button otherButton)
    {
        // Lower the opacity of the selected button's image
        Image selectedImage = selectedButton.GetComponent<Image>();
        if (selectedImage != null)
        {
            Color selectedColor = selectedImage.color;
            selectedColor.a = 0.5f; // Set opacity to 50%
            selectedImage.color = selectedColor;
        }

        // Reset the opacity of the other button's image
        Image otherImage = otherButton.GetComponent<Image>();
        if (otherImage != null)
        {
            Color otherColor = otherImage.color;
            otherColor.a = 1f; // Set opacity to 100%
            otherImage.color = otherColor;
        }

        // Change the text color of the selected button to black
        Text selectedText = selectedButton.GetComponentInChildren<Text>();
        if (selectedText != null)
        {
            selectedText.color = Color.black; // Selected text color
        }

        // Change the text color of the other button to white
        Text otherText = otherButton.GetComponentInChildren<Text>();
        if (otherText != null)
        {
            otherText.color = Color.white; // Default text color
        }
    }

    public void FilterBySubject(int subjectId)
    {
        if (currentSubjectId != subjectId)
        {
            currentSubjectId = subjectId;

            Debug.Log(
                $"id: {currentSubjectId} Subject changed to {(subjectId == 1 ? "English" : "Science")}. Reloading leaderboard..."
            );
            LoadLeaderboard();
        }
    }

    public void LoadLeaderboard()
    {
        // Show loading screen at the start of lesson completion check
        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.ShowLoadingScreen(true);
        }
        ResetLeaderboard();
        StartCoroutine(LoadLeaderboardData());
    }

    public void ResetLeaderboard()
    {
        // Clear podium entries
        foreach (Transform child in podiumContainer)
        {
            Destroy(child.gameObject);
        }

        // Clear list entries
        foreach (Transform child in listContainer)
        {
            Destroy(child.gameObject);
        }

        leaderboardEntries.Clear();
        //Debug.Log("Leaderboard has been reset. UI cleared.");
    }

    private IEnumerator LoadLeaderboardData()
    {
        int maxRetries = 3;
        int attempt = 0;
        float retryDelay = 2f; // seconds
        while (attempt < maxRetries)
        {
            attempt++;
            WWWForm form = new WWWForm();
            form.AddField("user_id", PlayerPrefs.GetString("User ID"));
            form.AddField("subject_id", currentSubjectId);
            Debug.Log(
                $"Sending request with subject_id: {currentSubjectId} (Attempt {attempt}/{maxRetries})"
            );

            using (
                UnityWebRequest www = UnityWebRequest.Post(
                    Web.BaseApiUrl + "getLeaderboard.php",
                    form
                )
            )
            {
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success)
                {
                    // Hide loading screen if there's an error
                    if (GameLoadingManager.Instance != null)
                    {
                        GameLoadingManager.Instance.HideLoadingScreen();
                    }
                    Debug.LogError(
                        $"Error fetching leaderboard: {www.error} (Attempt {attempt}/{maxRetries})"
                    );
                    if (attempt < maxRetries)
                    {
                        Debug.LogWarning("Retrying leaderboard fetch...");
                        yield return new WaitForSeconds(retryDelay);
                        continue;
                    }
                    yield break;
                }

                string jsonResponse = www.downloadHandler.text;
                Debug.Log($"Received response: {jsonResponse}");

                leaderboardEntries = JsonUtilityHelper.FromJsonList<LeaderboardData>(jsonResponse);
                if (leaderboardEntries.Count == 0)
                {
                    // Hide loading screen if there's an error
                    if (GameLoadingManager.Instance != null)
                    {
                        GameLoadingManager.Instance.HideLoadingScreen();
                    }
                    Debug.LogWarning(
                        "No leaderboard entries received for subject_id: " + currentSubjectId
                    );
                    yield break;
                }

                // Display top 3 players on podium
                for (int i = 0; i < Mathf.Min(3, leaderboardEntries.Count); i++)
                {
                    var entry = leaderboardEntries[i];
                    GameObject prefabToUse = podiumEntryPrefab;
                    if (currentSubjectId == 1 && podiumEntryEnglishPrefab != null)
                        prefabToUse = podiumEntryEnglishPrefab;
                    else if (currentSubjectId == 2 && podiumEntrySciencePrefab != null)
                        prefabToUse = podiumEntrySciencePrefab;
                    GameObject podiumInstance = Instantiate(prefabToUse, podiumContainer);
                    Debug.Log(
                        $"Instantiated podium prefab for: {entry.first_name} {entry.last_name}"
                    );
                    var podiumUI = podiumInstance.GetComponent<LeaderboardPodiumUI>();
                    if (podiumUI != null)
                    {
                        string fullName = $"{entry.first_name} {entry.last_name}";
                        podiumUI.SetPodiumData(fullName, entry.score, i + 1, entry.student_id);
                        // Position the podiums
                        RectTransform podiumTransform =
                            podiumInstance.GetComponent<RectTransform>();
                        if (podiumTransform != null)
                        {
                            podiumTransform.SetParent(podiumContainer, false); // Ensure it's a child of the container
                            // Calculate dynamic size and position
                            float containerWidth = ((RectTransform)podiumContainer).rect.width;
                            float spacing = containerWidth / 4; // Divide into 4 parts for even spacing
                            float prefabWidth = spacing * 0.8f; // Use 80% of the spacing for prefab width
                            // Adjust width and height based on rank
                            float prefabHeight = 150 + (3 - i) * 50; // Vary height: 1st tallest, 3rd shortest
                            podiumTransform.sizeDelta = new Vector2(prefabWidth, prefabHeight);
                            
                            // Set positions: 1st (center), 2nd (left), 3rd (right)
                            float xPosition = 0; // Default to center
                            switch (i)
                            {
                                case 0: // 1st place
                                    xPosition = 0;
                                    break;
                                case 1: // 2nd place
                                    xPosition = -spacing;
                                    break;
                                case 2: // 3rd place
                                    xPosition = spacing;
                                    break;
                            }

                            // Set the anchor to bottom center
                            podiumTransform.anchorMin = new Vector2(0.5f, 0f);
                            podiumTransform.anchorMax = new Vector2(0.5f, 0f);
                            podiumTransform.pivot = new Vector2(0.5f, 0f);
                            
                            // Position the podium at the bottom with the calculated x offset
                            podiumTransform.anchoredPosition = new Vector2(xPosition, 0);
                        }
                    }
                    else
                    {
                        // Hide loading screen if there's an error
                        if (GameLoadingManager.Instance != null)
                        {
                            GameLoadingManager.Instance.HideLoadingScreen();
                        }
                        Debug.LogWarning("Podium UI component is missing.");
                    }
                }

                // Display remaining players in the list
                for (int i = 3; i < leaderboardEntries.Count; i++)
                {
                    var entry = leaderboardEntries[i];
                    GameObject entryInstance = Instantiate(leaderboardEntryPrefab, listContainer);
                    Debug.Log($"Instantiated leaderboard entry prefab for: {entry.username}");
                    var entryUI = entryInstance.GetComponent<LeaderboardEntryUI>();
                    if (entryUI != null)
                    {
                        string fullName = $"{entry.first_name} {entry.last_name}";
                        entryUI.SetEntryData(fullName, entry.score, i + 1, entry.student_id);
                    }
                    else
                    {
                        // Hide loading screen if there's an error
                        if (GameLoadingManager.Instance != null)
                        {
                            GameLoadingManager.Instance.HideLoadingScreen();
                        }
                        Debug.LogWarning(
                            "LeaderboardEntryUI component is missing on leaderboardEntryPrefab."
                        );
                    }
                }

                // Adjust the size of the list container dynamically
                RectTransform listContainerRect = listContainer.GetComponent<RectTransform>();
                GridLayoutGroup gridLayout = listContainer.GetComponent<GridLayoutGroup>();
                if (listContainerRect != null && gridLayout != null)
                {
                    float itemHeight = gridLayout.cellSize.y;
                    float spacing = gridLayout.spacing.y;
                    int columns = Mathf.Max(
                        1,
                        Mathf.FloorToInt(listContainerRect.rect.width / gridLayout.cellSize.x)
                    );
                    int rows = Mathf.CeilToInt((float)leaderboardEntries.Count / columns);
                    // Calculate the new height based on the number of rows
                    float newHeight = (itemHeight + spacing) * rows - spacing; // Subtract spacing for the last row
                    listContainerRect.sizeDelta = new Vector2(
                        listContainerRect.sizeDelta.x,
                        newHeight
                    );
                }
                // Hide loading screen if there's an error
                if (GameLoadingManager.Instance != null)
                {
                    GameLoadingManager.Instance.HideLoadingScreen();
                }
                Debug.Log("Leaderboard UI updated successfully.");
                break; // Success, exit retry loop
            }
        }
    }

    private void ValidatePrefabs()
    {
        if (leaderboardEntryPrefab.GetComponent<LeaderboardEntryUI>() == null)
        {
            Debug.LogError(
                "leaderboardEntryPrefab is missing the LeaderboardEntryUI component. Please fix this in the Unity Editor."
            );
        }
    }
}

[System.Serializable]
public class LeaderboardData
{
    public string username;
    public int score;
    public int rank;
    public string first_name;
    public string last_name;
    public string student_id;
}
