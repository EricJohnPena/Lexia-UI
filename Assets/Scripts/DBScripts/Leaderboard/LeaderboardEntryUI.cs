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

    public void SetEntryData(string username, int score, int rank, string studentId)
    {
        Debug.Log($"Setting entry data: Username={username}, Score={score}, Rank={rank}, StudentId={studentId}");
        rankText.text = rank.ToString("00");
        usernameText.text = username;
        scoreText.text = score.ToString();

        // Load the student's profile picture
        if (profileImage != null && ProfilePictureManager.Instance != null)
        {
            ProfilePictureManager.Instance.LoadProfilePicture(studentId, profileImage);
        }
    }
}
