using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public float physicsPower;
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
            Vector3 t = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);
            Vector3 direction = t.normalized * physicsPower;

            Vector3 offset = transform.TransformPoint(new Vector3(0, 0, 1));
            lookAtTransform.position = direction + this.transform.position + (offset - transform.position);

            cameraTransform.LookAt(lookAtTransform);
        }



        //transform.localEulerAngles = CameraShake();
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
