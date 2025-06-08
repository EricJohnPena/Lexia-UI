using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class ProfileManager : MonoBehaviour
{
    public RadarChart.RadarChart radarChart; // Reference to the RadarChart component
    public Text fullNameText;
    public Text sectionText;
    public Image profileImage; // Reference to the profile image UI element

    [SerializeField]
    public Button editProfilePicButton;
    [SerializeField]
    public Button changePasswordButton;
    [SerializeField]
    public GameObject editModal;
    [SerializeField]
    public ChangePasswordManager changePasswordManager;

    private void Start()
    {
        UpdateProfileUI();
        
        // Set up button listeners
        editProfilePicButton.onClick.AddListener(OnEditProfilePictureClicked);
        changePasswordButton.onClick.AddListener(OnChangePasswordClicked);

        // Hide the edit modal initially
        editModal.SetActive(false);
    }

    private void OnEditProfilePictureClicked()
    {
        // Show the modal directly when edit button is clicked
        editModal.SetActive(true);
    }

    private void OnChangePasswordClicked()
    {
        if (ChangePasswordManager.Instance != null)
        {
            ChangePasswordManager.Instance.ShowRegularPasswordChange();
        }
        else
        {
            Debug.LogError("ChangePasswordManager not found. Please add it to your scene.");
        }
    }

    private IEnumerator CheckProfilePictureStatus(string studentId)
    {
        string url = Web.BaseApiUrl + "checkProfilePictureStatus.php?student_id=" + studentId;
        Debug.Log("Checking profile picture status for student: " + studentId);
        
        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                try
                {
                    Debug.Log("Profile picture status response: " + www.downloadHandler.text);
                    var response = JsonConvert.DeserializeObject<ProfilePictureStatusResponse>(www.downloadHandler.text);
                    if (response.success)
                    {
                        Debug.Log($"Profile picture status - ID: '{response.profile_picture_id}', Has Picture: {response.has_profile_picture}");
                        // Only show modal automatically if no profile picture exists
                        if (string.IsNullOrEmpty(response.profile_picture_id))
                        {
                            Debug.Log("Profile picture ID is null or empty, showing modal automatically");
                            editModal.SetActive(true);
                        }
                        else
                        {
                            Debug.Log("Profile picture ID exists, not showing modal automatically");
                        }
                    }
                    else
                    {
                        Debug.LogError("Error checking profile picture status: " + response.error);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error parsing profile picture status response: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("Error checking profile picture status: " + www.error);
            }
        }
    }

    public void ShowEditProfileModal()
    {
        editModal.SetActive(true);
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
