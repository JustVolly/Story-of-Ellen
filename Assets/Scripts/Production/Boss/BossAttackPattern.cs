using System.Collections;
using UnityEngine;

public class BossAttackPattern : MonoBehaviour
{
    [SerializeField] private BossController boss;
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private float phaseOneSpeed = 2f;
    [SerializeField] private float phaseTwoSpeed = 3.5f;
    [SerializeField] private float phaseThreeSpeed = 5f;
    [SerializeField] private float attackInterval = 1.4f;
    [SerializeField] private float lungeForce = 7f;

    private Coroutine attackLoop;

    private void Awake()
    {
        if (boss == null) boss = GetComponent<BossController>();
        if (body == null) body = GetComponent<Rigidbody2D>();
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
        }
    }

    private void OnEnable()
    {
        if (attackLoop == null) attackLoop = StartCoroutine(AttackLoop());
    }

    private void OnDisable()
    {
        if (attackLoop != null) StopCoroutine(attackLoop);
        attackLoop = null;
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Mathf.Max(0.1f, attackInterval));
            if (player == null || body == null || boss == null) continue;

            float direction = Mathf.Sign(player.position.x - transform.position.x);
            float speed = boss.CurrentPhase == BossController.Phase.One ? phaseOneSpeed
                : boss.CurrentPhase == BossController.Phase.Two ? phaseTwoSpeed : phaseThreeSpeed;

            body.linearVelocity = new Vector2(direction * speed, body.linearVelocity.y);
            if (boss.CurrentPhase == BossController.Phase.Three)
                body.AddForce(new Vector2(direction * lungeForce, lungeForce * 0.35f), ForceMode2D.Impulse);
        }
    }
}
