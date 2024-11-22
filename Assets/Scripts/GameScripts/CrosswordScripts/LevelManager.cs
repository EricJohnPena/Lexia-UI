using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class LevelManager : MonoBehaviour
{
    public string levelDirectory = "Assets/Levels";

    public CrosswordLevel LoadLevel(string levelFileName)
    {
        string filePath = Path.Combine(levelDirectory, levelFileName);

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Debug.Log("Loaded JSON: " + json);  // Log the content of the JSON
            return JsonUtility.FromJson<CrosswordLevel>(json);
        }

        Debug.LogError("Level file not found: " + filePath);
        return null;
    }

    public void SaveLevel(CrosswordLevel level, string levelFileName)
    {
        string json = JsonUtility.ToJson(level, true);
        string filePath = Path.Combine(levelDirectory, levelFileName);

        File.WriteAllText(filePath, json);
        Debug.Log("Level saved at: " + filePath);
    }
}
