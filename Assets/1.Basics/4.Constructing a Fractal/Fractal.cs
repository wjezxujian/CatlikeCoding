using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fractal : MonoBehaviour
{
    public Mesh[] meshs;
    public Material material;
    public int maxDepth = 4;
    public float childScale;
    [Range(0, 1)]
    public float spawnProbability;
    public float maxRotationSpeed;
    public float maxTwist;

    private int depth = 0;
    private float rotationSpeed;

    private static Vector3[] childDirections =
    {
        Vector3.up,
        Vector3.right,
        Vector3.left,
        Vector3.forward,
        Vector3.back
    };

    private static Quaternion[] childOrientations =
    {
        Quaternion.identity,
        Quaternion.Euler(0f, 0f, -90f),
        Quaternion.Euler(0f, 0f, 90f),
        Quaternion.Euler(90f, 0f, 0f),
        Quaternion.Euler(-90, 0f, 0f)
    };

    private Material[,] materials;
    private void InitalizeMaterials()
    {
        materials = new Material[maxDepth + 1, 2];
        for(int i = 0; i <= maxDepth; ++i)
        {
            float t = i / (maxDepth - 1f);
            t *= t;

            materials[i, 0] = new Material(material);
            materials[i, 0].color = Color.Lerp(Color.white, Color.yellow, t);

            materials[i, 1] = new Material(material);
            materials[i, 1].color = Color.Lerp(Color.white, Color.cyan, t);
        }

        materials[maxDepth, 0].color = Color.magenta;
        materials[maxDepth, 1].color = Color.red;
    }

    private void Start()
    {
        if (materials == null)
            InitalizeMaterials();

        rotationSpeed = Random.Range(-maxRotationSpeed, maxRotationSpeed);
        transform.Rotate(Random.Range(-maxTwist, maxTwist), 0f, 0f);

        gameObject.AddComponent<MeshFilter>().mesh = meshs[Random.Range(0, meshs.Length)];
        gameObject.AddComponent<MeshRenderer>().material = materials[depth, Random.Range(0, 2)];

        if(depth < maxDepth)
        {
            //new GameObject("Fractal Child").AddComponent<Fractal>();
            StartCoroutine(CreateChildren());            
        }

    }

    private IEnumerator CreateChildren()
    {
        for(int i = 0; i < childDirections.Length; ++i)
        {
            if(Random.value < spawnProbability)
            {
                yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
                new GameObject("Fractal Child").AddComponent<Fractal>().Initialize(this, i);
            }            
        }

        //yield return new WaitForSeconds(0.5f);
        //new GameObject("Fractal Child").AddComponent<Fractal>().Initialize(this, Vector3.up, Quaternion.identity);

        //yield return new WaitForSeconds(0.5f);
        //new GameObject("Fractal Child").AddComponent<Fractal>().Initialize(this, Vector3.right, Quaternion.Euler(0, 0, -90f));

        //yield return new WaitForSeconds(0.5f);
        //new GameObject("Fractal Child").AddComponent<Fractal>().Initialize(this, Vector3.left, Quaternion.Euler(0, 0, 90f));
    }

    private void Initialize(Fractal partent, int childIndex)
    {
        meshs = partent.meshs;
        materials = partent.materials;
        material = partent.material;
        maxDepth = partent.maxDepth;
        depth = partent.depth + 1;
        childScale = partent.childScale;
        spawnProbability = partent.spawnProbability;
        maxRotationSpeed = partent.maxRotationSpeed;
        maxTwist = partent.maxTwist;
        transform.parent = partent.transform;
        transform.localScale = Vector3.one * childScale;
        //transform.localPosition = direction * (0.5f + 0.5f * childScale);
        //transform.localRotation = orientation;
        transform.localPosition = childDirections[childIndex] * (0.5f + 0.5f * childScale);
        transform.localRotation = childOrientations[childIndex];
    }

    private void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0f);
    }
}
