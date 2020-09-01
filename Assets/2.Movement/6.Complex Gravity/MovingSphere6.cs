using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;

public class MovingSphere6 : MonoBehaviour
{
    [SerializeField]
    Transform playerInputSpace = default;

    [SerializeField, Range(0, 100f)]
    float maxSpeed = 10f;

    [SerializeField, Range(0, 100f)]
    float maxAcceleration = 10f, maxAirAcceleration = 1f;        //最大加速度

    [SerializeField, Range(0, 10f)]
    float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 0;

    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 25f, maxStairsAngle = 50f;

    [SerializeField, Range(0f, 100f)]
    float maxSnapSpeed = 100f;

    [SerializeField, Min(0f)]
    float probeDistance = 1f;

    [SerializeField]
    LayerMask probeMask = -1, stairsMask = -1;

    //[SerializeField]
    //Rect allowedArea = new Rect(-5f, -5f, 10f, 10f);

    //[SerializeField, Range(0f, 1f)]
    //float bounciness = 0.5f;

    Rigidbody body;

    Vector3 velocity, desiredVelocity;

    bool desiredJump;

    int jumpPhase;

    float minGroundDotProduct, minStairsDotProduct;

    Vector3 contactNormal, steepNoraml;

    //bool onGround;
    int groundContactCount, steepContactCount;

    int stepsSinceLastGrounded, stepsSinceLastJump;

    bool OnGround => groundContactCount > 0;

    bool OnSteep => steepContactCount > 0;

    Vector3 upAxis, rightAxis, forwardAxis; //指定自定义重力的轴

    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    //private Vector3 ProjectOnContactPlane(Vector3 vector)
    //{
    //    return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    //}

    private Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 noraml)
    {
        return (direction - noraml * Vector3.Dot(direction, noraml)).normalized;
    }

    private void AgjustVelocity()
    {
        //Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        //Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
        Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);

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
        body.useGravity = false;
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
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);


        //控制速度，时间决定增加速度
        //Vector3 desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        if (playerInputSpace)
        {

            //Vector3 right = playerInputSpace.right;
            //right.y = 0;
            //right.Normalize();

            //Vector3 forward = playerInputSpace.forward;
            //forward.y = 0f;
            //forward.Normalize();

            //desiredVelocity = playerInputSpace.TransformDirection(playerInput.x, 0f, playerInput.y) * maxSpeed;
            //desiredVelocity = (right * playerInput.x + forward * playerInput.y) * maxSpeed;

            rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
        }
        else
        {
            //desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
            rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
        }

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
        //GetComponent<Renderer>().material.SetColor("_Color", OnGround ? Color.black : Color.white) ;
        GetComponent<Renderer>().material.SetColor("_Color", OnSteep ? Color.red : OnGround ? Color.black : Color.white);

    }


    private void FixedUpdate()
    {
        //upAxis = -Physics.gravity.normalized;
        //5.2.2 自定义重力
        Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);

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
            Jump(gravity);
        }

        velocity += gravity * Time.deltaTime;

        //控制刚体速度
        body.velocity = velocity;

        //onGround = false;
        ClearState();
    }

    private void Jump(Vector3 gravity)
    {
        //velocity.y += 5f;
        Vector3 jumpDirection;
        if (OnGround)
        {
            jumpDirection = contactNormal;
        }
        else if (OnSteep)
        {
            jumpDirection = steepNoraml;
        }
        else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
        {
            if (jumpPhase == 0)
            {
                jumpPhase = 1;
            }
            jumpDirection = contactNormal;
        }
        else
        {
            return;
        }

        //½gt² = h; gt = v; 故v = √2gh;
        stepsSinceLastJump = 0;
        jumpPhase += 1;
        //velocity.y += Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        //velocity.y = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);

        //float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
        //jumpDirection = (jumpDirection + Vector3.up).normalized;
        jumpDirection = (jumpDirection + upAxis).normalized;

        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (velocity.y > 0f)
        {
            //jumpSpeed = jumpSpeed - velocity.y;
            //jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0);
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0);
        }
        //velocity.y += jumpSpeed;
        velocity += jumpDirection * jumpSpeed;
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
        float minDot = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; ++i)
        {
            Vector3 normal = collision.GetContact(i).normal;
            //onGround |= normal.y >= 0.9f;
            //onGround |= normal.y >= minGroundDotProduct;
            float upDot = Vector3.Dot(upAxis, normal);
            if (upDot >= minDot)
            {
                //onGround = true;
                groundContactCount += 1;
                contactNormal += normal;
            }
            else if (upDot > -0.01f)
            {
                steepContactCount += 1;
                steepNoraml += normal;
            }
        }
    }

    private void UpdateState()
    {
        velocity = body.velocity;
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;

        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceLastGrounded = 0;
            if (stepsSinceLastJump > 1)
            {
                jumpPhase = 0;
            }

            if (groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            //contactNormal = Vector3.up;
            contactNormal = upAxis;
        }
    }

    private void ClearState()
    {
        //onGround = false;
        groundContactCount = steepContactCount = 0;
        contactNormal = steepNoraml = Vector3.zero;
    }

    private bool SnapToGround()
    {
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
        {
            return false;
        }

        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }

        if (!Physics.Raycast(body.position, -upAxis, out RaycastHit hit, probeDistance, probeMask))
        {
            return false;
        }

        float upDot = Vector3.Dot(upAxis, hit.normal);
        if (upDot < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }

        groundContactCount = 1;
        contactNormal = hit.normal;
        //float speed = velocity.magnitude;
        float dot = Vector3.Dot(velocity, hit.normal);
        if (dot > 0)
        {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }

        return true;
    }

    private float GetMinDot(int layer)
    {
        return (stairsMask & (1 << layer)) == 0 ? minGroundDotProduct : minStairsDotProduct;
    }

    private bool CheckSteepContacts()
    {
        if (steepContactCount > 1)
        {
            steepNoraml.Normalize();
            if (steepNoraml.y >= minGroundDotProduct)
            {
                groundContactCount = 1;
                contactNormal = steepNoraml;
                return true;
            }
        }

        return false;
    }

}
