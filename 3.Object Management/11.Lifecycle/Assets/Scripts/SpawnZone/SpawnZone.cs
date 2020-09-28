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

        [System.Serializable]
        public struct SatelliteConfiguration
        {
            public IntRange amount;

            [FloatRangeSlider(0.1f, 1f)]
            public FloatRange relativeScale;

            public FloatRange orbitRaduis;

            public FloatRange orbitFrequency;
        }

        public SatelliteConfiguration satellite;     
    }

    [SerializeField]
    SpawnConfguration spawnConfig;

    public virtual Vector3 SpawnPoint { get; }

    //public virtual void ConfigureSpawn(Shape shape)
    public virtual void SpawnShapes()
    {
        int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
        Shape shape = spawnConfig.factories[factoryIndex].GetRandom();
        Transform t = shape.transform;
        t.localPosition = SpawnPoint;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;
        //shape.SetColor(Random.ColorHSV(hueMin: 0f, hueMax: 1f, saturationMin: 0.5f,
        //    saturationMax: 1f, valueMin: 0.25f, valueMax: 1f, alphaMin: 1f, alphaMax: 1f));
        SetupColor(shape);

        //shape.AngularVelocity = Random.onUnitSphere * spawnConfig.scale.RandomValueInRange;
        //shape.Velocity = Random.onUnitSphere * Random.Range(0f, 2f);
        float angularSpeed = spawnConfig.angularSpeed.RandomValueInRange;
        if(angularSpeed != 0f)
        {
            //var rotation = shape.gameObject.AddComponent<RotationShapeBehavior>();
            var rotation = shape.AddBehaviour<RotationShapeBehaviour>();
            rotation.AngularVelocity = Random.onUnitSphere * angularSpeed;
        }
        
        //shape.Velocity = direction * spawnConfig.speed.RandomValueInRange;
        float speed = spawnConfig.speed.RandomValueInRange;
        if(speed != 0)
        {
            //Vector3 direction;
            //switch (spawnConfig.movementDirection)
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
            movement.Velocity = GetDirectionVector(spawnConfig.movementDirection, t) * speed;
        }

        SetupOscillation(shape);

        int satelliteCount = spawnConfig.satellite.amount.RandomValueInRange;
        for (int i = 0; i < satelliteCount; ++i)
        {
            CreateSatelliteFor(shape);
        }

        //return shape;
    }

    void CreateSatelliteFor(Shape focalShape)
    {
        int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
        Shape shape = spawnConfig.factories[factoryIndex].GetRandom();
        Transform t = shape.transform;
        t.localRotation = Random.rotation;
        //t.localScale = focalShape.transform.localScale * 0.5f;
        t.localScale = focalShape.transform.localScale * spawnConfig.satellite.relativeScale.RandomValueInRange;
        //t.localPosition = focalShape.transform.localPosition + Vector3.up;
        //shape.AddBehaviour<MovementShapeBehaviour>().Velocity = Vector3.up;
        SetupColor(shape);
        shape.AddBehaviour<SatelliteShapeBehaviour>().Initialize(shape, focalShape, 
            spawnConfig.satellite.orbitRaduis.RandomValueInRange, spawnConfig.satellite.orbitFrequency.RandomValueInRange);
    }

    void SetupColor(Shape shape)
    {
        if (spawnConfig.uniformColor)
        {
            shape.SetColor(spawnConfig.color.RandomInRange);
        }
        else
        {
            for(int i = 0; i < shape.ColorCount; ++i)
            {
                var color = spawnConfig.color.RandomInRange;
                shape.SetColor(color, i);
            }
        }

    }

    void SetupOscillation(Shape shape)
    {
        float amplitude = spawnConfig.oscillationAmplitude.RandomValueInRange;
        float frequency = spawnConfig.oscillationFrequency.RandomValueInRange;
        if (amplitude == 0f || frequency == 0f)
        {
            return;
        }

        var oscillation = shape.AddBehaviour<OscillationShapeBehaviour>();
        oscillation.Offset = GetDirectionVector(spawnConfig.oscillationDirection, shape.transform) * amplitude;
        oscillation.Frequency = frequency;
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

    
}
