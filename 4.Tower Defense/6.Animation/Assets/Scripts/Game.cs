using System.Net.Http.Headers;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField]
    Vector2Int boardSize = new Vector2Int(11, 11);

    [SerializeField]
    GameBoard board = default;

    [SerializeField]
    GameTileContentFactory tileContentFactory = default;

    //[SerializeField]
    //EnemyFactory enemyFactory = default;

    [SerializeField]
    WarFactory warFactory = default;

    //[SerializeField, Range(0.1f, 10f)]
    //float spawnSpeed = 1f;

    [SerializeField]
    GameScenario scenario = default;

    [SerializeField, Range(0, 100)]
    int startingPlayerHealth = 10;

    [SerializeField, Range(1f, 100f)]
    float playSpeed = 1f;

    GameScenario.State activeScenario;

    //float spawnProgress;

    GameBehaviourCollection enemies = new GameBehaviourCollection();

    GameBehaviourCollection nonEnemies = new GameBehaviourCollection();

    TowerType selectedTowerType;

    static Game instance;

    int playerHealth;

    const float pausedTimeScale = 0f;

    Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    private void Awake()
    {
        playerHealth = startingPlayerHealth;
        board.Initialize(boardSize, tileContentFactory);
        board.ShowGrid = true;
        activeScenario = scenario.Begin();
    }

    private void OnEnable()
    {
        instance = this;
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            HandleTouch();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            HandleAlternativeTouch();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            board.ShowPaths = !board.ShowPaths;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            board.ShowGrid = !board.ShowGrid;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedTowerType = TowerType.Laser;
        }else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedTowerType = TowerType.Mortar;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space");
            Time.timeScale = Time.timeScale > pausedTimeScale ? pausedTimeScale : playSpeed;
        }
        else if(Time.timeScale > pausedTimeScale)
        {
            Time.timeScale = playSpeed;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            BeginNewGame();
        }

        //spawnProgress += spawnSpeed * Time.deltaTime;
        //while(spawnProgress >= 1f)
        //{
        //    spawnProgress -= 1f;
        //    SpawnEnemy();
        //}

        if(playerHealth <= 0 && startingPlayerHealth > 0)
        {
            Debug.Log("Defeat!");
            BeginNewGame();
        }

        if (!activeScenario.Progress() && enemies.IsEmpty)
        {
            Debug.Log("Victory!");
            BeginNewGame();
            activeScenario.Progress();
        }

        //activeScenario.Progress();

        enemies.GameUpdate();
        Physics.SyncTransforms();
        board.GameUpdate();
        nonEnemies.GameUpdate();
    }

    private void BeginNewGame()
    {
        playerHealth = startingPlayerHealth;
        enemies.Clear();
        nonEnemies.Clear();
        board.Clear();
        activeScenario = scenario.Begin();
    }

    private void OnValidate()
    {
        if(boardSize.x < 2)
        {
            boardSize.x = 2;
        }

        if(boardSize.y < 2)
        {
            boardSize.y = 2;
        }
    }

    private void HandleTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        if(tile != null)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                board.ToggleTower(tile, selectedTowerType);
            }
            else
            {
                board.ToggleWall(tile);
            }            
        }
    }

    private void HandleAlternativeTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        if(tile != null)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                board.ToggleDestination(tile);
            }
            else
            {
                board.ToggleSpawnPoint(tile);
            }
        }
    }

    public static void SpawnEnemy(EnemyFactory facotry, EnemyType type)
    {
        //GameTile spawnPoint = board.GetSpawnPoint(Random.Range(0, board.SpawnPointCount));
        //Enemy enemy = enemyFactory.Get((EnemyType)Random.Range(0, 3));
        GameTile spawnPoint = instance.board.GetSpawnPoint(Random.Range(0, instance.board.SpawnPointCount));
        Enemy enemy = facotry.Get(type);
        enemy.SpawnOn(spawnPoint);
        instance.enemies.Add(enemy);
    }

    public static Shell SpawnShell()
    {
        Shell shell = instance.warFactory.Shell;
        instance.nonEnemies.Add(shell);
        return shell;
    }

    public static Explosion SpawnExplosion()
    {
        Explosion explosion = instance.warFactory.Explosion;
        instance.nonEnemies.Add(explosion);
        return explosion;
    }

    public static void EnemyReachedDestination()
    {
        instance.playerHealth -= 1;
    }
    
}
