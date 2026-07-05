using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PowerUpSettings", menuName = "PowerUp")]
public class PowerUpSetting : ScriptableObject
{
    
    public int TotalDuration = 10;
    public float TimeSpeed = 4f;
    public float CountTime;
}
