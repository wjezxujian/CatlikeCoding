using UnityEngine;

public class GravityBox : GravitySource
{
    [SerializeField]
    float gravity = 9.81f;

    [SerializeField]
    Vector3 boundaryDistance = Vector3.one;

    [SerializeField, Min(0f)]
    float innerDistance = 0f, innerFalloffDistance = 0f;

    [SerializeField, Min(0F)]
    float outerDistance = 0f, outerFalloffDistance = 0f;

    float innerFalloffFactor, outerFalloffFactor = 0f;

    public override Vector3 GetGravity(Vector3 position)
    {
        //position -= transform.position;
        position = transform.InverseTransformDirection(position - transform.position);

        Vector3 vector = Vector3.zero;
        Vector3 distance;
        distance.x = boundaryDistance.x - Mathf.Abs(position.x);
        distance.y = boundaryDistance.y - Mathf.Abs(position.y);
        distance.z = boundaryDistance.z - Mathf.Abs(position.z);
        if(distance.x < distance.y)
        {
            if(distance.x < distance.z)
            {
                vector.x = GetGravityComponent(position.x, distance.x);
            }
            else
            {
                vector.z = GetGravityComponent(position.z, distance.z);
            }
        }
        else if(distance.y < distance.z)
        {
            vector.y = GetGravityComponent(position.y, distance.y);
        }
        else
        {
            vector.z = GetGravityComponent(position.z, distance.z);
        }

        return transform.TransformDirection(vector);
    }

    float GetGravityComponent(float coordinate, float distance)
    {
        if (distance > innerDistance)
        {
            return 0f;
        }

        float g = gravity;
        if (distance > innerDistance)
        {
            g *= 1f - (distance - innerDistance) * innerDistance;
        }

        return coordinate > 0f ? -g : g;
    }

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        boundaryDistance = Vector3.Max(boundaryDistance, Vector3.zero);

        float maxInner = Mathf.Min(Mathf.Min(boundaryDistance.x, boundaryDistance.y), boundaryDistance.z);
        innerDistance = Mathf.Min(innerDistance, maxInner);
        innerFalloffDistance = Mathf.Max(Mathf.Min(innerFalloffDistance, maxInner), innerDistance);
        outerFalloffDistance = Mathf.Max(outerFalloffFactor, outerDistance);

        innerFalloffFactor = 1f / (innerFalloffFactor - innerDistance);
        outerFalloffFactor = 1f / (outerFalloffDistance - outerDistance);
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        Vector3 size;
        if(innerFalloffDistance > innerDistance)
        {
            Gizmos.color = Color.cyan;
            size.x = 2f * (boundaryDistance.x - innerFalloffDistance);
            size.y = 2f * (boundaryDistance.y - innerFalloffDistance);
            size.z = 2f * (boundaryDistance.z - innerFalloffDistance);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }

        if(innerDistance > 0f)
        {
            Gizmos.color = Color.yellow;
            size.x = 2f * (boundaryDistance.x - innerDistance);
            size.y = 2f * (boundaryDistance.x - innerDistance);
            size.z = 2f * (boundaryDistance.z - innerDistance);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, 2f * boundaryDistance);
    }

    
}
