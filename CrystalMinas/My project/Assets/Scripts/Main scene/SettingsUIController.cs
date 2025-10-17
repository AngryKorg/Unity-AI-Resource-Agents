using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsUIController : MonoBehaviour
{
    public Button backToMainButton;

    private GameObject panel;

    void Start()
    {
        // Находим панель (предполагается, что она — прямой потомок)
        panel = transform.Find("Canvas/Panel")?.gameObject;

        // Скрываем кнопку возврата, если мы уже в StartScene
        if (SceneManager.GetActiveScene().name == "StartScene" && backToMainButton != null)
        {
            backToMainButton.gameObject.SetActive(false);
        }

        if (panel != null)
        {
            panel.SetActive(false); // Панель скрыта по умолчанию
        }
    }

    void Update()
    {
        // Обработка клавиши ESC
        if (Input.GetKeyDown(KeyCode.Escape) && panel != null)
        {
            panel.SetActive(!panel.activeSelf);
        }
    }

    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene("StartScene");
    }
}
