using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreenManager : MonoBehaviour
{
    public float loadingDelay = 2f; // Duration to show loading screen (2 seconds)
    private bool isLoading = false;
    private Coroutine loadingCoroutine;

    void OnEnable()
    {
        // Reset state when the loading screen is enabled
        isLoading = false;
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
        }
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

    IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(loadingDelay);
        
        // Ensure MenuManager exists before proceeding
        if (MenuManager.InstanceMenu != null)
        {
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