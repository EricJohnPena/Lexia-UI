using UnityEngine;

public enum GamePanel
{
    GameManagerPanel,
    ClassicGamePanel,
    JumbledLettersGamePanel,
    CrosswordGamePanel,
    GameComplete
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
}