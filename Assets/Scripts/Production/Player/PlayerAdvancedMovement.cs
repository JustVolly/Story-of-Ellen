using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAdvancedMovement : MonoBehaviour
{
    [Header("Dash")]
    [SerializeField] private bool dashUnlocked = true;
    [SerializeField] private float dashSpeed = 16f;
    [SerializeField] private float dashDuration = 0.16f;
    [SerializeField] private float dashCooldown = 0.35f;

    [Header("Wall")]
    [SerializeField] private bool wallJumpUnlocked = true;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckRadius = 0.18f;
    [SerializeField] private Vector2 wallJumpVelocity = new Vector2(8f, 12f);
    [SerializeField, Min(0f)] private float wallJumpControlLock = 0.14f;

    private Rigidbody2D body;
    private float dashTimer;
    private float cooldownTimer;
    private float originalGravity;
    private bool facingRight = true;
    private float legacyMovementLockTimer;
    private PlayerHealth health;

    public bool IsDashing => dashTimer > 0f;
    public bool OverridesLegacyMovement => IsDashing || legacyMovementLockTimer > 0f;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        originalGravity = body.gravityScale;
        health = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        cooldownTimer = Mathf.Max(0f, cooldownTimer - Time.deltaTime);
        legacyMovementLockTimer = Mathf.Max(0f, legacyMovementLockTimer - Time.deltaTime);
        if (dashTimer <= 0f) return;

        dashTimer -= Time.deltaTime;
        if (dashTimer <= 0f && (health == null || health.isAlive)) body.gravityScale = originalGravity;
    }

    public void SetFacing(bool right) => facingRight = right;

    public void Dash()
    {
        if (!dashUnlocked || cooldownTimer > 0f || IsDashing) return;
        dashTimer = dashDuration;
        cooldownTimer = dashCooldown;
        body.gravityScale = 0f;
        body.linearVelocity = new Vector2((facingRight ? 1f : -1f) * dashSpeed, 0f);
    }

    public void WallJump()
    {
        if (!wallJumpUnlocked || wallCheck == null) return;
        Collider2D wall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
        if (wall == null) return;

        float direction = facingRight ? -1f : 1f;
        body.linearVelocity = new Vector2(wallJumpVelocity.x * direction, wallJumpVelocity.y);
        legacyMovementLockTimer = wallJumpControlLock;
        facingRight = direction > 0f;
    }
}
