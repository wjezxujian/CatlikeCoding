using System.Collections.Generic;
using UnityEngine;

public class CustomGravity : MonoBehaviour
{
    static List<GravitySource> sources = new List<GravitySource>();

    public static void Register(GravitySource source)
    {
        Debug.Assert(!sources.Contains(source), "Duplicate registration of gravity source!", source);
        sources.Add(source);
    }

    public static void Unregister(GravitySource source)
    {
        Debug.Assert(sources.Contains(source), "Unregistration of unkonwn gravity source!", source);
        sources.Remove(source);
    }

    public static Vector3 GetGravity(Vector3 position)
    {
        //return Physics.gravity;
        //return position.normalized * Physics.gravity.y;

        Vector3 g = Vector3.zero;
        for (int i = 0; i < sources.Count; ++i)
        {
            g += sources[i].GetGravity(position);
        }

        return g;
    }

    public static Vector3 GetUpAxis(Vector3 position)
    {
        //return -Physics.gravity.normalized;
        //return position.normalized;
        //Vector3 up = position.normalized;
        //return Physics.gravity.y < 0f ? up : -up;

        var g = GetGravity(position);
        return -g.normalized;
    }

    public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
    {
        //upAxis = -Physics.gravity.normalized;
        //return Physics.gravity;

        //Vector3 up = position.normalized;
        //upAxis = Physics.gravity.y < 0f ? up : -up;
        //return up * Physics.gravity.y;

        var g = GetGravity(position);
        upAxis = -g.normalized;
        return g;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


}
