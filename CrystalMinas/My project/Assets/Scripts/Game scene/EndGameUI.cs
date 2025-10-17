using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EndGameUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;

    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;

    public Transform player1HeartsContainer;
    public Transform player2HeartsContainer;

    public void UpdateUI(int winner, int score1, int score2, int lives1, int lives2)
    {
        if (winner == 0)
            titleText.text = "Empate!";
        else
            titleText.text = $"O jogador {winner} vence!";

        player1ScoreText.text = $"{score1}";
        player2ScoreText.text = $"{score2}";

        UpdateHearts(player1HeartsContainer, lives1);
        UpdateHearts(player2HeartsContainer, lives2);
    }

    void UpdateHearts(Transform container, int lives)
    {
        // Сначала отключаем все сердца
        for (int i = 0; i < container.childCount; i++)
        {
            container.GetChild(i).gameObject.SetActive(false);
        }

        // Включаем нужное количество
        for (int i = 0; i < lives; i++)
        {
            if (i < container.childCount)
                container.GetChild(i).gameObject.SetActive(true);
        }
    }
}
