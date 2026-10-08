using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ChargerEnemy : MonoBehaviour
{
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float chargeSpeed = 7f;
    [SerializeField] private LayerMask playerLayer;
    private Rigidbody2D body;

    private void Awake() => body = GetComponent<Rigidbody2D>();

    private void FixedUpdate()
    {
        Collider2D player = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        if (player == null) { body.linearVelocity = new Vector2(0f, body.linearVelocity.y); return; }
        float direction = Mathf.Sign(player.transform.position.x - transform.position.x);
        body.linearVelocity = new Vector2(direction * chargeSpeed, body.linearVelocity.y);
    }
}
