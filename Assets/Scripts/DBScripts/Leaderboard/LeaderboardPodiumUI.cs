using UnityEngine;
using UnityEngine.UI;

public class LeaderboardPodiumUI : MonoBehaviour
{
    public Image profileImage;
    public Text usernameText;
    public Text scoreText;
    public Text rankText;
    public RectTransform podiumBar; // The bar/platform the player stands on

    private void Awake()
    {
        // Make profile image circular using a mask
        if (profileImage != null)
        {
            profileImage.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            profileImage.sprite = Resources.Load<Sprite>("DefaultProfilePic");
        }
    }

    public void SetPodiumData(string username, int score, int rank, string studentId)
    {
        Debug.Log($"Setting podium data: Username={username}, Score={score}, Rank={rank}, StudentId={studentId}");

        // Set text values
        usernameText.text = username;
        scoreText.text = score.ToString();
        rankText.text = rank.ToString();

        // Load the student's profile picture
        if (profileImage != null && ProfilePictureManager.Instance != null)
        {
            ProfilePictureManager.Instance.LoadProfilePicture(studentId, profileImage);
        }

        // Adjust podium height based on rank (1, 2, or 3)
        if (rank >= 1 && rank <= 3)
        {
            float[] podiumHeights = { 200f, 150f, 100f }; // Heights for 1st, 2nd, and 3rd place
            float[] podiumAlphas = { 1.0f, 0.7f, 0.4f }; // Alpha for 1st, 2nd, 3rd
            Vector2 sizeDelta = podiumBar.sizeDelta;
            sizeDelta.y = podiumHeights[rank - 1];
            podiumBar.sizeDelta = sizeDelta;
            // Set transparency
            Color c = podiumBar.GetComponent<Image>().color;
            c.a = podiumAlphas[rank - 1];
            podiumBar.GetComponent<Image>().color = c;
        }
    }
}
