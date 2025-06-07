using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Web : MonoBehaviour
{
    // Base URL for all API endpoints
    public static string BaseApiUrl = "https://lexiaweb.io/db_unity/";

    //public static string BaseApiUrl = "http://localhost/db_unity/";


    public List<SectionResponse> sectionResponse = new List<SectionResponse>();

    public IEnumerator GetSectionId(string student_id)
    {
        WWWForm form = new WWWForm();
        form.AddField("student_id", student_id);

        using UnityWebRequest www = UnityWebRequest.Post(BaseApiUrl + "getSectionId.php", form);
        //SetDefaultHeaders(www);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.Log("Section: " + www.downloadHandler.text);
            string jsonArray = www.downloadHandler.text;
        }
    }

    public IEnumerator CheckProfilePictureStatus(string studentId)
    {
        string url = BaseApiUrl + "checkProfilePictureStatus.php?student_id=" + studentId;
        Debug.Log("Checking profile picture status for student: " + studentId);
        
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    Debug.Log("Profile picture status response: " + www.downloadHandler.text);
                    var response = JsonConvert.DeserializeObject<ProfilePictureStatusResponse>(www.downloadHandler.text);
                    if (response.success)
                    {
                        Debug.Log($"Profile picture status - ID: '{response.profile_picture_id}', Has Picture: {response.has_profile_picture}");
                        // Just log the status, don't show modal automatically
                        if (string.IsNullOrEmpty(response.profile_picture_id))
                        {
                            Debug.Log("Profile picture ID is null or empty");
                        }
                        else
                        {
                            Debug.Log("Profile picture ID exists");
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

    public IEnumerator Login(string username, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("loginUser", username);
        form.AddField("loginPass", password);

        using (UnityWebRequest www = UnityWebRequest.Post(BaseApiUrl + "login.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
                // Show network error in UI
                Login loginComponent = FindObjectOfType<Login>();
                if (loginComponent != null)
                {
                    loginComponent.ShowError("Network error: " + www.error);
                }
            }
            else
            {
                string response = www.downloadHandler.text;
                // Debug.Log("Login response: " + response);

                if (response.Contains("error"))
                {
                    // Handle error response
                    LoginErrorResponse errorResponse = JsonConvert.DeserializeObject<LoginErrorResponse>(response);
                    Debug.LogError("Login error: " + errorResponse.error);
                    // Find the Login component and show the error
                    Login loginComponent = FindObjectOfType<Login>();
                    if (loginComponent != null)
                    {
                        loginComponent.ShowError(errorResponse.error);
                    }
                }
                else
                {
                    // Parse the JSON response safely
                    LoginResponse loginData = null;
                    try
                    {
                        loginData = JsonConvert.DeserializeObject<LoginResponse>(response);
                    }
                    catch (JsonException ex)
                    {
                        Debug.LogError("JSON Parsing Error: " + ex.Message);
                        // Show JSON parsing error in UI
                        Login loginComponent = FindObjectOfType<Login>();
                        if (loginComponent != null)
                        {
                            loginComponent.ShowError("Error processing server response. Please try again.");
                        }
                        yield break; // Exit if parsing fails
                    }

                    // Use the parsed data
                    if (loginData != null)
                    {
                        // Store user information in PlayerPrefs
                        PlayerPrefs.SetString("User ID", loginData.student_id);
                        PlayerPrefs.SetString("Username", loginData.first_name);
                        PlayerPrefs.SetString(
                            "Fullname",
                            loginData.first_name + " " + loginData.last_name
                        );
                        PlayerPrefs.SetString("Section ID", loginData.fk_section_id);
                        PlayerPrefs.Save();
                        string User = PlayerPrefs.GetString("Username", "Guest User");

                        UserInfo.Instance.SetId(loginData.student_id);
                        StartCoroutine(GetSectionName(loginData.fk_section_id));

                        // Check profile picture status
                        yield return StartCoroutine(CheckProfilePictureStatus(loginData.student_id));

                        // Proceed to the next menu/page
                        MenuManager.InstanceMenu.LogintoPage();
                        MenuManager.InstanceMenu.usernameText.text = "Hi " + User + ",";

                        // Call the OnLoginSuccess method
                        Login loginComponent = FindObjectOfType<Login>();
                        loginComponent?.OnLoginSuccess();
                    }
                }
            }
        }
    }

    // public IEnumerator Register(string username, string password, string firstName, string lastName, string email)
    // {
    //     WWWForm form = new WWWForm();
    //     form.AddField("loginUser", username);
    //     form.AddField("loginPass", password);
    //     form.AddField("first_name", firstName);
    //     form.AddField("last_name", lastName);
    //     form.AddField("email", email);

    //     using (UnityWebRequest www = UnityWebRequest.Post(BaseApiUrl + "register.php", form))
    //     {
    //         //SetDefaultHeaders(www);
    //         yield return www.SendWebRequest();

    //         if (www.result != UnityWebRequest.Result.Success)
    //         {
    //             Debug.LogError(www.error);
    //         }
    //         else
    //         {
    //             Debug.Log("Registration Response: " + www.downloadHandler.text);
    //         }
    //     }
    // }

    IEnumerator GetModules(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            //SetDefaultHeaders(webRequest);
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    // Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }
        }
    }

    public IEnumerator GetSectionName(string sectionId)
    {
        WWWForm form = new WWWForm();
        form.AddField("section_id", sectionId);

        using (UnityWebRequest www = UnityWebRequest.Post(BaseApiUrl + "getSection.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;
                // Debug.Log("Section Name Response: " + jsonResponse);

                try
                {
                    // Parse the JSON response
                    sectionResponse = JsonConvert.DeserializeObject<List<SectionResponse>>(
                        jsonResponse
                    );

                    foreach (var section in sectionResponse)
                    {
                        PlayerPrefs.SetString("SectionName", section.section_name);
                        PlayerPrefs.Save();
                        Manager.instance.UserInfo.SetSectionName(section.section_name);
                    }

                    // Update the profile UI after setting the section name
                    ProfileManager profileManager = FindObjectOfType<ProfileManager>();
                    if (profileManager != null)
                    {
                        profileManager.UpdateProfileUI();
                    }
                }
                catch (JsonException ex)
                {
                    Debug.LogError("Failed to parse Section Name JSON: " + ex.Message);
                }
            }
        }
    }

    public static (int subjectId, int moduleId, int lessonId) GetCurrentTrackingIds()
    {
        // Fetch the current tracking IDs from PlayerPrefs or set defaults
        int subjectId = PlayerPrefs.GetInt("CurrentSubjectId", 1); // Default to 1 if not set
        int moduleId = PlayerPrefs.GetInt("CurrentModuleId", 1); // Default to 1 if not set
        int lessonId = PlayerPrefs.GetInt("CurrentLessonId", 1); // Default to 1 if not set

        // Debug.Log(
        //     $"GetCurrentTrackingIds called. Returning Subject: {subjectId}, Module: {moduleId}, Lesson: {lessonId}"
        // ); // Debug log
        return (subjectId, moduleId, lessonId);
    }

    public static void SetCurrentTrackingIds(int subjectId, int moduleId, int lessonId)
    {
        string subjectName = subjectId == 1 ? "English" : (subjectId == 2 ? "Science" : "Unknown");
        // Debug.Log(
        //     $"Setting Tracking IDs - Subject: {subjectName} ({subjectId}), Module: {moduleId}, Lesson: {lessonId}"
        // );

        PlayerPrefs.SetInt("CurrentSubjectId", subjectId);
        PlayerPrefs.SetInt("CurrentModuleId", moduleId);
        PlayerPrefs.SetInt("CurrentLessonId", lessonId);
        PlayerPrefs.Save();

        // Debug.Log(
        //     $"SetCurrentTrackingIds called. Subject: {subjectId}, Module: {moduleId}, Lesson: {lessonId}"
        // );
    }
}

// JSON response models
[System.Serializable]
public class LoginResponse
{
    public string student_id;
    public string fk_section_id;
    public string first_name;
    public string last_name;
}

[System.Serializable]
public class SectionResponse
{
    public string section_name;
}

// Add new response class for profile picture status
[System.Serializable]
public class ProfilePictureStatusResponse
{
    public bool success;
    public bool has_profile_picture;
    public string profile_picture_id;
    public string error;
}

[System.Serializable]
public class LoginErrorResponse
{
    public string error;
}