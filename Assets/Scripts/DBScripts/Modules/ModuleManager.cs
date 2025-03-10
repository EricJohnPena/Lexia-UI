using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleManager : MonoBehaviour
{
    public static ModuleManager Instance { get; private set; }

    private string currentModuleNumber;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCurrentModule(string moduleNumber)
    {
        currentModuleNumber = moduleNumber;
        Debug.Log("Current Module Number: " + currentModuleNumber);
    }

    public string GetCurrentModule()
    {
        return currentModuleNumber;
    }

}
