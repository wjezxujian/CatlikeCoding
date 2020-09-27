using UnityEngine;

public sealed class MovementShapeBehaviour : ShapeBehaviour
{
    public Vector3 Velocity { get; set; }

    public override void GameUpdate(Shape shape)
    {
        shape.transform.localPosition += Velocity * Time.deltaTime;
    }

    public override void Save(GameDataWriter writer)
    {
        writer.Write(Velocity);   
    }

    public override void Load(GameDataReader reader)
    {
        Velocity = reader.ReadVector3();
    }

    public override ShapeBehaviourType BehaviourType
    {
        get { return ShapeBehaviourType.Movement; }
    }

    public override void Recyle()
    {
        ShapeBehaviourPool<MovementShapeBehaviour>.Reclaim(this);
    }
}
