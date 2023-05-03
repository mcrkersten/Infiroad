using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    private float speed;
    public Vector3 maxAngle;

    private Vector3 startRotation;
    private Vector3 cameraStartPosition;

    public Transform lookAtTransform;
    public Transform cameraTransform;


    public float wobbleSize;

    private void Start()
    {
        cameraStartPosition = cameraTransform.localPosition;
        startRotation = this.transform.localEulerAngles;
        rb = this.transform.root.GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        Vector3 velocity = cameraTransform.InverseTransformDirection(rb.velocity);
        velocity[2] = 0f;
        velocity[0] = 0f;
        cameraTransform.localPosition = Vector3.Lerp(cameraStartPosition, cameraStartPosition + velocity, .01f);
    }

    private Vector3 CameraShake()
    {
        speed = rb.velocity.magnitude * 3.6f;
        speed = (speed * speed) / 1000000f;
        float m = (transform.root.GetComponent<VehicleController>().vehicleInputActions.SteeringWheel.Acceleration.ReadValue<float>() + 1f) / 2;
        float yaw = maxAngle.y * speed * Random.Range(0f, 1f) * m;
        float pitch = maxAngle.x * speed * Random.Range(0f, 1f) * m;
        float roll = maxAngle.z * speed * Random.Range(0f, 1f) * m;
        Vector3 shake = startRotation + new Vector3(pitch, yaw, roll);
        return shake;
    }
}
