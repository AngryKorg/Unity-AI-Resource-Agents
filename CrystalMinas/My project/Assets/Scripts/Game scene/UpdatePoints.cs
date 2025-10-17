using UnityEngine;
using TMPro; 

public class UpdatePoints : MonoBehaviour
{
    public TextMeshProUGUI player1Text;
    public TextMeshProUGUI player2Text;

    public void UpdatePointsDisplay(int playerId, int score)
    {
        if (playerId == 0 && player1Text != null)
        {
            player1Text.text = "Player 1: " + score;
        }
        else if (playerId == 1 && player2Text != null)
        {
            player2Text.text = "Player 2: " + score;
        }
    }
}
