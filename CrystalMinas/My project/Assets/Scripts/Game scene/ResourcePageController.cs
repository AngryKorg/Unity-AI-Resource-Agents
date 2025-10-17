using UnityEngine;

public class ResourcePageController : MonoBehaviour
{
    public GameObject[] resourcePages;

    void Start()
    {
        ShowPage(0); // При запуске показываем первую страницу
    }

    public void ShowPage(int index)
    {
        for (int i = 0; i < resourcePages.Length; i++)
        {
            resourcePages[i].SetActive(i == index);
        }
    }
}
