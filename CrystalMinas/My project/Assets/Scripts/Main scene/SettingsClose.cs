using UnityEngine;

public class SettingsClose : MonoBehaviour
{
    public void CloseSettings()
    {
        GameObject panel = GameObject.Find("SettingsUI(Clone)");
        if (panel != null)
        {
            panel.SetActive(false);
            Debug.Log("SettingsClose: Панель скрыта");
        }
        else
        {
            Debug.LogWarning("SettingsClose: Клон панели не найден");
        }
    }
}
