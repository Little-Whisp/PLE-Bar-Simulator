using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    [Header("Score Display")]
    public TextMeshProUGUI scoreText; // Reference to the UI Text element for score display

    private int currentScore = 0;

    // Adds points to the current score and updates the UI
    public void AddPoints(int points)
    {
        currentScore += points;
        UpdateScoreUI();
        Debug.Log("[Score] Points Added: " + points);
    }

    // Updates the score display in the UI
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }
}
