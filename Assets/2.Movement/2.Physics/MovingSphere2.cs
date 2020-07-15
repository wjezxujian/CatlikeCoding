using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSphere2 : MonoBehaviour
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

    //[SerializeField]
    //Rect allowedArea = new Rect(-5f, -5f, 10f, 10f);

    //[SerializeField, Range(0f, 1f)]
    //float bounciness = 0.5f;

    Rigidbody body;
    bool desiredJump;
    bool onGround;
    bool printDebug;
    int jumpPhase;

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
    }

    private void FixedUpdate()
    {
        //velocity = body.velocity;
        UpdateState();

        //float maxSpeedChange = maxAcceleration * Time.deltaTime;
        float acceleration = onGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        //通过当前加速度逼近目标速度
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);


        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }

        //控制刚体速度
        body.velocity = velocity;

        onGround = false;
    }

    private void Jump()
    {
        //velocity.y += 5f;

        //½gt² = h; gt = v; 故v = √2gh;
        if (onGround || jumpPhase < maxAirJumps)
        {
            jumpPhase += 1;
            //velocity.y += Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            //velocity.y = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);

            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            if (velocity.y > 0f)
            {
                //jumpSpeed = jumpSpeed - velocity.y;
                jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0);
            }
            velocity.y += jumpSpeed;
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
        for(int i = 0; i < collision.contactCount; ++i)
        {
            Vector3 normal = collision.GetContact(i).normal;
            onGround |= normal.y >= 0.9f;
        }
    }

    private void UpdateState()
    {
        velocity = body.velocity;

        if (onGround)
        {
            jumpPhase = 0;
        }
    }
}
