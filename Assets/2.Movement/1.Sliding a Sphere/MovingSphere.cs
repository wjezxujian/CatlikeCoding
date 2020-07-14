using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSphere : MonoBehaviour
{
    [SerializeField, Range(0, 100f)]
    float maxSpeed = 10f;

    Vector3 velocity;

    [SerializeField, Range(0, 100f)]
    float maxAcceleration = 10f;        //最大加速度

    [SerializeField]
    Rect allowedArea = new Rect(-5f, -5f, 10f, 10f);

    [SerializeField, Range(0f, 1f)]
    float bounciness = 0.5f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");

        //控制位置（归一化在圆内）
        //playerInput.Normalize();
        //playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        //transform.localPosition = new Vector3(playerInput.x, 0f, playerInput.y);

        //控制位移
        //Vector3 displacement = new Vector3(playerInput.x, 0f, playerInput.y);
        //transform.localPosition += displacement;

        //控制速度
        //Vector3 velocity = new Vector3(playerInput.x, 0f, playerInput.y);
        //Vector3 displacement = velocity * maxSpeed * Time.deltaTime;
        //transform.localPosition += displacement;

        //控制加速度
        //Vector3 accleration = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        //velocity += accleration * Time.deltaTime;
        //Vector3 displacement = velocity * Time.deltaTime;
        //transform.localPosition += displacement;

        //控制速度，时间决定增加速度
        Vector3 desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        //if(velocity.x < desiredVelocity.x)
        //{
        //    //velocity.x += maxSpeedChange;
        //    velocity.x = Mathf.Min(velocity.x + maxSpeedChange, desiredVelocity.x);
        //}
        //else if(velocity.x > desiredVelocity.x)
        //{
        //    velocity.x = Mathf.Max(velocity.x - maxSpeedChange, desiredVelocity.x);
        //}
        //通过当前加速度逼近目标速度
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
        Vector3 displacement = velocity * Time.deltaTime;
        //transform.localPosition += displacement;

        //约束位置，在边缘时避免浮点跳跃越过辩越
        //Vector3 newPosition = transform.localPosition + displacement;
        //if (!allowedArea.Contains(new Vector2(newPosition.x, newPosition.z)))
        //{
        //    //newPosition = transform.localPosition;
        //    newPosition.x = Mathf.Clamp(newPosition.x, allowedArea.xMin, allowedArea.xMax);
        //    newPosition.z = Mathf.Clamp(newPosition.z, allowedArea.yMin, allowedArea.yMax);
        //}
        //transform.localPosition = newPosition;

        //消除抵达边缘时速度
        //Vector3 newPosition = transform.localPosition + displacement;
        //if (newPosition.x < allowedArea.xMin)
        //{
        //    newPosition.x = allowedArea.xMin;
        //    velocity.x = 0f;
        //}
        //else if (newPosition.x > allowedArea.xMax)
        //{
        //    newPosition.x = allowedArea.xMax;
        //    velocity.x = 0f;
        //}

        //if (newPosition.z < allowedArea.yMin)
        //{
        //    newPosition.z = allowedArea.yMin;
        //    velocity.z = 0f;
        //}
        //else if(newPosition.z > allowedArea.yMax)
        //{
        //    newPosition.z = allowedArea.yMax;
        //    velocity.z = 0f;
        //}
        //transform.localPosition = newPosition;

        //弹跳，反转速度
        //Vector3 newPosition = transform.localPosition + displacement;
        //if(newPosition.x < allowedArea.xMin)
        //{
        //    newPosition.x = allowedArea.xMin;
        //    velocity.x = -velocity.x;
        //}
        //else if(newPosition.x > allowedArea.xMax)
        //{
        //    newPosition.x = allowedArea.xMax;
        //    velocity.x = -velocity.x;
        //}

        //if (newPosition.z < allowedArea.yMin)
        //{
        //    newPosition.z = allowedArea.yMin;
        //    velocity.z = -velocity.z;
        //}
        //else if (newPosition.z > allowedArea.yMax)
        //{
        //    newPosition.z = allowedArea.yMax;
        //    velocity.z = -velocity.z;
        //}

        //transform.localPosition = newPosition;

        //弹力，反转速度 * 比率
        Vector3 newPosition = transform.localPosition + displacement;
        if (newPosition.x < allowedArea.xMin)
        {
            newPosition.x = allowedArea.xMin;
            velocity.x = -velocity.x * bounciness;
        }
        else if (newPosition.x > allowedArea.xMax)
        {
            newPosition.x = allowedArea.xMax;
            velocity.x = -velocity.x * bounciness;
        }

        if (newPosition.z < allowedArea.yMin)
        {
            newPosition.z = allowedArea.yMin;
            velocity.z = -velocity.z * bounciness;
        }
        else if (newPosition.z > allowedArea.yMax)
        {
            newPosition.z = allowedArea.yMax;
            velocity.z = -velocity.z * bounciness;
        }

        transform.localPosition = newPosition;

    }

}
