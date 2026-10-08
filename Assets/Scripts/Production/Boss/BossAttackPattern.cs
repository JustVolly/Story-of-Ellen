using System.Collections;
using UnityEngine;

/// <summary>
/// Readable Guardian combat loop: approach, anticipation, directional lunge,
/// recovery. Later phases trade windup time for speed, rather than spamming
/// untelegraphed impulses.
/// </summary>
public class BossAttackPattern : MonoBehaviour
{
    [SerializeField] private BossController boss;
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private float phaseOneSpeed = 2f;
    [SerializeField] private float phaseTwoSpeed = 3.5f;
    [SerializeField] private float phaseThreeSpeed = 5f;
    [SerializeField, Min(0.2f)] private float attackInterval = 1.4f;
    [SerializeField, Min(0.1f)] private float lungeForce = 7f;
    [SerializeField, Min(1f)] private float engageDistance = 24f;
    [SerializeField, Min(0.1f)] private float approachSpeed = 1.75f;
    [SerializeField, Range(0.15f, 1f)] private float telegraphDuration = 0.55f;
    [SerializeField, Range(0.1f, 1.5f)] private float chargeDuration = 0.45f;
    [SerializeField, Range(0.1f, 1.5f)] private float recoveryDuration = 0.7f;
    [SerializeField] private Color warningTint = new Color(1f, 0.38f, 0.25f, 1f);

    private SpriteRenderer sprite;
    private Color originalColor;
    private Coroutine attackLoop;
    private bool attackInProgress;

    private void Awake()
    {
        if (boss == null) boss = GetComponent<BossController>();
        if (body == null) body = GetComponent<Rigidbody2D>();
        sprite = GetComponentInChildren<SpriteRenderer>(true);
        if (sprite != null) originalColor = sprite.color;
        FindPlayer();
    }

    private void OnEnable()
    {
        if (attackLoop == null) attackLoop = StartCoroutine(AttackLoop());
    }

    private void OnDisable()
    {
        if (attackLoop != null) StopCoroutine(attackLoop);
        attackLoop = null;
        attackInProgress = false;
        if (sprite != null) sprite.color = originalColor;
    }

    private void FixedUpdate()
    {
        if (attackInProgress || body == null) return;
        if (player == null) FindPlayer();

        float horizontal = 0f;
        if (player != null && Mathf.Abs(player.position.x - transform.position.x) < engageDistance)
        {
            float dx = player.position.x - transform.position.x;
            if (Mathf.Abs(dx) > 3f)
                horizontal = Mathf.Sign(dx) * approachSpeed;
        }

        body.linearVelocity = new Vector2(horizontal, body.linearVelocity.y);
    }

    private IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(0.75f);

        while (true)
        {
            yield return new WaitForSeconds(Mathf.Max(0.2f, attackInterval));
            if (player == null) FindPlayer();
            if (player == null || body == null || boss == null || boss.GetComponent<EnemyHealth>().IsDead)
                continue;

            float delta = player.position.x - transform.position.x;
            if (Mathf.Abs(delta) > engageDistance || Mathf.Abs(delta) < 1f)
                continue;

            attackInProgress = true;
            float direction = Mathf.Sign(delta);

            // Telegraph direction before committing to the charge. Players can
            // dodge during this window; the lunge does not retarget them.
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            if (sprite != null) sprite.color = warningTint;
            float windup = Mathf.Max(0.15f, telegraphDuration -
                0.08f * (int)boss.CurrentPhase);
            yield return new WaitForSeconds(windup);
            if (sprite != null) sprite.color = originalColor;

            float speed = boss.CurrentPhase == BossController.Phase.One ? phaseOneSpeed
                : boss.CurrentPhase == BossController.Phase.Two ? phaseTwoSpeed
                : phaseThreeSpeed;

            body.linearVelocity = new Vector2(direction * speed, body.linearVelocity.y);
            if (boss.CurrentPhase == BossController.Phase.Three)
                body.AddForce(new Vector2(direction * lungeForce, lungeForce * 0.35f),
                    ForceMode2D.Impulse);

            yield return new WaitForSeconds(chargeDuration);
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            yield return new WaitForSeconds(recoveryDuration);
            attackInProgress = false;
        }
    }

    private void FindPlayer()
    {
        GameObject target = GameObject.FindGameObjectWithTag("Player");
        player = target != null ? target.transform : null;
    }
}
