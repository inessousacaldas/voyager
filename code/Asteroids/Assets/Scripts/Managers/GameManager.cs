using UnityEngine;

/// <summary>
/// The class to manage the highlevel game decisions
/// </summary>
public class GameManager : Singleton<GameManager>
{
    public enum GameState
    {
        Loading = 0,
        InGame = 1,
        Pause = 2
    }

    [SerializeField]
    private Player _player;

    [SerializeField]
    private int _asteroidStartAmount = 4;
    [SerializeField]
    private int _minActiveAsteroids = 6;
    [SerializeField]
    private float _alienSpawnTimer = 30f;

    [SerializeField]
    private GameObject _cameraObject;

    private int _score;
    private GameState _currGameState;

    //TODO REMOVE
    public bool start = false;

    /// <summary>
    /// The current state of the game (Loading, InGame, Pause)
    /// </summary>
    public GameState CurrGameState
    {
        get
        {
            return _currGameState;
        }

        set
        {
            _currGameState = value;
        }
    }

    /// <summary>
    /// The player object
    /// </summary>
    public Player Player
    {
        get
        {
            return _player;
        }

        set
        {
            _player = value;
        }
    }

    public void Start()
    {
        CurrGameState = GameState.Loading;
    }

    public void Update()
    {
        if (start)
        {
            CurrGameState = GameState.InGame;
            start = false;
            StartNewGame();
        }

        // TODO Pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 0;
        }
    }

    /// <summary>
    /// Starts a new game. Resets variables, activates the player and spwans the initial asteroids.
    /// </summary>
    public void StartNewGame()
    {
        UIManager.Instance.Show(Panel.PanelState.Ingame);
        _player.Activate();
        InitializeAsteroids(_asteroidStartAmount);
    }

    /// <summary>
    /// Spwan asteroids outside the viewport.
    /// </summary>
    /// <param name="amount">The number of asteroids to spwan</param>
    public void InitializeAsteroids(int amount)
    {
        AsteroidManager.Instance.DeactivateAllAsteroids();

        for (int i = 0; i < amount; i++)
        {
            // TODO TYPE
            int asteroidSize = Random.Range(0, 3);
            AsteroidManager.Instance.SpawnAsteroid(Asteroid.AsteroidType.Metal, (Asteroid.AsteroidSize)asteroidSize, SpawnOutsideViewport());
        }
    }


    public void SpawnNewAsteroids()
    {
        int activeAsteroids = AsteroidManager.Instance.GetActiveAsteroidAmount();

        if (activeAsteroids < _minActiveAsteroids)
        {
            int spawnAmount = _minActiveAsteroids - activeAsteroids;

            for (int i = 0; i < spawnAmount; i++)
            {
                Asteroid.AsteroidSize size = (Asteroid.AsteroidSize)Random.Range(0, 3);
                //TODO TYPE
                AsteroidManager.Instance.SpawnAsteroid(Asteroid.AsteroidType.Rock, size);
            }
        }
    }

    /// <summary>
    /// Generates a valid spawn position inside the viewport. 
    /// </summary>
    /// <param name="unoccupiedLocation">If its necessary the new position be unoccupied from other objects</param>
    /// <returns>The new spawn position.</returns>
    public Vector2 SpawnInsideViewport(bool unoccupiedLocation = true)
    {
        float distanceFromCamera = Camera.main.transform.position.z;

        Vector3 randomScreenCoordinate = new Vector3(Random.Range(0f, Screen.width), Random.Range(0f, Screen.height), distanceFromCamera);
        Vector3 randomWorldCoordinate = Camera.main.ScreenToWorldPoint(randomScreenCoordinate);

        print(randomWorldCoordinate);

        RaycastHit2D hit = Physics2D.Raycast(randomWorldCoordinate, -Vector2.up, Mathf.Infinity, Config.Layer.viewport);

        if (hit.collider != null && !hit.collider.CompareTag(Config.Tags.viewport) && unoccupiedLocation)
        {
            return SpawnInsideViewport();
        }
        else
        {
            return randomWorldCoordinate;
        }
    }

    /// <summary>
    /// Generates a valid spawn position outside the viewport.  
    /// </summary>
    /// <returns>The new spawn position.</returns>
    public Vector2 SpawnOutsideViewport()
    {
   
        float distanceFromCamera = Camera.main.transform.position.z;

        Vector3 randomScreenCoordinate = new Vector3(Random.Range(0f - (Screen.width * 0.4f), Screen.width * 1.4f), Random.Range(0f * (Screen.height * 0.2f), Screen.height * 1.2f), distanceFromCamera);
        Vector3 randomWorldCoordinate = Camera.main.ScreenToWorldPoint(randomScreenCoordinate);

        RaycastHit2D hit = Physics2D.Raycast(randomWorldCoordinate, -Vector2.up);

        if (hit.collider != null)
        {
            return SpawnOutsideViewport();
        }
        else
        {
            return randomWorldCoordinate;
        }
    }

    /// <summary>
    /// Calls all the functions and behaviours related to the gameover state.
    /// </summary>
    public void GameOver()
    {
        GoToPosition posCamera = _cameraObject.GetComponent<GoToPosition>();
        posCamera.LockTarget(_player.transform.position);
        posCamera.Move = true;

        CancelInvoke();
        UIManager.Instance.GetPanel<IngamePanel>(Panel.PanelState.Ingame).ShowGameOverScreen();
    }

    /// <summary>
    /// Add points to the current player's score.
    /// </summary>
    /// <param name="points">The number of points to add to the score.</param>
    public void AddScore(int points)
    {
        SetScore(_score + points);
    }

    /// <summary>
    /// Sets the player's score and updates UI panel.
    /// </summary>
    /// <param name="score">The new player's score.</param>
    public void SetScore(int score)
    {
        _score = score;

        UIManager.Instance.GetPanel<IngamePanel>(Panel.PanelState.Ingame).SetScore(_score);
    }
}
