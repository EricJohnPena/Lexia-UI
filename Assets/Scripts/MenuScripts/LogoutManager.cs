using System;
using UnityEngine;
using UnityEngine.UI;

public class LogoutManager : MonoBehaviour
{
    // Reference to the MenuManager to navigate between pages
    public MenuManager menuManager;

    [SerializeField]
    public Button logoutButton;

    [SerializeField]
    public Button YesBtn;

    [SerializeField]
    public Button NoBtn;

    [SerializeField]
    public GameObject LogoutPanel;

    private void Update()
    {
        logoutButton.onClick.AddListener(() =>
        {
            // Show the LogoutPanel when the logout button is clicked
            LogoutPanel.SetActive(true);
        });
        // Check if the LogoutPanel is active and handle Yes/No button clicks
        if (LogoutPanel.activeSelf)
        {
            if (YesBtn != null)
            {
                YesBtn.onClick.AddListener(ConfirmLogout);
            }

            if (NoBtn != null)
            {
                NoBtn.onClick.AddListener(CancelLogout);
            }
        }
    }

    public void ConfirmLogout()
    {
        // Hide the LogoutPanel
        LogoutPanel.SetActive(false);

        // Call the Logout method to perform logout actions
        Logout();
    }

    public void CancelLogout()
    {
        // Hide the LogoutPanel
        LogoutPanel.SetActive(false);
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
