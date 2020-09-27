using UnityEngine;

public sealed class RotationShapeBehaviour : ShapeBehaviour
{
    public Vector3 AngularVelocity { get; set; }

    public override void GameUpdate(Shape shape)
    {
        shape.transform.Rotate(AngularVelocity * Time.deltaTime);
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(AngularVelocity);
    }

    public override void Load(GameDataReader reader)
    {
        AngularVelocity = reader.ReadVector3();
    }

    public override ShapeBehaviourType BehaviourType
    {
        get
        {
            return ShapeBehaviourType.Movement;
        }
    }

    public override void Recyle()
    {
        ShapeBehaviourPool<RotationShapeBehaviour>.Reclaim(this);
    }
}
