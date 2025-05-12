using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    public RadarChart.RadarChart radarChart; // Reference to the RadarChart component

    public Text fullNameText;
    public Text sectionText;

    private void Start()
    {
        UpdateProfileUI();
    }

    public void UpdateProfileUI()
    {
        // Check if radarChart is assigned
        if (radarChart == null)
        {
            Debug.LogError("RadarChart reference is not assigned in ProfileManager.");
            return; // Exit the method if radarChart is null
        }

        // Retrieve user info from PlayerPrefs
        string fullName = PlayerPrefs.GetString("Fullname", "Guest User");
        string section = PlayerPrefs.GetString("SectionName", "Not Assigned");
        string userId = PlayerPrefs.GetString("User ID", "");

        // Update the UI elements with user information
        fullNameText.text = fullName;
        sectionText.text = "Grade 6 - " + section;

    }
}
