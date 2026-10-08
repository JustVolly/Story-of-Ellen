using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PatrolEnemy : MonoBehaviour
{
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;
    [SerializeField] private float speed = 2f;
    private Rigidbody2D body;
    private bool movingRight = true;

    private void Awake() => body = GetComponent<Rigidbody2D>();

    private void FixedUpdate()
    {
        if (leftPoint == null || rightPoint == null) return;
        float targetX = movingRight ? rightPoint.position.x : leftPoint.position.x;
        float direction = Mathf.Sign(targetX - transform.position.x);
        body.linearVelocity = new Vector2(direction * speed, body.linearVelocity.y);
        if (Mathf.Abs(targetX - transform.position.x) < 0.1f) movingRight = !movingRight;
    }
}
