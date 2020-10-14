using UnityEngine;

[CreateAssetMenu]
public class GameScenario : ScriptableObject
{
    [SerializeField]
    EnemyWave[] waves = { };
}
