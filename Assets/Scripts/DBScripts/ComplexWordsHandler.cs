using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ComplexWordsHandler : MonoBehaviour
{
    private const string ComplexWordsUrl = "getComplexWords.php";

    // HashSet for fast lookup of complex words
    private HashSet<string> complexWordsSet = new HashSet<string>();

    // Flag to indicate if complex words have been loaded
    public bool IsLoaded { get; private set; } = false;

    // Event to notify when complex words are loaded
    public event Action OnComplexWordsLoaded;

    // Call this method to start loading complex words from the server
    public void LoadComplexWords()
    {
        StartCoroutine(LoadComplexWordsCoroutine());
    }

    private IEnumerator LoadComplexWordsCoroutine()
    {
        string url = $"{Web.BaseApiUrl}{ComplexWordsUrl}";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string json = www.downloadHandler.text;
                    List<string> words = JsonUtility
                        .FromJson<ComplexWordsList>(WrapJson(json))
                        .words;
                    complexWordsSet = new HashSet<string>(words);
                    IsLoaded = true;
                    OnComplexWordsLoaded?.Invoke();
                    Debug.Log($"Loaded {complexWordsSet.Count} complex words.");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to parse complex words JSON: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"Failed to load complex words: {www.error}");
            }
        }
    }

    // Helper method to check if a word is complex
    public bool IsComplexWord(string word)
    {
        if (!IsLoaded)
        {
            Debug.LogWarning("Complex words not loaded yet.");
            return false;
        }
        return complexWordsSet.Contains(word.ToUpper());
    }

    // Unity's JsonUtility requires a wrapper class for arrays
    [Serializable]
    private class ComplexWordsList
    {
        public List<string> words;
    }

    // Wrap raw JSON array into an object with "words" property for JsonUtility
    private string WrapJson(string jsonArray)
    {
        return $"{{\"words\":{jsonArray}}}";
    }
}
