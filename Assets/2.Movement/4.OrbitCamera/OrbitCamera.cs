using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    [SerializeField]
    Transform focus = default;

    [SerializeField, Range(1f, 200f)]
    float distance = 5f;

    [SerializeField, Min(0f)]
    float focusRadius = 1f;

    [SerializeField, Range(0f, 1f)]
    float focusCentering = 0.5f;

    [SerializeField, Range(1f, 360)]
    float rotationSpeed = 90f;

    Vector3 focusPoint;

    Vector2 orbitAngles = new Vector2(45f, 0f);

    private void Awake()
    {
        focusPoint = focus.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        LateUpdate();
    }

    private void LateUpdate()
    {
        //Vector3 focusPoint = focus.position;
        UpdateFocusPoint();
        ManualRotation();
        Quaternion lookRotation = Quaternion.Euler(orbitAngles);
        //Vector3 lookDirection = transform.forward;
        Vector3 lookDirection = lookRotation * Vector3.forward;
        //transform.localPosition = focusPoint - lookDirection * distance;
        Vector3 lookPosition = focusPoint - lookDirection * distance;
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    private void UpdateFocusPoint()
    {
        Vector3 targetPoint = focus.position;

        if(focusRadius > 0)
        {
            float distance = Vector3.Distance(targetPoint, focusPoint);

            float t = 1f;
            if(distance > 0.01f && focusCentering > 0f)
            {
                t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
            }

            if(distance > focusRadius)
            {
                //focusPoint = Vector3.Lerp(targetPoint, focusPoint, focusRadius / distance);
                t = Mathf.Min(t, focusRadius / distance);
            }

            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
        }
        else
        {
            focusPoint = targetPoint;
        }               
    }

    private void ManualRotation()
    {
        Vector2 input = new Vector2(Input.GetAxis("Vertical Camera"), Input.GetAxis("Horizontal Camera"));

        const float e = 0.001f;
        if(input.x < -e || input.x > e || input.y < -e || input.y > e)
        {
            orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
        }
    }
}
