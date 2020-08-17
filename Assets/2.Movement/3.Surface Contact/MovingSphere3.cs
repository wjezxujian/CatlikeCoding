using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSphere3 : MonoBehaviour
{
    [SerializeField, Range(0, 100f)]
    float maxSpeed = 10f;

    Vector3 velocity, desiredVelocity;

    [SerializeField, Range(0, 100f)]
    float maxAcceleration = 10f, maxAirAcceleration = 1f;        //最大加速度

    [SerializeField, Range(0, 10f)]
    float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 0;

    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 25f;

    //[SerializeField]
    //Rect allowedArea = new Rect(-5f, -5f, 10f, 10f);

    //[SerializeField, Range(0f, 1f)]
    //float bounciness = 0.5f;

    Rigidbody body;
    bool desiredJump;
    //bool onGround;
    int groundContactCount;
    int jumpPhase;
    float minGroundDotProduct;
    Vector3 contactNormal;

    bool OnGround => groundContactCount > 0;

    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    private Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }

    private void AgjustVelocity()
    {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);

        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        OnValidate();
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


        //控制速度，时间决定增加速度
        //Vector3 desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

        //velocity = body.velocity;
        //float maxSpeedChange = maxAcceleration * Time.deltaTime;

        ////通过当前加速度逼近目标速度
        //velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        //velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
        ////Vector3 displacement = velocity * Time.deltaTime;


        //////弹力，反转速度 * 比率
        ////Vector3 newPosition = transform.localPosition + displacement;
        //////if (newPosition.x < allowedArea.xMin)
        //////{
        //////    newPosition.x = allowedArea.xMin;
        //////    velocity.x = -velocity.x * bounciness;
        //////}
        //////else if (newPosition.x > allowedArea.xMax)
        //////{
        //////    newPosition.x = allowedArea.xMax;
        //////    velocity.x = -velocity.x * bounciness;
        //////}

        //////if (newPosition.z < allowedArea.yMin)
        //////{
        //////    newPosition.z = allowedArea.yMin;
        //////    velocity.z = -velocity.z * bounciness;
        //////}
        //////else if (newPosition.z > allowedArea.yMax)
        //////{
        //////    newPosition.z = allowedArea.yMax;
        //////    velocity.z = -velocity.z * bounciness;
        //////}

        ////transform.localPosition = newPosition;

        ////控制刚体速度
        //body.velocity = velocity;

        desiredJump |= Input.GetButtonDown("Jump");

        //颜色
        GetComponent<Renderer>().material.SetColor("_Color", Color.white * (groundContactCount * 0.25f));
        GetComponent<Renderer>().material.SetColor("_Color", OnGround ? Color.black : Color.white) ;
    
    }


    private void FixedUpdate()
    {
        //velocity = body.velocity;
        UpdateState();
        AgjustVelocity();

        ////float maxSpeedChange = maxAcceleration * Time.deltaTime;
        //float acceleration = onGround ? maxAcceleration : maxAirAcceleration;
        //float maxSpeedChange = acceleration * Time.deltaTime;

        ////通过当前加速度逼近目标速度
        //velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        //velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);


        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }

        //控制刚体速度
        body.velocity = velocity;

        //onGround = false;
        ClearState();
    }

    private void Jump()
    {
        //velocity.y += 5f;

        //½gt² = h; gt = v; 故v = √2gh;
        if (OnGround || jumpPhase < maxAirJumps)
        {
            jumpPhase += 1;
            //velocity.y += Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            //velocity.y = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);

            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            float alignedSpeed = Vector3.Dot(velocity, contactNormal);
            if (velocity.y > 0f)
            {
                //jumpSpeed = jumpSpeed - velocity.y;
                //jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0);
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0);
            }
            //velocity.y += jumpSpeed;
            velocity += contactNormal * jumpSpeed;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //onGround = true;
        EvaluateCollision(collision);
    }

    //private void OnCollisionExit(Collision collision)
    //{
    //    onGround = false;
    //}

    private void OnCollisionStay(Collision collision)
    {
        //onGround = true;
        EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; ++i)
        {
            Vector3 normal = collision.GetContact(i).normal;
            //onGround |= normal.y >= 0.9f;
            //onGround |= normal.y >= minGroundDotProduct;
            if (normal.y >= minGroundDotProduct)
            {
                //onGround = true;
                groundContactCount += 1;
                contactNormal += normal;
            }
        }
    }

    private void UpdateState()
    {
        velocity = body.velocity;

        if (OnGround)
        {
            jumpPhase = 0;
            if (groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = Vector3.up;
        }
    }

    private void ClearState()
    {
        //onGround = false;
        groundContactCount = 0;
        contactNormal = Vector3.zero;
    }

}
