using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    public RadarChart.RadarChart radarChart; // Reference to the RadarChart component
    public Text fullNameText;
    public Text sectionText;
    public Image profileImage; // Reference to the profile image UI element

    [SerializeField]
    public Button editProfilePicButton;
    [SerializeField]
    public GameObject editModal;

    private void Start()
    {
        UpdateProfileUI();
    }
    private void Update()
    {
       editProfilePicButton.onClick.AddListener(() =>
        {
            // Show the LogoutPanel when the logout button is clicked
            editModal.SetActive(true);
        });
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

        // Load profile picture if the image component is assigned
        if (profileImage != null && ProfilePictureManager.Instance != null)
        {
            ProfilePictureManager.Instance.LoadProfilePicture(userId, profileImage);
        }
    }
}
