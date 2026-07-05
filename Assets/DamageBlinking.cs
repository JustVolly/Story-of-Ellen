using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageBlinking : MonoBehaviour
{
    BulletDamage bulletDamage;
    
    
    
    void Start()
    {
      bulletDamage = FindObjectOfType<BulletDamage>();
        
    }

  

}
