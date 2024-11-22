using UnityEngine;
using UnityEngine.UI;

public class GridCell : MonoBehaviour
{
    public int Row { get; private set; }
    public int Col { get; private set; }
    private char correctLetter = ' ';
    private Text cellText;

    void Awake()
    {
        cellText = GetComponentInChildren<Text>();
    }

    public void Initialize(int row, int col)
    {
        Row = row;
        Col = col;
        SetLetter(' '); // Initialize with empty space
    }

    public void SetCorrectLetter(char letter)
    {
        correctLetter = letter;
        SetLetter(letter); // Display correct letter for now (adjust for gameplay later)
    }

    public char GetLetter()
    {
        return correctLetter;
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    private void SetLetter(char letter)
    {
        if (cellText != null)
        {
            cellText.text = letter.ToString();
        }
    }
}
