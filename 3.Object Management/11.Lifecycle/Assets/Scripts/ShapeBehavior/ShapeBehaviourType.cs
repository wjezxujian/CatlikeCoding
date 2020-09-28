using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShapeBehaviourType
{
    Movement,
    Rotation,
    Oscillation,
    Satellite,
    Growing,
    Dying,
    Lifecycle
}

public static class ShapeBehaviourTypeMethods
{
    public static ShapeBehaviour GetInstance(this ShapeBehaviourType type)
    {
        switch (type)
        {
            case ShapeBehaviourType.Movement:
                return ShapeBehaviourPool<MovementShapeBehaviour>.Get();
            case ShapeBehaviourType.Rotation:
                return ShapeBehaviourPool<RotationShapeBehaviour>.Get();
            case ShapeBehaviourType.Oscillation:
                return ShapeBehaviourPool<OscillationShapeBehaviour>.Get();
            case ShapeBehaviourType.Satellite:
                return ShapeBehaviourPool<SatelliteShapeBehaviour>.Get();
            case ShapeBehaviourType.Growing:
                return ShapeBehaviourPool<GrowingShapeBehaviour>.Get();
            case ShapeBehaviourType.Lifecycle:
                return ShapeBehaviourPool<LifecycleShapeBehaviour>.Get();
        }

        Debug.LogError("Forgot to support " + type);
        return null;
    }
}