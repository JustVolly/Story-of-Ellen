using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;




public class EnemyMovement : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public float moveSpeed = 2f;

    private Vector3 currentTarget;
    private bool movingToEndPoint = true;

    public SpriteRenderer spriteRenderer;
    

    private void Start()
    {
        transform.position = startPoint.position;
        currentTarget = endPoint.position;

        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();    
    }

    private void Update()
    {
        MoveEnemy();
    }

    private void MoveEnemy()
    {
        transform.position = Vector3.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);
       

        if (Vector3.Distance(transform.position, currentTarget) < 0.01f)
        {
           
            if (movingToEndPoint)
            {
                currentTarget = startPoint.position;
                spriteRenderer.flipX = true;    
            }
            else
            {
                currentTarget = endPoint.position;
                spriteRenderer.flipX = false;
            }

            movingToEndPoint = !movingToEndPoint;
        }
    }
}












