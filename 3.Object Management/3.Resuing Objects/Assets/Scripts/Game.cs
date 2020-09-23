﻿using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Game : PersistableObject
{
    public ShapeFactory shapeFactory;
    public KeyCode createKey = KeyCode.C;
    public KeyCode newGameKey = KeyCode.N;
    public KeyCode saveKey = KeyCode.S;
    public KeyCode loadKey = KeyCode.L;

    const int saveVersion = 1;

    List<Shape> shapes;

    public PersistentStorage storage;

    void Awake()
    {
        shapes = new List<Shape>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(createKey))
        {
            CreateShape();
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
    }

    void CreateShape()
    {
        Shape instance = shapeFactory.GetRandom();
        Transform t = instance.transform;
        t.localPosition = Random.insideUnitSphere * 5f;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * Random.Range(0.1f, 1f);
        shapes.Add(instance);
    }

    void BeginNewGame()
    {
        for(int i = 0; i < shapes.Count; ++i)
        {
            Destroy(shapes[i].gameObject);
        }
        shapes.Clear();
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(shapes.Count);
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

        int count = version <= 0 ? -version : reader.ReaderInt();
        for(int i = 0; i < count; ++i)
        {
            int shapeId = version > 0 ? reader.ReaderInt() : 0;
            int materialId = version > 0 ? reader.ReaderInt() : 0;
            Shape instance = shapeFactory.Get(shapeId, materialId);
            instance.Load(reader);
            shapes.Add(instance);
        }
    }
}
