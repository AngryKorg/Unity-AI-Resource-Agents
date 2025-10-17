using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameTimer timer;
    public PlayerControl player1Control;
    public PlayerControl player2Control;
    public UpdatePoints scoreManager;
    public GameObject endGamePanel;
    public EndGameUI endGameUI;

    private bool gameEnded = false;

    void Start()
    {
        endGamePanel.SetActive(false);
    }

    void Update()
    {
        if (gameEnded)
            return;

        // Проверка окончания по таймеру
        if (timer != null && timer.CurrentTime <= 0f)
        {
            EndGameByTimer();
        }

        // Проверка окончания по потерянным жизням
        if (player1Control != null && player1Control.lives <= 0)
        {
            EndGameByDefeat(2);
        }
        else if (player2Control != null && player2Control.lives <= 0)
        {
            EndGameByDefeat(1);
        }
    }

    void EndGameByTimer()
    {
        gameEnded = true;

        int p1 = player1Control != null ? player1Control.score : 0;
        int p2 = player2Control != null ? player2Control.score : 0;

        int winner = p1 > p2 ? 1 : (p2 > p1 ? 2 : 0);

        ShowEndGameUI(winner);
    }

    void EndGameByDefeat(int winner)
    {
        gameEnded = true;
        ShowEndGameUI(winner);
    }

    void ShowEndGameUI(int winner)
    {
        endGamePanel.SetActive(true);

        int p1Score = player1Control != null ? player1Control.score : 0;
        int p2Score = player2Control != null ? player2Control.score : 0;
        int p1Lives = player1Control != null ? player1Control.lives : 0;
        int p2Lives = player2Control != null ? player2Control.lives : 0;

        endGameUI.UpdateUI(winner, p1Score, p2Score, p1Lives, p2Lives);
    }
}
