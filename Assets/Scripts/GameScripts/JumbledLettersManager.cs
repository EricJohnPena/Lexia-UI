using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class JumbledLettersManager : MonoBehaviour
{
    public static JumbledLettersManager instance;
    [SerializeField]
    private JLDataScriptable question;
    [SerializeField]
    private Text questionText;
    [SerializeField]
    private GameObject gameOver;
    [SerializeField]
    private WordData[] answerWordArray;
    [SerializeField]
    private WordData[] optionWordArray;

    private char[] charArray = new char[12];
    private int currentAnswerIndex = 0;
    private bool correctAnswer = true;
    private List<int> selectedWordIndex;
    private int currentQuestionIndex = 0;
    private GameStatus gameStatus = GameStatus.Playing;
    private string answerWord;

    private void Awake(){
        if(instance == null) instance = this;
        else Destroy(gameObject);

        selectedWordIndex = new List<int>();
    }

    private void Start(){
        SetQuestion();
    }

    private void SetQuestion(){
        currentAnswerIndex = 0;
        selectedWordIndex.Clear();
         questionText.text = question.jlQuestions[currentQuestionIndex].questionText;
         answerWord = question.jlQuestions[currentQuestionIndex].answer;
         ResetQuestion();
        for (int i = 0; i < answerWord.Length; i++){
            charArray[i] = char.ToUpper(answerWord[i]);
        }

        for (int i = answerWord.Length; i < optionWordArray.Length; i++){
            charArray[i] = (char)UnityEngine.Random.Range(65, 91);
        }
        charArray = ShuffleList.ShuffleListItems<char>(charArray.ToList()).ToArray();

        for (int i = 0; i < optionWordArray.Length; i++){
            optionWordArray[i].SetChar(charArray[i]);
        }          

        currentQuestionIndex++; 
        gameStatus = GameStatus.Playing; 
    }

    public void SelectedOption(WordData wordData){
        if(gameStatus == GameStatus.Next || currentAnswerIndex >= answerWord.Length) return;

        selectedWordIndex.Add(wordData.transform.GetSiblingIndex());
        wordData.gameObject.SetActive(false);
        answerWordArray[currentAnswerIndex].SetChar(wordData.charValue);
        currentAnswerIndex++;

        if (currentAnswerIndex == answerWord.Length){
            correctAnswer = true;

            for(int i = 0; i < answerWord.Length; i++){
                if(char.ToUpper(answerWord[i]) != char.ToUpper(answerWordArray[i].charValue)){
                    correctAnswer = false;
                    break;
                }
            }

            if(correctAnswer){
                Debug.Log("Answer correct!");
                gameStatus = GameStatus.Next;
                if (currentQuestionIndex < question.jlQuestions.Count){
                    Invoke("SetQuestion", 0.5f);
                }else{
                    gameOver.SetActive(true);
                }
            }else{
                Debug.Log("Answer incorrect!");
            }

        }
    }

    private void ResetQuestion(){
        for(int i = 0; i < answerWordArray.Length; i++){
            answerWordArray[i].gameObject.SetActive(true);
            answerWordArray[i].SetChar('_');
        }

        for(int i = answerWord.Length; i < answerWordArray.Length; i++){
            answerWordArray[i].gameObject.SetActive(false);
        }
        for(int i = 0; i < optionWordArray.Length; i++){
            optionWordArray[i].gameObject.SetActive(true);
        }
    }

    public void ResetLastWord(){
        if(selectedWordIndex.Count > 0){

            int index = selectedWordIndex[selectedWordIndex.Count - 1];
            optionWordArray[index].gameObject.SetActive(true);
            selectedWordIndex.RemoveAt(selectedWordIndex.Count - 1);
            currentAnswerIndex --;
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

[System.Serializable]
public class QuestionData {
    public string questionText;
    public string answer;
}

public enum GameStatus{
    Playing, 
    Next
}
