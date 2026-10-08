using UnityEngine;

public class PlayerMovementFeedback : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAdvancedMovement advancedMovement;
    [SerializeField] private ParticleSystem jumpDust;
    [SerializeField] private ParticleSystem airJumpBurst;
    [SerializeField] private ParticleSystem landingDust;
    [SerializeField] private ParticleSystem dashBurst;
    [SerializeField] private CameraJuice cameraJuice;
    [SerializeField, Min(0f)] private float hardLandingSpeed = 11f;

    private void Awake()
    {
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (advancedMovement == null) advancedMovement = GetComponent<PlayerAdvancedMovement>();
    }

    private void OnEnable()
    {
        if (movement != null)
        {
            movement.Jumped += OnJumped;
            movement.Landed += OnLanded;
        }

        if (advancedMovement != null)
        {
            advancedMovement.DashStarted += OnDashStarted;
            advancedMovement.WallJumped += OnWallJumped;
        }
    }

    private void OnDisable()
    {
        if (movement != null)
        {
            movement.Jumped -= OnJumped;
            movement.Landed -= OnLanded;
        }

        if (advancedMovement != null)
        {
            advancedMovement.DashStarted -= OnDashStarted;
            advancedMovement.WallJumped -= OnWallJumped;
        }
    }

    private void OnJumped(bool airJump)
    {
        ParticleSystem effect = airJump ? airJumpBurst : jumpDust;
        if (effect != null) effect.Play();

        if (airJump)
            cameraJuice?.Shake(0.06f, 0.035f);
    }

    private void OnLanded(float impactSpeed)
    {
        if (landingDust != null) landingDust.Play();

        if (impactSpeed >= hardLandingSpeed)
        {
            float strength = Mathf.InverseLerp(hardLandingSpeed, hardLandingSpeed * 2f, impactSpeed);
            cameraJuice?.Shake(0.08f, Mathf.Lerp(0.025f, 0.08f, strength));
        }
    }

    private void OnDashStarted()
    {
        if (dashBurst != null) dashBurst.Play();
        cameraJuice?.Shake(0.05f, 0.025f);
    }

    private void OnWallJumped()
    {
        if (jumpDust != null) jumpDust.Play();
    }
}
