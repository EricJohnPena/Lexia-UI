using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ClassicGameManager : MonoBehaviour
{
    private Trie wordTrie;
    public static ClassicGameManager instance;

    [SerializeField]
    private Text questionText;

    [SerializeField]
    private Image questionImage; // New image component

    [SerializeField]
    private WordData[] answerWordArray;

    [SerializeField]
    private GameObject gameOver;

    [SerializeField]
    private Button[] keyboardButtons;

    [SerializeField]
    private Button backspaceButton;

    private KeyboardQuestionList defaultQuestionData = new KeyboardQuestionList
    {
        questions = new List<KeyboardQuestion>
        {
            new KeyboardQuestion
            {
                questionText = "No Questions Loaded",
                answer = "ANSWER1",
                imagePath = "DefaultImages/default_image1",
            },
            new KeyboardQuestion
            {
                questionText = "No Questions Loaded",
                answer = "ANSWER2",
                imagePath = "DefaultImages/default_image2",
            },
        },
    };
    private KeyboardQuestionList questionData;
    private int currentQuestionIndex = 0;
    private string currentAnswer;
    private int currentAnswerIndex = 0;
    private GameStatus gameStatus = GameStatus.Playing;
    private List<int> selectedKeyIndex;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        selectedKeyIndex = new List<int>();
        wordTrie = new Trie();

        LoadValidWords();

        var (subjectId, moduleId, lessonId) = Web.GetCurrentTrackingIds();
        StartCoroutine(LoadQuestionData(subjectId, moduleId, lessonId));

        SetupKeyboardListeners();
    }

    private void LoadValidWords()
    {
        string path = System.IO.Path.Combine(
            Application.streamingAssetsPath,
            "classic_questions.json"
        );
        StartCoroutine(LoadWordsFromJson(path));
    }

    private IEnumerator LoadWordsFromJson(string path)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(path))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string jsonText = www.downloadHandler.text;
                    KeyboardQuestionList questionData = JsonUtility.FromJson<KeyboardQuestionList>(
                        jsonText
                    );

                    foreach (var question in questionData.questions)
                    {
                        wordTrie.Insert(question.answer.ToUpper());
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error parsing JSON: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("Failed to load words from file: " + www.error);
            }
        }
    }

    private IEnumerator LoadQuestionData(int subjectId, int moduleId, int lessonId)
    {
        string url =
            $"{Web.BaseApiUrl}getClassicQuestions.php?subject_id={subjectId}&module_id={moduleId}&lesson_id={lessonId}";
        Debug.Log("Fetching questions from URL: " + url);
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string jsonText = www.downloadHandler.text;
                    Debug.Log("Raw JSON Response: " + jsonText);

                    questionData = JsonUtility.FromJson<KeyboardQuestionList>(jsonText);

                    if (
                        questionData == null
                        || questionData.questions == null
                        || questionData.questions.Count == 0
                    )
                    {
                        Debug.LogWarning(
                            "Loaded JSON is empty or invalid. Using default questions."
                        );
                        questionData = defaultQuestionData;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error parsing JSON: " + e.Message);
                    questionData = defaultQuestionData;
                }
            }
            else
            {
                Debug.LogWarning(
                    "Failed to fetch questions from the server. Using default questions."
                );
                questionData = defaultQuestionData;
            }
        }

        SetQuestion();
    }

    private void SetQuestion()
    {
        if (currentQuestionIndex >= questionData.questions.Count)
        {
            gameOver.SetActive(true);
            return;
        }

        currentAnswerIndex = 0;
        selectedKeyIndex.Clear();

        KeyboardQuestion currentQuestion = questionData.questions[currentQuestionIndex];
        questionText.text = currentQuestion.questionText;
        currentAnswer = currentQuestion.answer.ToUpper();

        StartCoroutine(LoadQuestionImage(currentQuestion.imagePath));

        ResetQuestion();

        currentQuestionIndex++;
        gameStatus = GameStatus.Playing;
    }

    private IEnumerator LoadQuestionImage(string imagePath)
    {
        questionImage.gameObject.SetActive(false);

        string fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, imagePath);

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(fullPath))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);

                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );

                questionImage.sprite = sprite;
                questionImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"Failed to load image: {imagePath}");
                questionImage.gameObject.SetActive(false);
            }
        }
    }

    private void SetupKeyboardListeners()
    {
        for (int i = 0; i < keyboardButtons.Length; i++)
        {
            int index = i;
            keyboardButtons[i].onClick.AddListener(() => OnKeyPressed(keyboardButtons[index]));
        }

        if (backspaceButton != null)
        {
            backspaceButton.onClick.AddListener(ResetLastWord);
        }
    }

    private void ResetQuestion()
    {
        for (int i = 0; i < answerWordArray.Length; i++)
        {
            answerWordArray[i].gameObject.SetActive(true);
            answerWordArray[i].SetChar('_');
        }

        for (int i = currentAnswer.Length; i < answerWordArray.Length; i++)
        {
            answerWordArray[i].gameObject.SetActive(false);
        }

        foreach (Button button in keyboardButtons)
        {
            button.gameObject.SetActive(true);
        }
    }

    private void OnKeyPressed(Button keyButton)
    {
        if (gameStatus == GameStatus.Next || currentAnswerIndex >= currentAnswer.Length)
            return;

        string keyPressed = keyButton.GetComponentInChildren<Text>().text;

        selectedKeyIndex.Add(System.Array.IndexOf(keyboardButtons, keyButton));

        answerWordArray[currentAnswerIndex].SetChar(keyPressed[0]);
        currentAnswerIndex++;

        if (currentAnswerIndex == currentAnswer.Length)
        {
            CheckAnswer();
        }
    }

    private void ResetLastWord()
    {
        if (selectedKeyIndex.Count > 0)
        {
            int index = selectedKeyIndex[selectedKeyIndex.Count - 1];
            selectedKeyIndex.RemoveAt(selectedKeyIndex.Count - 1);

            currentAnswerIndex--;
            answerWordArray[currentAnswerIndex].SetChar('_');
        }
    }

    private void CheckAnswer()
    {
        bool isCharacterMatch = true;

        for (int i = 0; i < currentAnswer.Length; i++)
        {
            if (char.ToUpper(currentAnswer[i]) != char.ToUpper(answerWordArray[i].charValue))
            {
                isCharacterMatch = false;
                break;
            }
        }

        bool isValidWord = wordTrie.Search(currentAnswer.ToUpper());

        if (isCharacterMatch && isValidWord)
        {
            Debug.Log("Correct Answer!");
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
            Debug.Log("Incorrect Answer!");
            for (int i = 0; i < currentAnswerIndex; i++)
            {
                answerWordArray[i].SetChar('_');
            }

            currentAnswerIndex = 0;
            selectedKeyIndex.Clear();
        }
    }
}

[System.Serializable]
public class KeyboardQuestion
{
    public string questionText;
    public string answer;
    public string imagePath;
}

[System.Serializable]
public class KeyboardQuestionList
{
    public List<KeyboardQuestion> questions;
}
