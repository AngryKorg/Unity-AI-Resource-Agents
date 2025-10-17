using UnityEngine;
using UnityEngine.UI;

public class MusicToggleController : MonoBehaviour
{
    public Toggle musicToggle;

    void Start()
    {
        if (musicToggle == null)
            musicToggle = GetComponent<Toggle>();

        if (musicToggle != null && MusicManager.Instance != null)
        {
            musicToggle.isOn = MusicManager.Instance.IsMusicOn();
            musicToggle.onValueChanged.AddListener(OnToggleChanged);
        }
    }

    void OnToggleChanged(bool isOn)
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.ToggleMusic();
        }
    }
}
