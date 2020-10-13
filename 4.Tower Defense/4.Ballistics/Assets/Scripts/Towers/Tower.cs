using TMPro;
using UnityEngine;

public abstract class Tower : GameTileContent
{
    [SerializeField, Range(1.5f, 10.5f)]
    protected float targetingRange = 1.5f;

    const int enemyLayerMask = 1 << 9;

    static Collider[] targetsBuffer = new Collider[100];

    public abstract TowerType TowerType { get; }
   
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 position = transform.localPosition;
        position.y += 0.1f;
        Gizmos.DrawWireSphere(position, targetingRange);

        //if(target != null)
        //{
        //    Gizmos.DrawLine(position, target.Position);
        //}
    }

    protected bool AcquireTarget(ref TargetPoint target)
    {
        //Collider[] targets = Physics.OverlapSphere(transform.localPosition, targetingRange, enemyLayerMask);
        Vector3 a = transform.localPosition;
        Vector3 b = a;
        b.y += 2f;
        //Collider[] targets = Physics.OverlapCapsule(a, b, targetingRange, enemyLayerMask);
        int hits = Physics.OverlapCapsuleNonAlloc(a, b, targetingRange, targetsBuffer, enemyLayerMask);

        //if(targets.Length > 0)
        if(hits > 0)
        {
            //target = targets[0].GetComponent<TargetPoint>();
            target = targetsBuffer[Random.Range(0, hits)].GetComponent<TargetPoint>();
            Debug.Assert(target != null, "Targeted non-enemy!", targetsBuffer[0]);
            return true;
        }

        target = null;
        return false;
    }

    protected bool TrackTarget(ref TargetPoint target)
    {
        if(target == null)
        {
            return false;
        }

        Vector3 a = transform.localPosition;
        Vector3 b = target.Position;
        float x = a.x - b.x;
        float y = a.z - b.z;
        float r = targetingRange + 0.125f * target.Enemy.Scale;
        //if(Vector3.Distance(a, b) > targetingRange + 0.125f * target.Enemy.Scale)
        if(x * x + y * y > r * r)
        {
            target = null;
            return false;
        }

        return true;
    }
}
