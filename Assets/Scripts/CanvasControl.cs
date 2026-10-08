using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasControl : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Times;
    [SerializeField] private Image ClockFire;

    [Header("Time")]
    [SerializeField, Min(1)] private int TotalTime = 180;
    [SerializeField, Min(1)] private int TimeSpeed = 1;
    public float Timer = 1f;
    public float DecreaseTimerFillAmount;
    public float DecreaseSpeed = 10f;
    public int CurrentTime;

    [Header("Health Stars")]
    public Image Star1;
    public Image Star2;
    public Image Star3;

    [Header("Controls")]
    [SerializeField] private Button Left;
    [SerializeField] private Button Right;
    [SerializeField] private Button Up;
    [SerializeField] private Button Fire;
    [SerializeField] private Button Stop;

    private TextMeshProUGUI AppleExperience;
    private TextMeshProUGUI BulletStrawberry;
    private int exp_score;
    private bool waitingForRespawn;

    private PlayerHealth playerHealth;
    private EatingFruits eatingFruits;
    private CollectCoins collectCoins;
    private TrapThorns trapThorns;
    private ScenesManager scenesManager;
    private CharacterAttack characterAttack;
    private LevelUp levelUp;

    private void Awake()
    {
        SetStarFill(Star1, 1f);
        SetStarFill(Star2, 1f);
        SetStarFill(Star3, 1f);
    }

    private void Start()
    {
        AppleExperience = FindTaggedComponent<TextMeshProUGUI>("StarExpUI");
        BulletStrawberry = FindTaggedComponent<TextMeshProUGUI>("BulletCount");

        if (Times == null) Times = FindTaggedComponent<TextMeshProUGUI>("Tmer");
        if (ClockFire == null) ClockFire = FindTaggedComponent<Image>("ClockFire");

        CurrentTime = Mathf.Max(1, TotalTime);
        Timer = 1f;

        playerHealth = FindAnyObjectByType<PlayerHealth>();
        eatingFruits = FindAnyObjectByType<EatingFruits>();
        collectCoins = FindAnyObjectByType<CollectCoins>();
        trapThorns = FindAnyObjectByType<TrapThorns>();
        scenesManager = FindAnyObjectByType<ScenesManager>();
        characterAttack = FindAnyObjectByType<CharacterAttack>();
        levelUp = FindAnyObjectByType<LevelUp>();

        if (AppleExperience != null) AppleExperience.text = "0";
        if (Times != null) Times.text = CurrentTime.ToString();
        if (ClockFire != null) ClockFire.fillAmount = 1f;

        if (playerHealth != null)
        {
            playerHealth.HealthChanged += OnHealthChanged;
            SyncHealthUI();
        }

        if (characterAttack != null)
        {
            characterAttack.AmmoChanged += OnAmmoChanged;
            OnAmmoChanged(characterAttack.CurrentBullet, characterAttack.NumberConfinerofBullet);
        }

        UpdateControls();
    }

    private void OnDestroy()
    {
        if (playerHealth != null) playerHealth.HealthChanged -= OnHealthChanged;
        if (characterAttack != null) characterAttack.AmmoChanged -= OnAmmoChanged;
    }

    private void Update()
    {
        CanvasTimer();
        UpdateControls();
    }

    private void OnHealthChanged(int current, int max)
    {
        // The old scene timer reached zero and immediately killed Ellen again
        // after a checkpoint respawn. Give the next attempt its full time.
        if (current <= 0)
            waitingForRespawn = true;
        else if (waitingForRespawn)
        {
            waitingForRespawn = false;
            CurrentTime = Mathf.Max(1, TotalTime);
            Timer = 1f;
            if (Times != null) Times.text = CurrentTime.ToString();
            if (ClockFire != null) ClockFire.fillAmount = 1f;
        }
        SyncHealthUI();
        UpdateControls();
    }

    private void OnAmmoChanged(int current, int max)
    {
        if (BulletStrawberry != null)
            BulletStrawberry.text = current.ToString();
    }

    public void CanvasTimer()
    {
        if (playerHealth == null || !playerHealth.isAlive || (levelUp != null && levelUp.isFinish) || Time.timeScale <= 0f)
            return;

        if (scenesManager != null && scenesManager.isPressStopButton)
            return;

        Timer -= Time.deltaTime * Mathf.Max(1, TimeSpeed);

        bool changed = false;
        while (Timer <= 0f && CurrentTime > 0)
        {
            CurrentTime--;
            Timer += 1f;
            changed = true;
        }

        if (changed && Times != null)
            Times.text = CurrentTime.ToString();

        DecreaseTimerFillAmount = 1f / Mathf.Max(1, TotalTime);
        if (ClockFire != null)
            ClockFire.fillAmount = Mathf.Clamp01((float)CurrentTime / Mathf.Max(1, TotalTime));

        if (CurrentTime <= 0)
            playerHealth.Kill();
    }

    private void UpdateControls()
    {
        bool enabled = playerHealth == null || playerHealth.isAlive;
        SetInteractable(Left, enabled);
        SetInteractable(Right, enabled);
        SetInteractable(Up, enabled);
        SetInteractable(Fire, enabled);
        SetInteractable(Stop, enabled);
    }

    public void DecreaseBullet(int amount)
    {
        if (BulletStrawberry != null) BulletStrawberry.text = Mathf.Max(0, amount).ToString();
    }

    public void IncreaseBullet(int amount)
    {
        if (BulletStrawberry != null) BulletStrawberry.text = Mathf.Max(0, amount).ToString();
    }

    public void IncreaseExperience()
    {
        if (collectCoins == null || AppleExperience == null) return;
        exp_score += collectCoins.experienceValue;
        AppleExperience.text = exp_score.ToString();
    }

    public void IncreaseHealth()
    {
        if (eatingFruits == null || playerHealth == null || !eatingFruits.isEating) return;
        playerHealth.Heal();
    }

    public void FillHealth()
    {
        if (scenesManager != null && scenesManager.isRespawn && playerHealth != null)
            playerHealth.ResetHealth();
    }

    public void TakingDamage()
    {
        SyncHealthUI();
    }

    private void SyncHealthUI()
    {
        if (playerHealth == null) return;

        SetStarFill(Star1, playerHealth.currenthealth >= 3 ? 1f : 0f);
        SetStarFill(Star2, playerHealth.currenthealth >= 2 ? 1f : 0f);
        SetStarFill(Star3, playerHealth.currenthealth >= 1 ? 1f : 0f);
    }

    public void DieImmediate()
    {
        if (trapThorns != null && trapThorns.isTouchingthorn)
            playerHealth?.Kill();
    }

    private static T FindTaggedComponent<T>(string tag) where T : Component
    {
        try
        {
            GameObject target = GameObject.FindWithTag(tag);
            return target != null ? target.GetComponent<T>() : null;
        }
        catch (UnityException)
        {
            return null;
        }
    }

    private static void SetStarFill(Image image, float value)
    {
        if (image != null) image.fillAmount = value;
    }

    private static void SetInteractable(Selectable selectable, bool value)
    {
        if (selectable != null) selectable.interactable = value;
    }
}
