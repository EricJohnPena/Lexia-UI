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
