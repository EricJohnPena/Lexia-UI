using UnityEngine;
using UnityEngine.UI;

public class ModuleUI : MonoBehaviour
{
    public Text moduleNameText; // Assign this to the "Science" text element
    public Text moduleNumberText; // Assign this to the "Module 01" text element
    public Button actionButton; // Assign this to the button
    public string moduleNumber; // Field to store the module number

    public void SetModuleData(string moduleNumber, string subjectName, bool isEnabled)
    {
        moduleNameText.text = subjectName;
        moduleNumberText.text = "Week " + moduleNumber;
        this.moduleNumber = moduleNumber; // Store the module number

        actionButton.interactable = isEnabled; // Enable or disable the button based on the parameter

        if (isEnabled)
        {
            actionButton.onClick.AddListener(() => StartModule(moduleNumber));
        }
        // else
        // {
        //     Debug.Log($"Module {moduleNumber} is locked.");
        // }
    }

    private void StartModule(string moduleNumber)
    {
        if (ModuleManager.Instance == null)
        {
            Debug.LogError("ModuleManager instance is null. Ensure ModuleManager is present in the scene.");
            return;
        }

        // Debug.Log("Starting module: " + moduleNumber);
        ModuleManager.Instance.SetCurrentModule(moduleNumber);
        MenuManager.InstanceMenu.ToLessonsPage();
    }
}
