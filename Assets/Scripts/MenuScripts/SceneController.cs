using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }
    public GameObject lessonsPage;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public enum Scene
    {
        StartingScene,
        GameScene,
    }

    public void GoToStartingScene(string canvasName)
    {
        Debug.Log($"GoToStartingScene called with canvasName: {canvasName}");
        SceneManager.sceneLoaded += (scene, mode) => OnSceneLoaded(scene, mode, canvasName);
        SceneManager.LoadScene(Scene.StartingScene.ToString());
    }

    public Canvas studentDashboardCanvas; // Assign this in the Inspector

    private void OnSceneLoaded(
        UnityEngine.SceneManagement.Scene scene,
        LoadSceneMode mode,
        string canvasName
    )
    {
        Debug.Log($"OnSceneLoaded called for scene: {scene.name}, canvasName: {canvasName}");
        if (scene.name == Scene.StartingScene.ToString())
        {
            if (canvasName == "StudentDashboardCanvas")
            {
                studentDashboardCanvas.gameObject.SetActive(true);
                lessonsPage.SetActive(true); // Activate lessonsPage when returning to StartingScene
                Debug.Log("lessonspage activated.");
            }
            else
            {
                Debug.LogWarning("Canvas not found: " + canvasName);
            }
        }
        else if (scene.name == Scene.GameScene.ToString())
        {
            lessonsPage.SetActive(false); // Optionally deactivate lessonsPage when entering GameScene
        }
    }

    public void CreateNewGame()
    {
        SceneManager.LoadScene(Scene.GameScene.ToString());
    }
}
