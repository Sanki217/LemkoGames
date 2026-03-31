using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HudUI : MonoBehaviour
{
    [Header("HP")]
    public Image[] heartIcons;
    public Sprite heartFull;
    public Sprite heartEmpty;

    [Header("Score")]
    public TMP_Text scoreText;
    public TMP_Text multiplierText;

    [Header("Feedback")]
    public TMP_Text feedbackText;
    float feedbackTimer;

    void Update()
    {
        if (feedbackTimer > 0)
        {
            feedbackTimer -= Time.deltaTime;
            if (feedbackTimer <= 0)
                feedbackText.gameObject.SetActive(false);
        }
    }

    public void UpdateHP(int hp)
    {
        for (int i = 0; i < heartIcons.Length; i++)
            heartIcons[i].sprite = (i < hp) ? heartFull : heartEmpty;
    }

    public void UpdateScore(int score, int streak)
    {
        scoreText.text = $"Score: {score}";
        multiplierText.text = streak > 1 ? $"×{streak}" : "";
    }

    public void ShowFeedback(bool correct)
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = correct ? "✓ Добрі!" : "✗ Зле!";
        feedbackText.color = correct ? Color.green : Color.red;
        feedbackTimer = 1.2f;
    }
}