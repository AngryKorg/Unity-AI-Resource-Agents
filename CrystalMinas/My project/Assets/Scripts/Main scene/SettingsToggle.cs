using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsToggle : MonoBehaviour
{
    public static SettingsToggle Instance;

    public GameObject settingsPrefab;
    private GameObject instance;
    private GameObject panel;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded; // добавляем обработчик смены сцен
    }

    void Start()
    {
        Debug.Log("SettingsToggle: Start() вызван");

        if (settingsPrefab != null)
        {
            instance = Instantiate(settingsPrefab);
            instance.name = "SettingsUI";
            DontDestroyOnLoad(instance);

            panel = instance.transform.Find("Canvas/Panel")?.gameObject;

            if (panel != null)
            {
                panel.SetActive(false);
                Debug.Log("SettingsToggle: Панель создана и скрыта");
            }
            else
            {
                Debug.LogWarning("SettingsToggle: Ключ панели не найден");
            }

            // Назначаем кнопке Close действие
            var closeBtn = instance.transform.Find("Canvas/Panel/Close")?.GetComponent<Button>();
            if (closeBtn != null)
            {
                closeBtn.onClick.AddListener(ToggleSettings);
            }

            // Назначаем кнопке BackToMainMenu действие
            var backBtn = instance.transform.Find("Canvas/Panel/BackToMainButton")?.GetComponent<Button>();
            if (backBtn != null)
            {
                backBtn.onClick.AddListener(() =>
                {
                    SceneManager.LoadScene("StartScene");
                    ToggleSettings();
                });
            }
        }
        else
        {
            Debug.LogWarning("SettingsToggle: Префаб панели не назначен.");
        }
    }

    public void ToggleSettings()
    {
        if (instance != null && panel != null)
        {
            bool isActive = panel.activeSelf;
            panel.SetActive(!isActive);

            if (!isActive)
            {
                instance.transform.SetAsLastSibling();
            }

            Debug.Log("SettingsToggle: панель " + (isActive ? "скрыта" : "отображена"));
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (instance == null) return;

        var backBtn = instance.transform.Find("Canvas/Panel/BackToMainButton");
        if (backBtn != null)
        {
            bool isStartScene = scene.name == "StartScene";
            backBtn.gameObject.SetActive(!isStartScene);
        }
    }
}

