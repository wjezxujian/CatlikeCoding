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

    [SerializeField]
    //GameTileContent towerPrefab = default;
    Tower[] towerPrefabs = default;

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
            //case GameTileContentType.Tower:
            //    return Get(towerPrefab);
        }

        Debug.Assert(false, "Unsupported non-tower type: " + type);
        return null;
    }

    public void Reclaim(GameTileContent content)
    {
        Debug.Assert(content.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(content.gameObject);
    }

    public T Get<T>(T prefab) where T : GameTileContent
    {
        //GameTileContent instance = Instantiate(prefab);
        T instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
        //MoveToFactoryScene(instance.gameObject);
        return instance;
    }

    public Tower Get(TowerType type)
    {
        Debug.Assert((int)type < towerPrefabs.Length, "Unsupported tower type!");
        Tower prefab = towerPrefabs[(int)type];
        Debug.Assert(type == prefab.TowerType, "Tower prefab at wrong index!");
        return Get(prefab);
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
