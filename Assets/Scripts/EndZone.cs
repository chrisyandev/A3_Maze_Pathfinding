using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EndZone : MonoBehaviour
{
    public static GameObject WinTextGameObject;

    public static void HideWinText()
    {
        if ( WinTextGameObject )
        {
            WinTextGameObject.GetComponent<TextMeshProUGUI>().enabled = false;
        }
    }

    private void Awake()
    {
        WinTextGameObject = GameObject.FindGameObjectWithTag( "WinText" );
        if ( WinTextGameObject )
        {
            WinTextGameObject.GetComponent<TextMeshProUGUI>().enabled = false;
        }
    }

    private void OnTriggerEnter( Collider other )
    {
        if ( other.gameObject.CompareTag( "Player" ) )
        {
            if ( WinTextGameObject )
            {
                WinTextGameObject.GetComponent<TextMeshProUGUI>().enabled = true;
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if ( other.gameObject.CompareTag( "Player" ) )
        {
            if ( WinTextGameObject )
            {
                WinTextGameObject.GetComponent<TextMeshProUGUI>().enabled = false;
            }
        }

    }
}