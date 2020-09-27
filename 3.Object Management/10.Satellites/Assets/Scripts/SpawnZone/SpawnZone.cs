using UnityEditor.Build;
using UnityEngine;

public abstract class SpawnZone : PersistableObject
{
    [System.Serializable]
    public class SpawnConfguration
    {
        public enum MovementDirection
        {
            Forward,
            Upward,
            Outward,
            Random
        }

        public ShapeFactory[] factories;

        public MovementDirection movementDirection;

        public FloatRange speed;

        public FloatRange angularSpeed;

        public FloatRange scale;

        public ColorRangeHSV color;

        public bool uniformColor;

        public MovementDirection oscillationDirection;

        public FloatRange oscillationAmplitude;

        public FloatRange oscillationFrequency;
    }

    [SerializeField]
    SpawnConfguration spawnConfg;

    public virtual Vector3 SpawnPoint { get; }

    //public virtual void ConfigureSpawn(Shape shape)
    public virtual Shape SpawnShape()
    {
        int factoryIndex = Random.Range(0, spawnConfg.factories.Length);
        Shape shape = spawnConfg.factories[factoryIndex].GetRandom();
        Transform t = shape.transform;
        t.localPosition = SpawnPoint;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * spawnConfg.scale.RandomValueInRange;
        //shape.SetColor(Random.ColorHSV(hueMin: 0f, hueMax: 1f, saturationMin: 0.5f,
        //    saturationMax: 1f, valueMin: 0.25f, valueMax: 1f, alphaMin: 1f, alphaMax: 1f));
        if (spawnConfg.uniformColor)
        {
            shape.SetColor(spawnConfg.color.RandomInRange);
        }
        else
        {
            for(int i = 0; i < shape.ColorCount; ++i)
            {
                shape.SetColor(spawnConfg.color.RandomInRange, i);
            }
        }

        //shape.AngularVelocity = Random.onUnitSphere * spawnConfg.scale.RandomValueInRange;
        //shape.Velocity = Random.onUnitSphere * Random.Range(0f, 2f);
        float angularSpeed = spawnConfg.angularSpeed.RandomValueInRange;
        if(angularSpeed != 0f)
        {
            //var rotation = shape.gameObject.AddComponent<RotationShapeBehavior>();
            var rotation = shape.AddBehaviour<RotationShapeBehaviour>();
            rotation.AngularVelocity = Random.onUnitSphere * angularSpeed;
        }
        
        //shape.Velocity = direction * spawnConfg.speed.RandomValueInRange;
        float speed = spawnConfg.speed.RandomValueInRange;
        if(speed != 0)
        {
            //Vector3 direction;
            //switch (spawnConfg.movementDirection)
            //{
            //    case SpawnConfguration.MovementDirection.Upward:
            //        direction = transform.up;
            //        break;
            //    case SpawnConfguration.MovementDirection.Outward:
            //        direction = (t.localPosition - transform.position).normalized;
            //        break;
            //    case SpawnConfguration.MovementDirection.Random:
            //        direction = Random.onUnitSphere;
            //        break;
            //    default:
            //        direction = transform.forward;
            //        break;
            //}

            //var movement = shape.gameObject.AddComponent<MovementShapeBehavior>();
            var movement = shape.AddBehaviour<MovementShapeBehaviour>();
            movement.Velocity = GetDirectionVector(spawnConfg.movementDirection, t) * speed;
        }

        SetupOscillation(shape);

        return shape;
    }

    Vector3 GetDirectionVector(SpawnConfguration.MovementDirection direction, Transform t)
    {
        switch (direction)
        {
            case SpawnConfguration.MovementDirection.Upward:
                return transform.up;
            case SpawnConfguration.MovementDirection.Outward:
                return (t.localPosition - transform.position).normalized;
            case SpawnConfguration.MovementDirection.Random:
                return Random.onUnitSphere;
            default:
                return transform.forward;
        }
    }

    void SetupOscillation(Shape shape)
    {
        float amplitude = spawnConfg.oscillationAmplitude.RandomValueInRange;
        float frequency = spawnConfg.oscillationFrequency.RandomValueInRange;
        if(amplitude == 0f || frequency == 0f)
        {
            return;
        }

        var oscillation = shape.AddBehaviour<OscillationShapeBehaviour>();
        oscillation.Offset = GetDirectionVector(spawnConfg.oscillationDirection, shape.transform) * amplitude;
        oscillation.Frequency = frequency;
    }
}
