using UnityEngine;

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

        // Reset game state
        ResetGameState();

        // Switch to the current game mode panel
        ActivatePanel(_currentActivePanel);
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
}
