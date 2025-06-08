using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

public class ChangePasswordManager : MonoBehaviour
{
    public static ChangePasswordManager Instance { get; private set; }

    [Header("First Time Password Change")]
    public Canvas firstTimePasswordCanvas;
    public InputField firstTimeNewPasswordInput;
    public InputField firstTimeConfirmPasswordInput;
    public Button firstTimeChangePasswordButton;
    public Text firstTimePasswordErrorText;

    [Header("Regular Password Change")]
    public Canvas regularPasswordCanvas;
    public InputField oldPasswordInput;
    public InputField newPasswordInput;
    public InputField confirmPasswordInput;
    public Button changePasswordButton;
    public Button backButton;
    public Text passwordErrorText;

    private Action onPasswordChangeSuccess;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Set up button listeners
        firstTimeChangePasswordButton.onClick.AddListener(OnFirstTimeChangePasswordClicked);
        changePasswordButton.onClick.AddListener(OnRegularChangePasswordClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    private void OnBackButtonClicked()
    {
        // Hide the password change canvas
        regularPasswordCanvas.gameObject.SetActive(false);
        
        // Find and show the profile picture change modal
        ProfileManager profileManager = FindObjectOfType<ProfileManager>();
        if (profileManager != null && profileManager.editModal != null)
        {
            profileManager.editModal.SetActive(true);
        }
    }

    public void ShowFirstTimePasswordChange(Action onSuccess)
    {
        onPasswordChangeSuccess = onSuccess;
        
        // Ensure the canvas is properly set up
        if (firstTimePasswordCanvas != null)
        {
            // Hide regular password canvas first
            if (regularPasswordCanvas != null)
            {
                regularPasswordCanvas.gameObject.SetActive(false);
            }

            // Ensure the first time password canvas is active
            firstTimePasswordCanvas.gameObject.SetActive(true);
            
            // Set as last sibling to ensure it's on top
            firstTimePasswordCanvas.transform.SetAsLastSibling();
            
            // Force canvas update
            Canvas.ForceUpdateCanvases();
            
            Debug.Log("First time password change canvas activated");
        }
        else
        {
            Debug.LogError("First time password canvas reference is missing!");
        }
    }

    public void ShowRegularPasswordChange()
    {
        firstTimePasswordCanvas.gameObject.SetActive(false);
        regularPasswordCanvas.gameObject.SetActive(true);
    }

    private void OnFirstTimeChangePasswordClicked()
    {
        string newPassword = firstTimeNewPasswordInput.text;
        string confirmPassword = firstTimeConfirmPasswordInput.text;

        if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
        {
            firstTimePasswordErrorText.text = "Please fill in all fields";
            return;
        }

        if (newPassword != confirmPassword)
        {
            firstTimePasswordErrorText.text = "Passwords do not match";
            return;
        }

        if (newPassword.Length < 8)
        {
            firstTimePasswordErrorText.text = "Password must be at least 8 characters long";
            return;
        }

        StartCoroutine(ChangePassword(newPassword, null));
    }

    private void OnRegularChangePasswordClicked()
    {
        string oldPassword = oldPasswordInput.text;
        string newPassword = newPasswordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
        {
            passwordErrorText.text = "Please fill in all fields";
            return;
        }

        if (newPassword != confirmPassword)
        {
            passwordErrorText.text = "New passwords do not match";
            return;
        }

        if (newPassword.Length < 8)
        {
            passwordErrorText.text = "Password must be at least 8 characters long";
            return;
        }

        StartCoroutine(ChangePassword(newPassword, oldPassword));
    }

    private IEnumerator ChangePassword(string newPassword, string oldPassword)
    {
        string studentId = PlayerPrefs.GetString("User ID");
        WWWForm form = new WWWForm();
        form.AddField("student_id", studentId);
        form.AddField("new_password", newPassword);
        if (oldPassword != null)
        {
            form.AddField("old_password", oldPassword);
        }

        using (UnityWebRequest www = UnityWebRequest.Post(Web.BaseApiUrl + "changePassword.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    Debug.Log("Change password response: " + www.downloadHandler.text);
                    var response = JsonConvert.DeserializeObject<PasswordChangeResponse>(www.downloadHandler.text);
                    if (response.success)
                    {
                        // Clear input fields
                        ClearInputFields();
                        
                        // Hide the appropriate canvas
                        if (oldPassword == null)
                        {
                            firstTimePasswordCanvas.gameObject.SetActive(false);
                            onPasswordChangeSuccess?.Invoke();
                        }
                        else
                        {
                            regularPasswordCanvas.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        if (oldPassword == null)
                        {
                            firstTimePasswordErrorText.text = response.error;
                        }
                        else
                        {
                            passwordErrorText.text = response.error;
                        }
                    }
                }
                catch (Exception e)
                {
                    string errorMessage = "Error processing server response";
                    if (oldPassword == null)
                    {
                        firstTimePasswordErrorText.text = errorMessage;
                    }
                    else
                    {
                        passwordErrorText.text = errorMessage;
                    }
                    Debug.LogError("Error parsing response: " + e.Message);
                }
            }
            else
            {
                string errorMessage = "Network error. Please try again.";
                if (oldPassword == null)
                {
                    firstTimePasswordErrorText.text = errorMessage;
                }
                else
                {
                    passwordErrorText.text = errorMessage;
                }
                Debug.LogError("Error: " + www.error);
            }
        }
    }

    private void ClearInputFields()
    {
        firstTimeNewPasswordInput.text = "";
        firstTimeConfirmPasswordInput.text = "";
        oldPasswordInput.text = "";
        newPasswordInput.text = "";
        confirmPasswordInput.text = "";
    }
}

[Serializable]
public class PasswordChangeResponse
{
    public bool success;
    public string error;
}
