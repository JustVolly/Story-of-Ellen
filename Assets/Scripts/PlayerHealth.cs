using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHealth = 3;
    [SerializeField, Min(0f)] private float damageInvulnerabilityDuration = 0.75f;
    [SerializeField] private float GravityScale = 1f;

    public int currenthealth;
    public bool isAlive = true;
    public int MaxHealth => maxHealth;
    public bool IsInvulnerable => invulnerabilityTimer > 0f;

    public event Action<int, int> HealthChanged;
    public event Action Died;

    private PlayerMovement playerMovement;
    private Rigidbody2D playerRigid;
    private BoxCollider2D playerBoxCollider;
    private CapsuleCollider2D playerCapsuleCollider;
    private float invulnerabilityTimer;
    private bool deathApplied;
    private float initialGravityScale;
    private bool initialBoxTrigger;
    private bool initialCapsuleTrigger;
    private bool initialCompositeTrigger;

    public CompositeCollider2D CompositeCollider;

    private void Awake()
    {
        playerRigid = GetComponent<Rigidbody2D>();
        playerBoxCollider = GetComponent<BoxCollider2D>();
        playerCapsuleCollider = GetComponent<CapsuleCollider2D>();
        playerMovement = GetComponent<PlayerMovement>();
        if (playerRigid != null) initialGravityScale = playerRigid.gravityScale;
        if (playerBoxCollider != null) initialBoxTrigger = playerBoxCollider.isTrigger;
        if (playerCapsuleCollider != null) initialCapsuleTrigger = playerCapsuleCollider.isTrigger;
        if (CompositeCollider != null) initialCompositeTrigger = CompositeCollider.isTrigger;
    }

    private void Start()
    {
        ResetHealth();
    }

    private void Update()
    {
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer = Mathf.Max(0f, invulnerabilityTimer - Time.deltaTime);
        }
    }

    public bool TakeDamage(int amount = 1, bool ignoreInvulnerability = false)
    {
        if (!isAlive || amount <= 0 || (!ignoreInvulnerability && IsInvulnerable))
        {
            return false;
        }

        currenthealth = Mathf.Clamp(currenthealth - amount, 0, maxHealth);
        invulnerabilityTimer = damageInvulnerabilityDuration;
        HealthChanged?.Invoke(currenthealth, maxHealth);

        if (currenthealth <= 0)
        {
            ApplyDeath();
        }

        return true;
    }

    public void DecreaseHealth()
    {
        TakeDamage();
    }

    public void Kill()
    {
        if (!isAlive) return;
        currenthealth = 0;
        HealthChanged?.Invoke(currenthealth, maxHealth);
        ApplyDeath();
    }

    public void Heal(int amount = 1)
    {
        if (!isAlive || amount <= 0)
        {
            return;
        }

        currenthealth = Mathf.Clamp(currenthealth + amount, 0, maxHealth);
        HealthChanged?.Invoke(currenthealth, maxHealth);
    }

    public void ResetHealth()
    {
        currenthealth = maxHealth;
        isAlive = true;
        deathApplied = false;
        invulnerabilityTimer = 0f;
        if (playerRigid != null) { playerRigid.gravityScale = initialGravityScale; playerRigid.linearVelocity = Vector2.zero; }
        if (playerBoxCollider != null) playerBoxCollider.isTrigger = initialBoxTrigger;
        if (playerCapsuleCollider != null) playerCapsuleCollider.isTrigger = initialCapsuleTrigger;
        if (CompositeCollider != null) CompositeCollider.isTrigger = initialCompositeTrigger;
        HealthChanged?.Invoke(currenthealth, maxHealth);
    }

    private void ApplyDeath()
    {
        if (deathApplied)
        {
            return;
        }

        deathApplied = true;
        isAlive = false;

        if (playerMovement != null && playerMovement.CharacterAnimator != null)
        {
            playerMovement.CharacterAnimator.SetBool("fall", true);
            playerMovement.CharacterAnimator.SetBool("idle", false);
        }

        if (CompositeCollider != null)
        {
            CompositeCollider.isTrigger = true;
        }

        if (playerRigid != null)
        {
            playerRigid.gravityScale = GravityScale;
        }

        if (playerBoxCollider != null)
        {
            playerBoxCollider.isTrigger = true;
        }

        if (playerCapsuleCollider != null)
        {
            playerCapsuleCollider.isTrigger = true;
        }

        GameSession.Instance?.RegisterDeath();
        Died?.Invoke();
    }

    public bool isAliving()
    {
        return isAlive;
    }
}
