using System.Collections;
using UnityEngine;

public class GameLoadingManager : MonoBehaviour
{
    public static GameLoadingManager Instance { get; private set; }

    [SerializeField]
    private GameObject loadingScreenPrefab;

    [SerializeField]
    private GameObject logoLoadingScreenPrefab;

    private GameObject currentLoadingScreen;
    private bool isLoading = false;
    private Coroutine loadingCoroutine;
    private bool isSceneTransitioning = false;
    private const float MINIMUM_LOADING_TIME = 3f; // Minimum loading time in seconds
    public event System.Action OnLoadingComplete;

    private void Awake()
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

    public void ShowLoadingScreen(bool useAlternate = false)
    {
        if (!isLoading)
        {
            isLoading = true;
            if (currentLoadingScreen != null)
            {
                Destroy(currentLoadingScreen);
                currentLoadingScreen = null;
            }
            GameObject prefabToUse = useAlternate ? logoLoadingScreenPrefab : loadingScreenPrefab;
            if (prefabToUse != null)
            {
                currentLoadingScreen = Instantiate(prefabToUse);
                currentLoadingScreen.transform.SetParent(transform);
            }
            currentLoadingScreen?.SetActive(true);
        }
    }

    public void HideLoadingScreen()
    {
        if (isLoading)
        {
            isLoading = false;
            currentLoadingScreen?.SetActive(false);
        }
    }

    public void ShowLoadingScreenWithDelay(
        float delay,
        bool useAlternate = false,
        System.Action onComplete = null
    )
    {
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
        }
        loadingCoroutine = StartCoroutine(ShowLoadingScreenWithDelayCoroutine(delay, useAlternate, onComplete));
    }

    private IEnumerator ShowLoadingScreenWithDelayCoroutine(float delay, bool useAlternate, System.Action onComplete)
    {
        ShowLoadingScreen(useAlternate);
        yield return new WaitForSeconds(delay);
        onComplete?.Invoke();
    }

    private void GoToLoginPage()
    {
        // TODO: Implement navigation to login page here
        Debug.Log("Navigating to Login Page...");
    }

    public void ResetSceneTransitionFlag()
    {
        isSceneTransitioning = false;
    }
}
