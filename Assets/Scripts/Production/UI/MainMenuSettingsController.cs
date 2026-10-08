using UnityEngine;
using UnityEngine.UI;

public class MainMenuSettingsController : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private UIPanelTransition panelTransition;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private StartScene startScene;

    private bool initialized;

    private void Awake()
    {
        if (startScene == null) startScene = FindObjectOfType<StartScene>();
        if (panelTransition == null && panel != null) panelTransition = panel.GetComponent<UIPanelTransition>();

        ProgressionSave.Data data = ProgressionSave.Load();

        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.SetValueWithoutNotify(Mathf.Clamp01(data.musicVolume));
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.SetValueWithoutNotify(Mathf.Clamp01(data.sfxVolume));
        }

        initialized = true;

        if (panel != null)
            panel.SetActive(false);
    }

    public void Open()
    {
        if (panel == null) return;

        panel.SetActive(true);
        if (panelTransition != null) panelTransition.Show();
    }

    public void Close()
    {
        if (panel == null) return;

        if (panelTransition != null) panelTransition.Hide();
        else panel.SetActive(false);
    }

    public void SetMusicVolume(float value)
    {
        if (!initialized) return;

        value = Mathf.Clamp01(value);
        ProgressionSave.Data data = ProgressionSave.Load();
        data.musicVolume = value;
        ProgressionSave.Save(data);

        if (startScene != null && startScene.Audio != null)
        {
            startScene.Audio.volume = value;
            if (value <= 0.001f)
                startScene.Audio.Pause();
            else if (!startScene.Audio.isPlaying)
                startScene.Audio.UnPause();
        }

        AudioManager.Instance?.SetMusicVolume(value);
    }

    public void SetSfxVolume(float value)
    {
        if (!initialized) return;

        value = Mathf.Clamp01(value);
        ProgressionSave.Data data = ProgressionSave.Load();
        data.sfxVolume = value;
        ProgressionSave.Save(data);

        AudioManager.Instance?.SetSfxVolume(value);
    }
}
