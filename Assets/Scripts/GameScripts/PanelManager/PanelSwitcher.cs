using UnityEngine;
using UnityEngine.UI;

public class PanelSwitcher : MonoBehaviour
{
    public string canvasName; // Set this in the Inspector
    public Button yourButton; // Assign the button in the Inspector
    public Button hintButton; // Assign the hint button in the Inspector

    void Start()
    {
        // Ensure the button is assigned
        if (yourButton != null)
        {
            yourButton.onClick.AddListener(OnButtonClick);
        }

        if (hintButton != null)
        {
            hintButton.onClick.AddListener(RevealHint);
        }
    }

    private void OnButtonClick()
    {
        MenuManager.InstanceMenu.ToLessonsPage();
    }

    private void RevealHint()
    {
        Debug.Log("Hint button pressed. Implement hint logic in the respective game manager.");
    }

    public void SwitchToGameManagerPanel()
    {
        PanelManager.Instance.ActivatePanel(GamePanel.GameManagerPanel);
    }

    public void SwitchToClassicGamePanel()
    {
        Debug.Log("SwitchToClassicGamePanel called.");
        PanelManager.Instance.ActivatePanel(GamePanel.ClassicGamePanel);

        // Only load questions if the game is not initialized
        if (ClassicGameManager.instance != null && !ClassicGameManager.instance.isGameInitialized)
        {
            Debug.Log("Loading questions for Classic Game Panel.");
            //ClassicGameManager.instance.LoadQuestionsOnButtonClick();
        }
        else
        {
            Debug.Log("Classic Game Panel already initialized. Skipping question load.");
        }
    }

    public void SwitchToJumbledLettersGamePanel()
    {
        Debug.Log("Switching to Jumbled Letters Game Panel...");
        PanelManager.Instance.ActivatePanel(GamePanel.JumbledLettersGamePanel);
    }

    public void SwitchToCrosswordGamePanel()
    {
        PanelManager.Instance.ActivatePanel(GamePanel.CrosswordGamePanel);
    }

    public void SwitchToReplayGame()
    {
        Debug.Log("Switching to replay the game...");
        PanelManager.Instance.ReplayGame();
    }
}
