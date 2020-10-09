using UnityEngine;

public class LifeZone : MonoBehaviour
{
    [SerializeField]
    float dyingDuration;

    private void OnTriggerExit(Collider other)
    {
        var shape = other.GetComponent<Shape>();
        if (shape)
        {
            if(dyingDuration <= 0f)
            {
                shape.Die();
            }
            else if (!shape.IsMarkedAsDying)
            {
                shape.AddBehaviour<DyingShapeBehaviour>().Initialize(shape, dyingDuration);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        //Gizmos.matrix = transform.localToWorldMatrix;
        Collider collider = GetComponent<Collider>();
        BoxCollider boxCollider = collider as BoxCollider;
        if (boxCollider != null)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }

        SphereCollider sphereCollider = collider as SphereCollider;
        if (sphereCollider != null)
        {
            Vector3 scale = transform.lossyScale;
            scale = Vector3.one * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, scale);
            Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
        }
    }
}
