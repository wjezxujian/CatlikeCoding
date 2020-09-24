using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : PersistableObject
{
    [SerializeField]
    ShapeFactory shapeFactory;

    [SerializeField]
    KeyCode createKey = KeyCode.C, destroyKey = KeyCode.X,
        newGameKey = KeyCode.N, saveKey = KeyCode.S, loadKey = KeyCode.L;

    [SerializeField]
    PersistentStorage storage;

    [SerializeField]
    int levelCount;

    [SerializeField]
    bool ressedOnLoad;

    //public SpawnZone spawnZone;
    //public SpawnZone SpawnZoneOfLevel { get; set; }

    public float CreationSpeed { get; set; }

    public float DestructionSpeed { get; set; }

    //public static Game Instance { get; private set; }

    const int saveVersion = 3;

    List<Shape> shapes;

    float creationProgress, destructionProgress;

    int loadedLevelBuildIndex;

    Random.State mainRandomState;


    //void Awake()
    //{
    //    shapes = new List<Shape>();

    //    Scene loadedLevel = SceneManager.GetSceneByName("Level 1");
    //    if (loadedLevel.isLoaded)
    //    {
    //        SceneManager.SetActiveScene(loadedLevel);
    //        return;
    //    }

    //    StartCoroutine(LoadLevel());
    //}

    private void Start()
    {
        mainRandomState = Random.state;

        //Instance = this;

        shapes = new List<Shape>();

        if (Application.isEditor)
        {
            for(int i = 0; i < SceneManager.sceneCount; ++i)
            {
                Scene loadedLevel = SceneManager.GetSceneAt(i);
                if (loadedLevel.name.Contains("Level "))
                {
                    SceneManager.SetActiveScene(loadedLevel);
                    loadedLevelBuildIndex = loadedLevel.buildIndex;
                    return;
                }
            }
        }

        BeginNewGame();
        StartCoroutine(LoadLevel(1));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(createKey))
        {
            CreateShape();
        }
        else if (Input.GetKeyDown(destroyKey))
        {
            DestroyShape();
        }
        else if (Input.GetKey(newGameKey))
        {
            BeginNewGame();
        }
        else if (Input.GetKeyDown(saveKey))
        {
            storage.Save(this, saveVersion);
        }
        else if (Input.GetKeyDown(loadKey))
        {
            BeginNewGame();
            storage.Load(this);
        }
        else
        {
            for(int i = 0; i <= levelCount; ++i)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    BeginNewGame();
                    StartCoroutine(LoadLevel(i));
                }
            }
        }

        creationProgress += Time.deltaTime * CreationSpeed;
        while(creationProgress >= 1f)
        {
            creationProgress -= 1f;
            CreateShape();
        }

        destructionProgress += Time.deltaTime * DestructionSpeed;
        while(destructionProgress >= 1f)
        {
            destructionProgress -= 1f;
            DestroyShape();
        }
    }

    //void OnEnable()
    //{
    //    Instance = this;
    //}

    void CreateShape()
    {
        Shape instance = shapeFactory.GetRandom();
        Transform t = instance.transform;
        //t.localPosition = Random.insideUnitSphere * 5f;
        t.localPosition = GameLevel.Current.SpawnPoint;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * Random.Range(0.1f, 1f);
        shapes.Add(instance);
    }

    void DestroyShape()
    {
        if (shapes.Count > 0)
        {
            int index = Random.Range(0, shapes.Count);
            shapeFactory.Reclaim(shapes[index]);
            int lastIndex = shapes.Count - 1;
            shapes[index] = shapes[lastIndex];
            shapes.RemoveAt(lastIndex);
        } 
    }

    void BeginNewGame()
    {
        Random.state = mainRandomState;
        int seed = Random.Range(0, int.MaxValue) ^ (int)Time.unscaledTime;
        Random.InitState(seed);
        mainRandomState = Random.state;

        for(int i = 0; i < shapes.Count; ++i)
        {
            shapeFactory.Reclaim(shapes[i]);
        }
        shapes.Clear();
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(shapes.Count);
        writer.Write(Random.state);
        writer.Write(loadedLevelBuildIndex);
        GameLevel.Current.Save(writer);
        for(int i = 0; i < shapes.Count; ++i)
        {
            writer.Write(shapes[i].ShapeId);
            writer.Write(shapes[i].MaterialId);
            shapes[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        int version = reader.Version;
        if(version > saveVersion)
        {
            Debug.LogError("Unsupported future save version" + version);
            return;
        }

        StartCoroutine(LoadGame(reader));
    }

    IEnumerator LoadGame(GameDataReader reader)
    {
        int version = reader.Version;
        int count = version <= 0 ? -version : reader.ReaderInt();
        if (version >= 3)
        {
            Random.State state = reader.ReadRandomState();
            if (!ressedOnLoad)
            {
                Random.state = state;
            }
        }

        //StartCoroutine(LoadLevel(version < 2 ? 1 : reader.ReaderInt()));
        yield return LoadLevel(version < 2 ? 1 : reader.ReaderInt());
        if(version >= 3)
        {
            GameLevel.Current.Load(reader);
        }

        for (int i = 0; i < count; ++i)
        {
            int shapeId = version > 0 ? reader.ReaderInt() : 0;
            int materialId = version > 0 ? reader.ReaderInt() : 0;
            Shape instance = shapeFactory.Get(shapeId, materialId);
            instance.Load(reader);
            shapes.Add(instance);
        }
    }


    IEnumerator LoadLevel(int levelBuildIndex)
    {
        enabled = false;
        //SceneManager.LoadScene("Level 1", LoadSceneMode.Additive);
        //yield return null;
        //异步加载
        //yield return SceneManager.LoadSceneAsync("Level 1", LoadSceneMode.Additive);

        if(loadedLevelBuildIndex > 0)
        {
            yield return SceneManager.UnloadSceneAsync(loadedLevelBuildIndex);
        }

        yield return SceneManager.LoadSceneAsync(levelBuildIndex, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(levelBuildIndex));
        loadedLevelBuildIndex = levelBuildIndex;
        enabled = true;
    }
}
