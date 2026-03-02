using UnityEngine;
using UnityEngine.UI;

public class HUDText : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerController2D player;
    public Text vidaText;
    public Text scoreText;

    void Update()
    {
        if (player != null && vidaText != null)
        {
            vidaText.text = "VIDA: " + player.currentHealth;
        }

        if (ScoreManager.Instance != null && scoreText != null)
        {
            scoreText.text = "PUNTUACION: " + ScoreManager.Instance.score;
        }
    }
}