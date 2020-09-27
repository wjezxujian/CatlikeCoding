﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : PersistableObject
{
    [SerializeField]
    MeshRenderer[] meshRenderers;

    //MeshRenderer meshRenderer;

    int shapeId = int.MinValue;

    //Color color;
    Color[] colors;

    ShapeFactory originFactory;

    List<ShapeBehaviour> behaviourList = new List<ShapeBehaviour>();

    static int colorPropertyId = Shader.PropertyToID("_Color");
    static MaterialPropertyBlock sharedPropertyBlock;

    public int ShapeId
    {
        get { return shapeId; }
        set 
        { 
            if(shapeId == int.MinValue && value != int.MinValue)
            {
                shapeId = value;
            }
            else
            {
                Debug.LogError("Not allowed to change shapeId");
            }
            
        }
    }

    public int MaterialId
    {
        get;
        private set;
    }

    //public Vector3 AngularVelocity { get; set; }
    //public Vector3 Velocity { get; set; }

    public int ColorCount { get { return colors.Length; } }

    public ShapeFactory OriginFactory
    {
        get { return originFactory; }

        set
        {
            if (originFactory == null)
            {
                originFactory = value;
            }
            else
            {
                Debug.LogError("Not allowed to change origin facroty");
            }
        }
    }

    public float Age { get; private set; }

    private void Awake()
    {
        //meshRenderer = GetComponent<MeshRenderer>();
        colors = new Color[meshRenderers.Length];
    }

    //private void FixedUpdate()
    public void GameUpdate()
    {
        //transform.Rotate(AngularVelocity * Time.deltaTime);
        //transform.localPosition += Velocity * Time.deltaTime;

        Age += Time.deltaTime;

        for(int i = 0; i < behaviourList.Count; ++i)
        {
            behaviourList[i].GameUpdate(this);
        }
    }

    public void Recycle()
    {
        Age = 0f;

        for(int i = 0; i < behaviourList.Count; ++i)
        {
            //Destroy(behaviourList[i]);
            behaviourList[i].Recyle();
        }
        behaviourList.Clear();
        OriginFactory.Reclaim(this);
    }

    public void SetMaterial(Material material, int materialId)
    {
        MaterialId = materialId;
        for(int i = 0; i < meshRenderers.Length; ++i)
        {
            meshRenderers[i].material = material;
        }
    }

    public void SetColor(Color color)
    {
        //this.color = color;
        if(sharedPropertyBlock == null)
        {
            sharedPropertyBlock = new MaterialPropertyBlock();
        }
        sharedPropertyBlock.SetColor(colorPropertyId, color);

        for(int i = 0; i < meshRenderers.Length; i++)
        {
            colors[i] = color;
            meshRenderers[i].SetPropertyBlock(sharedPropertyBlock);
        }
    }

    public void SetColor(Color color, int index)
    {
        if (sharedPropertyBlock == null)
        {
            sharedPropertyBlock = new MaterialPropertyBlock();
        }
        sharedPropertyBlock.SetColor(colorPropertyId, color);
        colors[index] = color;
        meshRenderers[index].SetPropertyBlock(sharedPropertyBlock);
    }

    public override void Save(GameDataWriter writer)
    {
        base.Save(writer);
        //writer.Write(color);
        writer.Write(colors.Length);
        for(int i = 0; i < colors.Length; ++i)
        {
            writer.Write(colors[i]);
        }
        //writer.Write(AngularVelocity);
        //writer.Write(Velocity);
        writer.Write(Age);
        writer.Write(behaviourList.Count);
        for(int i = 0; i < behaviourList.Count; ++i)
        {
            writer.Write((int)behaviourList[i].BehaviourType);
            behaviourList[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        base.Load(reader);
        if(reader.Version >= 5)
        {
            //for(int i = 0; i < colors.Length; ++i)
            //{
            //    SetColor(reader.ReadColor(), i);
            //}
            LoadColors(reader);
        }
        else
        {
            SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
        }
        
        //AngularVelocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
        //Velocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
        if(reader.Version >= 6)
        {
            Age = reader.ReadFloat();
            int behaviourCount = reader.ReadInt();
            for(int i = 0; i < behaviourCount; ++i)
            {
                //AddBehaviour((ShapeBehaviourType)reader.ReadInt()).Load(reader);
                ShapeBehaviour behabviour = ((ShapeBehaviourType)reader.ReadInt()).GetInstance();
                behaviourList.Add(behabviour);
                behabviour.Load(reader);
                
            }
        }else if(reader.Version >= 4)
        {
            AddBehaviour<RotationShapeBehaviour>().AngularVelocity = reader.ReadVector3();
            AddBehaviour<MovementShapeBehaviour>().Velocity = reader.ReadVector3();
        }
    }

    void LoadColors(GameDataReader reader)
    {
        int count = reader.ReadInt();
        int max = count <= colors.Length ? count : colors.Length;
        int i = 0;
        for(; i < max; ++i)
        {
            SetColor(reader.ReadColor(), i);
        }

        if(count > colors.Length)
        {
            for(; i < count; ++i)
            {
                reader.ReadColor();
            }
        }else if(count < colors.Length)
        {
            for(; i < colors.Length; ++i)
            {
                SetColor(Color.white, i);
            }
        }
    }

    public T AddBehaviour<T>() where T : ShapeBehaviour, new()
    {
        T behaviour = ShapeBehaviourPool<T>.Get();
        behaviourList.Add(behaviour);
        return behaviour;
    }

    //private ShapeBehaviour AddBehaviour(ShapeBehaviourType type)
    //{
    //    switch (type)
    //    {
    //        case ShapeBehaviourType.Movement:
    //            return AddBehaviour<MovementShapeBehaviour>();
    //        case ShapeBehaviourType.Rotation:
    //            return AddBehaviour<RotationShapeBehaviour>();
    //    }

    //    Debug.LogError("Forgot to support " + type);
    //    return null;
    //}

    






}
