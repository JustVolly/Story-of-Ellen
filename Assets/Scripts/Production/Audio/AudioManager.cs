using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null || musicSource.clip == clip) return;
        musicSource.clip = clip; musicSource.loop = true; musicSource.Play();
    }

    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip, volume);
    }

    public void SetMusicVolume(float value) { if (musicSource != null) musicSource.volume = Mathf.Clamp01(value); }
    public void SetSfxVolume(float value) { if (sfxSource != null) sfxSource.volume = Mathf.Clamp01(value); }
}
