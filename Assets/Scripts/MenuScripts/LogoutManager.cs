using UnityEngine;
using UnityEngine.UI;
using System;

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
    Debug.Log("Logout button clicked");
    // Clear PlayerPrefs
    PlayerPrefs.DeleteAll();
    PlayerPrefs.Save();

    // Clear user data from other systems
    UserInfo userInfo = UserInfo.Instance;
    if (userInfo != null)
    {
        userInfo.ClearData();
    }
    else
    {
        Debug.LogError("UserInfo.Instance is null");
    }

    // Clear radar items
    RadarChart.RadarChart radarChart = FindObjectOfType(typeof(RadarChart.RadarChart)) as RadarChart.RadarChart;
    if (radarChart != null)
    {
        radarChart.ClearRadarItems();
    }
    else
    {
        Debug.LogError("RadarChart instance not found");
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