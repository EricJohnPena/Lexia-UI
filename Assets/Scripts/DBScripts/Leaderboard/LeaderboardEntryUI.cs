using UnityEngine;
using UnityEngine.UI;

public class LeaderboardEntryUI : MonoBehaviour
{
    public Text rankText;
    public Text usernameText;
    public Text scoreText;
    public Image profileImage;
    

    private void Awake()
    {
        // Make profile image circular using a mask
        if (profileImage != null)
        {
            profileImage.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            profileImage.sprite = Resources.Load<Sprite>("DefaultProfilePic");
        }
    }

    public void SetEntryData(string username, int score, int rank)
    {
        rankText.text = rank.ToString("00");
        usernameText.text = username;
        scoreText.text = score.ToString();

        // You can load user's profile picture here if available
        // For now, we'll use the default profile picture set in Awake
    }
} 