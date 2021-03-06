﻿using UnityEditor.Build;
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
        
        shape.AngularVelocity = Random.onUnitSphere * spawnConfg.scale.RandomValueInRange;
        //shape.Velocity = Random.onUnitSphere * Random.Range(0f, 2f);

        Vector3 direction;
        switch (spawnConfg.movementDirection)
        {
            case SpawnConfguration.MovementDirection.Upward:
                direction = transform.up;
                break;
            case SpawnConfguration.MovementDirection.Outward:
                direction = (t.localPosition - transform.position).normalized;
                break;
            case SpawnConfguration.MovementDirection.Random:
                direction = Random.onUnitSphere;
                break;
            default:
                direction = transform.forward;
                break;
        }
       
        shape.Velocity = direction * spawnConfg.speed.RandomValueInRange;

        return shape;
    }
}
