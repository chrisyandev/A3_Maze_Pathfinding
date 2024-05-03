using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPaddle : MonoBehaviour
{

    private GameObject ballGameObjectRef;
    private Ball ballComponentRef;

    [SerializeField]
    private float m_AIPaddleSpeed = 3.0f;

    private bool isPlaying = false;

    private void OnEnable()
    {
        ballGameObjectRef = GameObject.FindGameObjectWithTag("Ball");
        ballComponentRef =  ballGameObjectRef.GetComponent<Ball>();
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (PongGameManager.GetInstance().IsInGame())
        {
            isPlaying = true;
        }
        else
        {
            isPlaying = false;
        }
    }

    private void FixedUpdate()
    {
        if (isPlaying)
        {
            if (ballComponentRef != null)
            {
                float zDeltaFromBall = ballComponentRef.gameObject.transform.position.z - transform.position.z;
                transform.Translate(new Vector3(0, 0, zDeltaFromBall).normalized * m_AIPaddleSpeed * Time.fixedDeltaTime);
                transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Clamp(transform.position.z, -5.5f, 5.5f));
            }
        }
    }
}
