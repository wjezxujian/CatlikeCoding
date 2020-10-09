using UnityEngine;

public partial class GameLevel : PersistableObject
{
    [SerializeField]
    int popularionLimit;

    [SerializeField]
    SpawnZone spawnZone;

    [UnityEngine.Serialization.FormerlySerializedAs("persistableObjects")]
    [SerializeField]
    GameLevelObject[] levelObjects;

    public static GameLevel Current { get; private set; }

    public int PopulationLimit
    {
        get { return popularionLimit; }
    }

    public void SpawnShapes()
    {
        spawnZone.SpawnShapes();
    }

    private void OnEnable()
    {
        Current = this;

        if(levelObjects == null)
        {
            levelObjects = new GameLevelObject[0];
        }
    }

    public void GameUpdate()
    {
        for (int i = 0; i < levelObjects.Length; ++i)
        {
            levelObjects[i].GameUpdate();
        }
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(levelObjects.Length);
        for(int i = 0; i < levelObjects.Length; ++i)
        {
            levelObjects[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        int saveCount = reader.ReadInt();
        for (int i = 0; i < saveCount; ++i)
        {
            levelObjects[i].Load(reader);
        }
    }

}
