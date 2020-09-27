using System.Globalization;
using UnityEngine;

public class GameLevel : PersistableObject
{
    [SerializeField]
    SpawnZone spawnZone;

    [SerializeField]
    PersistableObject[] persistableObjects;

    public static GameLevel Current { get; private set; }

    //public Vector3 SpawnPoint
    //{
    //    get { return spawnZone.SpawnPoint; }
    //}

    //public void ConfigureSpawn(Shape shape)
    //{
    //    spawnZone.ConfigureSpawn(shape);
    //}
    public void SpawnShapes()
    {
        spawnZone.SpawnShapes();
    }

    private void OnEnable()
    {
        Current = this;

        if(persistableObjects == null)
        {
            persistableObjects = new PersistableObject[0];
        }
    }

    //void Start()
    //{
    //    Game.Instance.SpawnZoneOfLevel = spawnZone;
    //}

    public override void Save(GameDataWriter writer)
    {
        writer.Write(persistableObjects.Length);
        for(int i = 0; i < persistableObjects.Length; ++i)
        {
            persistableObjects[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        int saveCount = reader.ReadInt();
        for (int i = 0; i < saveCount; ++i)
        {
            persistableObjects[i].Load(reader);
        }
    }
}
