using UnityEngine;


public enum GamePanel
{
    GameManagerPanel,
    ClassicGamePanel,
    JumbledLettersGamePanel,
    CrosswordGamePanel
}

public class PanelManager : MonoBehaviour
{
    // Singleton instance
    public static PanelManager Instance { get; private set; }

    // Current active panel
    private GamePanel _currentActivePanel = GamePanel.GameManagerPanel;

    // Reference to panel game objects
    [SerializeField]
    public GameObject panel0;
    [SerializeField]
    public GameObject panel1;
    [SerializeField]
    public GameObject panel2;
    [SerializeField]
    public GameObject panel3;

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

        // Initially activate only the first panel
        ActivatePanel(GamePanel.GameManagerPanel);
    }

    // Method to activate a specific panel
    public void ActivatePanel(GamePanel panelToActivate)
    {
        // Deactivate all panels first
        panel0.SetActive(false);
        panel1.SetActive(false);
        panel2.SetActive(false);
        panel3.SetActive(false);

        // Activate the selected panel
        switch (panelToActivate)
        {
            case GamePanel.GameManagerPanel:
                panel0.SetActive(true);
                break;
            case GamePanel.ClassicGamePanel:
                panel1.SetActive(true);
                break;
            case GamePanel.JumbledLettersGamePanel:
                panel2.SetActive(true);
                break;
            case GamePanel.CrosswordGamePanel:
                panel3.SetActive(true);
                break;
        }

        // Update current active panel
        _currentActivePanel = panelToActivate;
    }

    // Method to get the current active panel
    public GamePanel GetCurrentActivePanel()
    {
        return _currentActivePanel;
    }
}