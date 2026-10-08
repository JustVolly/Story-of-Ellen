using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Legacy Scene References")]
    public GameObject Player;
    public Transform PlayerTransform;
    [SerializeField] private Transform RespawnPoint;
    public BoxCollider2D Box;
    public PolygonCollider2D Polygon;
    [SerializeField] private ParticleSystem TrapEffect;

    [Header("Horizontal Feel")]
    [SerializeField, Min(0.1f)] private float maxRunSpeed = 8.5f;
    [SerializeField, Min(0.1f)] private float groundAcceleration = 55f;
    [SerializeField, Min(0.1f)] private float groundDeceleration = 70f;
    [SerializeField, Min(0.1f)] private float airAcceleration = 35f;
    [SerializeField, Min(0.1f)] private float airDeceleration = 22f;
    [SerializeField, Range(1f, 2f)] private float boosterSpeedMultiplier = 1.3f;

    [Header("Jump Feel")]
    [SerializeField, Min(0.1f)] private float jumpVelocity = 15.5f;
    [SerializeField, Range(0f, 0.3f)] private float coyoteTime = 0.12f;
    [SerializeField, Range(0f, 0.3f)] private float jumpBufferTime = 0.12f;
    [SerializeField, Range(0.1f, 0.9f)] private float jumpCutMultiplier = 0.5f;
    [SerializeField, Min(1f)] private float fallGravityMultiplier = 1.7f;
    [SerializeField, Min(1f)] private float lowJumpGravityMultiplier = 1.25f;
    [SerializeField, Min(1f)] private float maxFallSpeed = 22f;
    [SerializeField, Min(1)] private int maxJumps = 2;

    public int TouchCountCheck;
    public int RemainingJumping;

    public Rigidbody2D myRigidbody;
    public Animator CharacterAnimator;
    public Transform mytransform;

    public bool isOutOfViewCamera;
    public bool isRunning;
    public bool isGround;
    public bool isinAir;
    public bool isPress_A;
    public bool isPress_D;
    public bool isPress_Up;
    public bool isFacingRight;
    public bool isPressD_Ground;
    public bool isPressA_Ground;

    private ScenesManager scenesManager;
    private PlayerHealth playerHealth;
    private TrapThorns trapThorns;
    private BoosterPowerUp boosterPowerUp;
    private LevelUp levelUp;
    private PowerUps powerUps;
    private TrapofEnemy trapofEnemy;
    private PlayerAdvancedMovement advancedMovement;

    private float coyoteCounter;
    private float jumpBufferCounter;
    private float baseGravityScale;
    private int jumpsUsed;
    private bool jumpHeld;

    private void Awake()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        CharacterAnimator = GetComponent<Animator>();
        mytransform = transform;
        if (Player == null) Player = gameObject;
        if (PlayerTransform == null) PlayerTransform = transform;

        playerHealth = GetComponent<PlayerHealth>();
        advancedMovement = GetComponent<PlayerAdvancedMovement>();

        scenesManager = FindObjectOfType<ScenesManager>();
        trapThorns = FindObjectOfType<TrapThorns>();
        boosterPowerUp = FindObjectOfType<BoosterPowerUp>();
        levelUp = FindObjectOfType<LevelUp>();
        powerUps = FindObjectOfType<PowerUps>();
        trapofEnemy = FindObjectOfType<TrapofEnemy>();

        if (myRigidbody != null) baseGravityScale = myRigidbody.gravityScale;
        maxJumps = Mathf.Max(1, maxJumps);
        RemainingJumping = maxJumps;
    }

    private void Start()
    {
        if (RespawnPoint != null) transform.position = RespawnPoint.position;
        isFacingRight = transform.localScale.x >= 0f;
        advancedMovement?.SetFacing(isFacingRight);
    }

    private void Update()
    {
        if (playerHealth == null || myRigidbody == null) return;

        coyoteCounter = isGround ? coyoteTime : Mathf.Max(0f, coyoteCounter - Time.deltaTime);
        jumpBufferCounter = Mathf.Max(0f, jumpBufferCounter - Time.deltaTime);

        if (!isGround && coyoteCounter <= 0f && jumpsUsed == 0)
            jumpsUsed = 1;

        if (isGround)
        {
            jumpsUsed = 0;
            RemainingJumping = maxJumps;
            isPressD_Ground = false;
            isPressA_Ground = false;
        }
        else
        {
            RemainingJumping = Mathf.Max(0, maxJumps - jumpsUsed);
        }

        TryConsumeBufferedJump();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (playerHealth == null || myRigidbody == null) return;
        if (!playerHealth.isAlive || (advancedMovement != null && advancedMovement.OverridesLegacyMovement)) return;

        ApplyHorizontalMovement();
        ApplyGravityFeel();
    }

    private void ApplyHorizontalMovement()
    {
        float input = 0f;
        if (isPress_D && !isPress_A) input = 1f;
        else if (isPress_A && !isPress_D) input = -1f;

        float speedMultiplier = boosterPowerUp != null && boosterPowerUp.isBooster ? boosterSpeedMultiplier : 1f;
        float targetSpeed = input * maxRunSpeed * speedMultiplier;

        bool accelerating = Mathf.Abs(targetSpeed) > 0.01f;
        float acceleration = isGround
            ? (accelerating ? groundAcceleration : groundDeceleration)
            : (accelerating ? airAcceleration : airDeceleration);

        float nextX = Mathf.MoveTowards(myRigidbody.linearVelocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);
        myRigidbody.linearVelocity = new Vector2(nextX, myRigidbody.linearVelocity.y);

        isRunning = Mathf.Abs(nextX) > 0.1f;
        if (input > 0f) FlipRight();
        else if (input < 0f) FlipLeft();
    }

    private void ApplyGravityFeel()
    {
        float gravity = baseGravityScale;

        if (myRigidbody.linearVelocity.y < -0.01f)
            gravity *= fallGravityMultiplier;
        else if (myRigidbody.linearVelocity.y > 0.01f && !jumpHeld)
            gravity *= lowJumpGravityMultiplier;

        myRigidbody.gravityScale = gravity;

        if (myRigidbody.linearVelocity.y < -maxFallSpeed)
            myRigidbody.linearVelocity = new Vector2(myRigidbody.linearVelocity.x, -maxFallSpeed);
    }

    public void Jump()
    {
        if (playerHealth == null || !playerHealth.isAlive || (levelUp != null && levelUp.isFinish)) return;

        jumpHeld = true;
        isPress_Up = true;
        jumpBufferCounter = jumpBufferTime;
        TryConsumeBufferedJump();
    }

    private void TryConsumeBufferedJump()
    {
        if (jumpBufferCounter <= 0f || playerHealth == null || !playerHealth.isAlive) return;
        if (advancedMovement != null && advancedMovement.OverridesLegacyMovement) return;

        bool groundedJump = isGround || coyoteCounter > 0f;
        bool airJump = !groundedJump && jumpsUsed < maxJumps;

        if (!groundedJump && !airJump) return;

        ExecuteJump(groundedJump);
    }

    private void ExecuteJump(bool groundedJump)
    {
        if (groundedJump)
            jumpsUsed = 1;
        else
            jumpsUsed++;

        RemainingJumping = Mathf.Max(0, maxJumps - jumpsUsed);
        jumpBufferCounter = 0f;
        coyoteCounter = 0f;

        myRigidbody.gravityScale = baseGravityScale;
        myRigidbody.linearVelocity = new Vector2(myRigidbody.linearVelocity.x, jumpVelocity);

        if (CharacterAnimator != null)
        {
            CharacterAnimator.SetBool("idle", false);
            CharacterAnimator.SetBool("run", false);
            CharacterAnimator.SetBool("jump", true);
        }
    }

    public void OnPress_W()
    {
        isPress_Up = true;
        jumpHeld = true;
    }

    public void OnPressUp_W() => ReleaseJump();

    public void UpStop() => ReleaseJump();

    private void ReleaseJump()
    {
        isPress_Up = false;
        jumpHeld = false;

        if (myRigidbody != null && myRigidbody.linearVelocity.y > 0f)
        {
            myRigidbody.linearVelocity = new Vector2(
                myRigidbody.linearVelocity.x,
                myRigidbody.linearVelocity.y * jumpCutMultiplier);
        }
    }

    public void OnButtonDown_D() => isPress_D = true;

    public void OnButtonUp_D()
    {
        isPress_D = false;
        StopRight();
    }

    public void OnButtonDown_A() => isPress_A = true;

    public void OnButtonUp_A()
    {
        isPress_A = false;
        StopLeft();
    }

    public void StopRight()
    {
        isPress_D = false;
        if (advancedMovement != null && advancedMovement.OverridesLegacyMovement) return;
        isRunning = false;
    }

    public void StopLeft()
    {
        isPress_A = false;
        if (advancedMovement != null && advancedMovement.OverridesLegacyMovement) return;
        isRunning = false;
    }

    public void FlipRight()
    {
        SetFacing(true);
    }

    public void FlipLeft()
    {
        SetFacing(false);
    }

    private void SetFacing(bool right)
    {
        if (isFacingRight == right && Mathf.Sign(transform.localScale.x) == (right ? 1f : -1f))
        {
            advancedMovement?.SetFacing(right);
            return;
        }

        isFacingRight = right;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (right ? 1f : -1f);
        transform.localScale = scale;
        advancedMovement?.SetFacing(right);
    }

    private void UpdateAnimator()
    {
        if (CharacterAnimator == null || playerHealth == null || !playerHealth.isAlive) return;

        bool airborne = !isGround;
        CharacterAnimator.SetBool("run", isRunning && !airborne);
        CharacterAnimator.SetBool("jump", airborne);
        CharacterAnimator.SetBool("idle", !isRunning && !airborne);
    }

    private bool DefenceActive()
    {
        bool legacyDefence = trapofEnemy != null && trapofEnemy.isActiveDefence;
        bool effectDefence = powerUps != null && powerUps.DefenderEffect != null && powerUps.DefenderEffect.isPlaying;
        return legacyDefence || effectDefence;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("CheckPoint"))
        {
            TouchCountCheck++;
            if (scenesManager != null) scenesManager.istouchCheckPoint = true;
        }

        if (other.CompareTag("Trap"))
        {
            if (DefenceActive()) return;
            if (TrapEffect != null) TrapEffect.Play();
            playerHealth?.TakeDamage();
        }

        if (other.CompareTag("Enemy") && boosterPowerUp != null && boosterPowerUp.isBooster)
        {
            BoxCollider2D collider = other.GetComponent<BoxCollider2D>();
            if (collider != null) collider.isTrigger = true;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Trap") || DefenceActive()) return;
        if (TrapEffect != null && !TrapEffect.isPlaying) TrapEffect.Play();
        playerHealth?.TakeDamage();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("CheckPoint") && scenesManager != null)
            scenesManager.istouchCheckPoint = false;

        if (other.CompareTag("Trap") && TrapEffect != null)
            TrapEffect.Stop();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Grounds")) return;
        isGround = true;
        jumpsUsed = 0;
        RemainingJumping = maxJumps;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Grounds")) return;
        isGround = false;
        coyoteCounter = coyoteTime;
    }

    private void OnBecameInvisible()
    {
        isOutOfViewCamera = true;
    }

    private void OnBecameVisible()
    {
        isOutOfViewCamera = false;
    }
}
