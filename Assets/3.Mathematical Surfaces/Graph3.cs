using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Graph3 : MonoBehaviour
{
    public Transform pointPrefab;

    [Range(10, 100)]
    public int resolution = 10;

    //[Range(0, 1)]
    //public int function;
    public GraphFunctionName function;


    Transform[] points;

    static GraphFunction[] functions = {
        SineFunction, Sine2DFunction, MultiSineFunction, MultiSine2DFunction, RippleFunction
    };

    private void Awake()
    {
        //1.生成预制体
        //Transform point = Instantiate(pointPrefab);
        //point.localPosition = Vector3.right;

        //point = Instantiate(pointPrefab);
        //point.localPosition = Vector3.right * 2;

        //2.循环生成10个预制体
        //int i = 0;
        //while(i++ < 10)
        //{
        //    Transform point = Instantiate(pointPrefab);
        //    point.localPosition = Vector3.right * i;
        //}

        //3.使用for循环
        float step = 2f / resolution;
        Vector3 scale = Vector3.one * step;
        Vector3 position = Vector3.zero;

        points = new Transform[(resolution + 1) * (resolution + 1)];
        for (int i = 0, z = 0; z < resolution + 1; ++z)
        {
            position.z = (z * step - 1f);
            for (int x = 0; x < resolution + 1; ++i, ++ x)
            {
                Transform point = Instantiate(pointPrefab);
                position.x = (x * step - 1f);
                //position.z = (z * step - 1f);
                //position.y = position.x;   // f(x) = x;
                //position.y = position.x * position.x; //f(x) = x ^ 2;
                //position.y = position.x * position.x * position.x; // f(x) = x ^ 3;

                point.localPosition = position;
                point.localScale = scale;
                point.SetParent(transform, false);

                points[i] = point;
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        float t = Time.time;
        //GraphFunction f;
        //if (function == 0)
        //    f = SineFunction;
        //else
        //    f = MultiSineFunction;

        GraphFunction f = functions[(int)function];
        for (int i = 0; i < points.Length; ++i)
        {
            Transform point = points[i];
            Vector3 position = point.localPosition;
            //position.y = position.x * position.x * position.x;
            //Debug.Log("Time.time: " + Time.time);
            //position.y = Mathf.Sin(Mathf.PI * (position.x + Time.time));
            //position.y = SineFunction(position.x, t);
            //position.y = MultiSineFunction(position.x, t);
            //if (function == 0)
            //    position.y = SineFunction(position.x, t);
            //else
            //    position.y = MultiSineFunction(position.x, t);
            position.y = f(position.x, position.z, t);

            point.localPosition = position;
        }

    }

    static private float SineFunction(float x, float z, float t)
    {
        return Mathf.Sin(Mathf.PI * (x + t));
    }

    static private float MultiSineFunction(float x, float z, float t)
    {
        float y = Mathf.Sin(Mathf.PI * (x + t));
        y += Mathf.Sin(2f * Mathf.PI * (x + 2f * t)) * 0.5f;
        y *= 2f / 3f;
        return y;
    }
    
    static private float Sine2DFunction(float x, float z, float t)
    {
        //return Mathf.Sin(Mathf.PI * (x + z + t)); //f(x, z, t) = sin(π * (x + z + t));

        float y = Mathf.Sin(Mathf.PI * (x + t));
        y += Mathf.Sin(Mathf.PI * (z + t));
        y *= 0.5f;
        return y;
    }

    const float ratio = 1f / 5.5f;
    static  private float MultiSine2DFunction(float x, float z, float t)
    {
        float y = 4f * Mathf.Sin(Mathf.PI * (x + z + t * 0.5f));
        y += Mathf.Sin(Mathf.PI * (x + t));
        y += Mathf.Sin(2f * Mathf.PI * (z + 2f * t))  * 0.5f;
        y *= ratio;

        return y;

    }

    static float RippleFunction(float x, float z, float t)
    {
        float d = Mathf.Sqrt(x * x + z * z);
        float y = Mathf.Sin(4f * Mathf.PI * d - t);
        y /= 1f + 10f * d;
        return y;
    }

}
