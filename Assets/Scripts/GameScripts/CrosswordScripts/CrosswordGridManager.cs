using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosswordGridManager : MonoBehaviour
{
    public GameObject gridCellPrefab;
    public Transform gridContainer;
    public Text cluesPanelText;

    public int gridSize = 10;
    private GridCell[,] gridCells;
    private LevelManager levelManager;
    public string levelFileName = "level1.json"; // Set this dynamically for different levels

    void Start()
    {
        levelManager = GetComponent<LevelManager>();

    if (levelManager == null)
    {
        Debug.LogError("LevelManager not found! Ensure it is attached to the same GameObject.");
        return;
    }

    CrosswordLevel level = levelManager.LoadLevel(levelFileName);

    if (level == null)
    {
        Debug.LogError("Failed to load level data!");
        return;
    }

    if (level.wordClues == null || level.wordClues.Count == 0)
    {
        Debug.LogError("Word clues in level data are null or empty!");
    }

    GenerateGrid();
    PlaceWords(level.fixedLayout);
    DisplayClues(level.wordClues);
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

                RectTransform rectTransform = newCell.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = gridStartPos + new Vector2(col * cellSize, -row * cellSize);

                gridCells[row, col] = cellScript;
            }
        }
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
            }
        }
    }

    void DisplayClues(List<WordClue> wordClues)
{
    string cluesText = "";
    int i = 1;
    foreach (var clue in wordClues)
    {
        cluesText += $"{i++}. {clue.clue}\n";  // Display word and clue
    }

    cluesPanelText.text = cluesText;
}

}
