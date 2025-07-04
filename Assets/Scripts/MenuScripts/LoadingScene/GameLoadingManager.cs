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
        // Ensure the delay is at least MINIMUM_LOADING_TIME
        float actualDelay = Mathf.Max(delay, MINIMUM_LOADING_TIME);
        loadingCoroutine = StartCoroutine(
            ShowLoadingScreenCoroutine(actualDelay, useAlternate, onComplete)
        );
    }

    public void ShowLoadingScreenUntilComplete(System.Action operation, System.Action onComplete = null)
    {
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
        }
        loadingCoroutine = StartCoroutine(ShowLoadingScreenUntilCompleteCoroutine(operation, onComplete));
    }

    private IEnumerator ShowLoadingScreenUntilCompleteCoroutine(System.Action operation, System.Action onComplete)
    {
        ShowLoadingScreen();
        float startTime = Time.time;
        
        // Execute the operation
        operation?.Invoke();
        
        // Ensure minimum loading time
        float elapsedTime = Time.time - startTime;
        if (elapsedTime < MINIMUM_LOADING_TIME)
        {
            yield return new WaitForSeconds(MINIMUM_LOADING_TIME - elapsedTime);
        }
        
        HideLoadingScreen();
        onComplete?.Invoke();
        OnLoadingComplete?.Invoke();
    }

    private IEnumerator ShowLoadingScreenCoroutine(
        float delay,
        bool useAlternate,
        System.Action onComplete
    )
    {
        ShowLoadingScreen(useAlternate);
        yield return new WaitForSeconds(delay);
        HideLoadingScreen();
        onComplete?.Invoke();
        OnLoadingComplete?.Invoke();
        if (useAlternate && !isSceneTransitioning)
        {
            isSceneTransitioning = true;
            GoToLoginPage();
        }
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
