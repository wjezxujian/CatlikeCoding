using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Graph : MonoBehaviour
{
    public Transform pointPrefab;

    [Range(10, 100)]
    public int resolution = 10;

    Transform[] points;

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
        Vector3 position = Vector3.one;

        points = new Transform[resolution + 1];
        for (int i = 0; i < resolution + 1; ++i)
        {
            Transform point = Instantiate(pointPrefab);
            position.x =  (i * step - 1f);
            //position.y = position.x;   // f(x) = x;
            //position.y = position.x * position.x; //f(x) = x ^ 2;
            //position.y = position.x * position.x * position.x; // f(x) = x ^ 3;

            point.localPosition = position;
            point.localScale = scale;
            point.SetParent(transform, false);

            points[i] = point;

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        for(int i = 0; i < points.Length; ++i)
        {
            Transform point = points[i];
            Vector3 position = point.localPosition;
            //position.y = position.x * position.x * position.x;
            Debug.Log("Time.time: " + Time.time);
            position.y = Mathf.Sin(Mathf.PI * (position.x + Time.time));
            point.localPosition = position;
        }
    }
}
