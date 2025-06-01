using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ProfilePictureSelector : MonoBehaviour
{
    public GameObject profilePictureButtonPrefab;
    public Transform buttonContainer;
    public Button selectButton;
    public Button closeButton;
    public GameObject modalPanel;
    
    private List<ProfilePictureData> availablePictures = new List<ProfilePictureData>();
    private ProfilePictureButton selectedButton;
    private ProfileManager profileManager;

    private void Start()
    {
        profileManager = FindObjectOfType<ProfileManager>();
        selectButton.onClick.AddListener(OnSelectButtonClicked);
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        LoadAvailableProfilePictures();
    }

    private void LoadAvailableProfilePictures()
    {
        StartCoroutine(LoadProfilePicturesCoroutine());
    }

    private IEnumerator LoadProfilePicturesCoroutine()
    {
        string url = Web.BaseApiUrl + "getAvailableProfilePictures.php";
        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonConvert.DeserializeObject<ProfilePicturesResponse>(www.downloadHandler.text);
                    if (response.success)
                    {
                        availablePictures = response.pictures;
                        PopulateProfilePictures();
                    }
                    else
                    {
                        Debug.LogError("Error loading profile pictures: " + response.error);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error parsing profile pictures response: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("Error fetching profile pictures: " + www.error);
            }
        }
    }

    private void PopulateProfilePictures()
    {
        // Clear existing buttons
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // Create buttons for each profile picture
        foreach (var picture in availablePictures)
        {
            GameObject buttonObj = Instantiate(profilePictureButtonPrefab, buttonContainer);
            ProfilePictureButton button = buttonObj.GetComponent<ProfilePictureButton>();
            
            if (button != null)
            {
                button.Initialize(picture, OnProfilePictureSelected);
            }
        }
    }

    private void OnProfilePictureSelected(ProfilePictureButton button)
    {
        // Deselect previous button if any
        if (selectedButton != null)
        {
            selectedButton.SetSelected(false);
        }

        // Select new button
        selectedButton = button;
        selectedButton.SetSelected(true);
        
        // Enable select button only when a picture is selected
        selectButton.interactable = true;
    }

    private void OnSelectButtonClicked()
    {
        if (selectedButton != null)
        {
            string studentId = PlayerPrefs.GetString("User ID", "");
            // Debug.Log($"Updating profile picture for student {studentId} to picture {selectedButton.PictureData.picture_id}");
            StartCoroutine(UpdateProfilePictureCoroutine(studentId, selectedButton.PictureData.picture_id));
        }
    }

    private IEnumerator UpdateProfilePictureCoroutine(string studentId, int pictureId)
    {
        WWWForm form = new WWWForm();
        form.AddField("student_id", studentId);
        form.AddField("picture_id", pictureId);

        string url = Web.BaseApiUrl + "updateProfilePicture.php";
        Debug.Log($"Sending update request to {url} with student_id={studentId} and picture_id={pictureId}");

        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                // Debug.Log($"Update response: {www.downloadHandler.text}");
                try
                {
                    var response = JsonConvert.DeserializeObject<UpdateProfilePictureResponse>(www.downloadHandler.text);
                    if (response.success)
                    {
                        Debug.Log("Profile picture updated successfully");
                        // Clear the profile picture cache to force a reload
                        if (ProfilePictureManager.Instance != null)
                        {
                            ProfilePictureManager.Instance.ClearCache(studentId);
                        }
                        // Update the profile picture in the UI
                        if (profileManager != null)
                        {
                            profileManager.UpdateProfileUI();
                        }
                        OnCloseButtonClicked();
                    }
                    else
                    {
                        Debug.LogError("Error updating profile picture: " + response.error);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error parsing update response: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("Error updating profile picture: " + www.error);
            }
        }
    }

    private void OnCloseButtonClicked()
    {
        modalPanel.SetActive(false);
        if (selectedButton != null)
        {
            selectedButton.SetSelected(false);
            selectedButton = null;
        }
        selectButton.interactable = false;
    }
}

[System.Serializable]
public class ProfilePictureData
{
    public int picture_id;
    public string image_path;
}

[System.Serializable]
public class ProfilePicturesResponse
{
    public bool success;
    public List<ProfilePictureData> pictures;
    public string error;
}

[System.Serializable]
public class UpdateProfilePictureResponse
{
    public bool success;
    public string error;
} 