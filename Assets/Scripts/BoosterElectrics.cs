using UnityEngine;

public class BoosterElectrics : MonoBehaviour
{
    public PowerUpSetting PowerUpSettings;
    public ParticleSystem ElectricEffect;
    public int CurrentDuration;
    public SpriteRenderer CharacterColor;
    public string TargetHexadecimalOfColor;
    public string NormalHexadecimalOfColor;
    public float InterpolationSpeed = 0.7f;

    private float remainingSeconds;
    private float secondsPerTick = 1f;
    private Color originalColor = Color.white;

    // The buff belongs to this persistent controller, not to a pickup that is destroyed.
    public bool IsActive => remainingSeconds > 0f;

    private void Awake()
    {
        if (CharacterColor == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) CharacterColor = player.GetComponentInChildren<SpriteRenderer>();
        }

        if (CharacterColor != null) originalColor = CharacterColor.color;
        CurrentDuration = PowerUpSettings != null ? Mathf.Max(1, PowerUpSettings.TotalDuration) : 10;
        SetEffectActive(false);
    }

    public void ActivateBooster()
    {
        int ticks = PowerUpSettings != null ? Mathf.Max(1, PowerUpSettings.TotalDuration) : 10;
        float speed = PowerUpSettings != null ? Mathf.Max(0.01f, PowerUpSettings.TimeSpeed) : 4f;

        // Preserve the legacy timing: each duration tick used a 10-unit countdown.
        // Time.deltaTime already includes timeScale; do not multiply it twice.
        secondsPerTick = 10f / speed;
        remainingSeconds = ticks * secondsPerTick;
        CurrentDuration = ticks;

        if (CharacterColor != null)
        {
            Color target = Color.red;
            if (!string.IsNullOrEmpty(TargetHexadecimalOfColor))
                ColorUtility.TryParseHtmlString("#" + TargetHexadecimalOfColor.TrimStart('#'), out target);
            CharacterColor.color = target;
        }

        SetEffectActive(true);
    }

    private void Update()
    {
        if (!IsActive) return;

        remainingSeconds = Mathf.Max(0f, remainingSeconds - Time.deltaTime);
        CurrentDuration = Mathf.CeilToInt(remainingSeconds / secondsPerTick);
        if (remainingSeconds <= 0f) StopBooster();
    }

    private void OnDisable()
    {
        StopBooster();
    }

    private void StopBooster()
    {
        remainingSeconds = 0f;
        CurrentDuration = PowerUpSettings != null ? Mathf.Max(1, PowerUpSettings.TotalDuration) : 10;

        if (CharacterColor != null)
        {
            Color normal = originalColor;
            if (!string.IsNullOrEmpty(NormalHexadecimalOfColor))
                ColorUtility.TryParseHtmlString("#" + NormalHexadecimalOfColor.TrimStart('#'), out normal);
            CharacterColor.color = normal;
        }

        SetEffectActive(false);
    }

    private void SetEffectActive(bool active)
    {
        if (ElectricEffect == null) return;
        if (!active) ElectricEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (ElectricEffect.gameObject != gameObject) ElectricEffect.gameObject.SetActive(active);
        if (active) ElectricEffect.Play();
    }
}
