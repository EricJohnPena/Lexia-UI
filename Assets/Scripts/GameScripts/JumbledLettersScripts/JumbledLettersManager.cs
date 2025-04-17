using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

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
    private bool isLessonCompleted = false;
    private bool isRefreshing = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        selectedWordIndex = new List<int>();
        wordTrie = new Trie();
    }

    void OnEnable()
    {
        Debug.Log("Jumbled Letters game enabled.");

        if (!isRefreshing)
        {
            int subjectId = LessonsLoader.subjectId;
            int moduleId;

            if (string.IsNullOrEmpty(LessonsLoader.moduleNumber))
            {
                Debug.LogError(
                    "LessonsLoader.moduleNumber is null or empty. Cannot parse module number."
                );
                return;
            }

            try
            {
                moduleId = int.Parse(LessonsLoader.moduleNumber);
            }
            catch (System.Exception e)
            {
                Debug.LogError(
                    $"Failed to parse LessonsLoader.moduleNumber: {LessonsLoader.moduleNumber}. Error: {e.Message}"
                );
                return;
            }

            int lessonId = LessonUI.lesson_id;
            int gameModeId = 2; // Assuming 2 is the ID for Jumbled Letters mode

            StartCoroutine(
                CheckLessonCompletion(
                    int.Parse(PlayerPrefs.GetString("User ID")),
                    lessonId,
                    gameModeId,
                    subjectId
                )
            );
        }
    }

    private IEnumerator CheckLessonCompletion(
        int studentId,
        int lessonId,
        int gameModeId,
        int subjectId
    )
    {
        if (isRefreshing)
        {
            Debug.LogWarning("CheckLessonCompletion is already running. Skipping...");
            yield break;
        }

        isRefreshing = true; // Mark as running
        Debug.Log(
            $"CheckLessonCompletion called with studentId={studentId}, lessonId={lessonId}, gameModeId={gameModeId}, subjectId={subjectId}"
        );

        if (studentId <= 0 || lessonId <= 0 || gameModeId <= 0)
        {
            Debug.LogError(
                $"Invalid parameters: studentId={studentId}, lessonId={lessonId}, gameModeId={gameModeId}, subjectId={subjectId}"
            );
            isRefreshing = false; // Reset flag
            yield break;
        }

        string url =
            $"{Web.BaseApiUrl}checkLessonCompletion.php?student_id={studentId}&lesson_id={lessonId}&game_mode_id={gameModeId}&subject_id={subjectId}";
        Debug.Log("Checking lesson completion from URL: " + url);

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string response = www.downloadHandler.text;
                    Debug.Log("Lesson Completion Response: " + response);

                    isLessonCompleted = response.Trim() == "true";
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error parsing lesson completion response: " + e.Message);
                    isLessonCompleted = false;
                }
            }
            else
            {
                Debug.LogError("Failed to check lesson completion: " + www.error);
                isLessonCompleted = false;
            }
        }

        isRefreshing = false; // Reset flag

        if (isLessonCompleted)
        {
            HandleLessonState();
        }
        else
        {
            RefreshJumbledLettersData();
        }
    }

    private void HandleLessonState()
    {
        if (isLessonCompleted)
        {
            Debug.Log("Lesson is already completed.");
            questionText.text = "Lesson Completed!";
            gameOver.SetActive(true);
        }
        else
        {
            Debug.Log("Lesson not completed. Loading lesson data...");
            RefreshJumbledLettersData();
        }
    }

    public void RefreshJumbledLettersData()
    {
        Debug.Log("Refreshing Jumbled Letters data...");
        ResetGameState();
        StartCoroutine(
            LoadQuestionData(
                LessonsLoader.subjectId,
                int.Parse(LessonsLoader.moduleNumber),
                LessonUI.lesson_id
            )
        );
    }

    private void ResetGameState()
    {
        Debug.Log("Resetting Jumbled Letters game state...");
        currentQuestionIndex = 0;
        currentAnswerIndex = 0;
        selectedWordIndex.Clear();
        gameStatus = GameStatus.Playing;

        if (questionText != null)
            questionText.text = "";

        foreach (var word in answerWordArray)
        {
            word.SetChar('_');
            word.gameObject.SetActive(true);
        }

        foreach (var word in optionWordArray)
        {
            word.gameObject.SetActive(true);
        }

        if (gameOver != null)
            gameOver.SetActive(false);
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

                    if (
                        questionData == null
                        || questionData.questions == null
                        || questionData.questions.Count == 0
                    )
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
        if (gameStatus == GameStatus.Next || currentAnswerIndex >= answerWord.Length)
            return;

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
