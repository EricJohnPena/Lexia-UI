using UnityEngine;
using UnityEngine.UI;

public enum GamePanel
{
    GameManagerPanel,
    ClassicGamePanel,
    JumbledLettersGamePanel,
    CrosswordGamePanel,
    GameComplete,
}

public class PanelManager : MonoBehaviour
{
    // Singleton instance
    public static PanelManager Instance { get; private set; }

    // Current active panel
    private GamePanel _currentActivePanel = GamePanel.GameManagerPanel;

    // Array of panel game objects
    [SerializeField]
    private GameObject[] panels; // Ensure these are assigned in the Unity Editor in order of GamePanel enum

    [SerializeField]
    private GameObject gameOver;

    // Game mode button references
    [SerializeField]
    private Button classicGameButton;
    [SerializeField]
    private Button jumbledLettersButton;
    [SerializeField]
    private Button crosswordButton;
    [SerializeField]
    private Button backButton;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Initially activate the Game Manager Panel
        ActivatePanel(GamePanel.GameManagerPanel);
        UpdateButtonColors();
    }

    private void OnEnable()
    {
        UpdateButtonColors();
    }

    // Method to activate a specific panel
    public void ActivatePanel(GamePanel panelToActivate)
    {
        if (_currentActivePanel == panelToActivate)
        {
            Debug.Log($"Panel {panelToActivate} is already active. Skipping activation.");
            return; // Prevent reactivating the same panel
        }

        DeactivateAllPanels();

        int panelIndex = (int)panelToActivate;
        if (panelIndex >= 0 && panelIndex < panels.Length && panels[panelIndex] != null)
        {
            panels[panelIndex].SetActive(true);
            Debug.Log($"Activated panel: {panelToActivate}");
            
            // Update button colors when switching to GameManagerPanel
            if (panelToActivate == GamePanel.GameManagerPanel)
            {
                UpdateButtonColors();
            }
        }
        else
        {
            Debug.LogWarning($"Panel index {panelIndex} is out of range or null.");
        }

        _currentActivePanel = panelToActivate;
    }

    // Method to deactivate all panels
    private void DeactivateAllPanels()
    {
        foreach (var panel in panels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
            else
            {
                Debug.LogWarning("One of the panels is null!");
            }
        }
    }

    // Method to initialize a specific panel
    private void InitializePanel(GamePanel panelToInitialize)
    {
        switch (panelToInitialize)
        {
            case GamePanel.GameManagerPanel:
                Debug.Log("Game Manager Panel initialized");
                UpdateButtonColors();
                break;

            case GamePanel.ClassicGamePanel:
                Debug.Log("Classic Game Panel initialized");
                break;

            case GamePanel.JumbledLettersGamePanel:
                break;

            case GamePanel.CrosswordGamePanel:
                Debug.Log("Crossword Game Panel initialized");
                break;
            case GamePanel.GameComplete:
                Debug.Log("Crossword Game Panel initialized");
                break;

            default:
                Debug.LogError($"No initialization logic for panel: {panelToInitialize}");
                break;
        }
    }

    // Method to get the current active panel
    public GamePanel GetCurrentActivePanel()
    {
        return _currentActivePanel;
    }

    public void ReplayGame()
    {
        Debug.Log("Replay button clicked. Resetting game state...");

        // Reset game state based on the current active panel
        switch (_currentActivePanel)
        {
            case GamePanel.ClassicGamePanel:
                ClassicGameManager.instance?.ReplayGame();
                break;

            case GamePanel.JumbledLettersGamePanel:
                JumbledLettersManager.instance?.ReplayGame();
                break;

            case GamePanel.CrosswordGamePanel:
                CrosswordGridManager crosswordGridManager = FindObjectOfType<CrosswordGridManager>();
                crosswordGridManager?.ReplayGame();
                break;

            default:
                Debug.LogWarning("No specific replay logic for the current game panel.");
                break;
        }
    }

    private void ResetGameState()
    {
        Debug.Log("Game state has been reset.");

        // Reset scores, progress, and other game-related data
        ResetScores();
        ResetProgress();

        // Reload questions based on the current active panel
        switch (_currentActivePanel)
        {
            case GamePanel.ClassicGamePanel:
                Debug.Log("Reloading questions for Classic Game Panel...");
                ClassicGameManager.instance.LoadQuestionsOnButtonClick();
                gameOver.SetActive(false);
                break;

            case GamePanel.JumbledLettersGamePanel:
                Debug.Log("Reloading questions for Jumbled Letters Game Panel...");
                JumbledLettersManager.instance.LoadQuestionsOnButtonClick();
                gameOver.SetActive(false);
                break;

            case GamePanel.CrosswordGamePanel:
                Debug.Log("Reloading questions for Crossword Game Panel...");
                gameOver.SetActive(false);
                CrosswordGridManager crosswordGridManager =
                    FindObjectOfType<CrosswordGridManager>();
                if (crosswordGridManager != null)
                {
                    crosswordGridManager.LoadQuestionsOnButtonClick();
                }
                else
                {
                    Debug.LogError("CrosswordGridManager instance not found!");
                }
                break;

            default:
                Debug.LogWarning("No specific reload logic for the current game panel.");
                break;
        }
    }

    private void ResetScores()
    {
        // Logic to reset scores
        Debug.Log("Scores have been reset.");
    }

    private void ResetProgress()
    {
        // Logic to reset progress
        Debug.Log("Progress has been reset.");
    }

    // Game mode button color management
    public void UpdateButtonColors()
    {
        Color outlineColor = Color.white;
        if (LessonsLoader.subjectId == 1) // English
        {
            outlineColor = new Color32(0, 102, 204, 255); // Blue
        }
        else if (LessonsLoader.subjectId == 2) // Science
        {
            outlineColor = new Color32(0, 153, 0, 255); // Green
        }

        UpdateButtonOutline(classicGameButton, outlineColor);
        UpdateButtonOutline(jumbledLettersButton, outlineColor);
        UpdateButtonOutline(crosswordButton, outlineColor);
        UpdateButtonOutline(backButton, outlineColor);
    }

    private void UpdateButtonOutline(Button button, Color color)
    {
        if (button != null)
        {
            var outline = button.GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = color;
            }
        }
    }
}
