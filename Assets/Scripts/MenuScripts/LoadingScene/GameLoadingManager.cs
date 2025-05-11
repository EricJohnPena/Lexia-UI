using UnityEngine;
using System.Collections;

public class GameLoadingManager : MonoBehaviour
{
    public static GameLoadingManager Instance { get; private set; }
    
    [SerializeField] private GameObject loadingScreenPrefab;
    private GameObject currentLoadingScreen;
    private bool isLoading = false;
    private Coroutine loadingCoroutine;

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

    public void ShowLoadingScreen()
    {
        if (!isLoading)
        {
            isLoading = true;
            if (currentLoadingScreen == null && loadingScreenPrefab != null)
            {
                currentLoadingScreen = Instantiate(loadingScreenPrefab);
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

    public void ShowLoadingScreenWithDelay(float delay)
    {
        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
        }
        loadingCoroutine = StartCoroutine(ShowLoadingScreenCoroutine(delay));
    }

    private IEnumerator ShowLoadingScreenCoroutine(float delay)
    {
        ShowLoadingScreen();
        yield return new WaitForSeconds(delay);
        HideLoadingScreen();
    }
} 