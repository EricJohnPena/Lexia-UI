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
        PanelManager.Instance.ActivatePanel(GamePanel.ClassicGamePanel);

        // Reload Classic Game UI and questions
        ClassicGameManager.instance.LoadQuestionsOnButtonClick();
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