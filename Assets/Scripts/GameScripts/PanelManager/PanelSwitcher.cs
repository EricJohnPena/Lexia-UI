using UnityEngine;
public class PanelSwitcher : MonoBehaviour
{
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