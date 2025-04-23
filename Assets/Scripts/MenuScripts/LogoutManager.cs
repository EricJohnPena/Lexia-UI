using System;
using UnityEngine;
using UnityEngine.UI;

public class LogoutManager : MonoBehaviour
{
    // Reference to the MenuManager to navigate between pages
    public MenuManager menuManager;

    // Reference to a button to trigger logout (optional)
    public Button logoutButton;

    private void Start()
    {
        // If you have a logout button, add a listener to it
        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(Logout);
        }
    }

    public void Logout()
    {
        // Clear all PlayerPrefs data
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Clear radar items from PlayerPrefs specifically
        PlayerPrefs.DeleteKey("RadarItems");
        PlayerPrefs.Save();

        // Clear radar items from the RadarChart component if it exists
        RadarChart.RadarChart radarChart = FindObjectOfType<RadarChart.RadarChart>();
        if (radarChart != null)
        {
            radarChart.ClearRadarItems();
        }

        // Redirect to the login page
        if (menuManager != null)
        {
            menuManager.ToLoginPage();
        }
        else
        {
            Debug.LogError("menuManager is null");
        }
    }
}
