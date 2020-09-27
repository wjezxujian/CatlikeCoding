using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu]
public class ShapeFactory : ScriptableObject
{
    [SerializeField]
    Shape[] prefabs;

    [SerializeField]
    Material[] materials;

    [SerializeField]
    bool recycle;

    List<Shape>[] pools;

    Scene poolScene;

    [System.NonSerialized]
    int factoryId = int.MinValue;

    public int FactoryId
    {
        get { return factoryId; }

        set
        {
            if(factoryId == int.MinValue && value != int.MinValue)
            {
                factoryId = value;
            }
            else
            {
                Debug.LogError("Not allowed to change factoryId.");
            }
        }
    }

    public Shape Get(int shapeId = 0, int materialId = 0)
    {
        Shape instance;
        if (recycle)
        {
            if(pools == null)
            {
                CreatePools();
            }

            List<Shape> pool = pools[shapeId];
            int lastIndex = pool.Count - 1;
            if(lastIndex >= 0)
            {
                instance = pool[lastIndex];
                instance.gameObject.SetActive(true);
                pool.RemoveAt(lastIndex);
            }
            else
            {
                instance = Instantiate(prefabs[shapeId]);
                instance.OriginFactory = this;
                instance.ShapeId = shapeId;
                SceneManager.MoveGameObjectToScene(instance.gameObject, poolScene);
            }        
        }
        else
        {
            instance = Instantiate(prefabs[shapeId]);
            instance.ShapeId = shapeId;
            instance.OriginFactory = this;
            //SceneManager.MoveGameObjectToScene(instance.gameObject, poolScene);
        }

        instance.SetMaterial(materials[materialId], materialId);
        //instance.SetColor(Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.25f, 1f, 1f, 1f));
        return instance;
    }

    public Shape GetRandom()
    {
        int shapeId = Random.Range(0, prefabs.Length);
        int materialId = Random.Range(0, materials.Length);
        return Get(shapeId, materialId);
    }

    public void Reclaim(Shape shapeToRecycle)
    {
        if(shapeToRecycle.OriginFactory != this)
        {
            Debug.LogError("Tried to reclaim shape with wrong factory.");
        }

        if (recycle)
        {
            if (pools == null)
            {
                CreatePools();
            }

            pools[shapeToRecycle.ShapeId].Add(shapeToRecycle);
            shapeToRecycle.gameObject.SetActive(false);
        }
        else
        {
            Destroy(shapeToRecycle.gameObject);
        }
    }

    private void CreatePools()
    {
        pools = new List<Shape>[prefabs.Length];
        for(int i = 0; i < prefabs.Length; ++i)
        {
            pools[i] = new List<Shape>();
        }

        if (Application.isEditor)
        {
            poolScene = SceneManager.GetSceneByName(name);
            if (poolScene.isLoaded)
            {
                GameObject[] rootObjects = poolScene.GetRootGameObjects();
                for(int i = 0; i < rootObjects.Length; ++i)
                {
                    Shape pooledShape = rootObjects[i].GetComponent<Shape>();
                    if (!pooledShape.gameObject.activeSelf)
                    {
                        pools[pooledShape.ShapeId].Add(pooledShape);
                    }
                }
                return;
            }
        }

        poolScene = SceneManager.CreateScene(name);
    }

    
}
