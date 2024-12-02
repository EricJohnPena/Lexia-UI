using System;
using System.Collections.Generic;

[System.Serializable]
public class QuestionData
{
    public string questionText;
    public string answer;
}

[System.Serializable]
public class QuestionsContainer
{
    public QuestionData[] questions;
}

public enum GameStatus
{
    Playing,
    Next
}

[System.Serializable]
public class JLQuestion
{
    public string questionText;
    public string answer;
}

[System.Serializable]
public class JLQuestionList
{
    public List<JLQuestion> questions;
}