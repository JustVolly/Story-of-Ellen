using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxScrollar : MonoBehaviour
{
    
    public Transform MainCam;     
    Vector3 CameraStartPos;
    float Distance;

    GameObject[] Backgrounds;
    Material[] Materials;
    [SerializeField] float[] BackSpeeds = new float[4];
    float BackFarth;

    [Range(0.01f,0.05f)]
    public float ParallaxSpeed;

    private void Start() 
    {
       MainCam = Camera.main.transform;
       CameraStartPos = MainCam.position;

       int BackCount = transform.childCount;
       Materials = new Material[BackCount];
       Backgrounds = new GameObject[BackCount];

       for (int i = 0; i < BackCount; i++)
       {
          Backgrounds[i] = transform.GetChild(i).gameObject;
          Materials[i] = Backgrounds[i].GetComponent<Renderer>().material;

          
       }
        CalculateBackSpeed(BackCount);
    }  

    void  CalculateBackSpeed(int backCount)
    {
         for (int i = 0; i < backCount; i++)
         {
            if ((Backgrounds[i].transform.position.z - MainCam.position.z) > BackFarth)
            {
               BackFarth = Backgrounds[i].transform.position.z - MainCam.position.z; 
            }
         }

         for (int i = 0; i < backCount; i++)
         {
            BackSpeeds[i] = 1 - (Backgrounds[i].transform.position.z - MainCam.position.z) / BackFarth;
         }

    }

    private void LateUpdate() 
    {
        Distance = MainCam.position.x - CameraStartPos.x;
        transform.position = new Vector3(MainCam.position.x,transform.position.y,0);

        for (int i = 0; i < Backgrounds.Length; i++)
        {
            float speed = BackSpeeds[i] * ParallaxSpeed;
            Materials[i].SetTextureOffset("_MainTex",new Vector2(Distance,0)* speed);
        }
        
    }
    
}


