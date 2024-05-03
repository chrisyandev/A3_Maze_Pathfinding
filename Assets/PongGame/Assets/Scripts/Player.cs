using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Math = System.Math;

public class Player : MonoBehaviour
{

    private InputActions m_inputActions;
    private InputAction m_movementAction;

    public float paddleSpeed = 5f;

    private void Awake()
    {
        m_inputActions = new InputActions();
        m_movementAction = m_inputActions.Movement.UpDown;
    }
    
    private void OnEnable()
    {
        m_inputActions.Enable();
    }

    // Start is called before the first frame update:w

    private void OnDisable()
    {
        m_inputActions.Disable();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 movement = m_movementAction.ReadValue<Vector2>();
        Vector3 position = new Vector3(0, 0, movement.y);

        //Get paddle postion
        float paddlePos = transform.position.z + position.z;
        if (paddlePos >= 6.5f || paddlePos <= -6.5f)
            position = Vector3.zero;        

        //Move paddle
        transform.Translate(position * paddleSpeed * Time.deltaTime);
    }

    void OnMoveUp()
    {

    }

    void OnMoveDown()
    {

    }
}
