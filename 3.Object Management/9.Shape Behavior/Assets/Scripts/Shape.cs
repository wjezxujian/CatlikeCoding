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

    public Vector3 AngularVelocity { get; set; }
    public Vector3 Velocity { get; set; }

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

    private void Awake()
    {
        //meshRenderer = GetComponent<MeshRenderer>();
        colors = new Color[meshRenderers.Length];
    }

    //private void FixedUpdate()
    public void GameUpdate()
    {
        transform.Rotate(AngularVelocity * Time.deltaTime);
        transform.localPosition += Velocity * Time.deltaTime;
    }

    public void Recyle()
    {
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
        writer.Write(AngularVelocity);
        writer.Write(Velocity);
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
        
        AngularVelocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
        Velocity = reader.Version >= 4 ? reader.ReadVector3() : Vector3.zero;
    }

    void LoadColors(GameDataReader reader)
    {
        int count = reader.ReaderInt();
        int max = count <= colors.Length ? count : colors.Length;
        int i = 0;
        for(; i < max; ++i)
        {
            SetColor(reader.ReadColor(), 1);
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




}
