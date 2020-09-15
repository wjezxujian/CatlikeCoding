using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;

public class MovingSphere9 : MonoBehaviour
{
    [SerializeField]
    Transform playerInputSpace = default;

    [SerializeField, Range(0, 100f)]
    float maxSpeed = 10f, maxClimbSpeed = 2f, maxSwimSpeed = 5f;

    [SerializeField, Range(0, 100f)]
    float maxAcceleration = 10f, maxAirAcceleration = 1f, maxClimbAcceleration = 20f, maxSwimAcceleration = 5f;        //最大加速度

    [SerializeField, Range(0, 10f)]
    float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 0;

    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 25f, maxStairsAngle = 50f;

    [SerializeField, Range(90f, 180f)]
    float maxClimbAngle = 140;

    [SerializeField, Range(0f, 100f)]
    float maxSnapSpeed = 100f;

    [SerializeField, Min(0f)]
    float probeDistance = 1f;

    [SerializeField]
    float submergenceOffset = 0.5f;

    [SerializeField, Min(0.1f)]
    float submergenceRange = 1f;

    [SerializeField, Min(0f)]
    float buoyancy = 1f;

    [SerializeField, Range(0f, 10f)]
    float waterDrag = 1f;

    [SerializeField, Range(0.01f, 1f)]
    float swimThreshold = 0.5f;

    [SerializeField]
    LayerMask probeMask = -1, stairsMask = -1, climbMask = -1, waterMask = -1;

    [SerializeField]
    Material normalMaterial = default, climbingMaterial = default, swimmingMaterial = default;



    //[SerializeField]
    //Rect allowedArea = new Rect(-5f, -5f, 10f, 10f);

    //[SerializeField, Range(0f, 1f)]
    //float bounciness = 0.5f;

    Rigidbody body, connectedBody, previousConnectedBody;

    Vector3 playerInput;

    //Vector3 velocity, desiredVelocity, connectionVelocity;
    Vector3 velocity, connectionVelocity;

    bool desiredJump, desiresClimbing;

    int jumpPhase;

    float minGroundDotProduct, minStairsDotProduct, minClimbDotProduct;

    Vector3 contactNormal, steepNoraml, climbNormal, lastClimbNormal;

    //bool onGround;
    int groundContactCount, steepContactCount, climbContactCount;

    int stepsSinceLastGrounded, stepsSinceLastJump;

    bool OnGround => groundContactCount > 0;

    bool OnSteep => steepContactCount > 0;

    bool Climbing => climbContactCount > 0 && stepsSinceLastJump > 2;

    bool InWater => submergence > 0;

    bool Swimming => submergence >= swimThreshold;

    float submergence;

    Vector3 upAxis, rightAxis, forwardAxis; //指定自定义重力的轴

    Vector3 connectionWorldPosition, connectionLocalPosition;

    MeshRenderer meshRenderer;

    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
        minClimbDotProduct = Mathf.Cos(maxClimbAngle * Mathf.Deg2Rad);
    }

    //private Vector3 ProjectOnContactPlane(Vector3 vector)
    //{
    //    return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    //}

    private Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 noraml)
    {
        return (direction - noraml * Vector3.Dot(direction, noraml)).normalized;
    }

    private void AdjustVelocity()
    {
        //Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        //Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        //Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
        //Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);

        float acceleration, speed;
        Vector3 xAxis, zAxis;
        if (Climbing)
        {
            acceleration = maxClimbAcceleration;
            speed = maxClimbSpeed;
            xAxis = Vector3.Cross(contactNormal, upAxis);
            zAxis = upAxis;
        }
        else if (InWater)
        {
            float swimFactor = Mathf.Min(1f, submergence / swimThreshold);
            acceleration = Mathf.LerpUnclamped(OnGround ? maxAcceleration : maxAirAcceleration, maxSwimAcceleration, swimFactor);
            speed = Mathf.LerpUnclamped(maxSpeed, maxSwimSpeed, swimFactor);
            xAxis = rightAxis;
            zAxis = forwardAxis;
        }
        else
        {
            acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
            speed = OnGround && desiresClimbing ? maxClimbSpeed : maxSpeed;
            xAxis = rightAxis;
            zAxis = forwardAxis;
        }
        xAxis = ProjectDirectionOnPlane(xAxis, contactNormal);
        zAxis = ProjectDirectionOnPlane(zAxis, contactNormal);


        Vector3 relativeVelocity = velocity - connectionVelocity;
        float currentX = Vector3.Dot(relativeVelocity, xAxis);
        float currentZ = Vector3.Dot(relativeVelocity, zAxis);

        //float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX = Mathf.MoveTowards(currentX, playerInput.x * speed, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, playerInput.y * speed, maxSpeedChange);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);

        if (Swimming)
        {
            float currentY = Vector3.Dot(relativeVelocity, upAxis);
            float newY = Mathf.MoveTowards(currentY, playerInput.z * speed, maxSpeedChange);
            velocity += upAxis * (newY - currentY);
        }
    }

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        meshRenderer = GetComponent<MeshRenderer>();
        OnValidate();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput.z = Swimming ? Input.GetAxis("UpDown") : 0f;
        playerInput = Vector3.ClampMagnitude(playerInput, 1f);


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

        //desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

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

        if (Swimming)
        {
            desiresClimbing = false;
        }
        else
        {
            desiredJump |= Input.GetButtonDown("Jump");
            desiresClimbing = Input.GetButton("Climb");
        }
        

        //颜色
        //GetComponent<Renderer>().material.SetColor("_Color", Color.white * (groundContactCount * 0.25f));
        //GetComponent<Renderer>().material.SetColor("_Color", OnGround ? Color.black : Color.white) ;
        if (Climbing)
        {
            meshRenderer.material = climbingMaterial;
        }
        else if (Swimming)
        {
            meshRenderer.material = swimmingMaterial;
        }
        else
        {
            normalMaterial.SetColor("_Color", OnSteep ? Color.red : OnGround ? Color.black : Color.white);
            meshRenderer.material = normalMaterial;
        }

        //meshRenderer.material.color = Color.white * submergence;
    }


    private void FixedUpdate()
    {
        //upAxis = -Physics.gravity.normalized;
        //5.2.2 自定义重力
        Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);

        //velocity = body.velocity;
        UpdateState();

        if (InWater)
        {
            velocity *= 1f - waterDrag * submergence * Time.deltaTime;
        }

        AdjustVelocity();

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

        if (Climbing)
        {
            velocity -= contactNormal * (maxClimbAcceleration * 0.9f * Time.deltaTime);
        }
        else if (InWater)
        {
            velocity += gravity * ((1f - buoyancy * submergence) * Time.deltaTime);
        }
        else if (OnGround && velocity.sqrMagnitude < 0.01f)
        {
            velocity += contactNormal * (Vector3.Dot(gravity, contactNormal) * Time.deltaTime);
        }
        else if (desiresClimbing && OnGround)
        {
            velocity += (gravity - contactNormal * (maxClimbAcceleration * 0.9f)) * Time.deltaTime;
        }
        else
        {
            velocity += gravity * Time.deltaTime;
        }

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
        if (InWater)
        {
            jumpSpeed *= Mathf.Max(0f, 1f - submergence / swimThreshold);
        }
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
        if (Swimming)
        {
            return;
        }

        int layer = collision.gameObject.layer;
        float minDot = GetMinDot(layer);
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
                connectedBody = collision.rigidbody;
            }
            else
            {
                if (upDot > -0.01f)
                {
                    steepContactCount += 1;
                    steepNoraml += normal;

                    if (groundContactCount == 0)
                    {
                        connectedBody = collision.rigidbody;
                    }
                }

                if (desiresClimbing && upDot >= minClimbDotProduct && (climbMask & (1 << layer)) != 0)
                {
                    climbContactCount += 1;
                    climbNormal += normal;
                    lastClimbNormal = normal;
                    connectedBody = collision.rigidbody;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if((waterMask & (1 << other.gameObject.layer)) != 0)
        {
            EvaluateSubmergence(other);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if((waterMask & ( 1 << other.gameObject.layer)) != 0)
        {
            EvaluateSubmergence(other);
        }
    }

    private void EvaluateSubmergence(Collider collider)
    {
        if(Physics.Raycast(body.position + upAxis * submergenceOffset, -upAxis, out RaycastHit hit, submergenceRange + 1f, waterMask, QueryTriggerInteraction.Collide))
        {
            submergence = 1f - hit.distance / submergenceRange;
        }
        else
        {
            submergence = 1f;
        }

        if (Swimming)
        {
            connectedBody = collider.attachedRigidbody;
        }
    }

    private void UpdateState()
    {
        velocity = body.velocity;
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;

        if (CheckClimbing() || CheckSwimming() || OnGround || SnapToGround() || CheckSteepContacts())
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

        if (connectedBody)
        {
            if (connectedBody.isKinematic || connectedBody.mass >= body.mass)
            {
                UpdateConnectionState();
            }
        }
    }

    void UpdateConnectionState()
    {
        if (connectedBody == previousConnectedBody)
        {
            //Vector3 connectionMovement = connectedBody.position - connectionWorldPosition;
            Vector3 connectionMovement = connectedBody.transform.TransformPoint(connectionLocalPosition) - connectionWorldPosition;
            connectionVelocity = connectionMovement / Time.deltaTime;
        }

        //connectionWorldPosition = connectedBody.position;
        connectionWorldPosition = body.position;
        connectionLocalPosition = connectedBody.transform.InverseTransformPoint(connectionWorldPosition);
    }

    private void ClearState()
    {
        //onGround = false;
        groundContactCount = steepContactCount = climbContactCount = 0;
        contactNormal = steepNoraml = climbNormal = Vector3.zero;
        connectionVelocity = Vector3.zero;
        previousConnectedBody = connectedBody;
        connectedBody = null;
        submergence = 0;
    }

    bool CheckClimbing()
    {
        if (Climbing)
        {
            if (climbContactCount > 1)
            {
                climbNormal.Normalize();
                float upDot = Vector3.Dot(upAxis, climbNormal);
                if (upDot >= minGroundDotProduct)
                {
                    climbNormal = lastClimbNormal;
                }
            }

            groundContactCount = 1;
            contactNormal = climbNormal;
            return true;
        }

        return false;
    }

    private bool CheckSwimming()
    {
        if (Swimming)
        {
            groundContactCount = 0;
            contactNormal = upAxis;
            return true;
        }

        return false;
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

        if (!Physics.Raycast(body.position, -upAxis, out RaycastHit hit, probeDistance, probeMask, QueryTriggerInteraction.Ignore))
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

        //跟踪连接物体
        connectedBody = hit.rigidbody;

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
