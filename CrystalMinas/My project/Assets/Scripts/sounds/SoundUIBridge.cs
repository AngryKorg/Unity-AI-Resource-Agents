using UnityEngine;
using UnityEngine.UI;

public class SoundUIBridge : MonoBehaviour
{
    public Slider volumeSlider;
    public Toggle muteToggle;

    private void Start()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (muteToggle != null)
        {
            muteToggle.onValueChanged.AddListener(OnMuteToggled);
        }
    }

    public void OnVolumeChanged(float value)
    {
        Debug.Log($"[SoundUIBridge] Slider volume set to {value}");
        SoundManager.Instance.SetVolume(value);
    }

    public void OnMuteToggled(bool isOn)
    {
        Debug.Log($"[SoundUIBridge] Mute toggled: {isOn}");
        SoundManager.Instance.Mute(isOn);
    }
}
