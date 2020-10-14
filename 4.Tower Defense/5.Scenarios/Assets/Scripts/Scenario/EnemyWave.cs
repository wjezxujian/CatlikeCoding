using UnityEngine;

[CreateAssetMenu]
public class EnemyWave : ScriptableObject
{
    [SerializeField]
    EnemySpawnSequence[] sapwnSequences = { new EnemySpawnSequence() };
}
