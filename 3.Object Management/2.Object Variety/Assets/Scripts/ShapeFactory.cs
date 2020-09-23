using UnityEngine;

[CreateAssetMenu]
public class ShapeFactory : ScriptableObject
{
    [SerializeField]
    Shape[] prefabs;

    [SerializeField]
    Material[] materials;

    public Shape Get(int shapeId = 0, int materialId = 0)
    {
        //return Instantiate(prefabs[shapeId]);
        Shape instance = Instantiate(prefabs[shapeId]);
        instance.ShapeId = shapeId;
        instance.SetMaterial(materials[materialId], materialId);
        instance.SetColor(Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.25f, 1f, 1f, 1f));
        return instance;
    }

    public Shape GetRandom()
    {
        int shapeId = Random.Range(0, prefabs.Length);
        int materialId = Random.Range(0, materials.Length);
        return Get(shapeId, materialId);
    }
}
