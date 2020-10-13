using TMPro;
using UnityEngine;

public class Tower : GameTileContent
{
    [SerializeField, Range(1.5f, 10.5f)]
    float targetingRange = 1.5f;

    [SerializeField, Range(1f, 100f)]
    float damagePerSecond = 10f;

    [SerializeField]
    Transform turret = default;

    [SerializeField]
    Transform laserBeam = default;
    
    Tower towerPrefab = default;

    TargetPoint target;

    Vector3 laserBeamScale;

    const int enemyLayerMask = 1 << 9;

    static Collider[] targetsBuffer = new Collider[100];

    public override void GameUpdate()
    {
        if (TrackTarget() || AcquireTarget())
        {
            //Debug.Log("Searching for target...");
            Shoot();
        }
        else
        {
            laserBeam.localScale = Vector3.zero;
        }
        
    }

    private void Shoot()
    {
        Vector3 point = target.Position;
        turret.LookAt(point);
        laserBeam.localRotation = turret.localRotation;

        float d = Vector3.Distance(turret.position, point);
        laserBeamScale.z = d;
        laserBeam.localScale = laserBeamScale;
        laserBeam.localPosition = turret.localPosition + 0.5f * d * laserBeam.forward;

        target.Enemy.ApplyDamage(damagePerSecond * Time.deltaTime);
    }

    private void Awake()
    {
        laserBeamScale = laserBeam.localScale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 position = transform.localPosition;
        position.y += 0.1f;
        Gizmos.DrawWireSphere(position, targetingRange);

        if(target != null)
        {
            Gizmos.DrawLine(position, target.Position);
        }
    }

    private bool AcquireTarget()
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

    private bool TrackTarget()
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
