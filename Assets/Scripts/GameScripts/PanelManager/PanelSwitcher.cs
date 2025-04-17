using UnityEngine;
using UnityEngine.UI;

public class PanelSwitcher : MonoBehaviour
{
    public string canvasName; // Set this in the Inspector
    public Button yourButton; // Assign the button in the Inspector

    void Start()
    {
        // Ensure the button is assigned
        if (yourButton != null)
        {
            yourButton.onClick.AddListener(OnButtonClick);
        }
    }

    private void OnButtonClick()
    {
        MenuManager.InstanceMenu.ToLessonsPage();
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
        PanelManager.Instance.ActivatePanel(GamePanel.JumbledLettersGamePanel);
    }

    public void SwitchToCrosswordGamePanel()
    {
        PanelManager.Instance.ActivatePanel(GamePanel.CrosswordGamePanel);
    }
}