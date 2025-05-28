using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class ProfilePictureButton : MonoBehaviour
{
    public Image profileImage;
    public Image selectionIndicator;
    public Button button;

    private ProfilePictureData pictureData;
    private Action<ProfilePictureButton> onSelected;

    private void Start()
    {
        button.onClick.AddListener(OnButtonClicked);
        selectionIndicator.gameObject.SetActive(false);
    }

    public void Initialize(ProfilePictureData data, Action<ProfilePictureButton> callback)
    {
        pictureData = data;
        onSelected = callback;
        LoadProfilePicture();
    }

    private void LoadProfilePicture()
    {
        StartCoroutine(LoadImageCoroutine());
    }

    private IEnumerator LoadImageCoroutine()
    {
        string imageUrl = Web.BaseApiUrl + "images/" + pictureData.image_path;
        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((UnityEngine.Networking.DownloadHandlerTexture)www.downloadHandler).texture;
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                profileImage.sprite = sprite;
            }
            else
            {
                Debug.LogError($"Error loading profile picture: {www.error}");
                // Load default image if the specific one fails
                Sprite defaultSprite = Resources.Load<Sprite>("DefaultProfilePic");
                if (defaultSprite != null)
                {
                    profileImage.sprite = defaultSprite;
                }
            }
        }
    }

    private void OnButtonClicked()
    {
        onSelected?.Invoke(this);
    }

    public void SetSelected(bool selected)
    {
        selectionIndicator.gameObject.SetActive(selected);
    }

    public ProfilePictureData PictureData => pictureData;
} 