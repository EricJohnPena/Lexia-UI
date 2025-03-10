
using UnityEngine;

public class UserInfo : MonoBehaviour
{
    public static UserInfo Instance;
    private void Awake()
    {
        Instance = this;
    }


    public string userId { get; private set; } //set preivately, but can get publicly
    public string username;
    public string password;
    public string firstname;
    public string lastname;
    public string sectionId;
    public string sectionName;

    public void SetCredentials(string student_username, string student_password, string first_name, string last_name)
    {
        username = student_username;
        password = student_password;
        firstname = first_name;
        lastname = last_name;
    }


    public void SetSectionName(string sectionName)
    {
        this.sectionName = sectionName;
        PlayerPrefs.SetString("SectionName", sectionName);
        PlayerPrefs.Save();
        Debug.Log("SectionName: " + sectionName);
    }

    public void SetId(string student_id)
    {
        userId = student_id;
        Debug.Log("Set UserID: " + userId);
    }
    public void SetSectionId(string sectionId)
    {
        this.sectionId = sectionId;
        Debug.Log("SectionID: " + sectionId);
    }

    public void ClearData()
    {
        // Clear user ID
        userId = "";
    
        // Clear username
        username = "";
    
        // Clear password
        password = "";
    
        // Clear first name
        firstname = "";
    
        // Clear last name
        lastname = "";
    
        // Clear section ID
        sectionId = "";
    
        // Clear section name
        sectionName = "";
    }

    public void SavePlayerInfo()
    {
        // Save player information to PlayerPrefs
        PlayerPrefs.SetString("User ID", userId);
        PlayerPrefs.SetString("Username", username);
        PlayerPrefs.SetString("Fullname", firstname + " " + lastname);
        PlayerPrefs.SetString("Section ID", sectionId);
        PlayerPrefs.SetString("Section Name", sectionName);
        PlayerPrefs.Save();
        Debug.Log("Player information saved.");
    }

    public void DeletePlayerInfo()
    {
        // Delete player information from PlayerPrefs
        PlayerPrefs.DeleteKey("User ID");
        PlayerPrefs.DeleteKey("Username");
        PlayerPrefs.DeleteKey("Fullname");
        PlayerPrefs.DeleteKey("Section ID");
        PlayerPrefs.DeleteKey("Section Name");
        PlayerPrefs.Save();
        Debug.Log("Player information deleted.");
    }
 


}
