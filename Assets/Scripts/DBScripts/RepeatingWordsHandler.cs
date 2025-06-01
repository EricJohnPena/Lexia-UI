using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RepeatingWordsHandler : MonoBehaviour
{
    private const string RepeatingWordsUrl = "getRepeatingWords.php";

    private HashSet<string> repeatingWordsSet = new HashSet<string>();
    public bool IsLoaded { get; private set; } = false;
    public event Action OnRepeatingWordsLoaded;

    public void LoadRepeatingWords()
    {
        StartCoroutine(LoadRepeatingWordsCoroutine());
    }

    private IEnumerator LoadRepeatingWordsCoroutine()
    {
        int maxRetries = 3;
        int attempt = 0;
        float retryDelay = 2f; // seconds
        string url = $"{Web.BaseApiUrl}{RepeatingWordsUrl}";

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
                        string json = www.downloadHandler.text;
                        // Debug.Log($"Received JSON: {json}");

                        // Wrap the JSON array in an object
                        string wrappedJson = $"{{\"words\":{json}}}";
                        RepeatingWordsList wordList = JsonUtility.FromJson<RepeatingWordsList>(
                            wrappedJson
                        );

                        if (wordList != null && wordList.words != null)
                        {
                            repeatingWordsSet = new HashSet<string>(
                                wordList.words,
                                StringComparer.OrdinalIgnoreCase
                            );
                            IsLoaded = true;
                            OnRepeatingWordsLoaded?.Invoke();

                            // Log each repeating word
                            // Debug.Log("=== Repeating Words Loaded ===");
                            // foreach (string word in repeatingWordsSet)
                            // {
                            //     Debug.Log($"Repeating Word: {word}");
                            // }
                            // Debug.Log($"=== Total Repeating Words: {repeatingWordsSet.Count} ===");
                        }
                        else
                        {
                            Debug.LogWarning(
                                "No repeating words found or invalid response format."
                            );
                            repeatingWordsSet.Clear();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to parse repeating words JSON: {e.Message}");
                        repeatingWordsSet.Clear();
                    }
                    yield break; // Success, exit coroutine
                }
                else
                {
                    Debug.LogError(
                        $"Failed to load repeating words: {www.error} (Attempt {attempt}/{maxRetries})"
                    );
                    if (attempt < maxRetries)
                    {
                        Debug.LogWarning("Retrying to load repeating words...");
                        yield return new WaitForSeconds(retryDelay);
                        continue;
                    }
                }
            }
        }
    }

    public bool IsRepeatingWord(string word)
    {
        if (!IsLoaded)
        {
            Debug.LogWarning("Repeating words not loaded yet.");
            return false;
        }
        return repeatingWordsSet.Contains(word.ToUpper());
    }

    [Serializable]
    private class RepeatingWordsList
    {
        public List<string> words;
    }

    private string WrapJson(string jsonArray)
    {
        return $"{{\"words\":{jsonArray}}}";
    }
}
