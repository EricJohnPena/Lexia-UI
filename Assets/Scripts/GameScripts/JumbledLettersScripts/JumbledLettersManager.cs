using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class JumbledLettersManager : MonoBehaviour
{
    private Trie wordTrie;
    public static JumbledLettersManager instance;

    [SerializeField]
    private Text questionText;
    [SerializeField]
    private GameObject gameOver;
    [SerializeField]
    private WordData[] answerWordArray;
    [SerializeField]
    private WordData[] optionWordArray;

 
    private string apiUrl = $"{Web.BaseApiUrl}getJumbledLettersQuestions.php";

    private JLQuestionList questionData;
    private char[] charArray = new char[12];
    private int currentAnswerIndex = 0;
    private List<int> selectedWordIndex;
    private int currentQuestionIndex = 0;
    private GameStatus gameStatus = GameStatus.Playing;
    private string answerWord;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        selectedWordIndex = new List<int>();
        wordTrie = new Trie();
    }

    void OnEnable()
    {
        Debug.Log("Jumbled Letters game enabled. Refreshing data...");
        RefreshJumbledLettersData();
    }

    public void RefreshJumbledLettersData()
    {
        Debug.Log("Refreshing Jumbled Letters data...");
        ResetGameState();
        StartCoroutine(LoadQuestionData(LessonsLoader.subjectId, int.Parse(LessonsLoader.moduleNumber), LessonUI.lesson_id));
    }

    private void ResetGameState()
    {
        Debug.Log("Resetting Jumbled Letters game state...");
        currentQuestionIndex = 0;
        currentAnswerIndex = 0;
        selectedWordIndex.Clear();
        gameStatus = GameStatus.Playing;

        if (questionText != null) questionText.text = "";

        foreach (var word in answerWordArray)
        {
            word.SetChar('_');
            word.gameObject.SetActive(true);
        }

        foreach (var word in optionWordArray)
        {
            word.gameObject.SetActive(true);
        }

        if (gameOver != null) gameOver.SetActive(false);
    }

    private IEnumerator LoadQuestionData(int subjectId, int moduleId, int lessonId)
    {
        string url = $"{apiUrl}?subject_id={subjectId}&module_id={moduleId}&lesson_id={lessonId}";
        Debug.Log("Fetching Jumbled Letters questions from URL: " + url);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string jsonText = www.downloadHandler.text;
                    Debug.Log("Raw JSON Response: " + jsonText);

                    questionData = JsonUtility.FromJson<JLQuestionList>(jsonText);

                    if (questionData == null || questionData.questions == null || questionData.questions.Count == 0)
                    {
                        Debug.LogWarning("No Jumbled Letters data received from the server.");
                        gameOver.SetActive(true);
                        yield break;
                    }

                    foreach (var question in questionData.questions)
                    {
                        wordTrie.Insert(question.answer.ToUpper());
                    }

                    SetQuestion();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error parsing JSON: " + e.Message);
                    gameOver.SetActive(true);
                }
            }
            else
            {
                Debug.LogError("Failed to fetch Jumbled Letters data: " + www.error);
                gameOver.SetActive(true);
            }
        }
    }

    private void SetQuestion()
    {
        if (currentQuestionIndex >= questionData.questions.Count)
        {
            gameOver.SetActive(true);
            return;
        }

        currentAnswerIndex = 0;
        selectedWordIndex.Clear();

        JLQuestion currentQuestion = questionData.questions[currentQuestionIndex];
        questionText.text = currentQuestion.questionText;
        answerWord = currentQuestion.answer.ToUpper();

        ResetQuestion();

        for (int i = 0; i < answerWord.Length; i++)
        {
            charArray[i] = answerWord[i];
        }

        for (int i = answerWord.Length; i < optionWordArray.Length; i++)
        {
            charArray[i] = (char)UnityEngine.Random.Range(65, 91);
        }

        charArray = ShuffleList.ShuffleListItems<char>(charArray.ToList()).ToArray();

        for (int i = 0; i < optionWordArray.Length; i++)
        {
            optionWordArray[i].SetChar(charArray[i]);
        }

        currentQuestionIndex++;
        gameStatus = GameStatus.Playing;
    }

    public void SelectedOption(WordData wordData)
    {
        if (gameStatus == GameStatus.Next || currentAnswerIndex >= answerWord.Length) return;

        selectedWordIndex.Add(wordData.transform.GetSiblingIndex());
        wordData.gameObject.SetActive(false);
        answerWordArray[currentAnswerIndex].SetChar(wordData.charValue);
        currentAnswerIndex++;

        if (currentAnswerIndex == answerWord.Length)
        {
            string formedWord = "";
            for (int i = 0; i < currentAnswerIndex; i++)
            {
                formedWord += answerWordArray[i].charValue;
            }

            if (wordTrie.Search(formedWord.ToUpper()))
            {
                Debug.Log("Answer correct!");
                gameStatus = GameStatus.Next;
                if (currentQuestionIndex < questionData.questions.Count)
                {
                    Invoke("SetQuestion", 0.5f);
                }
                else
                {
                    gameOver.SetActive(true);
                }
            }
            else
            {
                Debug.Log("Answer incorrect!");
                ResetCurrentInput();
            }
        }
    }

    private void ResetCurrentInput()
    {
        for (int i = 0; i < currentAnswerIndex; i++)
        {
            int originalIndex = selectedWordIndex[i];
            optionWordArray[originalIndex].gameObject.SetActive(true);
        }

        selectedWordIndex.Clear();
        currentAnswerIndex = 0;
        ResetQuestion();
    }

    private void ResetQuestion()
    {
        for (int i = 0; i < answerWordArray.Length; i++)
        {
            answerWordArray[i].SetChar('_');
            answerWordArray[i].gameObject.SetActive(i < answerWord.Length);
        }

        foreach (var word in optionWordArray)
        {
            word.gameObject.SetActive(true);
        }
    }
}

