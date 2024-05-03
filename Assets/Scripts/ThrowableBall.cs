using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableBall : MonoBehaviour
{

    [SerializeField]
    private AudioClip wallClip;
    [SerializeField]
    private AudioClip enemyClip;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        string tag = collision.gameObject.tag;

        switch (tag)
        {
            case "Enemy":
                AudioSource.PlayClipAtPoint( enemyClip, transform.position );
                Debug.Log("Hit enemy");
                break;
            case "Wall":
                audioSource.PlayOneShot( wallClip );
                Debug.Log("Hit wall");
                break;
            default:
                Debug.Log("Hit something else");
                Destroy( gameObject, 1f );
                break;
        }

    }
}
