using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    public float loadingDelay = 2f; // Duration to show loading screen (2 seconds)
    private bool isLoading = false;
    private Coroutine loadingCoroutine;

    [Header("Loading UI")]
    [SerializeField]
    private Image loadingBarFill; // Image with fill for loading bar

    [SerializeField]
    private Text loadingText; // Legacy UI Text for "Loading...100%"

    void OnEnable()
    {
        // Reset state when the loading screen is enabled
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
        while (elapsed < loadingDelay)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / loadingDelay);
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
            MenuManager.InstanceMenu.LogintoPage();
        }
        else
        {
            Debug.LogError("MenuManager instance not found!");
        }

        isLoading = false;
        loadingCoroutine = null;
    }
}
