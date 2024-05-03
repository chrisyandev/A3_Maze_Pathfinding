using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterContainer : MonoBehaviour
{
    public static MasterContainer Instance;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public void SetContainerEnable(bool enable)
    {
        this.gameObject.SetActive(enable);
    }
}
