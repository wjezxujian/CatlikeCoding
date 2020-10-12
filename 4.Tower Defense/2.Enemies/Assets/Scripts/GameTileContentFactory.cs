using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu]
public class GameTileContentFactory : GameObjectFactory
{
    [SerializeField]
    GameTileContent destinationPrefab = default;

    [SerializeField]
    GameTileContent emptyPrefab = default;

    [SerializeField]
    GameTileContent wallPrefab = default;

    [SerializeField]
    GameTileContent spawnPointPrefab = default;

    //Scene contentScene;

    public GameTileContent Get(GameTileContentType type)
    {
        switch (type)
        {
            case GameTileContentType.Destination:
                return Get(destinationPrefab);
            case GameTileContentType.Empty:
                return Get(emptyPrefab);
            case GameTileContentType.Wall:
                return Get(wallPrefab);
            case GameTileContentType.SpawnPoint:
                return Get(spawnPointPrefab);
        }

        Debug.Assert(false, "Unsupported type: " + type);
        return null;
    }

    public void Reclaim(GameTileContent content)
    {
        Debug.Assert(content.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(content.gameObject);
    }

    private GameTileContent Get(GameTileContent prefab)
    {
        //GameTileContent instance = Instantiate(prefab);
        GameTileContent instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
        //MoveToFactoryScene(instance.gameObject);
        return instance;
    }

    //private void MoveToFactoryScene(GameObject o)
    //{
    //    if (!contentScene.isLoaded)
    //    {
    //        if (Application.isEditor)
    //        {
    //            contentScene = SceneManager.GetSceneByName(name);
    //            if (!contentScene.isLoaded)
    //            {
    //                contentScene = SceneManager.CreateScene(name);
    //            }
    //        }
    //        else
    //        {
    //            contentScene = SceneManager.CreateScene(name);
    //        }
    //    }

    //    SceneManager.MoveGameObjectToScene(o, contentScene);
    //}


}
