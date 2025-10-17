using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio")]
    public AudioMixer audioMixer;
    public string mixerParameter = "SFXVolume";
    public AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("[SoundManager] AudioSource is missing on this GameObject.");
            }
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[SoundManager] Clip is null.");
            return;
        }
        if (audioSource == null)
        {
            Debug.LogWarning("[SoundManager] AudioSource is null.");
            return;
        }
        audioSource.PlayOneShot(clip);
    }

    public void PlayLoop(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[SoundManager] Clip is null.");
            return;
        }
        if (audioSource == null)
        {
            Debug.LogWarning("[SoundManager] AudioSource is null.");
            return;
        }
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void Stop()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("[SoundManager] AudioSource is null.");
            return;
        }
        audioSource.Stop();
        audioSource.clip = null;
        audioSource.loop = false;
    }

    public void SetVolume(float sliderValue)
    {
        float dB = Mathf.Lerp(-80f, 0f, sliderValue);
        Debug.Log($"[SoundManager] SetVolume: slider={sliderValue} => dB={dB}");
        audioMixer.SetFloat(mixerParameter, dB);
    }

    public void Mute(bool isMuted)
    {
        float dB = isMuted ? -80f : 0f;
        Debug.Log($"[SoundManager] Mute: isMuted={isMuted}, dB={dB}");
        audioMixer.SetFloat(mixerParameter, dB);
    }
}
