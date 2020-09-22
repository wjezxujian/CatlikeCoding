using UnityEngine;

public class GravityPlane : GravitySource
{
    [SerializeField]
    float gravity = 9.18f;

    [SerializeField, Min(0f)]
    float range = 1f;

    public override Vector3 GetGravity(Vector3 position)
    {
        Vector3 up = transform.up;    //重力方向跟局部坐标系保持一直
        //Vector3 up = Vector3.up;        //重力方向跟世界坐标系保持一直
        float distance = Vector3.Dot(up, position - transform.position);
        if(distance > range)
        {
            return Vector3.zero;
        }

        float g = -gravity;
        if(distance > 0f)
        {
            g *= 1f - distance / range;
        }

        return g * up;
    }

    void OnDrawGizmos()
    {
        //Gizmos.matrix = transform.localToWorldMatrix;
        Vector3 scale = transform.localScale;
        scale.y = range;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, scale);
        Vector3 size = new Vector3(1f, 0f, 1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, size);

        if(range > 0f)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.up, size);
        }
    }
}
