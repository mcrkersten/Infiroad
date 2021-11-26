using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public Vector3 maxAngle;
    [SerializeField]
    private Rigidbody rb;
    private Vector3 startRotation;
    private float speed;

    public Transform lookAtTransform;
    public Transform cameraTransform;

    private void Start()
    {
        startRotation = this.transform.localEulerAngles;
        rb = this.transform.root.GetComponent<Rigidbody>();
    }
    private void Update()
    {

        if(rb.velocity.magnitude > 3f)
        {
            speed = rb.velocity.magnitude * 3.6f;
            speed = (speed * speed) / 1000000f;
            Vector3 offset = transform.TransformPoint(new Vector3(0, 0, 3f));
            lookAtTransform.position = rb.velocity.normalized + this.transform.position + (offset - transform.position);
            cameraTransform.LookAt(lookAtTransform);
        }

        float m = (transform.root.GetComponent<VehicleController>().vehicleInputActions.Vehicle.Acceleration.ReadValue<float>() + 1f) / 2;
        float yaw = maxAngle.y * speed * Random.Range(0f, 1f) * m;
        float pitch = maxAngle.x * speed * Random.Range(0f, 1f) * m;
        float roll = maxAngle.z * speed * Random.Range(0f, 1f) * m;
        Vector3 rotation = startRotation + new Vector3(pitch, yaw, roll);
        transform.localEulerAngles = rotation;
    }
}
