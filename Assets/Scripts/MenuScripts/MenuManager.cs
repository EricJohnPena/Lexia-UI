using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager InstanceMenu;
    private void Awake(){
        InstanceMenu = this;
    }

    [SerializeField] Button goToLoginBtn;
    [SerializeField] Button goToSignupBtn;
    [SerializeField] Button loginBtn;
    [SerializeField] Button goToLoginBtn1; //from signup page
    [SerializeField] Button goToSignupBtn1; //from login page
   

    


    public Canvas StartingPage;
    public Canvas LoginPage;
    public Canvas SignupPage;
    public Canvas LoadingScene;

   
   void Start(){
    goToLoginBtn.onClick.AddListener(() => OpenPage(LoginPage));
    goToLoginBtn1.onClick.AddListener(() => OpenPage(LoginPage));
    goToSignupBtn.onClick.AddListener(() => OpenPage(SignupPage));
    goToSignupBtn1.onClick.AddListener(() => OpenPage(SignupPage));
    loginBtn.onClick.AddListener(() => OpenPage(LoadingScene));
    //loginBtn.onClick.AddListener(ToMenuScene);//for testing purposes only

   }

   public void OpenPage(Canvas canvas){
    StartingPage.gameObject.SetActive(false);
    LoginPage.gameObject.SetActive(false);
    SignupPage.gameObject.SetActive(false);
    LoadingScene.gameObject.SetActive(false);
    canvas.gameObject.SetActive(true);
   }

    void ToMenuScene(){
        SceneController.Instance.GoToMainMenu(); //
    }


}