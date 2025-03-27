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
    public GameObject podiumEntryPrefab;

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
        LoadLeaderboard();
    }

    private void SetupSubjectButtons()
    {
        englishButton.onClick.AddListener(() => FilterBySubject(1));
        scienceButton.onClick.AddListener(() => FilterBySubject(2));
    }

    public void FilterBySubject(int subjectId)
    {
        currentSubjectId = subjectId;
        LoadLeaderboard();
    }

    public void LoadLeaderboard()
    {
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
        Debug.Log("Leaderboard has been reset.");
    }

    private IEnumerator LoadLeaderboardData()
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", PlayerPrefs.GetString("User ID"));
        form.AddField("subject_id", currentSubjectId);

        using (UnityWebRequest www = UnityWebRequest.Post(Web.BaseApiUrl + "getLeaderboard.php", form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching leaderboard: " + www.error);
                yield break;
            }

            string jsonResponse = www.downloadHandler.text;
            Debug.Log("Leaderboard received: " + jsonResponse);
            
            leaderboardEntries = JsonUtilityHelper.FromJsonList<LeaderboardData>(jsonResponse);
            
            if (leaderboardEntries.Count == 0)
            {
                Debug.LogWarning("No leaderboard entries received.");
                yield break;
            }

            // Display top 3 players on podium
            for (int i = 0; i < Mathf.Min(3, leaderboardEntries.Count); i++)
            {
                var entry = leaderboardEntries[i];
                GameObject podiumInstance = Instantiate(podiumEntryPrefab, podiumContainer);
                var podiumUI = podiumInstance.GetComponent<LeaderboardPodiumUI>();
                
                if (podiumUI != null)
                {
                    podiumUI.SetPodiumData(entry.username, entry.score, i + 1);
                }
            }

            // Display remaining players in the list
            for (int i = 3; i < leaderboardEntries.Count; i++)
            {
                var entry = leaderboardEntries[i];
                GameObject entryInstance = Instantiate(leaderboardEntryPrefab, listContainer);
                var entryUI = entryInstance.GetComponent<LeaderboardEntryUI>();

                if (entryUI != null)
                {
                    entryUI.SetEntryData(entry.username, entry.score, i + 1);
                }
                else
                {
                    Debug.LogWarning("LeaderboardEntryUI component is missing on leaderboardEntryPrefab.");
                }
            }
        }
    }

    private void ValidatePrefabs()
    {
        if (leaderboardEntryPrefab.GetComponent<LeaderboardEntryUI>() == null)
        {
            Debug.LogError("leaderboardEntryPrefab is missing the LeaderboardEntryUI component. Please fix this in the Unity Editor.");
        }
    }
}

[System.Serializable]
public class LeaderboardData
{
    public string username;
    public int score;
    public int rank;
}

