using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject settingsUIPrefab;
    private static GameObject instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = Instantiate(settingsUIPrefab);
            DontDestroyOnLoad(instance);
            instance.SetActive(false); // По умолчанию скрываем настройки
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ToggleSettings()
    {
        if (instance != null)
        {
            bool isActive = instance.activeSelf;
            instance.SetActive(!isActive); // Переключаем видимость
        }
    }
}