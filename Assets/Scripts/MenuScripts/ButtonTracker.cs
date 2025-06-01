using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTracker : MonoBehaviour
{
    public static ButtonTracker Instance { get; private set; } // Singleton instance

    public Button button1;
    public Button button2;

    private readonly Dictionary<Button, int> buttonSubjectMapping = new Dictionary<Button, int>(); // Map buttons to subject IDs

    private int currentSubjectId; // Currently selected fk_subject_id

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Map buttons to their subject IDs
            buttonSubjectMapping[button1] = 1;
            buttonSubjectMapping[button2] = 2;

            // Default to button1's subject ID if assigned
            if (button1 != null)
            {
                currentSubjectId = buttonSubjectMapping[button1];
                SetButtonStyles(button1, true);
                SetButtonStyles(button2, false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Set the currently selected button and its associated subject ID
    public void SetCurrentButton(Button selectedButton)
    {
        if (buttonSubjectMapping.ContainsKey(selectedButton))
        {
            currentSubjectId = buttonSubjectMapping[selectedButton];

            // Update button styles
            SetButtonStyles(button1, selectedButton == button1);
            SetButtonStyles(button2, selectedButton == button2);
        }
        else
        {
            Debug.LogWarning("Selected button is not mapped to a subject ID.");
        }
    }

    // Get the current fk_subject_id
    public int GetCurrentSubjectId()
    {
        return currentSubjectId;
    }

    // Attach this method to button click events
    public void OnButtonClicked(Button clickedButton)
    {
        SetCurrentButton(clickedButton);
        ModuleLoader moduleLoader = FindObjectOfType<ModuleLoader>();

        if (moduleLoader != null)
        {
            moduleLoader.LoadModulesForSelectedSubject();
        }
    }

    private void Start()
    {
        // Attach listeners to buttons dynamically
        if (button1 != null)
        {
            button1.onClick.AddListener(() => OnButtonClicked(button1));
        }

        if (button2 != null)
        {
            button2.onClick.AddListener(() => OnButtonClicked(button2));
        }
    }

    // Update button styles based on selection state
    private void SetButtonStyles(Button button, bool isSelected)
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            if (isSelected)
            {
                if (button == button1)
                {
                    buttonImage.color = new Color32(49, 49, 49, 255);
                }
                else if (button == button2)
                {
                    buttonImage.color = new Color32(49, 49, 49, 255);
                }
            }
            else
            {
                buttonImage.color = Color.white;
            }
        }
    }
}
