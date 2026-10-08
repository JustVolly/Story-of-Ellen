using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerRespawnController : MonoBehaviour
{
    [SerializeField, Min(0f)] private float respawnDelay = 1.25f;
    [SerializeField] private Transform fallbackRespawnPoint;

    private PlayerHealth health;
    private bool respawning;

    private void Awake() => health = GetComponent<PlayerHealth>();

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

        if (target != null) transform.position = target.position;
        health.ResetHealth();

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null && movement.CharacterAnimator != null)
        {
            movement.CharacterAnimator.SetBool("fall", false);
            movement.CharacterAnimator.SetBool("idle", true);
        }

        respawning = false;
    }
}
