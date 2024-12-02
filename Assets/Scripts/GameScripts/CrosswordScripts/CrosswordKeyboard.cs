using UnityEngine;
using UnityEngine.UI;

public class CrosswordKeyboard : MonoBehaviour
{
    [SerializeField] private GameObject keyboardPanel;
    [SerializeField] public Button[] letterButtons;
    [SerializeField] public Button backspaceButton;

    private CrosswordGridManager gridManager;

    void Start()
    {
        keyboardPanel.SetActive(true); // Ensure the panel is set active in code

        gridManager = FindObjectOfType<CrosswordGridManager>();

        if (gridManager == null)
        {
            Debug.LogError("CrosswordGridManager not found in the scene!");
            return;
        }

        SetupKeyboardButtons();
    }

    void SetupKeyboardButtons()
    {
        // Setup letter buttons
        foreach (Button letterButton in letterButtons)
        {
            // Capture the button's text to use in the lambda
            string buttonText = letterButton.GetComponentInChildren<Text>().text;
            letterButton.onClick.AddListener(() =>
            {
                // Send the key press to the grid manager
                gridManager.HandleKeyInput(buttonText[0]);
            });
        }

        // Setup backspace button
        if (backspaceButton != null)
        {
            backspaceButton.onClick.AddListener(() =>
            {
                gridManager.HandleBackspace();
            });
        }
    }
}