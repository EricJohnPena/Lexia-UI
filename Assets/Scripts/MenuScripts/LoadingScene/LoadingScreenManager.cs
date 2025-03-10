using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreenManager : MonoBehaviour
{
    
    public float loadingDelay = 3f; // Duration to show loading screen (e.g., 3 seconds)

    void Start()
    {
        StartCoroutine(LoadNextSceneAfterDelay());
    }

    IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(loadingDelay);
        MenuManager.InstanceMenu.LogintoPage();
    }
}