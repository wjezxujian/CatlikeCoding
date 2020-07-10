using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSphere : MonoBehaviour
{
    [SerializeField, Range(0, 100f)]
    float maxSpeed = 10f;

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

        //playerInput.Normalize();
        //playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        //transform.localPosition = new Vector3(playerInput.x, 0f, playerInput.y);

        //Vector3 displacement = new Vector3(playerInput.x, 0f, playerInput.y);
        //transform.localPosition += displacement;

        Vector3 velocity = new Vector3(playerInput.x, 0f, playerInput.y);
        Vector3 displacement = velocity * maxSpeed * Time.deltaTime;
        transform.localPosition += displacement;
    }
}
