using UnityEngine;

public class DyingShapeBehaviour : ShapeBehaviour
{
    Vector3 origianlScale;
    float duration, dyingAge;

    public override ShapeBehaviourType BehaviourType
    {
        get { return ShapeBehaviourType.Dying; }
    }

    public void Initialize(Shape shape, float duration)
    {
        origianlScale = shape.transform.localScale;
        this.duration = duration;
        dyingAge = shape.Age;

        shape.MarkAsDying();
    }

    public override bool GameUpdate(Shape shape)
    {
        float dyingDuration = shape.Age - dyingAge;
        if(dyingDuration < duration)
        {
            float s = 1f - dyingDuration / duration;
            s = (3f - 2f * s) * s * s;
            shape.transform.localScale = s * origianlScale;
            return true;
        }

        //shape.transform.localScale = Vector3.zero;
        shape.Die();
        return true;
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(origianlScale);
        writer.Write(duration);
        writer.Write(dyingAge);
    }

    public override void Load(GameDataReader reader)
    {
        origianlScale = reader.ReadVector3();
        duration = reader.ReadFloat();
        dyingAge = reader.ReadFloat();
    }

    public override void Recyle()
    {
        ShapeBehaviourPool<DyingShapeBehaviour>.Reclaim(this);
    }

    
}
