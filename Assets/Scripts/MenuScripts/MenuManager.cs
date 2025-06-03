using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using RadarChart;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager InstanceMenu;

    [SerializeField]
    private RadarChart.RadarChart radarChart;

    private void Awake()
    {
        InstanceMenu = this;
    }

    [SerializeField]
    Button goToLoginBtn;

    //[SerializeField] Button goToSignupBtn;
    //[SerializeField] Button loginBtn;
    [SerializeField]
    Button goToLoginBtn1; //from signup page

    [SerializeField]
    Button goToSignupBtn1; //from login page

    [SerializeField]
    Button toLessonsPageBtn1;

    [SerializeField]
    Button toStudentLeaderboardBtn1;

    [SerializeField]
    Button toProfilePageBtn1;

    [SerializeField]
    Button toStudentDashboardPageBtn2;

    [SerializeField]
    Button toStudentLeaderboardBtn2;

    [SerializeField]
    Button toProfilePageBtn2;

    [SerializeField]
    Button toStudentDashboardPageBtn3;

    [SerializeField]
    Button toStudentLeaderboardBtn3;

    [SerializeField]
    Button toProfilePageBtn3;

    [SerializeField]
    Button backToDashboard;

    [SerializeField]
    public Text usernameText;

    [SerializeField]
    public Text subjectName;

    public Canvas StartingPage;
    public Canvas LoginPage;
    public Canvas SignupPage;
    public Canvas LoadingScene;
    public Canvas StudentDashboardPage;
    public Canvas StudentLeaderboardPage;
    public Canvas ProfilePage;
    public Canvas LessonsPage;
    public Canvas GameScene;

    public Button replayButton; // Assign this in the Unity Editor

    [SerializeField]
    private GameObject subjectProgressModalObject; // Reference to the GameObject

    [SerializeField]
    private SubjectProgressModal subjectProgressModal; // Reference to the component

    [SerializeField]
    private Button englishProgressButton;

    [SerializeField]
    private Button scienceProgressButton;


    [Header("Subject UI Elements")]
    [SerializeField]
    private Image[] subjectImages = new Image[8]; // Array of 8 UI images to change

    [SerializeField]
    private Text[] subjectTexts = new Text[7]; // Array of 7 text components to change

    // Subject-specific colors
    private readonly Color englishColor = new Color(16f/255f, 40f/255f, 110f/255f); // #10286e
    private readonly Color scienceColor = new Color(17f/255f, 84f/255f, 36f/255f); // #115424

    // Subject-specific sprites
    [SerializeField]
    private Sprite[] englishSprites = new Sprite[8];
    [SerializeField]
    private Sprite[] scienceSprites = new Sprite[8];

    void Start()
    {
        // Check if user is already logged in
        if (PlayerPrefs.HasKey("User ID"))
        {
            // User is logged in, go to Student Dashboard
            OpenPage(StudentDashboardPage);
            string User = PlayerPrefs.GetString("Username", "Guest User");
            usernameText.text = "Hi " + User + ",";
        }
        else
        {
            // No session found, show the login page
            OpenPage(StartingPage);
        }

        goToLoginBtn.onClick.AddListener(() => OpenPage(LoginPage));
        goToLoginBtn1.onClick.AddListener(() => OpenPage(LoginPage));
        //goToSignupBtn.onClick.AddListener(() => OpenPage(SignupPage));
        //goToSignupBtn1.onClick.AddListener(() => OpenPage(SignupPage));
        //loginBtn.onClick.AddListener(() => OpenPage(StudentDashboardPage));
        // toLessonsPageBtn1.onClick.AddListener(() => OpenPage(LessonsPage));
        //loginBtn.onClick.AddListener(ToMenuScene);//for testing purposes only
        toStudentLeaderboardBtn1.onClick.AddListener(() => OpenPage(StudentLeaderboardPage));
        toStudentLeaderboardBtn2.onClick.AddListener(() => OpenPage(StudentLeaderboardPage));
        toStudentLeaderboardBtn3.onClick.AddListener(() => OpenPage(StudentLeaderboardPage));

        toProfilePageBtn1.onClick.AddListener(() => OpenPage(ProfilePage));
        toProfilePageBtn2.onClick.AddListener(() => OpenPage(ProfilePage));
        toProfilePageBtn3.onClick.AddListener(() => OpenPage(ProfilePage));

        toStudentDashboardPageBtn2.onClick.AddListener(() => OpenPage(StudentDashboardPage));
        toStudentDashboardPageBtn3.onClick.AddListener(() => OpenPage(StudentDashboardPage));

        backToDashboard.onClick.AddListener(BackToStudentDashboard);

        // Add listener to the Replay button
        if (replayButton != null)
        {
            replayButton.onClick.AddListener(() =>
            {
                Debug.Log("Replay button clicked from GameComplete panel.");
                if (PanelManager.Instance != null)
                {
                    PanelManager.Instance.ReplayGame();
                }
                else
                {
                    Debug.LogError("PanelManager instance not found.");
                }
            });
        }

        // Add listeners for subject progress buttons
        if (englishProgressButton != null)
            englishProgressButton.onClick.AddListener(() => {
                SubjectProgressManager.Instance.ShowSubjectProgress(1);
                
            });
        if (scienceProgressButton != null)
            scienceProgressButton.onClick.AddListener(() => {
                SubjectProgressManager.Instance.ShowSubjectProgress(2);
                
            });
    }

    public void OpenPage(Canvas canvas)
    {
        // Create an array of all canvases
        Canvas[] allCanvases = new Canvas[]
        {
            StartingPage,
            LoginPage,
            SignupPage,
            LoadingScene,
            StudentDashboardPage,
            StudentLeaderboardPage,
            ProfilePage,
            LessonsPage,
            GameScene,
        };

        // Deactivate all canvases
        foreach (Canvas c in allCanvases)
        {
            c.gameObject.SetActive(false);
        }

        // Activate the selected canvas
        canvas.gameObject.SetActive(true);

        // If the profile page is opened, fetch radar chart data and subject progress
        if (canvas == ProfilePage)
        {
            Debug.Log("Fetching items for current user in ProfilePage.");
            ProfileManager profileManager = FindObjectOfType<ProfileManager>();
            profileManager.UpdateProfileUI();
            radarChart.FetchItemsForCurrentUser();

            // Load subject progress data without showing the modal
            if (SubjectProgressManager.Instance != null)
            {
                StartCoroutine(SubjectProgressManager.Instance.LoadSubjectProgress(1)); // English
                StartCoroutine(SubjectProgressManager.Instance.LoadSubjectProgress(2)); // Science
            }
        }
    }

    public void LogintoPage()
    {
        LoadingScene.gameObject.SetActive(false);
        LoginPage.gameObject.SetActive(false);
        StudentDashboardPage.gameObject.SetActive(true);
    }

    public void ToLoadingScene()
    {
        LoginPage.gameObject.SetActive(false);
        LoadingScene.gameObject.SetActive(true);
    }

    public void ToLessonsPage()
    {
        GameScene.gameObject.SetActive(false);
        StudentDashboardPage.gameObject.SetActive(false);
        LessonsPage.gameObject.SetActive(true);
        LessonsLoader.Instance.LoadLessonsForSelectedModuleAndSubject();
    }

    public void BackToStudentDashboard()
    {
        LessonsPage.gameObject.SetActive(false);
        StudentDashboardPage.gameObject.SetActive(true);
        LessonsLoader.Instance.ResetLessons();
        // Refresh modules so unlocked modules are updated
        var moduleLoader = FindObjectOfType<ModuleLoader>();
        if (moduleLoader != null)
        {
            moduleLoader.LoadModulesForSelectedSubject();
        }
    }

    public void ToLoginPage()
    {
        ProfilePage.gameObject.SetActive(false);
        LoginPage.gameObject.SetActive(true);
    }

    public void ToGameScene()
    {
        LessonsPage.gameObject.SetActive(false); // Deactivate the lessons page
        GameScene.gameObject.SetActive(true); // Activate the game scene

        // Get the current subject ID from ButtonTracker
        int subjectId = ButtonTracker.Instance.GetCurrentSubjectId();

        if (subjectId == 1)
        {
            UpdateSubjectUI(1);
            Debug.Log("Updated UI for English subject");
        }
        else if (subjectId == 2)
        {
            UpdateSubjectUI(2);
            Debug.Log("Updated UI for Science subject");
        }
        else
        {
            Debug.LogWarning("Invalid subject ID: " + subjectId);
        }

        Debug.Log("Transitioned from LessonsPage to GameScene.");
    }

    public void ToStudentLeaderboard()
    {
        StudentDashboardPage.gameObject.SetActive(false);
        StudentLeaderboardPage.gameObject.SetActive(true);
        LeaderboardManager.Instance.LoadLeaderboard();
    }

    public void BackToStudentDashboardFromLeaderboard()
    {
        StudentLeaderboardPage.gameObject.SetActive(false);
        StudentDashboardPage.gameObject.SetActive(true);
        LeaderboardManager.Instance.ResetLeaderboard();
    }

    public void ToClassicGamePanel()
    {
        Debug.Log("ToClassicGamePanel called.");
        PanelSwitcher panelSwitcher = FindObjectOfType<PanelSwitcher>();
        if (panelSwitcher != null)
        {
            panelSwitcher.SwitchToClassicGamePanel();
        }
        else
        {
            Debug.LogWarning("PanelSwitcher not found.");
        }
    }

    public void DisplayGameModes(List<string> gameModes)
    {
        Debug.Log("Displaying available game modes...");
        foreach (string mode in gameModes)
        {
            Debug.Log($"Game Mode: {mode}");
            // Add logic to display game modes in the UI
        }
    }

    public void OpenSubjectProgress(int subjectId)
    {
        SubjectProgressManager.Instance.ShowSubjectProgress(subjectId);
    }

    public void CloseSubjectProgress()
    {
        // This is now handled by the SubjectProgressManager
    }

    public void UpdateSubjectUI(int subjectId)
    {
        // Update images (8 images)
        for (int i = 0; i < subjectImages.Length; i++)
        {
            if (subjectImages[i] != null && i < englishSprites.Length && i < scienceSprites.Length)
            {
                subjectImages[i].sprite = subjectId == 1 ? englishSprites[i] : scienceSprites[i];
            }
        }

        // Update texts (7 texts)
        for (int i = 0; i < subjectTexts.Length; i++)
        {
            if (subjectTexts[i] != null)
            {
                subjectTexts[i].color = subjectId == 1 ? englishColor : scienceColor;
            }
        }

        // Update subject name text if it exists
        if (subjectName != null)
        {
            subjectName.text = subjectId == 1 ? "English" : "Science";
            subjectName.color = subjectId == 1 ? englishColor : scienceColor;
        }
    }
}

[System.Serializable]
public class ModuleProgressData
{
    public string module_number;
    public int completed_count;
}

public class SubjectProgressModal : MonoBehaviour
{
    public GameObject moduleProgressPrefab;
    public Transform contentParent;
    public Text subjectNameText;
    public Button closeButton;

    private int currentSubjectId;
    private List<ModuleProgressData> moduleProgressList = new List<ModuleProgressData>();

    private void Start()
    {
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    public void OpenSubjectProgress(int subjectId)
    {
        currentSubjectId = subjectId;
        subjectNameText.text =
            subjectId == 1 ? "English" : (subjectId == 2 ? "Science" : "Unknown");
        gameObject.SetActive(true);
        StartCoroutine(FetchSubjectProgress(subjectId));
    }

    private IEnumerator FetchSubjectProgress(int subjectId)
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

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
                moduleProgressList = JsonConvert.DeserializeObject<List<ModuleProgressData>>(
                    jsonResponse
                );
                DisplayModuleProgress();
            }
            else
            {
                Debug.LogError("Error fetching subject progress: " + www.error);
            }
        }
    }

    private void DisplayModuleProgress()
    {
        foreach (var moduleProgress in moduleProgressList)
        {
            GameObject progressItem = Instantiate(moduleProgressPrefab, contentParent);
            Text[] texts = progressItem.GetComponentsInChildren<Text>();

            texts[0].text = "Module " + moduleProgress.module_number;

            if (moduleProgress.completed_count >= 3)
            {
                texts[1].text = "Complete";
                texts[1].color = Color.green;
            }
            else
            {
                texts[1].text = $"{moduleProgress.completed_count}/3";
                texts[1].color = Color.yellow;
            }
        }
    }
}
