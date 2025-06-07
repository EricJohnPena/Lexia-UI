using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Newtonsoft.Json;

public class ChangePasswordManager : MonoBehaviour
{
    public InputField oldPasswordInput;
    public InputField newPasswordInput;
    public InputField confirmPasswordInput;
    public Button submitButton;
    public Button closeButton;
    public GameObject changePasswordCanvas;
    public Text errorText;

    private void Start()
    {
        submitButton.onClick.AddListener(OnSubmitButtonClicked);
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        errorText.gameObject.SetActive(false);
    }

    private void OnSubmitButtonClicked()
    {
        string oldPassword = oldPasswordInput.text;
        string newPassword = newPasswordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        // Validate inputs
        if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
        {
            ShowError("All fields are required");
            return;
        }

        if (newPassword != confirmPassword)
        {
            ShowError("New passwords do not match");
            return;
        }

        if (newPassword.Length < 6)
        {
            ShowError("New password must be at least 6 characters long");
            return;
        }

        // Start the password change process
        StartCoroutine(ChangePasswordCoroutine(oldPassword, newPassword));
    }

    private IEnumerator ChangePasswordCoroutine(string oldPassword, string newPassword)
    {
        string studentId = PlayerPrefs.GetString("User ID", "");
        if (string.IsNullOrEmpty(studentId))
        {
            ShowError("User not logged in");
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("student_id", studentId);
        form.AddField("old_password", oldPassword);
        form.AddField("new_password", newPassword);

        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Post(Web.BaseApiUrl + "changePassword.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                try
                {
                    var response = JsonConvert.DeserializeObject<ChangePasswordResponse>(www.downloadHandler.text);
                    if (response.success)
                    {
                        // Clear inputs and show success message
                        ClearInputs();
                        ShowError("Password changed successfully", true);
                        OnCloseButtonClicked();
                    }
                    else
                    {
                        ShowError(response.error);
                    }
                }
                catch (System.Exception e)
                {
                    ShowError("Error processing response: " + e.Message);
                }
            }
            else
            {
                ShowError("Network error: " + www.error);
            }
        }

        if (errorText.text == "Password changed successfully")
        {
            yield return new WaitForSeconds(2f);
            OnCloseButtonClicked();
        }
    }

    private void ShowError(string message, bool isSuccess = false)
    {
        errorText.text = message;
        errorText.color = isSuccess ? Color.green : Color.red;
        errorText.gameObject.SetActive(true);
    }

    private void ClearInputs()
    {
        oldPasswordInput.text = "";
        newPasswordInput.text = "";
        confirmPasswordInput.text = "";
        errorText.gameObject.SetActive(false);
    }

    private void OnCloseButtonClicked()
    {
        ClearInputs();
        changePasswordCanvas.SetActive(false);
    }

    public void ShowChangePasswordCanvas()
    {
        ClearInputs();
        changePasswordCanvas.SetActive(true);
    }
}

[System.Serializable]
public class ChangePasswordResponse
{
    public bool success;
    public string error;
} 