using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    public float staticFrictionalForce;

    public float collisionStrenght;
    public WheelPosition wheelPosition;
    public LayerMask layerMask;
    private Rigidbody rb;
    [Header("Suspension")]
    public float restLength;
    public float springTravel;
    public float springtStiffness;
    public float damperStiffness;

    public GameObject wheelModel;

    private float minLength;
    private float maxLength;
    private float lastLength;
    private float springLength;

    private float steeringWheelForce;
    public float WheelForce { get { return steeringWheelForce; } }

    private Vector3 wheelVelocityLocalSpace;
    public float RPM;

    [Header("Wheel")]
    public float wheelRadius;
    public float steerAngle;

    [Header("General Slip")]
    public AnimationCurve slipCurve;

    [HideInInspector] public Vector3 forceDirectionDebug;
    public float gripDebug;


    private void Start()
    {
        rb = transform.root.GetComponent<Rigidbody>();

        minLength = restLength - springTravel * 5f;
        maxLength = restLength + springTravel;
    }

    private void Update()
    {
        transform.localRotation = Quaternion.Euler(transform.localRotation.x, transform.localRotation.y + steerAngle, transform.localRotation.x);
        wheelModel.transform.localRotation = Quaternion.Euler(transform.localRotation.x, transform.localRotation.y + steerAngle, transform.localRotation.x);
    }

    public void SimulatePhysics(float brakeInput ,Engine engine, bool isDriveWheel)
    {
        float wheelSlipAnimate = 0f;
        if (Physics.SphereCast(transform.position, wheelRadius, -transform.up, out RaycastHit hit, maxLength, layerMask))
        {

            Debug.DrawLine(this.transform.position, this.transform.position + rb.GetPointVelocity(hit.point).normalized);
            Debug.DrawLine(this.transform.position, this.transform.position + this.transform.forward, Color.green);
            Debug.DrawRay(transform.position, -this.transform.up * hit.distance, Color.red);

            wheelModel.transform.position = transform.position + (-this.transform.up * (hit.distance));

            wheelVelocityLocalSpace = transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));

            Vector3 suspensionForce = CalculateSuspensionForce(hit);
            rb.AddForceAtPosition(suspensionForce, hit.point);
            float downForce = suspensionForce.y + 300;

            float accelerationForce = isDriveWheel ? CalculateAccelerationForce(engine) : 0f;
            float brakeForce = CalculateBrakingForce(brakeInput);
            float sidewaysForce = -wheelVelocityLocalSpace.x * downForce;

            Vector3 localForceDirection = CalculateForces(accelerationForce, brakeForce, sidewaysForce, downForce);
            forceDirectionDebug = localForceDirection;
            Vector3 worldForceDirection = transform.TransformDirection(localForceDirection);
            rb.AddForceAtPosition(worldForceDirection, hit.point);

            Debug.DrawRay(transform.position, -this.transform.up * hit.distance, Color.red);
        }
        else
        {
            steeringWheelForce = 0;
        }

        //Technical debt
        RotateWheelModel(wheelSlipAnimate);
    }

    private float CalculateBrakingForce(float brakeInput)
    {
        return Mathf.Clamp(wheelVelocityLocalSpace.z, -1, 1) * -VehicleConstants.BRAKE_FORCE * brakeInput;
    }

    private Vector3 CalculateForces(float accelerationForce, float brakeForce, float sidewaysForce, float downForce)
    {
        Vector2 a = new Vector2(0f, accelerationForce + brakeForce);
        Vector2 b = new Vector2(sidewaysForce, 0f);
        float c = Vector2.Distance(a, b);

        float p = c / downForce;
        p = slipCurve.Evaluate(Mathf.Abs(p));
        float gripForce = downForce * p;

        Vector2 result = a + b;
        steeringWheelForce = -result.x;
        gripDebug = p;
        return new Vector3(Mathf.Clamp(result.x, -gripForce, gripForce) * Mathf.Abs(result.normalized.x), 0f, Mathf.Clamp(result.y, -gripForce, gripForce) * Mathf.Abs(result.normalized.y));
    }

    private Vector3 CalculateSuspensionForce(RaycastHit hit)
    {
        lastLength = springLength;

        springLength = hit.distance;

        springLength = Mathf.Clamp(springLength, minLength, maxLength);
        float springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;
        float springForce = springtStiffness * (restLength - springLength);
        float damperForce = damperStiffness * springVelocity;
        if (springForce + damperForce > 0f)
        {
            Debug.DrawLine(this.transform.position, this.transform.position + this.transform.up * ((springForce + damperForce) / 2000f), Color.cyan);
            return (springForce + damperForce) * this.transform.up;
        }
        return Vector3.zero;
    }

    private float CalculateAccelerationForce(Engine engine)
    {
        float power = (engine.horsePower) * engine.EnergyOutput;
        return power;
    }

    private void RotateWheelModel(float wheelBrakeSlip)
    {
        float circumference = (Mathf.PI * 2f) * wheelRadius;
        Vector3 velocity = wheelVelocityLocalSpace * 3.6f;
        float speed = velocity.z / circumference;

        if (wheelBrakeSlip > 0f)
            speed = 0;

        if (wheelPosition == WheelPosition.FrontRight || wheelPosition == WheelPosition.RearRight)
            wheelModel.transform.GetChild(0).Rotate(Vector3.left, speed);
        else 
            wheelModel.transform.GetChild(0).Rotate(Vector3.left, -speed);

        if(speed > 7f)
            BlurWheel();
        else
            UnBlurWheel();

        float meterPerSecond = wheelVelocityLocalSpace.z;
        RPM = meterPerSecond / wheelRadius;
    }

    private void BlurWheel()
    {
        wheelModel.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
        wheelModel.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
    }
    private void UnBlurWheel()
    {
        wheelModel.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
        wheelModel.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
    }
}

public enum WheelPosition { FrontLeft, FrontRight, RearLeft, RearRight };
