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

    // Embedded default questions as a fallback
    private JLQuestionList defaultQuestionData = new JLQuestionList
    {
        questions = new List<JLQuestion>
        {
            new JLQuestion {
                questionText = "Unscramble this word",
                answer = "UNITY"
            },
            new JLQuestion {
                questionText = "Another word to solve",
                answer = "GAME"
            }
            // Add more default questions as needed
        }
    };

    private JLQuestionList questionData;
    private char[] charArray = new char[12];
    private int currentAnswerIndex = 0;
    private bool correctAnswer = false;
    private List<int> selectedWordIndex;
    private int currentQuestionIndex = 0;
    private GameStatus gameStatus = GameStatus.Playing;
    private string answerWord;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        selectedWordIndex = new List<int>();
        // Initialize the Trie
        wordTrie = new Trie();

        // Load valid words into the Trie
        LoadValidWords();
        // Start loading questions
        StartCoroutine(LoadQuestionData());
    }
    private void LoadValidWords()
    {
        // Load valid words from the JSON file
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "jumbled_letters_questions.json");

        // Use UnityWebRequest to read the JSON file
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
                    // Parse the JSON data
                    string jsonText = www.downloadHandler.text;
                    JLQuestionList questionData = JsonUtility.FromJson<JLQuestionList>(jsonText);

                    // Insert each answer into the Trie
                    foreach (var question in questionData.questions)
                    {
                        wordTrie.Insert(question.answer.ToUpper()); // Insert the answer in uppercase
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


    private IEnumerator LoadQuestionData()
    {
        // Attempt to load from StreamingAssets first
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "jumbled_letters_questions.json");

        using (UnityWebRequest www = UnityWebRequest.Get(path))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // Try to parse the JSON from file
                    string jsonText = www.downloadHandler.text;
                    questionData = JsonUtility.FromJson<JLQuestionList>(jsonText);

                    // Verify the data was loaded correctly
                    if (questionData == null || questionData.questions == null || questionData.questions.Count == 0)
                    {
                        Debug.LogWarning("Loaded JSON is empty or invalid. Using default questions.");
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
                Debug.LogWarning("Failed to load questions from file. Using default questions.");
                questionData = defaultQuestionData;
            }

            // Set the first question after loading
            SetQuestion();
        }
    }

    private void SetQuestion()
    {
        // Check if we have questions left
        if (currentQuestionIndex >= questionData.questions.Count)
        {
            gameOver.SetActive(true);
            return;
        }

        currentAnswerIndex = 0;
        selectedWordIndex.Clear();

        // Get current question from loaded data
        JLQuestion currentQuestion = questionData.questions[currentQuestionIndex];

        questionText.text = currentQuestion.questionText;
        answerWord = currentQuestion.answer;

        ResetQuestion();

        for (int i = 0; i < answerWord.Length; i++)
        {
            charArray[i] = char.ToUpper(answerWord[i]);
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

        // Check if the current answer is valid
        if (currentAnswerIndex == answerWord.Length)
        {
            string formedWord = "";
            for (int i = 0; i < currentAnswerIndex; i++)
            {
                formedWord += answerWordArray[i].charValue;
            }

            if (wordTrie.Search(formedWord.ToUpper())) // Check if the formed word is valid
            {
                correctAnswer = true;
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
                // Reset the current input
                for (int i = 0; i < currentAnswerIndex; i++)
                {
                    // Reactivate the used options
                    int originalIndex = selectedWordIndex[i];
                    optionWordArray[originalIndex].gameObject.SetActive(true);
                }

                // Clear selected indices and reset answer
                selectedWordIndex.Clear();
                currentAnswerIndex = 0;
                ResetQuestion();
            }
        }
    }

    private void ResetQuestion()
    {
        for (int i = 0; i < answerWordArray.Length; i++)
        {
            answerWordArray[i].gameObject.SetActive(true);
            answerWordArray[i].SetChar('_');
        }

        for (int i = answerWord.Length; i < answerWordArray.Length; i++)
        {
            answerWordArray[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < optionWordArray.Length; i++)
        {
            optionWordArray[i].gameObject.SetActive(true);
        }
    }

    public void ResetLastWord()
    {
        if (selectedWordIndex.Count > 0)
        {
            int index = selectedWordIndex[selectedWordIndex.Count - 1];
            optionWordArray[index].gameObject.SetActive(true);
            selectedWordIndex.RemoveAt(selectedWordIndex.Count - 1);
            currentAnswerIndex--;
            answerWordArray[currentAnswerIndex].SetChar('_');
        }
    }

    public void ShuffleOptions()
    {
        if (currentAnswerIndex > 0)
        {
            Debug.Log("Cannot shuffle while building an answer.");
            return;
        }

        charArray = ShuffleList.ShuffleListItems<char>(charArray.ToList()).ToArray();

        for (int i = 0; i < optionWordArray.Length; i++)
        {
            optionWordArray[i].SetChar(charArray[i]);
            optionWordArray[i].gameObject.SetActive(true);
        }

        Debug.Log("Options shuffled!");
    }
}

