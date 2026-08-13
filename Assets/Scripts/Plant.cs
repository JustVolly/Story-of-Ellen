using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Plants
{
     public class Plant : MonoBehaviour
{
    public Transform Player;
    [SerializeField] GameObject Bullet;
    public GameObject DestroyableBullet;
   
    [SerializeField] Transform ShootPos;

    
    [SerializeField] float DistancetoPlayer;
   
    [Range(0, 600)]
    [SerializeField] float ForcePower;

    [Range(0, 6)]
    [SerializeField] float BulletDestroyTime;

    [Range(0, 10)]
    [SerializeField] float ShootingRange;

    public int Gravity;
   
    [Range(0, 10)]
    [SerializeField] float StartTime;


    
   

    
    
    private bool canShoot = true;
    
    
    Animator PlantAnim;

    DefencePowerUp defencePowerUp;
    BulletDamage bulletDamage;
    PowerUps powerUps;


    private void Start()
    {
        
        defencePowerUp = FindObjectOfType<DefencePowerUp>();
        bulletDamage = FindObjectOfType<BulletDamage>();
        powerUps = FindObjectOfType<PowerUps>();
        StartTime = Time.time;
        PlantAnim = gameObject.GetComponent<Animator>();

        
    }


    private void Update()
    {
        DistancetoPlayer = Vector3.Distance(transform.position, Player.position);

        Shoot();

        
    }

    public IEnumerator CanShoot()
    {
        canShoot = false; 
        
        PlantAnim.SetBool("attack", true);
        PlantAnim.SetBool("idle", false);
        
        DestroyableBullet = Instantiate(Bullet, ShootPos.position, Quaternion.identity);
        Rigidbody2D bulletRb = DestroyableBullet.GetComponent<Rigidbody2D>();
        bulletRb.linearVelocity = new Vector2(-ForcePower, 0);

        
        
        Destroy(DestroyableBullet, BulletDestroyTime);

        

        yield return new WaitForSeconds(ShootingRange);

        canShoot = true;
    }

    public void Shoot()
    {
        if (canShoot && DistancetoPlayer < 30)
        {
            StartCoroutine(CanShoot());
        }
        
        else
        {
            PlantAnim.SetBool("idle", true);
            PlantAnim.SetBool("attack", false);
        }
    }


}
   
    
}

