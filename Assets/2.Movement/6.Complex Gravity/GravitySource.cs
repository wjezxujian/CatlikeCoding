using UnityEngine;

public class GravitySource : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual Vector3 GetGravity(Vector3 position)
    {
        return Physics.gravity;
    }

    private void OnEnable()
    {
        CustomGravity.Register(this);
    }

    private void OnDisable()
    {
        CustomGravity.Unregister(this);
    }
}
