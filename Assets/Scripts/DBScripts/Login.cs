using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    public InputField usernameInput;
    public InputField passwordInput;
    public Button loginBtn;
    public Text errorText; // Reference to the error message Text component

    // Start is called before the first frame update
    void Start()
    {
        loginBtn.onClick.AddListener(() =>
        {
            // Clear previous error message
            if (errorText != null)
            {
                errorText.text = "";
            }

            // Client-side validation
            if (string.IsNullOrEmpty(usernameInput.text))
            {
                if (errorText != null)
                {
                    errorText.text = "Username is required.";
                }
                return;
            }
            if (string.IsNullOrEmpty(passwordInput.text))
            {
                if (errorText != null)
                {
                    errorText.text = "Password is required.";
                }
                return;
            }

            StartCoroutine(Manager.instance.web.Login(usernameInput.text, passwordInput.text));
        });
    }

    public void OnLoginSuccess()
    {
        // Show loading screen first, then transition to dashboard after a short delay
        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.ShowLoadingScreenWithDelay(
                3f, // Use minimum loading time
                false,
                () =>
                {
                    if (MenuManager.InstanceMenu != null)
                    {
                        MenuManager.InstanceMenu.LogintoPage();
                    }
                    else
                    {
                        Debug.LogError("MenuManager instance not found!");
                    }
                }
            );
        }
        else
        {
            if (MenuManager.InstanceMenu != null)
            {
                MenuManager.InstanceMenu.LogintoPage();
            }
            else
            {
                Debug.LogError("MenuManager instance not found!");
            }
        }
    }
}
