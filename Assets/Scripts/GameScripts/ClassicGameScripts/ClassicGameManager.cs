using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ClassicGameManager : MonoBehaviour
{
    public static ClassicGameManager instance;

    [SerializeField] private Text questionText;
    [SerializeField] private Image questionImage; // New image component
    [SerializeField] private WordData[] answerWordArray;
    [SerializeField] private GameObject gameOver;
    [SerializeField] private Button[] keyboardButtons;
    [SerializeField] private Button backspaceButton;

    // Updated default questions to include image paths
    private KeyboardQuestionList defaultQuestionData = new KeyboardQuestionList 
    {
        questions = new List<KeyboardQuestion> 
        {
            new KeyboardQuestion { 
                questionText = "Default Question 1", 
                answer = "ANSWER1", 
                imagePath = "DefaultImages/default_image1" // Path to default image
            },
            new KeyboardQuestion { 
                questionText = "Default Question 2", 
                answer = "ANSWER2", 
                imagePath = "DefaultImages/default_image2" // Path to another default image
            }
        }
    };
    private KeyboardQuestionList questionData;
    private int currentQuestionIndex = 0;
    private string currentAnswer;
    private int currentAnswerIndex = 0;
    private GameStatus gameStatus = GameStatus.Playing;
    private List<int> selectedKeyIndex;

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);

        selectedKeyIndex = new List<int>();
        
        StartCoroutine(LoadQuestionData());
        
        SetupKeyboardListeners();
    }

    private IEnumerator LoadQuestionData()
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "classic_questions.json");
        
        using (UnityWebRequest www = UnityWebRequest.Get(path))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try 
                {
                    string jsonText = www.downloadHandler.text;
                    questionData = JsonUtility.FromJson<KeyboardQuestionList>(jsonText);
                    
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
        }

        SetQuestion();
    }

    private void SetQuestion()
    {
        // Check if we have questions left
        if (currentQuestionIndex >= questionData.questions.Count)
        {
            gameOver.SetActive(true);
            return;
        }

        // Reset for new question
        currentAnswerIndex = 0;
        selectedKeyIndex.Clear();
        
        // Get current question
        KeyboardQuestion currentQuestion = questionData.questions[currentQuestionIndex];
        questionText.text = currentQuestion.questionText;
        currentAnswer = currentQuestion.answer.ToUpper();

        // Load and display the question image
        StartCoroutine(LoadQuestionImage(currentQuestion.imagePath));

        ResetQuestion();
        
        currentQuestionIndex++;
        gameStatus = GameStatus.Playing;
    }

    private IEnumerator LoadQuestionImage(string imagePath)
    {
        // Disable image initially
        questionImage.gameObject.SetActive(false);

        // Attempt to load image from StreamingAssets
        string fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, imagePath);
        
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(fullPath))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                
                // Create sprite from texture
                Sprite sprite = Sprite.Create(
                    texture, 
                    new Rect(0, 0, texture.width, texture.height), 
                    new Vector2(0.5f, 0.5f)
                );

                // Set sprite to image component
                questionImage.sprite = sprite;
                questionImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"Failed to load image: {imagePath}");
                // Optionally hide the image if loading fails
                questionImage.gameObject.SetActive(false);
            }
        }
    }

    

    private void SetupKeyboardListeners()
    {
        // Attach click listeners to all keyboard buttons
        for (int i = 0; i < keyboardButtons.Length; i++)
        {
            int index = i; // Capture the current index for the lambda
            keyboardButtons[i].onClick.AddListener(() => OnKeyPressed(keyboardButtons[index]));
        }

        // Add listener for backspace button
        if (backspaceButton != null)
        {
            backspaceButton.onClick.AddListener(ResetLastWord);
        }
    }

    

    

    private void ResetQuestion()
    {
        // Reset answer word array display
        for(int i = 0; i < answerWordArray.Length; i++)
        {
            answerWordArray[i].gameObject.SetActive(true);
            answerWordArray[i].SetChar('_');
        }

        // Hide extra answer slots if answer is shorter than array
        for(int i = currentAnswer.Length; i < answerWordArray.Length; i++)
        {
            answerWordArray[i].gameObject.SetActive(false);
        }

        // Ensure all keyboard buttons are visible
        foreach (Button button in keyboardButtons)
        {
            button.gameObject.SetActive(true);
        }
    }

    private void OnKeyPressed(Button keyButton)
    {
        if (gameStatus == GameStatus.Next || currentAnswerIndex >= currentAnswer.Length) return;

        // Get the text of the pressed key
        string keyPressed = keyButton.GetComponentInChildren<Text>().text;

        // Track the index of the pressed key
        selectedKeyIndex.Add(System.Array.IndexOf(keyboardButtons, keyButton));

        // Set the character in the answer array
        answerWordArray[currentAnswerIndex].SetChar(keyPressed[0]);
        currentAnswerIndex++;

        // Check for answer completion
        if (currentAnswerIndex == currentAnswer.Length)
        {
            CheckAnswer();
        }
    }

    private void ResetLastWord()
    {
        // Similar to JumbledLettersManager's ResetLastWord method
        if(selectedKeyIndex.Count > 0)
        {
            // Get the last selected key index
            int index = selectedKeyIndex[selectedKeyIndex.Count - 1];
            
            // Remove the last selected index
            selectedKeyIndex.RemoveAt(selectedKeyIndex.Count - 1);
            
            // Decrement answer index and clear the character
            currentAnswerIndex--;
            answerWordArray[currentAnswerIndex].SetChar('_');
        }
    }

    private void CheckAnswer()
    {
        bool correctAnswer = true;

        // Verify each character
        for(int i = 0; i < currentAnswer.Length; i++)
        {
            if(char.ToUpper(currentAnswer[i]) != char.ToUpper(answerWordArray[i].charValue))
            {
                correctAnswer = false;
                break;
            }
        }

        if(correctAnswer)
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
            // Reset the current input
            for (int i = 0; i < currentAnswerIndex; i++)
            {
                answerWordArray[i].SetChar('_');
            }
            
            currentAnswerIndex = 0;
            selectedKeyIndex.Clear();
        }
    }
}

// Existing data structures
[System.Serializable]
public class KeyboardQuestion
{
    public string questionText;
    public string answer;
    public string imagePath; // New field for image path
}

[System.Serializable]
public class KeyboardQuestionList
{
    public List<KeyboardQuestion> questions;
}
