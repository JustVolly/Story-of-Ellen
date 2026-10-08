using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerRespawnController : MonoBehaviour
{
    [SerializeField, Min(0f)] private float respawnDelay = 1.25f;
    [SerializeField] private Transform fallbackRespawnPoint;

    private PlayerHealth health;
    private bool respawning;
    private Vector3 fallbackPosition;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        fallbackPosition = transform.position;
    }

    private void OnEnable()
    {
        if (health != null) health.Died += HandleDeath;
    }

    private void OnDisable()
    {
        if (health != null) health.Died -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (!respawning) StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        respawning = true;
        yield return new WaitForSecondsRealtime(respawnDelay);

        Transform checkpoint = GameSession.Instance != null ? GameSession.Instance.ActiveCheckpoint : null;
        Transform target = checkpoint != null ? checkpoint : fallbackRespawnPoint;

        // Clear ability locks *before* resetting health and restoring physics.
        GetComponent<PlayerAdvancedMovement>()?.ResetForRespawn();
        transform.position = target != null ? target.position : fallbackPosition;
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body != null)
        {
            // Clear inherited fall/dash momentum before control is restored.
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
        health.ResetHealth();

        PlayerMovement movement = GetComponent<PlayerMovement>();
        movement?.ResetForRespawn();
        if (movement != null && movement.CharacterAnimator != null)
        {
            movement.CharacterAnimator.SetBool("fall", false);
            movement.CharacterAnimator.SetBool("idle", true);
        }

        respawning = false;
    }
}
