using UnityEngine;

public class CustomGravity : MonoBehaviour
{
    public static Vector3 GetGravity(Vector3 position)
    {
        //return Physics.gravity;
        return position.normalized * Physics.gravity.y;
    }

    public static Vector3 GetUpAxis(Vector3 position)
    {
        //return -Physics.gravity.normalized;
        //return position.normalized;
        Vector3 up = position.normalized;
        return Physics.gravity.y < 0f ? up : -up;
    }
    
    public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
    {
        //upAxis = -Physics.gravity.normalized;
        //return Physics.gravity;
        Vector3 up = position.normalized;
        upAxis = Physics.gravity.y < 0f ? up : -up;
        return up * Physics.gravity.y;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   
}
