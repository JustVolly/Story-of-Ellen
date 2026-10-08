using UnityEngine;

/// <summary>Deterministic kinematic platform movement for Level 3 challenges.</summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public sealed class EllenCryptMovingPlatform : MonoBehaviour
{
    [SerializeField] private Vector2 travel = new Vector2(0f, 3f);
    [SerializeField, Min(1.5f)] private float period = 4f;
    [SerializeField] private float phase;

    private Rigidbody2D body;
    private Vector2 home;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        home = body.position;
    }

    private void FixedUpdate()
    {
        float wave = Mathf.Sin((2f * Mathf.PI * Time.fixedTime / period) + phase);
        body.MovePosition(home + travel * wave);
    }
}
