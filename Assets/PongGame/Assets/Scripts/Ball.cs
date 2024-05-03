using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField]
    private float m_Speed = 15.0f;

    private Rigidbody rb;
    private Vector3 lastFrameVelocity;
    private Vector3 directionBeforeCollision;
    private Vector3 collisionNormal;

    private float lockoutTime;
    private bool canBallMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        SetStartVelocity();
    }

    void Update()
    {
        if ( canBallMove ) 
        {
            lastFrameVelocity = rb.velocity;
        }

        if (!canBallMove && Time.time >= lockoutTime)
        {
            canBallMove = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            int randomDirection = Random.Range(0, 2) * 2 - 1;
            SetStartVelocity();
        }
    }
    private void SetStartVelocity()
    {
        rb.velocity = new Vector3(Random.insideUnitCircle.x, 0, Random.insideUnitCircle.y).normalized * m_Speed;
        while (Mathf.Abs(Vector3.Angle(rb.velocity, Vector3.forward)) < 30f)
        {
            rb.velocity = new Vector3(Random.insideUnitCircle.x, 0, Random.insideUnitCircle.y).normalized * m_Speed;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Player Wall"))
        {
            // Update P2 Score since ball hit Player1 Goal
            PongGameManager.UpdateScore( 2 );
            lockoutTime = Time.time + 1.5f;
            transform.position = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            canBallMove = false;
        }
        else if (collider.gameObject.CompareTag("AI Wall"))
        {
            // Update P1 Score since ball hit Player2 Goal
            PongGameManager.UpdateScore( 1 );
            lockoutTime = Time.time + 1.5f;
            transform.position = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            canBallMove = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        directionBeforeCollision = lastFrameVelocity.normalized;
        collisionNormal = collision.GetContact(0).normal;
    }

    private void OnCollisionStay(Collision collision)
    {
        ForceBounce();
        Debug.Log("OnCollisionStay");
    }

    private void ForceBounce()
    {
        Vector3 bounceDirection = Vector3.Reflect(directionBeforeCollision, collisionNormal);
        rb.velocity = bounceDirection * m_Speed;
    }

    public void SetSpeed(float speed) { // example cmd for console
        if (rb)
        {
            rb.velocity = lastFrameVelocity.normalized * speed;
        }
        m_Speed = speed;
        Debug.Log("BALL SPEED SET!!!! BATCHEST: " + speed);
    }
}
