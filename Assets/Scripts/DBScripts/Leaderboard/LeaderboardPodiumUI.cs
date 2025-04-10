using UnityEngine;
using UnityEngine.UI;

public class LeaderboardPodiumUI : MonoBehaviour
{
    public Image profileImage;
    public Text usernameText;
    public Text scoreText;
    public Text rankText;
    public RectTransform podiumBar; // The bar/platform the player stands on

    // Position configurations for 1st, 2nd, and 3rd place
    private readonly Vector2[] podiumHeights = new Vector2[]
    {
        new Vector2(0, 200), // 1st place height
        new Vector2(0, 150), // 2nd place height
        new Vector2(0, 100), // 3rd place height
    };

    public void SetPodiumData(string username, int score, int rank)
    {
        Debug.Log($"Setting podium data: Username={username}, Score={score}, Rank={rank}");

        // Set text values
        usernameText.text = username;
        scoreText.text = score.ToString();
        rankText.text = rank.ToString();

        // Adjust podium height based on rank (1, 2, or 3)
        if (rank >= 1 && rank <= 3)
        {
            float[] podiumHeights = { 200f, 150f, 100f }; // Heights for 1st, 2nd, and 3rd place
            Vector2 sizeDelta = podiumBar.sizeDelta;
            sizeDelta.y = podiumHeights[rank - 1];
            podiumBar.sizeDelta = sizeDelta;
        }

        // You can add profile image loading here if you have a profile picture system
        // For now, we'll just use a default image
    }
}
