using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LessonManager : MonoBehaviour
{
    public static LessonManager instance { get; private set; }
    private string currentLessonNumber;
    private string currentModuleNumber;
    private string currentSubjectId;

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

    public void RecordLessonData(string lessonNumber, string moduleNumber, string subjectId)
    {
        currentLessonNumber = lessonNumber;
        currentModuleNumber = moduleNumber;
        currentSubjectId = subjectId;

        Debug.Log($"[LessonManager] Recorded lesson data - Lesson: {lessonNumber}, Module: {moduleNumber}, Subject: {subjectId}");
    }

    public (string lessonNumber, string moduleNumber, string subjectId) GetCurrentLessonData()
    {
        return (currentLessonNumber, currentModuleNumber, currentSubjectId);
    }
}

