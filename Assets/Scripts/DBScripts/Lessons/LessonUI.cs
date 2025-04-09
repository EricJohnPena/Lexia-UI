using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LessonUI : MonoBehaviour
{
    public Text lessonNameText;
    public Button actionButton;
    public Button playMenuButton;

    private bool isLessonDataSet = false;

    private string currentSubject;
    private string currentModule;
    private string currentLessonNumber;
    private string lessonId;    // Field to store the lesson ID
    public static int lesson_id;

    public void SetLessonData(string lessonName, string lessonNumber, string subjectName, string moduleName)
    {
        if (isLessonDataSet)
        {
            Debug.LogWarning("SetLessonData was called again, but lesson data is already set.");
            return;
        }

        isLessonDataSet = true;
        lessonNameText.text = lessonName;

        currentSubject = subjectName;
        currentModule = moduleName;
        currentLessonNumber = lessonNumber;
        this.lessonId = lessonNumber; // Assign the lesson ID

        MenuManager.InstanceMenu.subjectName.text = subjectName;

        Debug.Log($"Setting lesson data. Subject: {subjectName}, Module: {moduleName}, Lesson number: {lessonNumber}");

        actionButton.onClick.RemoveAllListeners();
        Debug.Log("Removed all listeners from actionButton.");
        actionButton.onClick.AddListener(() => {
            Debug.Log("Action button clicked. Starting lesson.");
            StartLesson();
        });

        playMenuButton.onClick.RemoveAllListeners();
        Debug.Log("Removed all listeners from playMenuButton.");
        playMenuButton.onClick.AddListener(() => {
            Debug.Log($"Play button clicked. Recording lesson number: {currentLessonNumber}");
            MenuManager.InstanceMenu.ToGameScene(); // Use MenuManager for page transition
            lesson_id = int.Parse(currentLessonNumber);
            RecordLessonNumber();
           
        });
    }

    

    public string GetLessonId()
    {
        return lessonId; // Expose the lesson ID
    }

    private void StartLesson()
    {
        LessonManager.instance.SetCurrentLesson(currentLessonNumber);
        Debug.Log($"Starting lesson: {currentLessonNumber}");
        

    }

    private void RecordLessonNumber()
    {
        // Logic to record the lesson number
        Debug.Log($"Recorded lesson number: {currentLessonNumber} for Subject: {currentSubject}, Module: {currentModule}");
    }
}
