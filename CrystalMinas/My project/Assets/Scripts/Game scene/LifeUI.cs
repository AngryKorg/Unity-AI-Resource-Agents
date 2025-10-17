using UnityEngine;

public class LifeUI : MonoBehaviour
{
    public GameObject heartPrefab;
    public int maxLives = 3;

    private GameObject[] hearts;

    public void Init()
    {
        hearts = new GameObject[maxLives];

        for (int i = 0; i < maxLives; i++)
        {
            GameObject heart = Instantiate(heartPrefab, transform);
            hearts[i] = heart;
        }
    }

    public void UpdateHearts(int currentLives)
    {
        if (hearts == null || hearts.Length == 0)
        {
            Debug.LogWarning("Hearts array is empty, calling Init()");
            Init();
        }

        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].SetActive(i < currentLives);
        }
    }
}
