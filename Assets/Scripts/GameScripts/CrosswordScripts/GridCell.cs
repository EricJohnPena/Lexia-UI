// GridCell.cs
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridCell : MonoBehaviour, IPointerClickHandler
{
    public int Row { get; private set; }
    public int Col { get; private set; }

    [SerializeField]
    private Text letterText;

    [SerializeField]
    private Text numberText;
    private Image cellBackground;
    private char correctLetter = ' ';
    public bool IsLocked { get; private set; }

    public event Action<GridCell> OnCellClicked;

    private Color defaultColor = Color.white;
    private Color highlightColor = new Color(0.8f, 0.9f, 1f);
    private Color lockedColor = new Color(0.9f, 0.9f, 0.9f);
    private Color wrongColor = new Color(1f, 0.8f, 0.8f);

    private Animator animator;

    private bool isHinted = false;
    private bool isCorrect = false;

    void Awake()
    {
        cellBackground = GetComponent<Image>();
        animator = GetComponent<Animator>();

        if (letterText == null)
            letterText = transform.Find("LetterText")?.GetComponent<Text>();
        if (numberText == null)
            numberText = transform.Find("NumberText")?.GetComponent<Text>();

        if (letterText == null || numberText == null)
        {
            Debug.LogError("Missing required Text components on GridCell!");
        }
    }

    public void Initialize(int row, int col)
    {
        Row = row;
        Col = col;
        IsLocked = false;
        SetInputLetter(' ');
        SetNumber(0);
        ClearHighlight();
    }

    public void SetNumber(int number)
    {
        if (numberText != null)
        {
            numberText.text = number > 0 ? number.ToString() : "";
            numberText.gameObject.SetActive(number > 0);
        }
    }

    public void SetCorrectLetter(char letter)
    {
        correctLetter = char.ToUpper(letter);
        SetInputLetter(' ');
    }

    public void SetInputLetter(char letter, bool isHint = false)
    {
        if (!IsLocked && letterText != null)
        {
            letterText.text = char.ToUpper(letter).ToString();
            if (letter != ' ')
            {
                PlayTypingAnimation();
            }
            isHinted = isHint;
            UpdateTextStyle();
        }
    }

    private void UpdateTextStyle()
    {
        if (letterText != null)
        {
            letterText.fontStyle = (isHinted || isCorrect) ? FontStyle.Bold : FontStyle.Normal;
        }
    }

    void PlayTypingAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Type");
        }
    }

    public char GetCurrentLetter()
    {
        return letterText != null ? (letterText.text.Length > 0 ? letterText.text[0] : ' ') : ' ';
    }

    public void LockCell()
    {
        IsLocked = true;
        letterText.text = correctLetter.ToString();
        cellBackground.color = lockedColor;
        isCorrect = true;
        UpdateTextStyle();
        PlayCorrectAnimation();
    }

    void PlayCorrectAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Correct");
        }
    }

    public void Highlight()
    {
        if (!IsLocked)
        {
            cellBackground.color = highlightColor;
        }
    }

    public void ClearHighlight()
    {
        if (!IsLocked && cellBackground != null) // Ensure the Image component is not null
        {
            cellBackground.color = defaultColor;
        }
    }

    public void FlashRed(float duration)
    {
        if (!IsLocked)
        {
            StartCoroutine(FlashColorRoutine(wrongColor, duration));
            if (animator != null)
            {
                animator.SetTrigger("Wrong");
            }
        }
    }

    private IEnumerator FlashColorRoutine(Color flashColor, float duration)
    {
        Color originalColor = cellBackground.color;
        cellBackground.color = flashColor;
        yield return new WaitForSeconds(duration);
        if (!IsLocked)
        {
            cellBackground.color = originalColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsLocked)
        {
            Debug.Log($"GridCell clicked: Row {Row}, Col {Col}");
            OnCellClicked?.Invoke(this);
        }
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}
