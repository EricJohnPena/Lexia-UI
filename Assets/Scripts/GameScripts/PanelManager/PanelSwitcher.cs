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
        // You can also add any other logic you want to execute when the button is clicked
    }
    public void SwitchToGameManagerPanel()
    {
        PanelManager.Instance.ActivatePanel(GamePanel.GameManagerPanel);
    }
    public void SwitchToClassicGamePanel()
    {
        PanelManager.Instance.ActivatePanel(GamePanel.ClassicGamePanel);
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