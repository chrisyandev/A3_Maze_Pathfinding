using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PongGameManager : MonoBehaviour
{
    [SerializeField]
    private Canvas m_MenuCanvas;
    [SerializeField]
    private Canvas m_ScoreCanvas;
    [SerializeField]
    private Canvas m_TimerCanvas;


    [Space( 20 )]
    [SerializeField]
    private InputActionAsset m_DefaultInputActionAsset;
    [SerializeField]
    private InputActionReference m_StartGameAction;

    [Space( 20 )]
    [SerializeField]
    private Player m_PlayerScript;
    [SerializeField]
    private Player2 m_Player2Script;
    [SerializeField]
    private AIPaddle m_AiPaddle;
    [SerializeField]
    private Ball m_Ball;


    [SerializeField]
    private TMP_Text Player1ScoreDisplay;
    [SerializeField]
    private TMP_Text Player2ScoreDisplay;
    [SerializeField]
    private TMP_Text RoundTimerDisplay;
    [SerializeField]
    private TMP_Text WinnerDisplay;

    private int Player1Score = 0;
    private int Player2Score = 0;
    private float DEFAULT_GAME_TIME_IN_SECONDS = 30.0f;
    private float timeLeft = 30.0f;
    private bool timerOn = false;
    private bool isInGame = false;
    public bool isPvp;

    private static PongGameManager _instance;

    public static PongGameManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new PongGameManager();
        }

        return _instance;
    }



    private void Awake()
    {

        _instance = this;

        // Enable all action maps inside the default action asset.
        foreach ( var actionMap in m_DefaultInputActionAsset.actionMaps )
        {
            actionMap.Enable();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        timeLeft = DEFAULT_GAME_TIME_IN_SECONDS;
        timerOn = true;

        Player1Score = 0;
        Player2Score = 0;

    }

    // Update is called once per frame
    void Update()
    {
        if ( timerOn )
        {
            if ( timeLeft > 0 )
            {
                timeLeft -= Time.deltaTime;
                UpdateTimer( timeLeft );
            }
            else
            {
                Debug.Log( "Time up!" );
                timeLeft = 0;
                timerOn = false;
                AnnounceWinner();
                Invoke("ResetGame", 3.0f);
            }
        }
    }

    void AnnounceWinner()
    {
        WinnerDisplay.gameObject.SetActive(true);
        if ( Player1Score > Player2Score )
        {
            WinnerDisplay.text = "Blue Wins!!!!!!!";
        }
        else if (Player1Score == Player2Score )
        {
            WinnerDisplay.text = "TIE!";
        }
        else
        {
            WinnerDisplay.text = "Red Wins!!!!!!!";
        }

        m_Ball.enabled = false;
    }

    public void ResetGame()
    {
        isInGame = false;
        m_MenuCanvas.enabled = true;
        m_AiPaddle.enabled = false;
        m_Player2Script.enabled = false;
        m_Ball.enabled = false;
        WinnerDisplay.gameObject.SetActive(false);
        timeLeft = DEFAULT_GAME_TIME_IN_SECONDS;
        Player1Score = 0;
        Player2Score = 0;

        m_PlayerScript.gameObject.transform.position = new Vector3(7f, 0.1f, 0);
        m_AiPaddle.gameObject.transform.position = new Vector3(-7f, 0.1f, 0);
        m_Ball.transform.position = new Vector3(0, 0, 0);
    }

    public static void UpdateScore( int PlayerID )
    {
        if ( PlayerID == 1 )
        {
            _instance.Player1Score++;
            _instance.Player1ScoreDisplay.text = String.Format( "{0:00}", _instance.Player1Score );
            
        }
        else
        {
            _instance.Player2Score++;
            _instance.Player2ScoreDisplay.text = String.Format( "{0:00}", _instance.Player2Score );
        }
    }

    void UpdateTimer( float currentTime )
    {
        currentTime += 1;
        float minutes = Mathf.FloorToInt( currentTime / 60 );
        float seconds = Mathf.FloorToInt( currentTime % 60 );

        RoundTimerDisplay.text = string.Format( "{0:00} : {1:00}", minutes, seconds );

    }

    public void SetIsInGame( bool toSet )
    {
        isInGame = toSet;
    }

    public void OnStartGame()
    {
        isInGame = true;
        Debug.Log( "Started Game" );
        this.m_MenuCanvas.enabled = false;
        timeLeft = DEFAULT_GAME_TIME_IN_SECONDS;
        timerOn = true;

        Player1ScoreDisplay.text = "00";
        Player2ScoreDisplay.text = "00";
        Invoke("EnableBall", 2.0f);
    }

    public void EnableBall()
    {
        m_Ball.enabled = true;
    }

    public bool IsInGame()
    {
        return isInGame;
    }

    private void OnDestroy()
    {

    }

    // On click function for Exit Game
    public void OnExitGame()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Destroy(GameObject.FindGameObjectWithTag("PongContainer"));
        MasterContainer.Instance.SetContainerEnable(true);
    }

    //On click function for Player vs Player button
    public void OnPlayerVsPlayer()
    {
        ResetGame();
        isPvp = true;
        isInGame = true;
        m_Player2Script.enabled = true;

        Debug.Log("Started PvP Game");
        this.m_MenuCanvas.enabled = false;
        OnStartGame();
    }

    //On click function for Player vs AI button
    public void OnPlayerVsAI()
    {
        ResetGame();
        isPvp = false;
        isInGame = true;
        m_AiPaddle.enabled = true;

        Debug.Log("Started PvAI Game");
        this.m_MenuCanvas.enabled = false;
        OnStartGame();
    }
}
