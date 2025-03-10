using UnityEngine;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    public Text fullNameText;
    public Text sectionText;

    private void Start()
    {
        UpdateProfileUI();
    }

    public void UpdateProfileUI()
{
    // Retrieve user info from PlayerPrefs
    
    string fullName = PlayerPrefs.GetString("Fullname", "Guest User");
    string section = PlayerPrefs.GetString("SectionName", "Not Assigned");
    //Debug.Log(fullName);
    //Debug.Log(section);

    // Update the UI elements with user information
    fullNameText.text = fullName;
    sectionText.text = "Grade 6 - " + section; // Adjust this if you have a specific format for sections
}
}