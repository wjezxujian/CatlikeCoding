using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomGravityRigidbody : MonoBehaviour
{
    [SerializeField]
    bool floatToSleep = false;

    Rigidbody body;

    float floatDelay;


    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (floatToSleep)
        {
            if (body.IsSleeping())
            {
                floatDelay = 0f;
                return;
            }

            if (body.velocity.sqrMagnitude < 0.0001f)
            {
                floatDelay += Time.deltaTime;
                if (floatDelay >= 1f)
                {
                    return;
                }
            }
        }

        

        body.AddForce(CustomGravity.GetGravity(body.position), ForceMode.Acceleration);    
    }
}
