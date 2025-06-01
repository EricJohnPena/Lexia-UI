using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance { get; private set; }

    private const float MINIMUM_LOADING_TIME = 3f; // Minimum loading time in seconds
    private bool isLoading = false;
    private Coroutine loadingCoroutine;

    [Header("Loading UI")]
    [SerializeField]
    private Image loadingBarFill; // Image with fill for loading bar

    [SerializeField]
    private Text loadingText; // Legacy UI Text for "Loading...100%"

    void Awake()
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

    void OnEnable()
    {
        // Reset state when the loading screen is enabled
        if (Instance != this)
        {
            return; // Prevent multiple instances from running loading coroutine
        }
        isLoading = false;
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
        }
        ResetLoadingUI();
        StartLoading();
    }

    void OnDisable()
    {
        if (Instance != this)
        {
            return;
        }
        // Clean up when the loading screen is disabled
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }
        isLoading = false;
    }

    public void StartLoading()
    {
        if (!isLoading)
        {
            isLoading = true;
            loadingCoroutine = StartCoroutine(LoadNextSceneAfterDelay());
        }
    }

    private void ResetLoadingUI()
    {
        if (loadingBarFill != null)
            loadingBarFill.fillAmount = 0f;
        if (loadingText != null)
            loadingText.text = "Loading...0%";
    }

    IEnumerator LoadNextSceneAfterDelay()
    {
        float elapsed = 0f;
        while (elapsed < MINIMUM_LOADING_TIME)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / MINIMUM_LOADING_TIME);
            if (loadingBarFill != null)
                loadingBarFill.fillAmount = progress;
            if (loadingText != null)
                loadingText.text = $"Loading...{Mathf.RoundToInt(progress * 100f)}%";
            yield return null;
        }

        // Ensure MenuManager exists before proceeding
        if (MenuManager.InstanceMenu != null)
        {
            if (loadingBarFill != null)
                loadingBarFill.fillAmount = 1f;
            if (loadingText != null)
                loadingText.text = "Loading...100%";
            Debug.Log("Loading complete. Proceeding to next scene.");
            //LogintoPage();
        }
        else
        {
            Debug.LogError("MenuManager instance not found!");
        }

        isLoading = false;
        loadingCoroutine = null;
    }

    public void LogintoPage()
    {
        MenuManager.InstanceMenu.LogintoPage();
    }
}
