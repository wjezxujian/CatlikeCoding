using UnityEngine;

public class SatelliteShapeBehaviour : ShapeBehaviour
{
    Shape focalShape;

    float frequency;

    Vector3 cosOffset, sinOffset;

    public override ShapeBehaviourType BehaviourType 
    {
        get { return ShapeBehaviourType.Satellite; }
    }

    public void Initialize(Shape shape, Shape focalShape, float radius, float frequency)
    {
        this.focalShape = focalShape;
        this.frequency = frequency;
        cosOffset = Vector3.right;
        sinOffset = Vector3.forward;
        cosOffset *= radius;
        sinOffset *= radius;

        GameUpdate(shape);
    }

    public override void GameUpdate(Shape shape)
    {
        float t = 2f * Mathf.PI * frequency * shape.Age;
        shape.transform.localPosition = focalShape.transform.localPosition + cosOffset * Mathf.Cos(t) + sinOffset * Mathf.Sin(t);
    }

    public override void Save(GameDataWriter writer)
    {

    }

    public override void Load(GameDataReader reader)
    {
        
    }

    public override void Recyle()
    {
        ShapeBehaviourPool<SatelliteShapeBehaviour>.Reclaim(this);
    }

    

}
