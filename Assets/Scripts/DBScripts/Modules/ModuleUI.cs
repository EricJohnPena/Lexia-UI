using UnityEngine;
using UnityEngine.UI;

public class ModuleUI : MonoBehaviour
{
    public Text moduleNameText; // Assign this to the "Science" text element
    public Text moduleNumberText; // Assign this to the "Module 01" text element
    public Button actionButton; // Assign this to the button

    public void SetModuleData(string moduleNumber, string subjectName)
    {
        moduleNameText.text = subjectName;
        moduleNumberText.text = "Module " + moduleNumber;

        // Set up button action (e.g., start module)
        actionButton.onClick.AddListener(() => StartModule(moduleNumber));
    }

    private void StartModule(string moduleNumber)
    {
        if (ModuleManager.Instance == null)
        {
            Debug.LogError("ModuleManager instance is null. Ensure ModuleManager is present in the scene.");
            return;
        }

        Debug.Log("Starting module: " + moduleNumber);
        ModuleManager.Instance.SetCurrentModule(moduleNumber);
        MenuManager.InstanceMenu.ToLessonsPage();
    }
}
