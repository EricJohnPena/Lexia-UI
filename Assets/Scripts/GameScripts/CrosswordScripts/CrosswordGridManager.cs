// CrosswordGridManager.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CrosswordGridManager : MonoBehaviour
{
    private Trie wordTrie;
    public GameObject gridCellPrefab;
    public Transform gridContainer;
    public Text cluesPanelText;
    public Text currentClueText;
    public int gridSize = 15;
    private GameProgressHandler gameProgressHandler; // Added declaration

    private GridCell[,] gridCells;
    private LevelManager levelManager;
    public string levelFileName = "level1.json";
    public string apiUrl = $"{Web.BaseApiUrl}getCrosswordData.php";

    private GridCell selectedCell;
    private List<GridCell> highlightedCells = new List<GridCell>();
    private bool isHorizontalInput = true;
    private WordPlacement currentWord;
    private CrosswordLevel currentLevel;
    private Dictionary<WordPlacement, int> wordNumbers = new Dictionary<WordPlacement, int>();
    private CrosswordKeyboard crosswordKeyboard;
    private bool isLessonCompleted = false;
    private bool isRefreshing = false;
    public GameObject gameOver;
    public TimerManager timerManager; // Assign in the Inspector

    [SerializeField]
    private Button hintButton; // Assign the hint button in the Inspector

    [SerializeField]
    private Text hintCounterText; // Assign the Text UI in the Inspector

    private int hintCounter = 3; // Maximum number of hints allowed
    private int correctAnswers = 0;
    private int totalAttempts = 0;
    internal static object instance;

    private HashSet<GridCell> hintedCells = new HashSet<GridCell>(); // Add this field to track hinted cells

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

        // Initialize GameProgressHandler reference
        gameProgressHandler = FindObjectOfType<GameProgressHandler>();
        if (gameProgressHandler == null)
        {
            Debug.LogError("GameProgressHandler not found in the scene.");
        }
        // Initialize the Trie and insert words
        wordTrie = new Trie();
        foreach (var placement in currentLevel.fixedLayout)
        {
            wordTrie.Insert(placement.word.ToUpper());
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
        crosswordKeyboard = FindObjectOfType<CrosswordKeyboard>();

        if (crosswordKeyboard == null)
        {
            Debug.LogWarning("CrosswordKeyboard not found in the scene!");
        }
        else
        {
            crosswordKeyboard.UpdateButtonColorsBySubject();
        }

        if (hintButton != null)
        {
            // Set outline color based on subject id
            var outline = hintButton.GetComponent<UnityEngine.UI.Outline>();
            if (outline != null)
            {
                Color outlineColor = Color.white;
                if (LessonsLoader.subjectId == 1) // English
                {
                    outlineColor = new Color32(0, 102, 204, 255); // Example: blue
                }
                else if (LessonsLoader.subjectId == 2) // Science
                {
                    outlineColor = new Color32(0, 153, 0, 255); // Example: green
                }
                outline.effectColor = outlineColor;
            }
            hintButton.onClick.AddListener(RevealHint);
        }

        UpdateHintCounterUI();

        // Do not load crossword data here; it will be loaded after checking lesson completion
    }

    private int CalculateWordDifficulty(string word)
    {
        if (string.IsNullOrEmpty(word))
            return 0;

        int length = word.Length;

        if (length <= 3)
            return 1;
        else if (length <= 5)
            return 3;
        else if (length <= 7)
            return 5;
        else if (length <= 9)
            return 7;
        else
            return 10;
    }

    void OnEnable()
    {
        Debug.Log("Crossword game enabled.");

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

            int module_number = int.Parse(LessonsLoader.moduleNumber);
            int gameModeId = 3; // Assuming 3 is the ID for Crossword mode

            StartCoroutine(
                CheckLessonCompletion(
                    int.Parse(PlayerPrefs.GetString("User ID")),
                    module_number,
                    gameModeId,
                    subjectId
                )
            );
        }
    }

    private IEnumerator CheckLessonCompletion(
        int studentId,
        int module_number,
        int gameModeId,
        int subjectId
    )
    {
        if (isRefreshing)
        {
            Debug.LogWarning("CheckLessonCompletion is already running. Skipping...");
            yield break;
        }

        // Show loading screen at the start of lesson completion check
        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.ShowLoadingScreen();
        }

        isRefreshing = true; // Mark as running
        Debug.Log(
            $"CheckLessonCompletion called with studentId={studentId}, module_number={module_number}, gameModeId={gameModeId}, subjectId={subjectId}"
        );

        if (studentId <= 0 || module_number <= 0 || gameModeId <= 0)
        {
            Debug.LogError(
                $"Invalid parameters: studentId={studentId}, module_number={module_number}, gameModeId={gameModeId}, subjectId={subjectId}"
            );
            isRefreshing = false; // Reset flag
            // Hide loading screen if there's an error
            if (GameLoadingManager.Instance != null)
            {
                GameLoadingManager.Instance.HideLoadingScreen();
            }
            yield break;
        }

        string url =
            $"{Web.BaseApiUrl}checkLessonCompletion.php?student_id={studentId}&module_number={module_number}&game_mode_id={gameModeId}&subject_id={subjectId}";
        Debug.Log("Checking lesson completion from URL: " + url);
        int maxRetries = 3;
        int attempt = 0;
        float retryDelay = 2f; // seconds

        while (attempt < maxRetries)
        {
            attempt++;
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
                        break; // Exit loop on success
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Error parsing lesson completion response: " + e.Message);
                        isLessonCompleted = false;
                    }
                }
                else
                {
                    Debug.LogError(
                        $"Failed to check lesson completion: {www.error} (Attempt {attempt}/{maxRetries})"
                    );
                    if (attempt >= maxRetries)
                    {
                        isLessonCompleted = false;
                    }
                    else
                    {
                        yield return new WaitForSeconds(retryDelay);
                    }
                }
            }
        }

        isRefreshing = false; // Reset flag

        if (isLessonCompleted)
        {
            // Hide loading screen before handling lesson state
            if (GameLoadingManager.Instance != null)
            {
                GameLoadingManager.Instance.HideLoadingScreen();
            }
            HandleLessonState();
        }
        else
        {
            // Don't hide loading screen here as LoadCrosswordData will handle it
            StartCoroutine(
                LoadCrosswordData(LessonsLoader.subjectId, int.Parse(LessonsLoader.moduleNumber))
            );
        }
    }

    private void HandleLessonState()
    {
        if (isLessonCompleted)
        {
            Debug.Log("Lesson is already completed.");
            timerManager?.StopTimer();
            currentClueText.text = "Lesson Completed!";
            gameOver.SetActive(true);
        }
        timerManager?.StopTimer();
    }

    public void RefreshCrosswordData()
    {
        Debug.Log("Refreshing crossword data...");

        // Reset game state
        ClearGrid();
        ResetCrosswordGameState();
        DisplayEmptyMessage();

        // Update button colors based on subject
        if (crosswordKeyboard != null)
        {
            crosswordKeyboard.UpdateButtonColorsBySubject();
        }
        if (hintButton != null)
        {
            var outline = hintButton.GetComponent<UnityEngine.UI.Outline>();
            if (outline != null)
            {
                Color outlineColor = Color.white;
                if (LessonsLoader.subjectId == 1) // English
                {
                    outlineColor = new Color32(0, 102, 204, 255); // Example: blue
                }
                else if (LessonsLoader.subjectId == 2) // Science
                {
                    outlineColor = new Color32(0, 153, 0, 255); // Example: green
                }
                outline.effectColor = outlineColor;
            }
        }

        // Reload crossword data
        StartCoroutine(
            LoadCrosswordData(LessonsLoader.subjectId, int.Parse(LessonsLoader.moduleNumber))
        );
    }

    private void ResetCrosswordGameState()
    {
        Debug.Log("Resetting crossword game state...");

        // Reset variables
        selectedCell = null;
        highlightedCells.Clear();
        currentWord = null;
        wordNumbers.Clear();
        isRefreshing = false;
        isLessonCompleted = false;
        correctAnswers = 0;
        totalAttempts = 0;
        hintCounter = 3;
        hintedCells.Clear(); // Clear the hinted cells when resetting the game

        // Reset UI
        if (cluesPanelText != null)
        {
            cluesPanelText.text = "";
        }

        if (currentClueText != null)
        {
            currentClueText.text = "";
        }

        ClearGrid();

        if (gameOver != null)
        {
            gameOver.SetActive(false);
        }

        UpdateHintCounterUI();
    }

    private IEnumerator LoadCrosswordData(int subjectId, int module_number)
    {
        // Show loading screen
        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.ShowLoadingScreen();
        }

        // Reset game state before loading new data
        ResetGameState();

        string url = $"{apiUrl}?subject_id={subjectId}&module_id={module_number}";
        Debug.Log("Fetching Crossword questions from URL: " + url);
        int maxRetries = 3;
        int attempt = 0;
        float retryDelay = 2f; // seconds

        while (attempt < maxRetries)
        {
            attempt++;
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string jsonText = www.downloadHandler.text;
                        Debug.Log("Raw JSON Response: " + jsonText);

                        currentLevel = JsonUtility.FromJson<CrosswordLevel>(jsonText);

                        if (currentLevel == null || currentLevel.fixedLayout == null || currentLevel.fixedLayout.Count == 0)
                        {
                            Debug.LogWarning("No crossword data received from the server. Displaying an empty crossword.");
                            timerManager?.StopTimer();
                            ClearGrid();
                            DisplayEmptyMessage();
                            break;
                        }

                        Debug.Log("Successfully loaded crossword data.");
                        
                        // Clear and rebuild word trie
                        wordTrie = new Trie();
                        foreach (var placement in currentLevel.fixedLayout)
                        {
                            wordTrie.Insert(placement.word.ToUpper());
                        }

                        if (currentLevel.fixedLayout.Count > 0)
                        {
                            timerManager?.StartTimer(); // Start the timer
                        }

                        GenerateGrid();
                        AssignWordNumbers();
                        PlaceWords(currentLevel.fixedLayout);
                        DisplayClues(currentLevel.wordClues);

                        if (currentClueText != null)
                        {
                            currentClueText.text = "Tap a cell to begin";
                        }
                        break; // Exit loop on success
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Error parsing JSON: " + e.Message);
                        if (attempt >= maxRetries)
                        {
                            timerManager?.StopTimer();
                            ClearGrid();
                            DisplayEmptyMessage();
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Failed to fetch crossword data: {www.error} (Attempt {attempt}/{maxRetries})");
                    if (attempt >= maxRetries)
                    {
                        timerManager?.StopTimer();
                        ClearGrid();
                        DisplayEmptyMessage();
                    }
                    else
                    {
                        yield return new WaitForSeconds(retryDelay);
                    }
                }
            }
        }

        // Hide loading screen
        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.HideLoadingScreen();
        }
    }

    private void ClearGrid()
    {
        if (gridCells != null)
        {
            foreach (var cell in gridCells)
            {
                if (cell != null)
                {
                    // Unsubscribe from events to avoid memory leaks
                    cell.OnCellClicked -= HandleCellClick;
                    Destroy(cell.gameObject); // Destroy the cell GameObject
                }
            }
        }

        gridCells = null; // Clear the gridCells array to ensure a fresh start
    }

    private void DisplayEmptyMessage()
    {
        if (cluesPanelText != null)
        {
            cluesPanelText.text = "Loading crossword data...";
        }

        if (currentClueText != null)
        {
            currentClueText.text = "";
        }
    }

    void Update()
    {
        if (selectedCell == null)
        {
            //Debug.Log("No selected cell.");
            return;
        }
        HandleKeyboardInput();
    }

    void HandleKeyboardInput()
    {
        if (selectedCell == null)
            return;

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

    // Method to handle key input from on-screen keyboard
    public void HandleKeyInput(char letter)
    {
        if (selectedCell == null || currentWord == null)
            return;

        InputLetter(char.ToUpper(letter));
    }

    // Method to handle backspace from on-screen keyboard
    public void HandleBackspace()
    {
        RemoveLastLetter();
    }

    // Method to handle space input (optional, depending on your game design)
    public void HandleSpaceInput()
    {
        // You can define how space should be handled
        // For example, move to next word or clear current input
        Debug.Log("Space input handled");
    }

    void InputLetter(char letter)
    {
        if (selectedCell == null || currentWord == null)
            return;

        // Skip cells that are already locked or have a letter
        while (selectedCell != null && selectedCell.GetCurrentLetter() != ' ')
        {
            MoveCursorToNextCell();
        }

        if (selectedCell == null || selectedCell.GetCurrentLetter() != ' ')
        {
            Debug.Log("No available cells to place the letter.");
            return;
        }

        selectedCell.SetInputLetter(letter, false); // Set isHint to false for user input

        // Move to the next cell in the word
        MoveCursorToNextCell();

        // Check if the word is complete
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
        if (currentWord == null)
            return cells;

        for (int i = 0; i < currentWord.word.Length; i++)
        {
            int row = currentWord.startRow + (currentWord.horizontal ? 0 : i);
            int col = currentWord.startCol + (currentWord.horizontal ? i : 0);
            if (row >= 0 && row < gridSize && col >= 0 && col < gridSize)
            {
                cells.Add(gridCells[row, col]);
            }
            else
            {
                Debug.LogWarning($"Word '{currentWord.word}' cell ({row},{col}) is out of bounds.");
            }
        }
        return cells;
    }

    void MoveCursorToNextCell()
    {
        if (currentWord == null)
            return;

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
        if (currentWord == null)
            return;

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
        if (currentWord == null)
            return -1;

        return currentWord.horizontal
            ? cell.Col - currentWord.startCol
            : cell.Row - currentWord.startRow;
    }

    void RemoveLastLetter()
    {
        if (selectedCell != null && currentWord != null)
        {
            // Only remove the letter if it's not a hinted cell and not already locked (correct)
            if (!hintedCells.Contains(selectedCell) && !selectedCell.IsLocked)
            {
                selectedCell.SetInputLetter(' ');
                // Only move to the previous cell if the current cell is empty
                if (selectedCell.GetCurrentLetter() == ' ')
                {
                    MoveCursorToPreviousEditableCell();
                }
            }
            else
            {
                // If the current cell is a hinted or locked cell, skip to previous editable cell
                MoveCursorToPreviousEditableCell();
            }
        }
    }

    // Move to the previous cell in the word that is not locked or hinted
    void MoveCursorToPreviousEditableCell()
    {
        if (currentWord == null || selectedCell == null)
            return;

        int currentIndex = GetCellIndexInWord(selectedCell);
        for (int i = currentIndex - 1; i >= 0; i--)
        {
            int row = currentWord.startRow + (currentWord.horizontal ? 0 : i);
            int col = currentWord.startCol + (currentWord.horizontal ? i : 0);
            if (row >= 0 && row < gridSize && col >= 0 && col < gridSize)
            {
                GridCell cell = gridCells[row, col];
                if (!cell.IsLocked && !hintedCells.Contains(cell))
                {
                    SelectCell(cell);
                    return;
                }
            }
        }
        // If no editable cell found, stay on current cell
    }

    void CheckWord()
    {
        totalAttempts++;
        string enteredWord = string.Join(
                "",
                GetCurrentWordCells().Select(cell => cell.GetCurrentLetter())
            )
            .Trim()
            .ToUpper();
        string expectedWord = currentWord.word.Trim().ToUpper();

        Debug.Log($"Expected Word: {expectedWord}");
        Debug.Log($"Entered Word: {enteredWord}");

        if (enteredWord.Equals(expectedWord, System.StringComparison.OrdinalIgnoreCase))
        {
            correctAnswers++;
            // Word is correct - lock it in
            foreach (var cell in GetCurrentWordCells())
            {
                cell.LockCell();
            }

            // Update vocabulary range tracking
            if (gameProgressHandler != null)
            {
                int difficulty = CalculateWordDifficulty(currentWord.word);
                gameProgressHandler.OnWordSolved(currentWord.word, difficulty);
            }

            // Move to next word if available
            SelectNextWord();
        }
        else
        {
            // Word is incorrect - flash cells red
            foreach (var cell in GetCurrentWordCells())
            {
                cell.FlashRed(1f);
                // Only clear non-hinted cells
                if (!hintedCells.Contains(cell))
                {
                    cell.SetInputLetter(' ');
                }
            }
            if (gameProgressHandler != null)
            {
                gameProgressHandler.OnIncorrectAnswer(currentWord.word);
            }
            Debug.Log("Incorrect word. Try again.");

            // Return to first cell of word
            SelectCell(GetCurrentWordCells().First());
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
            Debug.Log("Crossword puzzle completed. Game over.");
            timerManager?.StopTimer();
            if (gameOver != null)
            {
                gameOver.SetActive(true);
                // Set game over panel color based on subject
                var image = gameOver.GetComponent<Image>();
                if (image != null)
                {
                    if (LessonsLoader.subjectId == 1) // English
                    {
                        image.color = new Color32(0, 102, 204, 255); // Blue
                    }
                    else if (LessonsLoader.subjectId == 2) // Science
                    {
                        image.color = new Color32(0, 153, 0, 255); // Green
                    }
                }
            }

            int studentId = int.Parse(PlayerPrefs.GetString("User ID"));
            int gameModeId = 3; // Crossword mode ID (adjust as needed)
            int subjectId = LessonsLoader.subjectId;
            float solveTime = timerManager?.elapsedTime ?? 0;
            int module_number = int.Parse(LessonsLoader.moduleNumber);

            if (GameLoadingManager.Instance != null)
            {
                GameLoadingManager.Instance.ShowLoadingScreenWithDelay(
                    0.5f,
                    false,
                    () =>
                    {
                        StartCoroutine(
                            UpdateGameCompletionAndAttributes(
                                studentId,
                                module_number,
                                gameModeId,
                                subjectId,
                                solveTime
                            )
                        );
                    }
                );
            }
            else
            {
                StartCoroutine(
                    UpdateGameCompletionAndAttributes(
                        studentId,
                        module_number,
                        gameModeId,
                        subjectId,
                        solveTime
                    )
                );
            }
        }
    }

    private IEnumerator UpdateGameCompletionAndAttributes(
        int studentId,
        int module_number,
        int gameModeId,
        int subjectId,
        float solveTime
    )
    {
        // Update game completion status
        var completionCoroutine = StartCoroutine(
            UpdateGameCompletionStatus(studentId, module_number, gameModeId, subjectId, solveTime)
        );
        yield return completionCoroutine;
        
        if (completionCoroutine != null)
        {
            // Update attributes
            var attributesCoroutine = StartCoroutine(UpdateAttributes());
            yield return attributesCoroutine;
        }

        if (GameLoadingManager.Instance != null)
        {
            GameLoadingManager.Instance.HideLoadingScreen();
        }
    }

    private IEnumerator UpdateAttributes()
    {
        int studentId = int.Parse(PlayerPrefs.GetString("User ID"));
        int module_number = int.Parse(LessonsLoader.moduleNumber);
        int gameModeId = 3; // Crossword mode ID (adjust as needed)
        int subjectId = LessonsLoader.subjectId;
        Debug.Log(
            $"Updating attributes for studentId: {studentId}, module_number: {module_number}"
        );

        if (gameProgressHandler != null)
        {
            yield return gameProgressHandler.UpdateAccuracy(
                studentId,
                module_number,
                gameModeId,
                subjectId,
                correctAnswers,
                totalAttempts
            );

            yield return gameProgressHandler.UpdateSpeed(
                studentId,
                module_number,
                gameModeId,
                subjectId,
                timerManager?.elapsedTime ?? 0
            );

            yield return gameProgressHandler.UpdateProblemSolving(
                studentId,
                module_number,
                gameModeId,
                subjectId,
                3 - hintCounter,
                0 // No skip logic for crossword, adjust if needed
            );
            yield return gameProgressHandler.UpdateConsistency(studentId, 10);

            yield return gameProgressHandler.UpdateVocabularyRange(
                studentId,
                module_number,
                gameModeId,
                subjectId,
                gameProgressHandler.ComplexWordAttemptCount,
                gameProgressHandler.hintUsageCount,
                gameProgressHandler.IncorrectAnswerCount
            );

            yield return gameProgressHandler.UpdateRetention(
                studentId,
                module_number,
                gameModeId,
                subjectId,
                0, // Assuming no skips in crossword
                gameProgressHandler.HintOnRepeatingWordCount,
                gameProgressHandler.IncorrectRepeatingAnswerCount
            );
        }
    }

    private IEnumerator UpdateGameCompletionStatus(
        int studentId,
        int module_number,
        int gameModeId,
        int subjectId,
        float solveTime
    )
    {
        string url = $"{Web.BaseApiUrl}updateGameCompletion.php";
        WWWForm form = new WWWForm();
        form.AddField("student_id", studentId);
        form.AddField("module_number", module_number);
        form.AddField("game_mode_id", gameModeId);
        form.AddField("subject_id", subjectId);
        form.AddField("solve_time", Mathf.FloorToInt(solveTime)); // Save solve time in seconds

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Game completion status updated successfully.");
            }
            else
            {
                Debug.LogError("Failed to update game completion status: " + www.error);
            }
        }
    }

    void AssignWordNumbers()
    {
        int currentNumber = 1;
        bool[,] numberedCells = new bool[gridSize, gridSize];

        foreach (var placement in currentLevel.fixedLayout)
        {
            if (
                placement.startRow >= 0
                && placement.startRow < gridSize
                && placement.startCol >= 0
                && placement.startCol < gridSize
            )
            {
                if (!numberedCells[placement.startRow, placement.startCol])
                {
                    wordNumbers[placement] = currentNumber;
                    numberedCells[placement.startRow, placement.startCol] = true;
                    currentNumber++;
                }
            }
            else
            {
                Debug.LogWarning(
                    $"Word '{placement.word}' start cell ({placement.startRow},{placement.startCol}) is out of bounds."
                );
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

    private void GenerateGrid()
    {
        ClearGrid(); // Ensure the grid is cleared before generating a new one

        gridCells = new GridCell[gridSize, gridSize];
        float cellSize = 60f;

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
                rectTransform.anchoredPosition =
                    gridStartPos + new Vector2(col * cellSize, -row * cellSize);

                gridCells[row, col] = cellScript;
            }
        }
    }

    void HandleCellClick(GridCell cell)
    {
        Debug.Log($"Cell clicked: Row {cell.Row}, Col {cell.Col}");

        if (cell == null)
        {
            Debug.LogWarning("Clicked cell is null.");
            return;
        }

        ClearHighlights();

        WordPlacement horizontalWord = FindWordAtCell(cell.Row, cell.Col, true);
        WordPlacement verticalWord = FindWordAtCell(cell.Row, cell.Col, false);

        Debug.Log($"Horizontal Word: {(horizontalWord != null ? horizontalWord.word : "None")}");
        Debug.Log($"Vertical Word: {(verticalWord != null ? verticalWord.word : "None")}");

        // Improved word selection logic
        if (horizontalWord != null && verticalWord != null)
        {
            // If we're already in a word, switch to the other orientation
            if (currentWord != null)
            {
                if (currentWord.horizontal && verticalWord != null)
                {
                    currentWord = verticalWord;
                    isHorizontalInput = false;
                }
                else if (!currentWord.horizontal && horizontalWord != null)
                {
                    currentWord = horizontalWord;
                    isHorizontalInput = true;
                }
            }
            else
            {
                // Default to horizontal if no current word
                currentWord = horizontalWord;
                isHorizontalInput = true;
            }
        }
        else
        {
            currentWord = horizontalWord ?? verticalWord;
            isHorizontalInput = currentWord?.horizontal ?? true;
        }

        Debug.Log($"Selected Word: {(currentWord != null ? currentWord.word : "None")}");

        if (currentWord != null)
        {
            // Find the first cell of the current word
            int firstRow = currentWord.startRow;
            int firstCol = currentWord.startCol;

            // Select the first cell of the word
            SelectCell(gridCells[firstRow, firstCol]);
        }
        else
        {
            Debug.LogWarning("No word found at this cell");
        }
    }

    WordPlacement FindWordAtCell(int row, int col, bool horizontal)
    {
        foreach (var placement in currentLevel.fixedLayout)
        {
            // Only check words with matching orientation
            if (placement.horizontal != horizontal)
                continue;

            for (int i = 0; i < placement.word.Length; i++)
            {
                int checkRow = placement.startRow + (placement.horizontal ? 0 : i);
                int checkCol = placement.startCol + (placement.horizontal ? i : 0);

                if (checkRow == row && checkCol == col)
                {
                    // Only return if within bounds
                    if (
                        checkRow >= 0
                        && checkRow < gridSize
                        && checkCol >= 0
                        && checkCol < gridSize
                    )
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
            WordClue clue = currentLevel.wordClues.Find(c =>
                c.word.ToUpper() == word.word.ToUpper()
            );
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

            if (row >= 0 && row < gridSize && col >= 0 && col < gridSize)
            {
                GridCell cell = gridCells[row, col];
                cell.Highlight();
                highlightedCells.Add(cell);
            }
            else
            {
                Debug.LogWarning($"Highlight out of bounds: {word.word} ({row},{col})");
            }
        }
    }

    void ClearHighlights()
    {
        foreach (var cell in highlightedCells)
        {
            if (cell != null) // Ensure the cell is not null
            {
                cell.ClearHighlight();
            }
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
                if (row >= 0 && row < gridSize && col >= 0 && col < gridSize)
                {
                    GridCell cell = gridCells[row, col];
                    cell.SetCorrectLetter(placement.word[i]);
                    cell.SetActive(true);

                    // Set number for first cell of word
                    if (i == 0 && wordNumbers.ContainsKey(placement))
                    {
                        cell.SetNumber(wordNumbers[placement]);
                    }
                }
                else
                {
                    Debug.LogWarning(
                        $"Word '{placement.word}' cell ({row},{col}) is out of bounds."
                    );
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

    public void NavigateToNextWord()
    {
        if (currentWord == null || currentLevel.fixedLayout == null)
            return;

        // Find the index of the current word
        int currentIndex = currentLevel.fixedLayout.IndexOf(currentWord);
        if (currentIndex < 0)
        {
            Debug.LogWarning("Current word not found in the layout.");
            return;
        }

        // Move to the next word, or wrap around to the first word
        int nextIndex = (currentIndex + 1) % currentLevel.fixedLayout.Count;
        currentWord = currentLevel.fixedLayout[nextIndex];

        // Select the first cell of the next word
        int firstRow = currentWord.startRow;
        int firstCol = currentWord.startCol;
        SelectCell(gridCells[firstRow, firstCol]);
    }

    public void NavigateToPreviousWord()
    {
        if (currentWord == null || currentLevel.fixedLayout == null)
            return;

        // Find the index of the current word
        int currentIndex = currentLevel.fixedLayout.IndexOf(currentWord);
        if (currentIndex < 0)
        {
            Debug.LogWarning("Current word not found in the layout.");
            return;
        }

        // Move to the previous word, or wrap around to the last word
        int previousIndex =
            (currentIndex - 1 + currentLevel.fixedLayout.Count) % currentLevel.fixedLayout.Count;
        currentWord = currentLevel.fixedLayout[previousIndex];

        // Select the first cell of the previous word
        int firstRow = currentWord.startRow;
        int firstCol = currentWord.startCol;
        SelectCell(gridCells[firstRow, firstCol]);
    }

    private void RevealHint()
    {
        if (hintCounter <= 0)
        {
            Debug.Log("No hints remaining.");
            return;
        }

        if (currentWord == null)
        {
            Debug.Log("No word selected. Cannot reveal a hint.");
            return;
        }

        List<GridCell> unrevealedCells = new List<GridCell>();
        foreach (var cell in GetCurrentWordCells())
        {
            if (cell.GetCurrentLetter() == ' ' && !hintedCells.Contains(cell))
            {
                unrevealedCells.Add(cell);
            }
        }

        if (unrevealedCells.Count > 0)
        {
            GridCell randomCell = unrevealedCells[
                UnityEngine.Random.Range(0, unrevealedCells.Count)
            ];
            int indexInWord = GetCellIndexInWord(randomCell);
            randomCell.SetInputLetter(currentWord.word[indexInWord], true); // Set isHint to true
            hintedCells.Add(randomCell); // Track this cell as a hinted cell
            hintCounter--;
            gameProgressHandler.OnHintUsed(currentWord.word);
            UpdateHintCounterUI();
            Debug.Log(
                $"Hint revealed at cell ({randomCell.Row}, {randomCell.Col}): {currentWord.word[indexInWord]}"
            );

            // Check if the word is complete after revealing the hint
            if (IsWordComplete())
            {
                CheckWord();
            }
        }
        else
        {
            Debug.Log("No unrevealed letters remain in the current word.");
        }
    }

    private void UpdateHintCounterUI()
    {
        if (hintCounterText != null)
        {
            hintCounterText.text = $"Hints Remaining: {hintCounter}";
        }
    }

    public void LoadQuestionsOnButtonClick()
    {
        // Reset the game state and reload crossword data
        ClearGrid();
        ResetGameState();

        // Update button colors based on subject
        if (crosswordKeyboard != null)
        {
            crosswordKeyboard.UpdateButtonColorsBySubject();
        }
        if (hintButton != null)
        {
            var outline = hintButton.GetComponent<UnityEngine.UI.Outline>();
            if (outline != null)
            {
                Color outlineColor = Color.white;
                if (LessonsLoader.subjectId == 1) // English
                {
                    outlineColor = new Color32(0, 102, 204, 255); // Example: blue
                }
                else if (LessonsLoader.subjectId == 2) // Science
                {
                    outlineColor = new Color32(0, 153, 0, 255); // Example: green
                }
                outline.effectColor = outlineColor;
            }
        }

        // Always load crossword data, regardless of lesson completion status
        StartCoroutine(
            LoadCrosswordData(LessonsLoader.subjectId, int.Parse(LessonsLoader.moduleNumber))
        );
    }

    public void ReplayGame()
    {
        Debug.Log("Replay button clicked. Resetting Crossword game state...");

        // Reset game state
        ResetGameState();

        // Update button colors based on subject
        if (crosswordKeyboard != null)
        {
            crosswordKeyboard.UpdateButtonColorsBySubject();
        }
        if (hintButton != null)
        {
            var outline = hintButton.GetComponent<UnityEngine.UI.Outline>();
            if (outline != null)
            {
                Color outlineColor = Color.white;
                if (LessonsLoader.subjectId == 1) // English
                {
                    outlineColor = new Color32(0, 102, 204, 255); // Example: blue
                }
                else if (LessonsLoader.subjectId == 2) // Science
                {
                    outlineColor = new Color32(0, 153, 0, 255); // Example: green
                }
                outline.effectColor = outlineColor;
            }
        }

        // Reload crossword data
        StartCoroutine(
            LoadCrosswordData(LessonsLoader.subjectId, int.Parse(LessonsLoader.moduleNumber))
        );
    }

    private void ResetGameState()
    {
        Debug.Log("Resetting Crossword game state...");
        isRefreshing = false;
        isLessonCompleted = false;
        correctAnswers = 0;
        totalAttempts = 0;
        hintCounter = 3;
        hintedCells.Clear(); // Clear the hinted cells when resetting the game
        
        // Reset GameProgressHandler counters
        if (gameProgressHandler != null)
        {
            gameProgressHandler.ResetVocabularyRangeCounters();
        }
        
        // Clear grid and reset UI
        ClearGrid();
        
        if (cluesPanelText != null)
        {
            cluesPanelText.text = "";
        }
        
        if (currentClueText != null)
        {
            currentClueText.text = "";
        }
        
        if (gameOver != null)
        {
            gameOver.SetActive(false);
        }
        
        // Reset word tracking
        selectedCell = null;
        highlightedCells.Clear();
        currentWord = null;
        wordNumbers.Clear();
        
        UpdateHintCounterUI();
    }
}
