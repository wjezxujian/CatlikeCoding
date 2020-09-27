using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShapeBehaviourType
{
    Movement,
    Rotation,
    Oscillation
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
        }

        Debug.LogError("Forgot to support " + type);
        return null;
    }
}