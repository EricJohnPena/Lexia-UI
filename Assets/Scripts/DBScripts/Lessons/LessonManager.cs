using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LessonManager : MonoBehaviour
{
    public static LessonManager instance { get; private set; }
    private string currentLessonNumber;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCurrentLesson(string lessonNumber)
    {
        currentLessonNumber = lessonNumber;
        Debug.Log("Current Lesson Number: " + currentLessonNumber);
    }

    public string GetCurrentLesson()
    {
        return currentLessonNumber;
    }

}

