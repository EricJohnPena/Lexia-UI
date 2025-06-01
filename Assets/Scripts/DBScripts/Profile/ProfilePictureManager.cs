using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePictureManager : MonoBehaviour
{
    public static ProfilePictureManager Instance { get; private set; }

    private Dictionary<string, Sprite> profilePictureCache = new Dictionary<string, Sprite>();
    private const int MAX_RETRIES = 3;
    private const float RETRY_DELAY = 2f;

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

    public void LoadProfilePicture(string studentId, Image targetImage)
    {
        StartCoroutine(LoadProfilePictureCoroutine(studentId, targetImage));
    }

    public void LoadProfilePictureById(string pictureId, Image targetImage)
    {
        StartCoroutine(LoadProfilePictureByIdCoroutine(pictureId, targetImage));
    }

    private IEnumerator LoadProfilePictureCoroutine(string studentId, Image targetImage)
    {
        // Check cache first
        if (profilePictureCache.ContainsKey(studentId))
        {
            targetImage.sprite = profilePictureCache[studentId];
            yield break;
        }

        int attempts = 0;
        bool success = false;

        while (attempts < MAX_RETRIES && !success)
        {
            attempts++;
            // Create URL with query parameter
            string url = Web.BaseApiUrl + "getProfilePicture.php?student_id=" + studentId;

            using (
                UnityEngine.Networking.UnityWebRequest www =
                    UnityEngine.Networking.UnityWebRequest.Get(url)
            )
            {
                yield return www.SendWebRequest();

                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    ProfilePictureResponse response;
                    try
                    {
                        response = JsonConvert.DeserializeObject<ProfilePictureResponse>(
                            www.downloadHandler.text
                        );
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(
                            $"Error parsing profile picture response (Attempt {attempts}/{MAX_RETRIES}): {e.Message}"
                        );
                        continue;
                    }

                    if (response.success)
                    {
                        yield return StartCoroutine(
                            LoadImageFromPath(response.image_path, studentId, targetImage)
                        );
                        success = true;
                        break;
                    }
                    else
                    {
                        Debug.LogError(
                            $"Error in profile picture response (Attempt {attempts}/{MAX_RETRIES}): {response.error}"
                        );
                    }
                }
                else
                {
                    Debug.LogError(
                        $"Error fetching profile picture (Attempt {attempts}/{MAX_RETRIES}): {www.error}"
                    );
                }

                if (!success && attempts < MAX_RETRIES)
                {
                    Debug.LogWarning($"Retrying profile picture load in {RETRY_DELAY} seconds...");
                    yield return new WaitForSeconds(RETRY_DELAY);
                }
            }
        }

        if (!success)
        {
            Debug.LogWarning(
                "Failed to load profile picture after all attempts. Loading default picture."
            );
            LoadDefaultProfilePicture(targetImage);
        }
    }

    private IEnumerator LoadProfilePictureByIdCoroutine(string pictureId, Image targetImage)
    {
        // Check cache first
        if (profilePictureCache.ContainsKey(pictureId))
        {
            targetImage.sprite = profilePictureCache[pictureId];
            yield break;
        }

        int attempts = 0;
        bool success = false;

        while (attempts < MAX_RETRIES && !success)
        {
            attempts++;
            // Create URL with query parameter
            string url = Web.BaseApiUrl + "getProfilePictureById.php?picture_id=" + pictureId;

            using (
                UnityEngine.Networking.UnityWebRequest www =
                    UnityEngine.Networking.UnityWebRequest.Get(url)
            )
            {
                yield return www.SendWebRequest();

                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    ProfilePictureResponse response;
                    try
                    {
                        response = JsonConvert.DeserializeObject<ProfilePictureResponse>(
                            www.downloadHandler.text
                        );
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(
                            $"Error parsing profile picture response (Attempt {attempts}/{MAX_RETRIES}): {e.Message}"
                        );
                        continue;
                    }

                    if (response.success)
                    {
                        yield return StartCoroutine(
                            LoadImageFromPath(response.image_path, pictureId, targetImage)
                        );
                        success = true;
                        break;
                    }
                    else
                    {
                        Debug.LogError(
                            $"Error in profile picture response (Attempt {attempts}/{MAX_RETRIES}): {response.error}"
                        );
                    }
                }
                else
                {
                    Debug.LogError(
                        $"Error fetching profile picture (Attempt {attempts}/{MAX_RETRIES}): {www.error}"
                    );
                }

                if (!success && attempts < MAX_RETRIES)
                {
                    Debug.LogWarning($"Retrying profile picture load in {RETRY_DELAY} seconds...");
                    yield return new WaitForSeconds(RETRY_DELAY);
                }
            }
        }

        if (!success)
        {
            Debug.LogWarning(
                "Failed to load profile picture after all attempts. Loading default picture."
            );
            LoadDefaultProfilePicture(targetImage);
        }
    }

    private IEnumerator LoadImageFromPath(string imagePath, string cacheKey, Image targetImage)
    {
        int attempts = 0;
        bool success = false;

        while (attempts < MAX_RETRIES && !success)
        {
            attempts++;
            using (
                UnityEngine.Networking.UnityWebRequest imageRequest =
                    UnityEngine.Networking.UnityWebRequestTexture.GetTexture(
                        Web.BaseApiUrl + "images/" + imagePath
                    )
            )
            {
                yield return imageRequest.SendWebRequest();

                if (imageRequest.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Texture2D texture = (
                        (UnityEngine.Networking.DownloadHandlerTexture)imageRequest.downloadHandler
                    ).texture;
                    Sprite sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f)
                    );

                    // Cache the sprite
                    profilePictureCache[cacheKey] = sprite;

                    // Set the image
                    targetImage.sprite = sprite;
                    success = true;
                    break;
                }
                else
                {
                    Debug.LogError(
                        $"Error loading image (Attempt {attempts}/{MAX_RETRIES}): {imageRequest.error}"
                    );
                }

                if (!success && attempts < MAX_RETRIES)
                {
                    Debug.LogWarning($"Retrying image load in {RETRY_DELAY} seconds...");
                    yield return new WaitForSeconds(RETRY_DELAY);
                }
            }
        }

        if (!success)
        {
            Debug.LogWarning("Failed to load image after all attempts. Loading default picture.");
            LoadDefaultProfilePicture(targetImage);
        }
    }

    private void LoadDefaultProfilePicture(Image targetImage)
    {
        Sprite defaultSprite = Resources.Load<Sprite>("DefaultProfilePic");
        if (defaultSprite != null)
        {
            targetImage.sprite = defaultSprite;
        }
        else
        {
            Debug.LogError("Default profile picture not found in Resources folder!");
        }
    }

    public void ClearCache(string key)
    {
        if (profilePictureCache.ContainsKey(key))
        {
            profilePictureCache.Remove(key);
        }
    }
}

[System.Serializable]
public class ProfilePictureResponse
{
    public bool success;
    public string image_path;
    public string error;
}
