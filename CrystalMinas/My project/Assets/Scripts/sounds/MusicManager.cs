using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    private AudioSource audioSource;
    private bool musicEnabled = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("MusicManager: AudioSource отсутствует!");
            return;
        }

        audioSource.loop = true;

        musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);

        audioSource.volume = musicEnabled ? savedVolume : 0f;

        if (musicEnabled && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void ToggleMusic()
    {
        musicEnabled = !musicEnabled;
        PlayerPrefs.SetInt("MusicEnabled", musicEnabled ? 1 : 0);
        PlayerPrefs.Save();

        audioSource.volume = musicEnabled ? PlayerPrefs.GetFloat("MusicVolume", 1f) : 0f;
    }

    public void SetVolume(float volume)
    {
        if (!musicEnabled)
        {
            audioSource.volume = 0f;
            return;
        }

        audioSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    public bool IsMusicOn()
    {
        return musicEnabled;
    }
}
