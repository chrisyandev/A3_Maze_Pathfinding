using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public Maze mazePrefab;
    public GameObject player;
    public GameObject ai;

    public Maze mazeInstance;
    private NavMeshAgent aiNavMeshAgent;

    private MazeCell playerStartingCell;

    private Camera FPSCamera;
    private Quaternion CameraStartingRotation;

    public bool isDay = true;
    public bool finishedGeneratingMaze = false;

    [SerializeField]
    private AudioSource bgMusic;

    [SerializeField]
    private AudioClip dayMusic;

    [SerializeField]
    private AudioClip nightMusic;

    [SerializeField]
    private TMP_Text scoreText;

    private PlayerController playerController;
    private AIController aiController;
    private int score;

    private bool isPaused = false;

    [SerializeField]
    private Material EnemyMaterial;

    private GameObject Fog;

    private bool isFogOn = true;

    private SkinnedMeshRenderer EnemyMesh;

    private void Awake()
    {
        CameraStartingRotation = player.GetComponentInChildren<Camera>().transform.rotation;
        playerController = player.GetComponent<PlayerController>();
        aiController = ai.GetComponent<AIController>();
        Fog = GameObject.Find("Fog");
    }

    private void OnEnable()
    {
        playerController.OnDie += HandlePlayerDie;
        aiController.OnDie += HandleAIDie;
        aiController.OnBallHit += HandleAIBallHit;
    }

    private void OnDisable()
    {
        playerController.OnDie -= HandlePlayerDie;
        aiController.OnDie -= HandleAIDie;
        aiController.OnBallHit -= HandleAIBallHit;
    }

    void Start()
    {
        FPSCamera = player.GetComponentInChildren<Camera>();
        aiNavMeshAgent = ai.GetComponent<NavMeshAgent>();
        EnemyMesh = ai.GetComponentInChildren<SkinnedMeshRenderer>();
        GameStart();
        bgMusic.Play();
    }

    void Update()
    {
        if ( Input.GetKeyDown( KeyCode.Home ) || Input.GetButtonDown("Restart") )
        {
            GameRestart();
        }

        if ( Input.GetKeyDown( KeyCode.Q ) || Input.GetButtonDown("Birdseye") )
        {
            ChangePOV();
        }

        if ( Input.GetKeyDown( KeyCode.P ) )
        {
            isPaused = !isPaused;
            PauseAndPlayBgMusic();
        }

        if ( Input.GetKeyDown( KeyCode.L ) )
        {
            isDay = !isDay;
        }

        if ( Input.GetKeyDown( KeyCode.M ) )
        {
            Fog.SetActive( !Fog.activeInHierarchy );
            isFogOn = !isFogOn;
        }

        if ( aiNavMeshAgent.enabled && !aiNavMeshAgent.pathPending && !aiNavMeshAgent.hasPath ) // if AI has reached destination
        {
            MazeCell randomCell = mazeInstance.GetCell( mazeInstance.RandomCoordinates );
            aiNavMeshAgent.SetDestination( randomCell.transform.position );
        }
        CheckAbientSetting();
        UpdateBgSound();

    }

    private void CheckAbientSetting()
    {
        if (isDay)
        {
            playerController.SetDay();
            mazeInstance.DayLight();
            if (bgMusic.clip != dayMusic)
            {
                bgMusic.clip = dayMusic;
                PauseAndPlayBgMusic();
            }
            EnemyMesh.material.SetFloat("_IsDay", 1.0f);
        }
        else
        {
            playerController.SetNight();
            mazeInstance.NightLight();
            if (bgMusic.clip != nightMusic)
            {
                bgMusic.clip = nightMusic;
                PauseAndPlayBgMusic();
            }
            EnemyMesh.material.SetFloat("_IsDay", 0.0f);
        }


    }

    void UpdateBgSound()
    {
        float distance = GetDistanceToPlayer();
        if (distance < 2f)
        {
            bgMusic.volume = isFogOn ? 0.5f : 1f;
        } else if (distance < 5f)
        {
            bgMusic.volume = isFogOn ? 0.125f: 0.25f;
        } else if (distance < 10f)
        {
            bgMusic.volume = isFogOn ? 0.075f: 0.15f;
        } else
        {
            bgMusic.volume = isFogOn ? 0.025f: 0.05f;
        }
    }
    

    private void PauseAndPlayBgMusic()
    {
        if ( isPaused )
        {
            bgMusic.Stop();
        }
        else
        {
            bgMusic.Play();
        }
    }

    private void ChangePOV()
    {
        if ( mazeInstance.birdEyeViewCamera.enabled )
        {
            FPSCamera.enabled = true;
            mazeInstance.birdEyeViewCamera.enabled = false;
        }
        else
        {
            FPSCamera.enabled = false;
            mazeInstance.birdEyeViewCamera.enabled = true;
        }
    }

    private void GameStart()
    {
        player.transform.rotation = CameraStartingRotation;
        player.GetComponentInChildren<Camera>().transform.rotation = CameraStartingRotation;

        mazeInstance = Instantiate(mazePrefab, this.transform) as Maze;
        mazeInstance.GenerateMaze();

        // move player to first cell
        //playerStartingCell = mazeInstance.GetCell( mazeInstance.RandomCoordinates ); //Change function name below if using this
        playerStartingCell = mazeInstance.GetCell( new IntVector2 ( 0, 0 ) );

        SetupStartingPlayerPosition();

        // without delay, only part of navmesh will be built
        Invoke("FinishedGeneratingMaze", 0.5f);
    }

    private void FinishedGeneratingMaze()
    {
        finishedGeneratingMaze = true;
        //SetupAI();
    }

    private void SetupStartingPlayerPosition()
    {
        playerController.enabled = false;
        player.transform.position = new Vector3( playerStartingCell.gameObject.transform.position.x,
                                                 playerStartingCell.gameObject.transform.position.y + 0.5f,
                                                 playerStartingCell.gameObject.transform.position.z );
        playerController.enabled = true;
        EndZone.HideWinText();
    }

    private void GameOver()
    {
  
    }

    private void GameRestart()
    {
        CancelInvoke("SetupAI");
        if ( mazeInstance.birdEyeViewCamera.enabled )
        {
            ChangePOV();
        }

        aiNavMeshAgent.enabled = false;
        Destroy(mazeInstance.gameObject);
        score = 0;
        scoreText.SetText("Score: " +  score.ToString());
        GameStart(); 
    }

    private void SetupAI()
    {
        // build nav mesh
        NavMeshSurface navMeshSurface = mazeInstance.GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();

        // move AI to last cell
        MazeCell aiStartingCell = mazeInstance.GetCell(new IntVector2(mazeInstance.size.x - 1, mazeInstance.size.z - 1));
        ai.transform.position = new(aiStartingCell.transform.position.x, aiStartingCell.transform.position.y, aiStartingCell.transform.position.z);
        aiNavMeshAgent.enabled = true;
        GameObject.Find( "KillTrigger" ).GetComponent<CapsuleCollider>().enabled = true;
    }

    private void RespawnAI()
    {
        MazeCell randomCell = mazeInstance.GetCell(mazeInstance.RandomCoordinates);
        ai.transform.position = randomCell.transform.position;
        ai.GetComponent<AIController>().Respawn();
    }

    private void HandleAIDie()
    {
        Invoke(nameof(RespawnAI), 5.0f);
    }

    private void HandlePlayerDie()
    {
        Invoke(nameof(GameRestart), 1.0f);
    }

    private void HandleAIBallHit()
    {
        score++;
        scoreText.text = "Score: " + score;
    }

    private float GetDistanceToPlayer()
    {
        return Vector3.Distance( player.transform.position, ai.transform.position );
    }
    
    public int GetScore()
    {
        return score;
    }

    public void SetScore(int scoreToSet)
    {
        this.score = scoreToSet;
        scoreText.text = "Score: " + this.score;
    }
}
