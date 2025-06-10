using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FirstTimeUserManager : MonoBehaviour
{
    public static FirstTimeUserManager Instance { get; private set; }

    [Header("First Time Password Change")]
    public Canvas firstTimePasswordCanvas;
    public InputField newPasswordInput;
    public InputField confirmPasswordInput;
    public Button changePasswordButton;
    public Text passwordErrorText;

    [Header("Welcome Canvas")]
    public Canvas welcomeCanvas;
    public Button welcomeNextButton;

    [Header("Introduction Canvas")]
    public Canvas introductionCanvas;
    public Button introductionNextButton;

    [Header("Features Canvas")]
    public Canvas featuresCanvas;
    public Button featuresNextButton;

    [Header("Getting Started Canvas")]
    public Canvas gettingStartedCanvas;
    public Button getStartedButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManager();
        }
    }

    private void InitializeManager()
    {
        // Set up button listeners
        if (changePasswordButton != null)
            changePasswordButton.onClick.AddListener(OnChangePasswordClicked);
        if (welcomeNextButton != null)
            welcomeNextButton.onClick.AddListener(() => ShowCanvas(introductionCanvas));
        if (introductionNextButton != null)
            introductionNextButton.onClick.AddListener(() => ShowCanvas(featuresCanvas));
        if (featuresNextButton != null)
            featuresNextButton.onClick.AddListener(OnFeaturesNextClicked);

        // Hide all canvases initially
        HideAllCanvases();
    }

    public void StartFirstTimeUserFlow()
    {
        Debug.Log("Starting first-time user flow");
        // Ensure the manager is active
        gameObject.SetActive(true);
        StartCoroutine(CheckFirstTimeStatus());
    }

    private IEnumerator CheckFirstTimeStatus()
    {
        string studentId = PlayerPrefs.GetString("User ID");
        if (string.IsNullOrEmpty(studentId))
        {
            Debug.LogError("No student ID found in PlayerPrefs");
            yield break;
        }

        Debug.Log("Checking first time status for student ID: " + studentId);

        WWWForm form = new WWWForm();
        form.AddField("student_id", studentId);

        using (
            UnityWebRequest www = UnityWebRequest.Post(
                Web.BaseApiUrl + "checkFirstTimeUser.php",
                form
            )
        )
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                bool isFirstTime = false;
                try
                {
                    Debug.Log("First time check response: " + www.downloadHandler.text);
                    var response = JsonConvert.DeserializeObject<FirstTimeResponse>(
                        www.downloadHandler.text
                    );
                    isFirstTime = response.is_first_time;
                }
                catch (Exception e)
                {
                    Debug.LogError("Error parsing first-time check response: " + e.Message);
                    Debug.LogError("Full response: " + www.downloadHandler.text);
                    yield break;
                }

                if (isFirstTime)
                {
                    Debug.Log(
                        "User is confirmed first time user. Calling ShowFirstTimePasswordCanvas()"
                    );
                    // No need for WaitForEndOfFrame here, as UI operations should be fine immediately after web request.
                    ShowFirstTimePasswordCanvas();
                }
                else
                {
                    Debug.Log("User is not first time user, hiding all canvases");
                    HideAllCanvases();
                }
            }
            else
            {
                Debug.LogError("Error checking first-time status: " + www.error);
                Debug.LogError("Response code: " + www.responseCode);
            }
        }
    }

    private void ShowFirstTimePasswordCanvas()
    {
        Debug.Log("Entering ShowFirstTimePasswordCanvas method.");

        if (firstTimePasswordCanvas == null)
        {
            Debug.LogError("First time password canvas reference is missing in the Inspector!");
            return;
        }

        // Ensure all other canvases are hidden first
        HideAllCanvases();
        Debug.Log("All other canvases hidden.");

        // Ensure the canvas GameObject is active
        if (!firstTimePasswordCanvas.gameObject.activeSelf)
        {
            firstTimePasswordCanvas.gameObject.SetActive(true);
        }

        // Ensure the canvas component is enabled
        if (!firstTimePasswordCanvas.enabled)
        {
            firstTimePasswordCanvas.enabled = true;
        }

        // Set the canvas to be the last sibling to ensure it's on top
        firstTimePasswordCanvas.transform.SetAsLastSibling();

        // Get or add CanvasGroup component
        CanvasGroup canvasGroup = firstTimePasswordCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = firstTimePasswordCanvas.gameObject.AddComponent<CanvasGroup>();
            Debug.Log("Added CanvasGroup component to firstTimePasswordCanvas");
        }

        // Set CanvasGroup properties
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // Force canvas to update
        Canvas.ForceUpdateCanvases();

        // Final verification
        if (!firstTimePasswordCanvas.gameObject.activeInHierarchy)
        {
            Debug.LogError(
                "Failed to activate first time password canvas in hierarchy after all attempts!"
            );
            return;
        }

        Debug.Log("First time password canvas should now be visible and interactive.");
    }

    private void OnChangePasswordClicked()
    {
        if (
            string.IsNullOrEmpty(newPasswordInput.text)
            || string.IsNullOrEmpty(confirmPasswordInput.text)
        )
        {
            ShowPasswordError("Please fill in all fields");
            return;
        }

        if (newPasswordInput.text != confirmPasswordInput.text)
        {
            ShowPasswordError("Passwords do not match");
            return;
        }

        if (newPasswordInput.text.Length < 8)
        {
            ShowPasswordError("Password must be at least 8 characters long");
            return;
        }

        StartCoroutine(ChangePassword(newPasswordInput.text));
    }

    private void ShowPasswordError(string message)
    {
        if (passwordErrorText != null)
        {
            passwordErrorText.text = message;
        }
        Debug.LogWarning("Password error: " + message);
    }

    private IEnumerator ChangePassword(string newPassword)
    {
        string studentId = PlayerPrefs.GetString("User ID");
        WWWForm form = new WWWForm();
        form.AddField("student_id", studentId);
        form.AddField("new_password", newPassword);

        using (
            UnityWebRequest www = UnityWebRequest.Post(Web.BaseApiUrl + "changePassword.php", form)
        )
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    Debug.Log("Change password response: " + www.downloadHandler.text);
                    var response = JsonConvert.DeserializeObject<PasswordChangeResponse>(
                        www.downloadHandler.text
                    );

                    if (response.success)
                    {
                        ShowWelcomeCanvas();
                    }
                    else
                    {
                        ShowPasswordError(response.error);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error parsing password change response: " + e.Message);
                    ShowPasswordError("Error processing server response");
                }
            }
            else
            {
                Debug.LogError("Error changing password: " + www.error);
                ShowPasswordError("Network error. Please try again.");
            }
        }
    }

    private void ShowWelcomeCanvas()
    {
        if (welcomeCanvas == null)
        {
            Debug.LogError("Welcome canvas or text reference is missing!");
            return;
        }

        ShowCanvas(welcomeCanvas);
    }

    private void HideAllCanvases()
    {
        Debug.Log("Hiding all canvases.");

        // Only set to inactive, leave CanvasGroup properties to ShowCanvas or specific Show methods
        if (firstTimePasswordCanvas != null)
        {
            firstTimePasswordCanvas.gameObject.SetActive(false);
        }
        if (welcomeCanvas != null)
            welcomeCanvas.gameObject.SetActive(false);
        if (introductionCanvas != null)
            introductionCanvas.gameObject.SetActive(false);
        if (featuresCanvas != null)
            featuresCanvas.gameObject.SetActive(false);
        if (gettingStartedCanvas != null)
            gettingStartedCanvas.gameObject.SetActive(false);
    }

    private void ShowCanvas(Canvas canvas)
    {
        if (canvas == null)
        {
            Debug.LogError("Attempted to show null canvas!");
            return;
        }

        HideAllCanvases();
        canvas.gameObject.SetActive(true);
        Debug.Log("Showing canvas: " + canvas.name);
    }

    private void OnFeaturesNextClicked()
    {
        Debug.Log("Features Next clicked, transitioning to student dashboard");

        // Hide all canvases
        HideAllCanvases();

        // Navigate to student dashboard
        if (MenuManager.InstanceMenu != null)
        {
            MenuManager.InstanceMenu.LoginPage.gameObject.SetActive(false);
            MenuManager.InstanceMenu.LogintoPage();
            MenuManager.InstanceMenu.usernameText.text =
                "Hi " + PlayerPrefs.GetString("Username", "Guest User") + ",";
        }
        else
        {
            Debug.LogError("MenuManager not found. Cannot navigate to student dashboard.");
        }
    }
}

[Serializable]
public class FirstTimeResponse
{
    public bool is_first_time;
    public string error;
}
