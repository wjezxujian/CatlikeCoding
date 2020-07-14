using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSphere2 : MonoBehaviour
{
    [SerializeField, Range(0, 100f)]
    float maxSpeed = 10f;

    Vector3 velocity;

    [SerializeField, Range(0, 100f)]
    float maxAcceleration = 10f;        //最大加速度

    //[SerializeField]
    //Rect allowedArea = new Rect(-5f, -5f, 10f, 10f);

    //[SerializeField, Range(0f, 1f)]
    //float bounciness = 0.5f;

    Rigidbody body;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

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

        velocity = body.velocity;

        //控制速度，时间决定增加速度
        Vector3 desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;

        //通过当前加速度逼近目标速度
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
        //Vector3 displacement = velocity * Time.deltaTime;


        ////弹力，反转速度 * 比率
        //Vector3 newPosition = transform.localPosition + displacement;
        ////if (newPosition.x < allowedArea.xMin)
        ////{
        ////    newPosition.x = allowedArea.xMin;
        ////    velocity.x = -velocity.x * bounciness;
        ////}
        ////else if (newPosition.x > allowedArea.xMax)
        ////{
        ////    newPosition.x = allowedArea.xMax;
        ////    velocity.x = -velocity.x * bounciness;
        ////}

        ////if (newPosition.z < allowedArea.yMin)
        ////{
        ////    newPosition.z = allowedArea.yMin;
        ////    velocity.z = -velocity.z * bounciness;
        ////}
        ////else if (newPosition.z > allowedArea.yMax)
        ////{
        ////    newPosition.z = allowedArea.yMax;
        ////    velocity.z = -velocity.z * bounciness;
        ////}

        //transform.localPosition = newPosition;

        //控制刚体速度
        body.velocity = velocity;
        
       

    }

}
