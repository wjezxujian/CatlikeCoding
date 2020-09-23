using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Game : PersistableObject
{
    //public Transform prefab;
    public PersistableObject prefab;
    public KeyCode createKey = KeyCode.C;
    public KeyCode newGameKey = KeyCode.N;
    public KeyCode saveKey = KeyCode.S;
    public KeyCode loadKey = KeyCode.L;

    //List<Transform> objects;
    List<PersistableObject> objects;

    public PersistentStorage storage;

    //string savePath;

    void Awake()
    {
        objects = new List<PersistableObject>();
        //savePath = Path.Combine(Application.persistentDataPath, "saveFile");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(createKey))
        {
            //Instantiate(prefab);
            CreateObject();
        }
        else if (Input.GetKey(newGameKey))
        {
            BeginNewGame();
        }
        else if (Input.GetKeyDown(saveKey))
        {
            //Save();
            storage.Save(this);
        }
        else if (Input.GetKeyDown(loadKey))
        {
            //Load();
            BeginNewGame();
            storage.Load(this);
        }
    }

    void CreateObject()
    {
        //Transform t = Instantiate(prefab);
        PersistableObject o = Instantiate(prefab);
        Transform t = o.transform;
        t.localPosition = Random.insideUnitSphere * 5f;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * Random.Range(0.1f, 1f);
        objects.Add(o);
    }

    void BeginNewGame()
    {
        for(int i = 0; i < objects.Count; ++i)
        {
            Destroy(objects[i].gameObject);
        }
        objects.Clear();
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(objects.Count);
        for(int i = 0; i < objects.Count; ++i)
        {
            objects[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        int count = reader.ReaderInt();
        for(int i = 0; i < count; ++i)
        {
            PersistableObject o = Instantiate(prefab);
            o.Load(reader);
            objects.Add(o);
        }
    }

    //void Save()
    //{
    //    //File.Open(savePath, FileMode.Create);
    //    using (
    //        BinaryWriter writer = new BinaryWriter(File.Open(savePath, FileMode.Create))
    //    ){
    //        Debug.Log("savePath: " + savePath);
    //        writer.Write(objects.Count);   
    //        for(int i = 0; i < objects.Count; ++i)
    //        {
    //            Transform t = objects[i];
    //            writer.Write(t.localPosition.x);
    //            writer.Write(t.localPosition.y);
    //            writer.Write(t.localPosition.z);
    //        }
    //    }
            
    //}

    //void Load()
    //{
    //    BeginNewGame();
    //    using (
    //        var reader = new BinaryReader(File.Open(savePath, FileMode.Open))
    //    ){
    //        int count = reader.ReadInt32();
    //        for(int i = 0; i < count; ++i)
    //        {
    //            Vector3 p;
    //            p.x = reader.ReadSingle();
    //            p.y = reader.ReadSingle();
    //            p.z = reader.ReadSingle();

    //            Transform t = Instantiate(prefab);
    //            t.localPosition = p;
    //            objects.Add(t);
                 
    //        }
    //    }
    //}
}
