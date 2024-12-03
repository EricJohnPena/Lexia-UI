using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{ 
    public static SceneController Instance { get; private set; }
   void Awake() {
    if (Instance == null) {
        Instance = this;
        
    }
}
    public enum Scene{
        LoginScene,
        MenuScene, 
        LoadingScene,
        GameScene
    }
    
    public void GoToScene(Scene sceneName)
    {
        SceneManager.LoadScene(sceneName.ToString());
      
    }
    public void GoToStartingScene(){
        SceneManager.LoadScene(Scene.LoginScene.ToString());
    }

    public void GoToMainMenu(){
        SceneManager.LoadScene(Scene.LoadingScene.ToString());
        
    }

    public void CreateNewGame(){
        SceneManager.LoadScene(Scene.GameScene.ToString());
        
     }
}