using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscillationShapeBehaviour : ShapeBehaviour
{
    float previousOscillation;

    public override ShapeBehaviourType BehaviourType 
    {
        get
        {
            return ShapeBehaviourType.Oscillation;
        }
    }

    public Vector3 Offset { get; set; }

    public float Frequency { get; set; }

    public override bool GameUpdate(Shape shape)
    {
        float oscillation = Mathf.Sin(2f * Mathf.PI * Frequency * shape.Age);
        shape.transform.localPosition += (oscillation - previousOscillation) * Offset;
        previousOscillation = oscillation;

        return true;
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(Offset);
        writer.Write(Frequency);
        writer.Write(previousOscillation);
    }

    public override void Load(GameDataReader reader)
    {
        Offset = reader.ReadVector3();
        Frequency = reader.ReadFloat();
        previousOscillation = reader.ReadFloat();
    }

    public override void Recyle()
    {
        previousOscillation = 0f;
        ShapeBehaviourPool<OscillationShapeBehaviour>.Reclaim(this);
    }

    
}
