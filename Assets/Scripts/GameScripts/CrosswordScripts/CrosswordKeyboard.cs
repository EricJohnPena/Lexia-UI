using UnityEngine;
using UnityEngine.UI;

public class CrosswordKeyboard : MonoBehaviour
{
    [SerializeField]
    private GameObject keyboardPanel;

    [SerializeField]
    public Button[] letterButtons;

    [SerializeField]
    public Button backspaceButton;

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

        // Update button colors based on subject
        UpdateButtonColorsBySubject();
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

    public void UpdateButtonColorsBySubject()
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

        foreach (var button in letterButtons)
        {
            var outline = button.GetComponent<UnityEngine.UI.Outline>();
            var textComponent = button.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.color = outlineColor;
            }
            if (outline != null)
            {
                outline.effectColor = outlineColor;
            }
        }

        if (backspaceButton != null)
        {
            var outline = backspaceButton.GetComponent<UnityEngine.UI.Outline>();
            var textComponent = backspaceButton.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.color = outlineColor;
            }
            if (outline != null)
            {
                outline.effectColor = outlineColor;
            }
        }
    }
}
