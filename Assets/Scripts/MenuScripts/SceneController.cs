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
        StartingScene,
        GameScene

    }
    
   
    public void GoToStartingScene(){
        SceneManager.LoadScene(Scene.StartingScene.ToString());
    }


    public void CreateNewGame(){
        SceneManager.LoadScene(Scene.GameScene.ToString());
        
     }
     
}