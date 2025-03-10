using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class Web : MonoBehaviour
{
    public List<SectionResponse> sectionResponse = new List<SectionResponse>();
    void Start()
    {

        StartCoroutine(GetDate("http://192.168.1.154/db_unity/get_date.php"));
    }


    IEnumerator GetDate(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {

            yield return webRequest.SendWebRequest();

            

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError( ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError( ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log( ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }
        }
    }

    public IEnumerator GetSectionId(string student_id)
    {
        WWWForm form = new WWWForm();
        form.AddField("student_id", student_id);



        using UnityWebRequest www = UnityWebRequest.Post("http://192.168.1.154/db_unity/getSectionId.php", form);
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


    public IEnumerator Login(string username, string password)
{
    WWWForm form = new WWWForm();
    form.AddField("loginUser", username);
    form.AddField("loginPass", password);

    using (UnityWebRequest www = UnityWebRequest.Post("http://192.168.1.154/db_unity/login.php", form))
    {
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            string errorMessage = $"Login Error: {www.error}";
            Debug.LogError($"Login Error: {www.error}");
            // Display the error message in a Text component
        GameObject errorTextObject = GameObject.Find("ErrorText");
        Text errorText = errorTextObject.GetComponent<Text>();
        errorText.text = errorMessage;
            yield break;
        }

        string response = www.downloadHandler.text.Trim();
        Debug.Log("Raw Response: " + response);

        if (response.StartsWith("{") || response.StartsWith("["))
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
                yield break; // Exit if parsing fails
            }

            // Use the parsed data
            if (loginData != null)
            {
                // Store user information in PlayerPrefs
                PlayerPrefs.SetString("User ID", loginData.student_id);
                PlayerPrefs.SetString("Username", loginData.first_name);
                PlayerPrefs.SetString("Fullname", loginData.first_name + " " + loginData.last_name);
                PlayerPrefs.SetString("Section ID", loginData.fk_section_id); 
                PlayerPrefs.Save();
                string User = PlayerPrefs.GetString("Username", "Guest User");
                
                UserInfo.Instance.SetId(loginData.student_id);
                StartCoroutine(GetSectionName(loginData.fk_section_id));

                // Proceed to the next menu/page
                MenuManager.InstanceMenu.LogintoPage();
                MenuManager.InstanceMenu.usernameText.text = "Hi " + User + ",";

                // Call the OnLoginSuccess method
                Login loginComponent = FindObjectOfType<Login>();
                loginComponent?.OnLoginSuccess();
            }
        }
        else
        {
            Debug.LogError("Invalid JSON Response: " + response);
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

    //     using (UnityWebRequest www = UnityWebRequest.Post("http://192.168.1.154/db_unity/register.php", form))
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
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    break;
            }
        }

    }

    public IEnumerator GetSectionName(string sectionId)
{
    WWWForm form = new WWWForm();
    form.AddField("section_id", sectionId);

    using (UnityWebRequest www = UnityWebRequest.Post("http://192.168.1.154/db_unity/getSection.php", form))
    {
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string jsonResponse = www.downloadHandler.text;
            Debug.Log("Section Name Response: " + jsonResponse);

            try
            {
                // Parse the JSON response
                sectionResponse = JsonConvert.DeserializeObject<List<SectionResponse>>(jsonResponse);

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







    private void SetDefaultHeaders(UnityWebRequest www)
    {
        //www.SetRequestHeader("Accept", "application/json");
        www.SetRequestHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        www.SetRequestHeader("Accept-Encoding", "gzip, deflate");
        www.SetRequestHeader("Accept-Language", "en-US,en;q=0.5");
        www.SetRequestHeader("Cache-Control", "max-age=0");
        www.SetRequestHeader("Cookie", "__test=a179c2531f72ee6d3d402e20c138e870");
        www.SetRequestHeader("Upgrade-Insecure-Requests", "1");
        www.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:134.0) Gecko/20100101 Firefox/134.0");
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

