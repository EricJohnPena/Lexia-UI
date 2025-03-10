using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    public InputField usernameInput;
    public InputField passwordInput;
    public Button loginBtn;

    // Start is called before the first frame update
    void Start()
    {
        loginBtn.onClick.AddListener(() =>
        {
            StartCoroutine(Manager.instance.web.Login(usernameInput.text, passwordInput.text));
        });
    }

    public void OnLoginSuccess()
    {
        // Update the profile UI after successful login
        ProfileManager profileManager = FindObjectOfType<ProfileManager>();
        if (profileManager != null)
        {
            profileManager.UpdateProfileUI();
        }
    }
}
