using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScene : MonoBehaviour
{
    public bool isPlay;
    public bool isPressHome;
    public bool isPlayingSound = true;
    public bool isPressSoundButton;

    public Image SoundsImage;
    public Sprite Silent;
    public Sprite Sound;

    [SerializeField] private AudioClip[] Clips;
    [SerializeField] private TextMeshProUGUI playLabel;
    [SerializeField] private GameObject replayLevelOneButton;
    public AudioSource Audio;

    private bool isPressAudioControl;
    private LoaderPanel loaderPanel;

    private void Awake()
    {
        loaderPanel = FindObjectOfType<LoaderPanel>();
    }

    private void Start()
    {
        ConfigureMusic();
        RefreshProgressionUi();
    }

    private void RefreshProgressionUi()
    {
        ProgressionSave.Data data = ProgressionSave.Load();
        bool canContinue = data.highestUnlockedLevel >= 2;

        if (playLabel != null)
            playLabel.text = data.campaignCompleted ? "REPLAY LEVEL 3" : canContinue ? "CONTINUE" : "PLAY";
        if (replayLevelOneButton != null) replayLevelOneButton.SetActive(canContinue);
    }

    private void ConfigureMusic()
    {
        if (Audio == null) return;

        int clipIndex = SceneManager.GetActiveScene().buildIndex == 0 ? 0 : 1;
        if (Clips != null && clipIndex >= 0 && clipIndex < Clips.Length)
            Audio.clip = Clips[clipIndex];

        ProgressionSave.Data data = ProgressionSave.Load();
        float volume = Mathf.Clamp01(data.musicVolume);

        Audio.loop = true;
        Audio.volume = volume;
        isPlayingSound = volume > 0.001f;
        UpdateSoundIcon();

        if (Audio.clip != null && isPlayingSound && !Audio.isPlaying)
            Audio.Play();
    }

    public void PressAudioControl()
    {
        if (isPressAudioControl || Audio == null) return;

        isPressAudioControl = true;
        isPressSoundButton = true;
        SetSoundEnabled(!isPlayingSound);
    }

    public void UpPressAudioControl()
    {
        isPressAudioControl = false;
        isPressSoundButton = false;
    }

    private void SetSoundEnabled(bool enabled)
    {
        isPlayingSound = enabled;

        if (enabled)
        {
            Audio.volume = 1f;
            if (Audio.clip != null && !Audio.isPlaying) Audio.Play();
        }
        else
        {
            Audio.Stop();
            Audio.volume = 0f;
        }

        ProgressionSave.Data data = ProgressionSave.Load();
        data.musicVolume = enabled ? 1f : 0f;
        ProgressionSave.Save(data);
        UpdateSoundIcon();
    }

    private void UpdateSoundIcon()
    {
        if (SoundsImage == null) return;
        SoundsImage.sprite = isPlayingSound ? Sound : Silent;
    }

    public void StartGame()
    {
        if (isPlay) return;

        isPlay = true;
        Time.timeScale = 1f;

        ProgressionSave.Data data = ProgressionSave.Load();
        string targetScene = data.highestUnlockedLevel >= 3
            ? "ThreeScene"
            : data.highestUnlockedLevel >= 2 ? "TwoScene" : "OneScene";

        if (loaderPanel != null)
            loaderPanel.LoadScene(targetScene);
        else
            SceneManager.LoadSceneAsync(targetScene);
    }

    public void StartFromBeginning()
    {
        if (isPlay) return;
        isPlay = true;
        Time.timeScale = 1f;

        if (loaderPanel != null)
            loaderPanel.LoadScene("OneScene");
        else
            SceneManager.LoadSceneAsync("OneScene");
    }

    public void StartNewJourney()
    {
        if (isPlay) return;

        // Reset the *whole* campaign (ranks, collected totals and abilities),
        // while retaining the player's music and SFX preferences.
        ProgressionSave.ResetForNewJourney();

        isPlay = true;
        Time.timeScale = 1f;

        if (loaderPanel != null)
            loaderPanel.LoadScene("OneScene");
        else
            SceneManager.LoadSceneAsync("OneScene");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
