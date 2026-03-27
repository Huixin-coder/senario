using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelRotation : MonoBehaviour
{
    
    public float wheelRadius = 0.15f;
    private Rigidbody vehicle;
    // Start is called before the first frame update
    void Start()
    {
        vehicle = this.transform.parent.GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {
        // Vector3 velocity = vehicle.velocity;
        float speed = vehicle.velocity.magnitude;
        // float rpm = (speed * 60) / (2*Mathf.PI*wheelRadius);
        // float rotationAngle = 360*rpm*Time.deltaTime / 60;
        // foreach (Transform wheel in this.transform) {
        //     wheel.Rotate(Vector3.right, rotationAngle);
        // }

        if (speed > 0.5) {
            foreach (Transform wheel in this.transform) {
                wheel.Rotate(Vector3.left, 165);
            }
        }

        
    }
}
