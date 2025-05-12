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
                        List<string> words = JsonUtility
                            .FromJson<RepeatingWordsList>(WrapJson(json))
                            .words;
                        repeatingWordsSet = new HashSet<string>(words);
                        IsLoaded = true;
                        OnRepeatingWordsLoaded?.Invoke();
                        Debug.Log($"Loaded {repeatingWordsSet.Count} repeating words.");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to parse repeating words JSON: {e.Message}");
                    }
                    yield break; // Success, exit coroutine
                }
                else
                {
                    Debug.LogError($"Failed to load repeating words: {www.error} (Attempt {attempt}/{maxRetries})");
                    if (attempt < maxRetries)
                    {
                        Debug.LogWarning("Retrying to load repeating words...");
                        yield return new WaitForSeconds(retryDelay);
                        continue;
                    }
                }
            }
            // If we reach here, all retries failed
            yield break;
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
