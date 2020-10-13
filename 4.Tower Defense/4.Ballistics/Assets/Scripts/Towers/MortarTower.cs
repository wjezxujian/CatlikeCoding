using UnityEngine;

public class MortarTower : Tower
{
    [SerializeField, Range(0.5f, 2f)]
    float shotsPerSecond = 1f;

    [SerializeField]
    Transform mortar = default;

    public override TowerType TowerType => TowerType.Mortar;
}
