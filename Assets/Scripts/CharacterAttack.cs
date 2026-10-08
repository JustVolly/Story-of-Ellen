using System;
using UnityEngine;

public class CharacterAttack : MonoBehaviour
{
    [Header("Bullet")]
    public GameObject Bullet;
    [SerializeField] private ParticleSystem BulletParticle;
    public Transform FirePoint;
    [SerializeField, Min(0.1f)] public float BulletForcing = 7f;
    [SerializeField, Min(0)] public int AllBullet = 5;
    public int CurrentBullet;
    [SerializeField, Min(0)] public int NumberConfinerofBullet = 5;

    public event Action<int, int> AmmoChanged;

    private PlayerMovement playerMovement;
    private LevelUp levelUp;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null) playerMovement = FindObjectOfType<PlayerMovement>();
        levelUp = FindObjectOfType<LevelUp>();

        if (BulletParticle != null) BulletParticle.Stop();

        NumberConfinerofBullet = Mathf.Max(0, NumberConfinerofBullet);
        CurrentBullet = Mathf.Clamp(AllBullet, 0, NumberConfinerofBullet);
        NotifyAmmoChanged();
    }

    public void AttackStart()
    {
        if (Time.timeScale <= 0f || (levelUp != null && levelUp.isFinish)) return;
        if (CurrentBullet <= 0) return;

        if (playerMovement == null || Bullet == null || FirePoint == null)
        {
            Debug.LogError("[CharacterAttack] Missing PlayerMovement, Bullet prefab, or FirePoint.", this);
            return;
        }

        GameObject projectile = Instantiate(Bullet, FirePoint.position, FirePoint.rotation);
        Rigidbody2D body = projectile.GetComponent<Rigidbody2D>();

        if (body == null)
        {
            Debug.LogError("[CharacterAttack] Bullet prefab requires Rigidbody2D.", projectile);
            Destroy(projectile);
            return;
        }

        CurrentBullet = Mathf.Clamp(CurrentBullet - 1, 0, NumberConfinerofBullet);
        NotifyAmmoChanged();

        if (BulletParticle != null) BulletParticle.Play();

        Vector2 direction = playerMovement.isFacingRight ? Vector2.right : Vector2.left;
        body.AddForce(direction * BulletForcing, ForceMode2D.Impulse);
        Destroy(projectile, 4f);
    }

    public void AddAmmo(int amount = 1, bool increaseCapacity = false)
    {
        if (amount <= 0) return;

        if (increaseCapacity)
            NumberConfinerofBullet = Mathf.Max(0, NumberConfinerofBullet + amount);

        CurrentBullet = Mathf.Clamp(CurrentBullet + amount, 0, NumberConfinerofBullet);
        NotifyAmmoChanged();
    }

    public void RefillAmmo()
    {
        CurrentBullet = NumberConfinerofBullet;
        NotifyAmmoChanged();
    }

    private void NotifyAmmoChanged()
    {
        AmmoChanged?.Invoke(CurrentBullet, NumberConfinerofBullet);
    }
}
