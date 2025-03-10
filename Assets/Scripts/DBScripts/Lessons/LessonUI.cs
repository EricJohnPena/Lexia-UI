using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LessonUI : MonoBehaviour
{
    public Text lessonNameText;
    public Button actionButton;
    public Button playMenuButton;

    public void SetLessonData(string lessonName, string lessonNumber, string subjectName)
    {
        lessonNameText.text = lessonName;

        MenuManager.InstanceMenu.subjectName.text = subjectName;

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(() => StartLesson(lessonNumber));

        playMenuButton.onClick.RemoveAllListeners();
        playMenuButton.onClick.AddListener(SceneController.Instance.CreateNewGame);
    }

    private void StartLesson(string lessonNumber)
    {
        LessonManager.instance.SetCurrentLesson(lessonNumber);
        Debug.Log("Starting lesson: " + lessonNumber);

    }

}
