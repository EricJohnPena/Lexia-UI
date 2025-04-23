using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    // No need to specify the path manually; use Application.streamingAssetsPath
    private string levelDirectory;

    private void Awake()
    {
        // Set the directory to the StreamingAssets path
        levelDirectory = Application.streamingAssetsPath;
    }

    public CrosswordLevel LoadLevel(string levelFileName)
    {
        string filePath = Path.Combine(levelDirectory, "Levels/", levelFileName);

        // StreamingAssets behaves differently on Android; use UnityWebRequest for those cases
#if UNITY_ANDROID
        return LoadFromAndroid(filePath);
#else
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Debug.Log("Loaded JSON: " + json);
            return JsonUtility.FromJson<CrosswordLevel>(json);
        }
#endif
    }

    public void SaveLevel(CrosswordLevel level, string levelFileName)
    {
        string json = JsonUtility.ToJson(level, true);
        string filePath = Path.Combine(levelDirectory, levelFileName);

#if UNITY_ANDROID
        Debug.LogError("Saving files is not supported in StreamingAssets on Android.");
#else
        File.WriteAllText(filePath, json);
        Debug.Log("Level saved at: " + filePath);
#endif
    }

#if UNITY_ANDROID
    private CrosswordLevel LoadFromAndroid(string filePath)
    {
        // Use UnityWebRequest to access StreamingAssets on Android
        using (
            UnityEngine.Networking.UnityWebRequest request =
                UnityEngine.Networking.UnityWebRequest.Get(filePath)
        )
        {
            request.SendWebRequest();
            while (!request.isDone)
            {
                // Wait for completion
            }

            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                Debug.Log("Loaded JSON from Android: " + json);
                return JsonUtility.FromJson<CrosswordLevel>(json);
            }
            else
            {
                Debug.LogError("Failed to load file: " + filePath + "\nError: " + request.error);
                return null;
            }
        }
    }
#endif
}
