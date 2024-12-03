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
    [SerializeField] Button toLessonsPageBtn1;
    [SerializeField] Button toStudentLeaderboardBtn1;
     [SerializeField] Button toProfilePageBtn1;


     [SerializeField] Button toStudentDashboardPageBtn2;
    [SerializeField] Button toStudentLeaderboardBtn2;
     [SerializeField] Button toProfilePageBtn2;


     [SerializeField] Button toStudentDashboardPageBtn3;
    [SerializeField] Button toStudentLeaderboardBtn3;
     [SerializeField] Button toProfilePageBtn3;

     [SerializeField] Button backToDashboard;
   

    


    public Canvas StartingPage;
    public Canvas LoginPage;
    public Canvas SignupPage;
    public Canvas LoadingScene;
    public Canvas StudentDashboardPage;
    public Canvas StudentLeaderboardPage;
    public Canvas ProfilePage;
    public Canvas LessonsPage;

   
   void Start(){
    goToLoginBtn.onClick.AddListener(() => OpenPage(LoginPage));
    goToLoginBtn1.onClick.AddListener(() => OpenPage(LoginPage));
    goToSignupBtn.onClick.AddListener(() => OpenPage(SignupPage));
    goToSignupBtn1.onClick.AddListener(() => OpenPage(SignupPage));
    loginBtn.onClick.AddListener(() => OpenPage(StudentDashboardPage));
    toLessonsPageBtn1.onClick.AddListener(() => OpenPage(LessonsPage));
    //loginBtn.onClick.AddListener(ToMenuScene);//for testing purposes only
    toStudentLeaderboardBtn1.onClick.AddListener(() => OpenPage(StudentLeaderboardPage));
    toStudentLeaderboardBtn2.onClick.AddListener(() => OpenPage(StudentLeaderboardPage));
    toStudentLeaderboardBtn3.onClick.AddListener(() => OpenPage(StudentLeaderboardPage));

    toProfilePageBtn1.onClick.AddListener(() => OpenPage(ProfilePage));
    toProfilePageBtn2.onClick.AddListener(() => OpenPage(ProfilePage));
    toProfilePageBtn3.onClick.AddListener(() => OpenPage(ProfilePage));


    toStudentDashboardPageBtn2.onClick.AddListener(() => OpenPage(StudentDashboardPage));
    toStudentDashboardPageBtn3.onClick.AddListener(() => OpenPage(StudentDashboardPage));

    backToDashboard.onClick.AddListener(() => OpenPage(StudentDashboardPage));

   }

   public void OpenPage(Canvas canvas) {
    // Create an array of all canvases
    Canvas[] allCanvases = new Canvas[] {
        StartingPage,
        LoginPage,
        SignupPage,
        LoadingScene,
        StudentDashboardPage,
        StudentLeaderboardPage,
        ProfilePage,
        LessonsPage
    };

    // Deactivate all canvases
    foreach (Canvas c in allCanvases) {
        c.gameObject.SetActive(false);
    }

    // Activate the selected canvas
    canvas.gameObject.SetActive(true);
}

  


}