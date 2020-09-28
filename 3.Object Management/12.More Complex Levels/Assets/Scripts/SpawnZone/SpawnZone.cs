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

            public bool uniformLifecycles;
        }

        public SatelliteConfiguration satellite;
        
        [System.Serializable]
        public struct LifecycleConfiguration
        {
            [FloatRangeSlider(0f, 2f)]
            public FloatRange growingDurection;

            [FloatRangeSlider(0f, 100f)]
            public FloatRange adultDuration;

            [FloatRangeSlider(0f, 2f)]
            public FloatRange dyingDurection;


            public Vector3 RandomDurations
            { 
                get 
                { 
                    return new Vector3(growingDurection.RandomValueInRange, 
                        adultDuration.RandomValueInRange,
                        dyingDurection.RandomValueInRange); 
                }
            }

        }

        public LifecycleConfiguration lifecycle;
    }

    [SerializeField]
    SpawnConfguration spawnConfig;

    public virtual Vector3 SpawnPoint { get; }

    public virtual void SpawnShapes()
    {
        int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
        Shape shape = spawnConfig.factories[factoryIndex].GetRandom();
        Transform t = shape.transform;
        t.localPosition = SpawnPoint;
        t.localRotation = Random.rotation;
        t.localScale = Vector3.one * spawnConfig.scale.RandomValueInRange;
        SetupColor(shape);

        float angularSpeed = spawnConfig.angularSpeed.RandomValueInRange;
        if(angularSpeed != 0f)
        {
            var rotation = shape.AddBehaviour<RotationShapeBehaviour>();
            rotation.AngularVelocity = Random.onUnitSphere * angularSpeed;
        }
        
        float speed = spawnConfig.speed.RandomValueInRange;
        if(speed != 0)
        {
            var movement = shape.AddBehaviour<MovementShapeBehaviour>();
            movement.Velocity = GetDirectionVector(spawnConfig.movementDirection, t) * speed;
        }

        SetupOscillation(shape);

        Vector3 lifecycleDurations = spawnConfig.lifecycle.RandomDurations;

        int satelliteCount = spawnConfig.satellite.amount.RandomValueInRange;
        for (int i = 0; i < satelliteCount; ++i)
        {
            Vector3 durations = spawnConfig.satellite.uniformLifecycles ? 
                lifecycleDurations : spawnConfig.lifecycle.RandomDurations;
            CreateSatelliteFor(shape, durations);
        }

        SetupLifecycle(shape, lifecycleDurations);
    }

    void CreateSatelliteFor(Shape focalShape, Vector3 lifecycleDurations)
    {
        int factoryIndex = Random.Range(0, spawnConfig.factories.Length);
        Shape shape = spawnConfig.factories[factoryIndex].GetRandom();
        Transform t = shape.transform;
        t.localRotation = Random.rotation;
        t.localScale = focalShape.transform.localScale * spawnConfig.satellite.relativeScale.RandomValueInRange;

        SetupColor(shape);
        shape.AddBehaviour<SatelliteShapeBehaviour>().Initialize(shape, focalShape, 
            spawnConfig.satellite.orbitRaduis.RandomValueInRange, spawnConfig.satellite.orbitFrequency.RandomValueInRange);

        SetupLifecycle(shape, lifecycleDurations);
    }

    void SetupLifecycle(Shape shape, Vector3 durations)
    {
        if(durations.x > 0f)
        {
            if(durations.y > 0f || durations.z > 0f)
            {
                shape.AddBehaviour<LifecycleShapeBehaviour>().Initialize(shape, durations.x, durations.y, durations.z);
            }
            else
            {
                shape.AddBehaviour<GrowingShapeBehaviour>().Initialize(shape, durations.x);
            }  
        }
        else if(durations.y > 0f)
        {
            shape.AddBehaviour<LifecycleShapeBehaviour>().Initialize(shape, durations.x, durations.y, durations.z);
        }
        else if (durations.z > 0f)
        {
            shape.AddBehaviour<DyingShapeBehaviour>().Initialize(shape, durations.z);
        }

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
