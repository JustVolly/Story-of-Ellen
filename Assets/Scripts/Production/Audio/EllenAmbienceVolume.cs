using UnityEngine;

// Ambient loops are scene-local, while audio preferences persist across scenes.
// Unlike the menu's music source this is managed per-level and should honor mute.
[RequireComponent(typeof(AudioSource))]
public sealed class EllenAmbienceVolume : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float baseVolume = 0.15f;

    private void Awake()
    {
        AudioSource source = GetComponent<AudioSource>();
        ProgressionSave.Data settings = ProgressionSave.Load();
        source.volume = baseVolume * Mathf.Clamp01(settings.sfxVolume);
    }
}
