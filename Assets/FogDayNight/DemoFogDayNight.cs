using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoFogDayNight : MonoBehaviour
{
    public GameObject fog;
    public Material dayNightMaterial;
    
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            fog.SetActive(!fog.activeInHierarchy);
            
            float isDay = dayNightMaterial.GetFloat("_IsDay");
            dayNightMaterial.SetFloat("_IsDay", isDay == 0.0f ? 1.0f : 0.0f);
        }
    }
}
