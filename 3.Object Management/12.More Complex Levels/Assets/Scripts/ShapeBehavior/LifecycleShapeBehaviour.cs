using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifecycleShapeBehaviour : ShapeBehaviour
{
    float adultDuration, dyingDuration, dyingAge;

    public override ShapeBehaviourType BehaviourType
    {
        get { return ShapeBehaviourType.Lifecycle; }
    }

    public void Initialize(Shape shape, float growingDuration, float adultDuration, float dyingDuration)
    {
        this.adultDuration = adultDuration;
        this.dyingDuration = dyingDuration;
        dyingAge = growingDuration + adultDuration;

        if(growingDuration > 0f)
        {
            shape.AddBehaviour<GrowingShapeBehaviour>().Initialize(shape, growingDuration);
        }
    }

    public override bool GameUpdate(Shape shape)
    {
        if(shape.Age >= dyingAge)
        {
            if (dyingDuration <= 0f)
            {
                shape.Die();
                return true;
            }

            if (!shape.IsMarkedAsDying)
            {
                shape.AddBehaviour<DyingShapeBehaviour>().Initialize(shape, dyingDuration + dyingAge - shape.Age);
            }
            
            return false;
        }

        return true;
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(adultDuration);
        writer.Write(dyingDuration);
        writer.Write(dyingAge);
    }

    public override void Load(GameDataReader reader)
    {
        adultDuration = reader.ReadFloat();
        dyingDuration = reader.ReadFloat();
        dyingAge = reader.ReadFloat();
    }

    public override void Recyle()
    {
        ShapeBehaviourPool<LifecycleShapeBehaviour>.Reclaim(this);
    }
}
