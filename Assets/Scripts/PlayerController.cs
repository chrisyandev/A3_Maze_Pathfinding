using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public event Action OnDie;

    private CharacterController controller;
    private Vector2 moveInput;

    public float speed;

    private Vector3 playerVelocity;
    private bool grounded;
    public float gravity = -9.8f;
    public float jumpForce = 2f;

    public Camera cam;
    private Vector2 lookPos;
    private float xRotation = 0f;
    public float xSens = 30f;
    public float ySens = 30f;

    private GameObject lastWallHit;

    [SerializeField]
    private AudioSource wallCollision;
    [SerializeField]
    private AudioSource deathSound;

    private GameObject flashCone;

    [SerializeField]
    private Material flashDay;

    [SerializeField]
    private Material flashNight;

    [SerializeField]
    private GameObject ballPrefab;

    public GameObject SaveLoadButtonBox;

    private void Awake()
    {
        flashCone = GameObject.Find("FlashCone");
    }

    public void OnMove( InputAction.CallbackContext context )
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump( InputAction.CallbackContext context )
    {
        jump();
    }    
    
    public void ResetLookGamePad(InputAction.CallbackContext context)
    {
        lookPos = new Vector2(0, 0);
    }

    public void OnLook( InputAction.CallbackContext context )
    {
        lookPos = context.ReadValue<Vector2>();
    }

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        grounded = controller.isGrounded;
        movePlayer();
        playerLook();

        if (Input.GetKeyDown(KeyCode.E) || Input.GetButtonDown("NoClip"))
        {
            gameObject.layer = LayerMask.NameToLayer("Ignore Walls");
        }
        if (Input.GetKeyUp(KeyCode.E) || Input.GetButtonUp("NoClip"))
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ThrowBall();
        }
        if (Input.GetKeyDown(KeyCode.F)
        //|| Input.GetButtonDown("Flashlight")
        )
        {
            ToggleFlashlight();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleSaveLoadButtonBox();
        }

        lookPos.y = Input.GetAxis("Vertical") * (ySens);
        lookPos.x = Input.GetAxis("Horizontal") * xSens;
    }

    public void movePlayer()
    {
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = moveInput.x;
        moveDirection.z = moveInput.y;

        controller.Move( transform.TransformDirection( moveDirection ) * speed * Time.deltaTime );
        playerVelocity.y += gravity * Time.deltaTime;

        // if the player is on the ground and moving the play footstep sound
        if ( moveDirection != Vector3.zero )
        {
            Debug.Assert(this.GetComponent<AudioSource>() != null, "AudioSource is null");

            if ( !this.GetComponent<AudioSource>().isPlaying )
            {
                this.GetComponent<AudioSource>().Play();
            }
        }
        else
        {
            this.GetComponent<AudioSource>().Stop();
        }        

    }

    // check if the player is colliding with a wall
    // if the player is colliding with a wall, play a collision sound
    // stop the sound when the player is no longer colliding with a wall with a tag Wall
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if ( hit.gameObject.CompareTag( "Wall" ) && hit.gameObject != lastWallHit )
        {
            lastWallHit = hit.gameObject;
            wallCollision.Play();
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Die();
        }
    }

    public void ToggleFlashlight()
    {
        Light flashlight = GetComponentInChildren<Light>();
        flashlight.enabled = !flashlight.enabled;
        flashCone.SetActive(!flashCone.activeSelf);
    }

    void ThrowBall()
    {
        Vector3 playerPos = this.transform.position;
        Vector3 playerDirection = this.transform.forward;
        Camera mainCam = this.GetComponent<Camera>();
        float shootForce = 10.0f;

        Vector3 playerRotation = this.transform.rotation.eulerAngles;
        
        playerDirection = Quaternion.Euler(playerRotation) * playerDirection;

        playerPos.Set( playerPos.x, playerPos.y + 0.25f , playerPos.z );
        GameObject ball = Instantiate(ballPrefab, playerPos, Quaternion.identity);
        Rigidbody MyBallRB = ball.GetComponent<Rigidbody>();

        // Calculate the shooting direction
        Vector3 shootingDirection = CalculateShootingDirection();
        MyBallRB.AddForce( shootingDirection * shootForce, ForceMode.Impulse );

}

    Vector3 CalculateShootingDirection()
    {
        // Get the horizontal direction from the camera
        Vector3 horizontalDirection = Camera.main.transform.forward;
        horizontalDirection.y = 0; // Remove vertical component

        // Get the vertical direction from the player
        Vector3 verticalDirection = new Vector3( 0, Mathf.Sin( transform.eulerAngles.x * Mathf.Deg2Rad ), 0 );

        // Combine the directions
        Vector3 combinedDirection = horizontalDirection + verticalDirection;
        return combinedDirection.normalized; // Return the normalized direction
    }

    public void jump()
    {
        if ( grounded )
        {
            playerVelocity.y = Mathf.Sqrt( jumpForce * -3f * gravity );
        }
    }

    public void playerLook()
    {
        xRotation -= (lookPos.y * Time.deltaTime) * ySens;
        xRotation = Mathf.Clamp( xRotation, -80f, 80f );

        cam.transform.localRotation = Quaternion.Euler( xRotation, 0, 0 );
        transform.Rotate( Vector3.up * (lookPos.x * Time.deltaTime) * xSens );
    }

    public void SetDay()
    {
        flashCone.GetComponent<MeshRenderer>().material = flashDay;
    }

    public void SetNight()
    {
        flashCone.GetComponent<MeshRenderer>().material = flashNight;
    }

    public void Die()
    {
        deathSound.Play();
        OnDie?.Invoke();
    }

    public void ToggleSaveLoadButtonBox()
    {
        SaveLoadButtonBox.SetActive(!SaveLoadButtonBox.activeInHierarchy);
        if (SaveLoadButtonBox.activeInHierarchy)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

    }
}
