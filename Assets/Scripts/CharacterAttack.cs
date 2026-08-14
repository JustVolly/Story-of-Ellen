using UnityEngine;

public class CharacterAttack : MonoBehaviour
{
    [Header("Bullet")]
    public GameObject Bullet;
    [SerializeField] private ParticleSystem BulletParticle;
    public Transform FirePoint;
    public float BulletForcing = 7f;
    public int AllBullet = 5;
    public int CurrentBullet;
    public int NumberConfinerofBullet = 5;

    private PlayerMovement playerMovement;
    private LevelUp levelUp;

    private void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        levelUp = FindObjectOfType<LevelUp>();

        if (BulletParticle != null)
        {
            BulletParticle.Stop();
        }

        CurrentBullet = Mathf.Clamp(AllBullet, 0, NumberConfinerofBullet);
    }

    private void Update()
    {
        CurrentBullet = Mathf.Clamp(CurrentBullet, 0, NumberConfinerofBullet);
    }

    public void AttackStart()
    {
        if (levelUp != null && levelUp.isFinish)
        {
            return;
        }

        if (CurrentBullet <= 0)
        {
            return;
        }

        if (!CanFire())
        {
            return;
        }

        Vector2 fireDirection = playerMovement != null && playerMovement.isFacingRight
            ? Vector2.right
            : Vector2.left;

        GameObject spawnedBullet = Instantiate(Bullet, FirePoint.position, FirePoint.rotation);

        if (!spawnedBullet.TryGetComponent(out Rigidbody2D bulletRigidbody))
        {
            Debug.LogError(
                $"{nameof(CharacterAttack)}: Bullet prefab requires a Rigidbody2D component.",
                spawnedBullet
            );
            Destroy(spawnedBullet);
            return;
        }

        CurrentBullet--;
        BulletParticle?.Play();
        bulletRigidbody.AddForce(fireDirection * BulletForcing, ForceMode2D.Impulse);

        Destroy(spawnedBullet, 4f);
    }

    private bool CanFire()
    {
        if (Bullet == null)
        {
            Debug.LogError($"{nameof(CharacterAttack)}: Bullet prefab is not assigned.", this);
            return false;
        }

        if (FirePoint == null)
        {
            Debug.LogError($"{nameof(CharacterAttack)}: FirePoint is not assigned.", this);
            return false;
        }

        return true;
    }
}
