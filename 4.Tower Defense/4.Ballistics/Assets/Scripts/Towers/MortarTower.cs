using UnityEngine;

public class MortarTower : Tower
{
    [SerializeField, Range(0.5f, 2f)]
    float shotsPerSecond = 1f;

    [SerializeField]
    Transform mortar = default;

    public override TowerType TowerType => TowerType.Mortar;

    public override void GameUpdate()
    {
        Launch(new Vector3(3f, 0, 0f));
        Launch(new Vector3(0f, 0, 1f));
        Launch(new Vector3(1f, 0, 1f));
        Launch(new Vector3(3f, 0, 1f));
    }

    public void Launch(Vector3 offset)
    {
        Vector3 launchPoint = mortar.position;
        //Vector3 targetPoint = new Vector3(launchPoint.x + 3f, 0f, launchPoint.z);
        Vector3 targetPoint = launchPoint + offset;
        targetPoint.y = 0f;

        Vector2 dir;
        dir.x = targetPoint.x - launchPoint.x;
        dir.y = targetPoint.z - launchPoint.z;
        float x = dir.magnitude;
        float y = -launchPoint.y;
        dir /= x;

        Debug.DrawLine(launchPoint, targetPoint, Color.yellow);
        Debug.DrawLine(
            new Vector3(launchPoint.x, 0.01f, launchPoint.z),
            new Vector3(launchPoint.x + dir.x * x, 0.01f, launchPoint.z + dir.y * x),
            //new Vector3(targetPoint.x, 0.01f, targetPoint.z),
            Color.white);
    }
}
