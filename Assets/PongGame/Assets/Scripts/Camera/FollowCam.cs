using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Followcam : MonoBehaviour
{
    [SerializeField]
    private float m_FollowSpeed = 5.0f;

    private GameObject ballObjectRef;

    private Vector3 ballToCamOffset;
    private Vector3 targetPosition;


    private void Awake()
    {
        ballObjectRef = GameObject.FindGameObjectWithTag("Ball");
        if (ballObjectRef)
        {
            ballToCamOffset = ballObjectRef.transform.position - transform.position;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        targetPosition = ballObjectRef.transform.position - ballToCamOffset; 
        transform.position = Vector3.Lerp(transform.position, targetPosition, m_FollowSpeed * Time.deltaTime); 
    }

    private void FixedUpdate()
    {
    }
}
