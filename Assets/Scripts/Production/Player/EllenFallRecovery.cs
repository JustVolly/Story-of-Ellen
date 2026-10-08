using UnityEngine;

/// <summary>
/// Last-resort anti-softlock when Ellen falls beneath the authored platform
/// layout. Uses the normal death/respawn pipeline so checkpoints still apply.
/// </summary>
[RequireComponent(typeof(PlayerHealth))]
public sealed class EllenFallRecovery : MonoBehaviour
{
    [SerializeField] private float deathPlaneY = -85f;

    private PlayerHealth health;

    private void Awake() => health = GetComponent<PlayerHealth>();

    private void LateUpdate()
    {
        if (health != null && health.isAlive && transform.position.y < deathPlaneY)
            health.Kill();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.25f, 0.8f);
        Vector3 center = new Vector3(transform.position.x, deathPlaneY, 0f);
        Gizmos.DrawLine(center + Vector3.left * 50f, center + Vector3.right * 50f);
    }
#endif
}
