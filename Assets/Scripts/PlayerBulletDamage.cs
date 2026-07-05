using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerBulletDamage : MonoBehaviour
{

    public float GravityScale = 5f;
    public bool isSnailhitted = false;
    public bool isPlayingAnim;
   
   
    EnemyDamage enemyDamage;
    public ScriptableObject scriptableObject;

    private void Start()
    {
        enemyDamage = FindObjectOfType<EnemyDamage>();
      
        scriptableObject.GetComponent<CollectableCoins>().experience = 7;
    }

    void OnTriggerEnter2D(Collider2D other) 
{
    if(other.gameObject.tag == "Enemy")
    {
         
         Destroy(other.gameObject,4f);
        //bullet
         gameObject.GetComponent<CircleCollider2D>().isTrigger = false;
         gameObject.GetComponent<Rigidbody2D>().gravityScale = 5f;
         
         //enemy
         other.GetComponent<Animator>().SetBool("hit",true);
         other.gameObject.GetComponentInChildren<ParticleSystem>().Play();
         Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
         rb.bodyType = RigidbodyType2D.Dynamic;
         other.GetComponent<Rigidbody2D>().simulated = true;        
         other.GetComponent<BoxCollider2D>().isTrigger = true;
         
         
         Debug.Log("Player mermisi temas etdi");
         Destroy(gameObject);

    }

    if (other.gameObject.tag == "Snail")
    {
      other.GetComponent<EnemyDamage>().isPlaySnailHitted = true;
       
       
       Debug.Log("Mermi Snail değdi");
       isPlayingAnim = other.GetComponent<Animator>().GetBool("hitted");
         
       other.GetComponent<PolygonCollider2D>().isTrigger = false;
       StartCoroutine(WaitBullet());
       other.GetComponent<PolygonCollider2D>().isTrigger = true;
        
       if (!isPlayingAnim)
        {
           other.GetComponent<EnemyMovement>().enabled = false;
           gameObject.GetComponent<CircleCollider2D>().isTrigger = false;
           gameObject.GetComponent<Rigidbody2D>().gravityScale = 5f;
           other.GetComponent<Animator>().SetBool("hitted",true);
           other.GetComponent<Animator>().SetBool("walk",false);
           
          
           isSnailhitted = true;
           Debug.Log("Animasyon oynuyor" + isPlayingAnim);
           
           Destroy(gameObject); 
            
        }

        else
        {
            
            gameObject.GetComponent<CircleCollider2D>().isTrigger = true;
            Debug.Log("Fonksiyondan çıkıldı");
            
        }
       
        
    }

        
        
        
    
}

IEnumerator WaitBullet()
{

    yield return new WaitForSeconds(1f);
    

}

  void OnCollisionEnter2D(Collision2D collision) 
    {
        if(collision.gameObject.tag == "Ground")
        {
            GetComponent<PolygonCollider2D>().isTrigger = false;

        }
    }


}
