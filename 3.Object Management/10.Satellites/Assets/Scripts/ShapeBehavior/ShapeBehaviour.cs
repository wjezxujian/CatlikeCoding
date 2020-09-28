using UnityEngine;

public abstract class ShapeBehaviour
#if UNITY_EDITOR
    : ScriptableObject
#endif
{
    public abstract bool GameUpdate(Shape shape);

    public abstract void Save(GameDataWriter writer);

    public abstract void Load(GameDataReader reader);

    public abstract ShapeBehaviourType BehaviourType { get; }

    public abstract void Recyle();

    public virtual void ResolveShapeInstances() {}

#if UNITY_EDITOR
    public bool IsReclaimed { get; set; }

    private void OnEnable()
    {
        if (IsReclaimed)
        {
            Recyle();
        }
    }
#endif
}
