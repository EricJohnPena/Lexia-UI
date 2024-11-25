// CrosswordGridManager.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosswordGridManager : MonoBehaviour
{
    public GameObject gridCellPrefab;
    public Transform gridContainer;
    public Text cluesPanelText;
    public Text currentClueText;
    public int gridSize = 10;
    
    private GridCell[,] gridCells;
    private LevelManager levelManager;
    public string levelFileName = "level1.json";
    
    private GridCell selectedCell;
    private List<GridCell> highlightedCells = new List<GridCell>();
    private bool isHorizontalInput = true;
    private WordPlacement currentWord;
    private CrosswordLevel currentLevel;
    private Dictionary<WordPlacement, int> wordNumbers = new Dictionary<WordPlacement, int>();

    void Start()
    {
        levelManager = GetComponent<LevelManager>();
        
        if (levelManager == null)
        {
            Debug.LogError("LevelManager not found! Ensure it is attached to the same GameObject.");
            return;
        }

        currentLevel = levelManager.LoadLevel(levelFileName);

        if (currentLevel == null)
        {
            Debug.LogError("Failed to load level data!");
            return;
        }

        if (currentLevel.wordClues == null || currentLevel.wordClues.Count == 0)
        {
            Debug.LogError("Word clues in level data are null or empty!");
        }

        GenerateGrid();
        AssignWordNumbers();
        PlaceWords(currentLevel.fixedLayout);
        DisplayClues(currentLevel.wordClues);
        
        if (currentClueText != null)
        {
            currentClueText.text = "Tap a cell to begin";
        }
        
        TouchScreenKeyboard.hideInput = true;
    }

    void Update()
    {
        if (selectedCell == null)
    {
        Debug.Log("No selected cell.");
        return;
    }
        HandleKeyboardInput();
    }

    void HandleKeyboardInput()
    {
        if (selectedCell == null) return;

        foreach (char c in Input.inputString)
        {
            if (char.IsLetter(c))
            {
                InputLetter(char.ToUpper(c));
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            RemoveLastLetter();
        }
    }

    void InputLetter(char letter)
    {
        if (selectedCell == null || currentWord == null) return;

        selectedCell.SetInputLetter(letter);

        // Move to next cell in word
        MoveCursorToNextCell();

        // Check if word is complete
        if (IsWordComplete())
        {
            CheckWord();
        }
    }

    bool IsWordComplete()
    {
        foreach (var cell in GetCurrentWordCells())
        {
            if (cell.GetCurrentLetter() == ' ')
                return false;
        }
        return true;
    }

    List<GridCell> GetCurrentWordCells()
    {
        List<GridCell> cells = new List<GridCell>();
        if (currentWord == null) return cells;

        for (int i = 0; i < currentWord.word.Length; i++)
        {
            int row = currentWord.startRow + (currentWord.horizontal ? 0 : i);
            int col = currentWord.startCol + (currentWord.horizontal ? i : 0);
            cells.Add(gridCells[row, col]);
        }
        return cells;
    }

    void MoveCursorToNextCell()
    {
        if (currentWord == null) return;

        int currentIndex = GetCellIndexInWord(selectedCell);
        if (currentIndex < currentWord.word.Length - 1)
        {
            int nextRow = currentWord.startRow + (currentWord.horizontal ? 0 : currentIndex + 1);
            int nextCol = currentWord.startCol + (currentWord.horizontal ? currentIndex + 1 : 0);
            SelectCell(gridCells[nextRow, nextCol]);
        }
    }

    void MoveCursorToPreviousCell()
    {
        if (currentWord == null) return;

        int currentIndex = GetCellIndexInWord(selectedCell);
        if (currentIndex > 0)
        {
            int prevRow = currentWord.startRow + (currentWord.horizontal ? 0 : currentIndex - 1);
            int prevCol = currentWord.startCol + (currentWord.horizontal ? currentIndex - 1 : 0);
            SelectCell(gridCells[prevRow, prevCol]);
        }
    }

    int GetCellIndexInWord(GridCell cell)
    {
        if (currentWord == null) return -1;
        
        return currentWord.horizontal ? 
            cell.Col - currentWord.startCol :
            cell.Row - currentWord.startRow;
    }

    void RemoveLastLetter()
    {
        if (selectedCell != null)
        {
            selectedCell.SetInputLetter(' ');
            MoveCursorToPreviousCell();
        }
    }

    void CheckWord()
    {
        string enteredWord = "";
        var cells = GetCurrentWordCells();
        
        foreach (var cell in cells)
        {
            enteredWord += cell.GetCurrentLetter();
        }

        if (enteredWord == currentWord.word.ToUpper())
        {
            // Word is correct - lock it in
            foreach (var cell in cells)
            {
                cell.LockCell();
            }
            
            // Move to next word if available
            SelectNextWord();
        }
        else
        {
            // Word is incorrect - flash cells red
            foreach (var cell in cells)
            {
                cell.FlashRed(0.5f);
                cell.SetInputLetter(' ');
            }
            
            // Return to first cell of word
            SelectCell(cells[0]);
        }
    }

    void SelectNextWord()
    {
        var nextWord = FindNextWord();
        if (nextWord != null)
        {
            currentWord = nextWord;
            SelectCell(gridCells[nextWord.startRow, nextWord.startCol]);
        }
        else
        {
            CheckPuzzleCompletion();
        }
    }

    WordPlacement FindNextWord()
    {
        // Find first unlocked word
        foreach (var placement in currentLevel.fixedLayout)
        {
            bool isUnlocked = false;
            for (int i = 0; i < placement.word.Length; i++)
            {
                int row = placement.startRow + (placement.horizontal ? 0 : i);
                int col = placement.startCol + (placement.horizontal ? i : 0);
                
                if (!gridCells[row, col].IsLocked)
                {
                    isUnlocked = true;
                    break;
                }
            }
            
            if (isUnlocked)
                return placement;
        }
        return null;
    }

    void CheckPuzzleCompletion()
    {
        bool isComplete = true;
        foreach (var cell in gridCells)
        {
            if (cell.gameObject.activeSelf && !cell.IsLocked)
            {
                isComplete = false;
                break;
            }
        }

        if (isComplete)
        {
            currentClueText.text = "Congratulations! Puzzle Complete!";
        }
    }

    void AssignWordNumbers()
    {
        int currentNumber = 1;
        bool[,] numberedCells = new bool[gridSize, gridSize];

        foreach (var placement in currentLevel.fixedLayout)
        {
            if (!numberedCells[placement.startRow, placement.startCol])
            {
                wordNumbers[placement] = currentNumber;
                numberedCells[placement.startRow, placement.startCol] = true;
                currentNumber++;
            }
        }
    }

    void SelectCell(GridCell cell)
    {
        selectedCell = cell;
        ClearHighlights();
        HighlightWord(currentWord);
        DisplayCurrentClue(currentWord);
    }

    void GenerateGrid()
    {
        gridCells = new GridCell[gridSize, gridSize];
        float cellSize = 70f;

        Vector2 gridStartPos = new Vector2(-gridSize * cellSize / 2, gridSize * cellSize / 2);

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                GameObject newCell = Instantiate(gridCellPrefab, gridContainer);
                newCell.name = $"Cell ({row}, {col})";

                GridCell cellScript = newCell.GetComponent<GridCell>();
                cellScript.Initialize(row, col);
                cellScript.SetActive(false);
                cellScript.OnCellClicked += HandleCellClick;

                RectTransform rectTransform = newCell.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = gridStartPos + new Vector2(col * cellSize, -row * cellSize);

                gridCells[row, col] = cellScript;
            }
        }
    }

    void HandleCellClick(GridCell cell)
    {
        ClearHighlights();
        
        WordPlacement horizontalWord = FindWordAtCell(cell.Row, cell.Col, true);
        WordPlacement verticalWord = FindWordAtCell(cell.Row, cell.Col, false);

        if (horizontalWord != null && verticalWord != null)
        {
            if (currentWord != null && currentWord.horizontal && verticalWord != null)
            {
                currentWord = verticalWord;
                isHorizontalInput = false;
            }
            else
            {
                currentWord = horizontalWord;
                isHorizontalInput = true;
            }
        }
        else
        {
            currentWord = horizontalWord ?? verticalWord;
            isHorizontalInput = currentWord?.horizontal ?? true;
        }
        currentWord = FindWordAtCell(cell.Row, cell.Col, isHorizontalInput);
    if (currentWord != null)
    {
        SelectCell(cell); // This sets the selectedCell and highlights the word.
    }
    else
    {
        Debug.LogWarning("No word found for the selected cell.");
    }
        if (currentWord != null)
        {
            SelectCell(cell);
        }
    }

    WordPlacement FindWordAtCell(int row, int col, bool horizontal)
    {
        foreach (var placement in currentLevel.fixedLayout)
        {
            // Only check words with matching orientation
            if (placement.horizontal != horizontal) continue;

            for (int i = 0; i < placement.word.Length; i++)
            {
                int checkRow = placement.startRow + (placement.horizontal ? 0 : i);
                int checkCol = placement.startCol + (placement.horizontal ? i : 0);
                
                if (checkRow == row && checkCol == col)
                {
                    return placement;
                }
            }
        }
        return null;
    }

    void DisplayCurrentClue(WordPlacement word)
    {
        if (currentClueText != null && word != null)
        {
            // Find the corresponding clue
            WordClue clue = currentLevel.wordClues.Find(c => c.word.ToUpper() == word.word.ToUpper());
            if (clue != null)
            {
                string direction = word.horizontal ? "Across" : "Down";
                currentClueText.text = $"{direction}: {clue.clue}";
            }
        }
    }

    void HighlightWord(WordPlacement word)
    {
        for (int i = 0; i < word.word.Length; i++)
        {
            int row = word.startRow + (word.horizontal ? 0 : i);
            int col = word.startCol + (word.horizontal ? i : 0);
            
            GridCell cell = gridCells[row, col];
            cell.Highlight();
            highlightedCells.Add(cell);
        }
    }

    void ClearHighlights()
    {
        foreach (var cell in highlightedCells)
        {
            cell.ClearHighlight();
        }
        highlightedCells.Clear();
    }

    void PlaceWords(List<WordPlacement> fixedLayout)
    {
        foreach (var placement in fixedLayout)
        {
            for (int i = 0; i < placement.word.Length; i++)
            {
                int row = placement.startRow + (placement.horizontal ? 0 : i);
                int col = placement.startCol + (placement.horizontal ? i : 0);

                GridCell cell = gridCells[row, col];
                cell.SetCorrectLetter(placement.word[i]);
                cell.SetActive(true);
                
                // Set number for first cell of word
                if (i == 0 && wordNumbers.ContainsKey(placement))
                {
                    cell.SetNumber(wordNumbers[placement]);
                }
            }
        }
    }

    void DisplayClues(List<WordClue> wordClues)
    {
        if (cluesPanelText != null)
        {
            string acrossClues = "ACROSS:\n";
            string downClues = "\nDOWN:\n";
            
            // First, match clues with their placements and sort them
            var sortedClues = new Dictionary<WordPlacement, WordClue>();
            foreach (var placement in currentLevel.fixedLayout)
            {
                var clue = wordClues.Find(c => c.word.ToUpper() == placement.word.ToUpper());
                if (clue != null)
                {
                    sortedClues.Add(placement, clue);
                }
            }

            // Now build the clue text
            int acrossNum = 1;
            int downNum = 1;
            foreach (var pair in sortedClues)
            {
                if (pair.Key.horizontal)
                {
                    acrossClues += $"{acrossNum++}. {pair.Value.clue}\n";
                }
                else
                {
                    downClues += $"{downNum++}. {pair.Value.clue}\n";
                }
            }

            cluesPanelText.text = acrossClues + downClues;
        }
    }
}
