using UnityEngine;

public class SettingsButtonRelay : MonoBehaviour
{
    // Этот метод можно привязать к кнопке через инспектор
    public void OnSettingsButtonClicked()
    {
        if (SettingsToggle.Instance != null)
        {
            SettingsToggle.Instance.ToggleSettings();
            Debug.Log("SettingsButtonRelay: вызван ToggleSettings()");
        }
        else
        {
            Debug.LogWarning("SettingsButtonRelay: SettingsToggle не найден в текущей сцене. (Instance = null)");
        }
    }
}
