using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ProfilePictureManager : MonoBehaviour
{
    public static ProfilePictureManager Instance { get; private set; }
    
    private Dictionary<string, Sprite> profilePictureCache = new Dictionary<string, Sprite>();
    
    
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

    private IEnumerator LoadProfilePictureCoroutine(string studentId, Image targetImage)
    {
        // Check cache first
        if (profilePictureCache.ContainsKey(studentId))
        {
            targetImage.sprite = profilePictureCache[studentId];
            yield break;
        }

        // Create URL with query parameter
        string url = Web.BaseApiUrl + "getProfilePicture.php?student_id=" + studentId;

        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching profile picture: " + www.error);
                LoadDefaultProfilePicture(targetImage);
                yield break;
            }

            ProfilePictureResponse response;
            try
            {
                response = JsonConvert.DeserializeObject<ProfilePictureResponse>(www.downloadHandler.text);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parsing profile picture response: " + e.Message);
                LoadDefaultProfilePicture(targetImage);
                yield break;
            }

            if (!response.success)
            {
                Debug.LogError("Error in profile picture response: " + response.error);
                LoadDefaultProfilePicture(targetImage);
                yield break;
            }

            // Load the image from the server
            using (UnityEngine.Networking.UnityWebRequest imageRequest = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(Web.BaseApiUrl + "images/" + response.image_path))
            {
                yield return imageRequest.SendWebRequest();

                if (imageRequest.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Texture2D texture = ((UnityEngine.Networking.DownloadHandlerTexture)imageRequest.downloadHandler).texture;
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    
                    // Cache the sprite
                    profilePictureCache[studentId] = sprite;
                    
                    // Set the image
                    targetImage.sprite = sprite;
                }
                else
                {
                    Debug.LogError("Error loading image: " + imageRequest.error);
                    LoadDefaultProfilePicture(targetImage);
                }
            }
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
}

[System.Serializable]
public class ProfilePictureResponse
{
    public bool success;
    public string image_path;
    public string error;
} 