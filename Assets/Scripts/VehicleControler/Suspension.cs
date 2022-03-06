using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Suspension : MonoBehaviour
{
    public WheelRaycast wheel;

    public float collisionStrenght;
    public SuspensionPosition suspensionPosition;
    public LayerMask layerMask;
    private Rigidbody rb;
    public float brakeBias;

    [Header("Suspension")]
    [Tooltip("Position of spring under 0 load")]
    public float restLength;
    [Tooltip("Distance spring can travel in y axis")]
    public float springTravel;

    public float springtStiffness;
    public float damperStiffness;

    private float minLength;
    private float maxLength;
    private float lastLength;

    private void Start()
    {
        rb = transform.root.GetComponent<Rigidbody>();

        minLength = restLength - springTravel * 5f;
        maxLength = restLength + springTravel;
    }

    public void SimulatePhysics(float brakeInput, float engineForce)
    {
        float wheelSlipAnimate = 0f;

        if (wheel.Raycast(maxLength, layerMask, out RaycastHit hit))
        {
            Debug.DrawLine(this.transform.position, this.transform.position + rb.GetPointVelocity(hit.point).normalized);
            Debug.DrawLine(this.transform.position, this.transform.position + this.transform.forward, Color.green);
            Debug.DrawRay(transform.position, -this.transform.up * hit.distance, Color.red);

            wheel.wheelVelocityLocalSpace = wheel.transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));

            Vector3 suspensionForce = CalculateSuspensionForce(hit);
            rb.AddForceAtPosition(suspensionForce, hit.point);
            float downForce = suspensionForce.y + 300;

            float accelerationForce = engineForce;
            float brakeForce = CalculateBrakingForce(brakeInput * brakeBias);
            float sidewaysForce = -wheel.wheelVelocityLocalSpace.x * downForce;

            Vector3 localForceDirection = CalculateForces(accelerationForce, brakeForce, sidewaysForce, downForce);
            wheel.forceDirectionDebug = localForceDirection;
            Vector3 worldForceDirection = wheel.transform.TransformDirection(localForceDirection);
            rb.AddForceAtPosition(worldForceDirection, hit.point);

            Debug.DrawRay(wheel.transform.position, -this.transform.up * hit.distance, Color.red);
        }
        else
        {
            wheel.steeringWheelForce = 0;
        }

        //Technical debt
        wheel.RotateWheelModel(wheelSlipAnimate, suspensionPosition);
    }

    private Vector3 CalculateForces(float accelerationForce, float brakeForce, float sidewaysForce, float downForce)
    {
        Vector2 a = new Vector2(0f, accelerationForce + brakeForce);
        Vector2 b = new Vector2(sidewaysForce, 0f);
        float c = Vector2.Distance(a, b);

        float p = c / downForce;
        p = wheel.slipCurve.Evaluate(Mathf.Abs(p));
        float gripForce = downForce * p;

        Vector2 result = a + b;
        wheel.steeringWheelForce = -result.x;
        wheel.gripDebug = p;
        return new Vector3(Mathf.Clamp(result.x, -gripForce, gripForce), 0f, Mathf.Clamp(result.y, -gripForce, gripForce));
    }

    private Vector3 CalculateSuspensionForce(RaycastHit hit)
    {
        float springLength = Mathf.Clamp(hit.distance, minLength, maxLength);
        float springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;

        float springForce = springtStiffness * (restLength - springLength);
        float damperForce = damperStiffness * springVelocity;
        lastLength = springLength;
        if (springForce + damperForce > 0f)
        {
            Debug.DrawLine(this.transform.position, this.transform.position + this.transform.up * ((springForce + damperForce) / 2000f), Color.cyan);
            return this.transform.up * (springForce + damperForce);
        }
        return Vector3.zero;
    }

    private float CalculateBrakingForce(float brakeInput)
    {
        return Mathf.Clamp(wheel.wheelVelocityLocalSpace.z, -1, 1) * -VehicleConstants.BRAKE_FORCE * brakeInput;
    }
}
[System.Serializable]
public class SuspensionPointer
{
    public Transform suspensionPart;
    public Transform pointer;

    public void UpdatePointer()
    {
        suspensionPart.LookAt(pointer);
    }
}


public enum SuspensionPosition { FrontLeft, FrontRight, RearLeft, RearRight };
