
using UnityEngine;
using UnityEngine.SceneManagement;



public class MazeWallWithDoor : MazeCellEdge
{
    private float timeTouching = 0f;
    private const float requiredTime = 3f;

    // Need this such that when the player returns from Pong, 
    bool CanTransfer = true;

    void OnTriggerEnter( Collider other )
    {
        // When an object enters the trigger, start counting
        if ( other.CompareTag( "Player" ) )
        {
            timeTouching = 0f;
        }
    }

    void OnTriggerStay( Collider other )
    {
        // While the object is in the trigger, keep counting
        if ( other.CompareTag( "Player" ) )
        {
            timeTouching += Time.deltaTime;

            // If the object has been touching for more than 3 seconds
            if ( timeTouching >= requiredTime && CanTransfer )
            {
                CanTransfer = false;
                LoadPongGame();
            }
        }
    }

    void OnTriggerExit( Collider other )
    {

        if ( other.CompareTag( "Player" ) )
        {
            // When the object leaves the trigger, reset the timer
            timeTouching = 0f;
            CanTransfer = true;
        }
    }

    void LoadPongGame()
    {
        // Your loading logic goes here
        //Debug.Log( "Pong Game is being loaded!" );

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        MasterContainer.Instance.SetContainerEnable( false );
        SceneManager.LoadScene( "Pong" );
    }
}
