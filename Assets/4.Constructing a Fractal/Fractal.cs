using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fractal : MonoBehaviour
{
    public Mesh mesh;
    public Material material;
    public int maxDepth = 4;
    public float childScale;

    private int depth = 0;

    private void Start()
    {
        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        gameObject.AddComponent<MeshRenderer>().material = material;

        if(depth < maxDepth)
        {
            //new GameObject("Fractal Child").AddComponent<Fractal>();
            StartCoroutine(CreateChildren());            
        }
    }

    private IEnumerator CreateChildren()
    {
        yield return new WaitForSeconds(0.5f);
        new GameObject("Fractal Child").AddComponent<Fractal>().Initialize(this, Vector3.up, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);
        new GameObject("Fractal Child").AddComponent<Fractal>().Initialize(this, Vector3.right, Quaternion.Euler(0, 0, -90f));

        yield return new WaitForSeconds(0.5f);
        new GameObject("Fractal Child").AddComponent<Fractal>().Initialize(this, Vector3.left, Quaternion.Euler(0, 0, 90f));
    }

    private void Initialize(Fractal partent, Vector3 direction, Quaternion orientation)
    {
        mesh = partent.mesh;
        material = partent.material;
        maxDepth = partent.maxDepth;
        depth = partent.depth + 1;
        childScale = partent.childScale;
        transform.parent = partent.transform;
        transform.localScale = Vector3.one * childScale;
        transform.localPosition = direction * (0.5f + 0.5f * childScale);
        transform.localRotation = orientation;
    }
}
