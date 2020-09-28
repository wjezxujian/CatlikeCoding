using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : PersistableObject
{
    [SerializeField]
    MeshRenderer[] meshRenderers;

    int shapeId = int.MinValue;

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

    public int InstanceId { get; private set; }

    public int SaveIndex { get; set; }

    private void Awake()
    {
        colors = new Color[meshRenderers.Length];
    }

    public void GameUpdate()
    {
        Age += Time.deltaTime;

        for(int i = 0; i < behaviourList.Count; ++i)
        {
            if (!behaviourList[i].GameUpdate(this))
            {
                behaviourList[i].Recyle();
                behaviourList.RemoveAt(i--);
            }
        }
    }

    public void Recycle()
    {
        Age = 0f;
        InstanceId += 1;

        for(int i = 0; i < behaviourList.Count; ++i)
        {
            behaviourList[i].Recyle();
        }
        behaviourList.Clear();
        OriginFactory.Reclaim(this);
    }

    public void ResolveShapeInstances()
    {
        for(int i = 0; i < behaviourList.Count; ++i)
        {
            behaviourList[i].ResolveShapeInstances();
        }
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
        writer.Write(colors.Length);
        for(int i = 0; i < colors.Length; ++i)
        {
            writer.Write(colors[i]);
        }
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
            LoadColors(reader);
        }
        else
        {
            SetColor(reader.Version > 0 ? reader.ReadColor() : Color.white);
        }
        
        if(reader.Version >= 6)
        {
            Age = reader.ReadFloat();
            int behaviourCount = reader.ReadInt();
            for(int i = 0; i < behaviourCount; ++i)
            {
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
}
