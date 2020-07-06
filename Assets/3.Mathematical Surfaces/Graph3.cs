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
        SineFunction, Sine2DFunction, MultiSineFunction, MultiSine2DFunction, 
        RippleFunction, CylinderFunction, SphereFunciton, TorusFunction,
    };

    const float pi = Mathf.PI;

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
        //for (int i = 0, z = 0; z < resolution + 1; ++z)
        //{
        //    position.z = (z * step - 1f);
        //    for (int x = 0; x < resolution + 1; ++i, ++ x)
        //    {
        //        Transform point = Instantiate(pointPrefab);
        //        position.x = (x * step - 1f);
        //        //position.z = (z * step - 1f);
        //        //position.y = position.x;   // f(x) = x;
        //        //position.y = position.x * position.x; //f(x) = x ^ 2;
        //        //position.y = position.x * position.x * position.x; // f(x) = x ^ 3;

        //        point.localPosition = position;
        //        point.localScale = scale;
        //        point.SetParent(transform, false);

        //        points[i] = point;
        //    }
        //}

        for(int i = 0; i < points.Length; ++i)
        {
            Transform point = Instantiate(pointPrefab);
            point.localScale = scale;
            point.SetParent(transform, false);
            points[i] = point;
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

        float step = 2f / resolution;
        GraphFunction f = functions[(int)function];
        //for (int i = 0; i < points.Length; ++i)
        //{
        //    Transform point = points[i];
        //    Vector3 position = point.localPosition;
        //    //position.y = position.x * position.x * position.x;
        //    //Debug.Log("Time.time: " + Time.time);
        //    //position.y = Mathf.Sin(Mathf.PI * (position.x + Time.time));
        //    //position.y = SineFunction(position.x, t);
        //    //position.y = MultiSineFunction(position.x, t);
        //    //if (function == 0)
        //    //    position.y = SineFunction(position.x, t);
        //    //else
        //    //    position.y = MultiSineFunction(position.x, t);
        //    position.y = f(position.x, position.z, t);

        //    point.localPosition = position;
        //}

        for(int i = 0, z = 0; z < resolution + 1; ++z)
        {
            float v = (z * step) - 1;
            for(int x = 0; x < resolution + 1; ++i, ++x)
            {
                float u = (x * step) - 1;
                Transform point = points[i];
                point.localPosition = f(u, v, t);
            }
        }

    }

    static private Vector3 SineFunction(float x, float z, float t)
    {
        //return Mathf.Sin(Mathf.PI * (x + t));
        Vector3 p;
        p.x = x;
        p.y = Mathf.Sin(pi * (x + t));
        p.z = z;
        return p;
    }

    static private Vector3 MultiSineFunction(float x, float z, float t)
    {
        //float y = Mathf.Sin(Mathf.PI * (x + t));
        //y += Mathf.Sin(2f * Mathf.PI * (x + 2f * t)) * 0.5f;
        //y *= 2f / 3f;
        //return y;

        Vector3 p;
        p.x = x;
        p.y = Mathf.Sin(pi * (x + t));
        p.y += Mathf.Sin(2f * pi *(x + 2f + t)) * 0.5f;
        p.y *= 2f / 3f;
        p.z = z;
        return p;
    }
    
    static private Vector3 Sine2DFunction(float x, float z, float t)
    {
        //return Mathf.Sin(Mathf.PI * (x + z + t)); //f(x, z, t) = sin(π * (x + z + t));

        //float y = Mathf.Sin(Mathf.PI * (x + t));
        //y += Mathf.Sin(Mathf.PI * (z + t));
        //y *= 0.5f;
        //return y;

        Vector3 p;
        p.x = x;
        p.y = Mathf.Sin(pi * (x + t));
        p.y += Mathf.Sin(pi * (z + t));
        p.y *= 0.5f;
        p.z = z;
        return p;
    }

    const float ratio = 1f / 5.5f;
    static  private Vector3 MultiSine2DFunction(float x, float z, float t)
    {
        //float y = 4f * Mathf.Sin(Mathf.PI * (x + z + t * 0.5f));
        //y += Mathf.Sin(Mathf.PI * (x + t));
        //y += Mathf.Sin(2f * Mathf.PI * (z + 2f * t))  * 0.5f;
        //y *= ratio;

        //return y;

        Vector3 p;
        p.x = x;
        p.y = 4f * Mathf.Sin(pi * (x + z + t * 0.5f));
        p.y += Mathf.Sin(pi * (x + t));
        p.y += Mathf.Sin(2f * pi * (x + 2f * t)) * 0.5f;
        p.y *= ratio;
        p.z = z;
        return p;
    }

    static Vector3 RippleFunction(float x, float z, float t)
    {
        //float d = Mathf.Sqrt(x * x + z * z);
        //float y = Mathf.Sin(4f * Mathf.PI * d - t);
        //y /= 1f + 10f * d;
        //return y;

        Vector3 p;
        float d = Mathf.Sqrt(x * x + z * z);
        p.x = x;
        p.y = Mathf.Sin(pi * (4f * d - t));
        p.y /= 1f + 10f * d;
        p.z = z;
        return p;
    }

    static Vector3 CylinderFunction(float u, float v, float t)
    {
        //Vector3 p;
        //p.x = Mathf.Sin(pi * u);
        //p.y = v;
        //p.z = Mathf.Cos(pi * u);
        //return p;

        //float r = 1f + Mathf.Sin(6 * pi * u) * 0.2f;  //六边形柱子
        //float r = 1f + Mathf.Sin(2f * pi * v) * 0.2f;   //花盆形状
        float r = 0.8f + Mathf.Sin(pi * (6f * u + 2f * v + t)) * 0.2f;  //六边形柱子 + 花盆 + 扭曲
        Vector3 p;
        p.x = r * Mathf.Sin(pi * u);
        p.y = v;
        p.z = r * Mathf.Cos(pi * u);
        return p;
    }

    static Vector3 SphereFunciton(float u, float v, float t)
    {
        Vector3 p;
        float r = 0.8f + Mathf.Sin(pi * (6f * u + t)) * 0.1f;
        r += Mathf.Sin(pi * (4f * v + t)) * 0.1f;
        //float r = Mathf.Cos(pi * 0.5f * v);
        float s = r * Mathf.Cos(pi * 0.5f * v);
        p.x = s * Mathf.Sin(pi * u);
        //p.y = v;
        p.y = r * Mathf.Sin(pi * 0.5f * v);
        p.z = s * Mathf.Cos(pi * u);
        return p;
    }

    static Vector3 TorusFunction(float u, float v, float t)
    {
        //Vector3 p;
        ////float s = Mathf.Cos(pi * 0.5f * v);
        //float s = Mathf.Cos(pi * 0.5f * v) + 0.5f;
        //p.x = s * Mathf.Sin(pi * u);
        //p.y = Mathf.Sin(pi * 0.5f * v);
        //p.z = s * Mathf.Cos(pi * u);
        //return p;

        //Vector3 p;
        //float s = Mathf.Cos(pi * v) + 0.5f;
        //p.x = s * Mathf.Sin(pi * u);
        //p.y = Mathf.Sin(pi * v);
        //p.z = s * Mathf.Cos(pi * u);
        //return p;

        //Vector3 p;
        //float r1 = 1f;
        //float s = Mathf.Cos(pi * v) + r1;
        //p.x = s * Mathf.Sin(pi * u);
        //p.y = Mathf.Sin(pi * v);
        //p.z = s * Mathf.Cos(pi * u);
        //return p;

        //Vector3 p;
        //float r1 = 1f;
        //float r2 = 0.5f;
        //float s = r2 * Mathf.Cos(pi * v) + r1;
        //p.x = s * Mathf.Sin(pi * u);
        //p.y = r2 * Mathf.Sin(pi * v);
        //p.z = s * Mathf.Cos(pi * u);
        //return p;

        Vector3 p;
        float r1 = 0.65f + Mathf.Sin(pi * (6f * u + t)) * 0.1f;
        float r2 = 0.2f + Mathf.Sin(pi * (4f * v + t)) * 0.05f;
        float s = r2 * Mathf.Cos(pi * v) + r1;
        p.x = s * Mathf.Sin(pi * u);
        p.y = r2 * Mathf.Sin(pi * v);
        p.z = s * Mathf.Cos(pi * u);
        return p;
    }


}
