using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Suspension : MonoBehaviour
{
    public Wheel_Raycast wheel;

    public float collisionStrenght;
    public SuspensionPosition suspensionPosition;
    public LayerMask layerMask;
    private Rigidbody rb;
    [HideInInspector] public float brakeBias;

    [Header("Suspension")]
    [Tooltip("Position of spring under standard vehicle load")]
    public float springLenght;

    public float springConstant;
    public float damperStiffness;
    public AnimationCurve suspensionPower;

    private float lastSpringCompression;
    [SerializeField] private Rollbar rollbar;

    //For audio;
    [HideInInspector] public float stressSuspensionAudio;

    private FeedbackComponent feedbackComponent;
    public AnimationCurve Slip_feedbackCurve;

    private void Awake()
    {
        rb = transform.root.GetComponent<Rigidbody>();
    }

    private void Start()
    {
        feedbackComponent = new FeedbackComponent("Suspension", .25f);
        FeedbackSystem.instance.RegisterFeedbackComponent(feedbackComponent);
    }

    public Vector3 SimulatePhysics(float brakeInput, float engineForce, out float wheelSpin, out float engineWobble)
    {
        Vector3 result = Vector3.zero;
        wheelSpin = 0f;
        engineWobble = 0f;
        if (wheel.broken)
            return result;

        if (wheel.Raycast(springLenght + wheel.wheelRadius, layerMask, out RaycastHit hit))
        {
            //Debug.DrawLine(this.transform.position, this.transform.position + rb.GetPointVelocity(hit.point).normalized);
            Debug.DrawLine(wheel.transform.position, wheel.transform.position + wheel.transform.forward, Color.green);
            //Debug.DrawRay(transform.position, -this.transform.up * hit.distance, Color.red);

            wheel.wheelVelocityLocalSpace = wheel.transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));

            float suspensionVelocity;
            Vector3 suspensionForce = CalculateSuspensionForce(hit, out suspensionVelocity);
            engineWobble = suspensionVelocity;

            result += suspensionForce;

            float brakeForce = CalculateBrakingForce(brakeInput);
            Vector3 localForceDirection = CalculateForces(engineForce, brakeForce, suspensionForce.y, out wheelSpin);
            wheel.forceDirectionDebug = localForceDirection;
            Vector3 worldForceDirection = wheel.transform.TransformDirection(localForceDirection);
            //Check if valid
            worldForceDirection = float.IsNaN(worldForceDirection.y) ? Vector3.zero : worldForceDirection;
            result += worldForceDirection;

            Debug.DrawRay(wheel.transform.position, -wheel.transform.up * hit.distance, Color.red);

            stressSuspensionAudio = Mathf.Lerp(stressSuspensionAudio, engineWobble, Time.deltaTime/2f);
        }
        return result;
    }
    bool slip;
    private Vector3 CalculateForces(float accelerationForce, float brakeForce, float downForce, out float wheelSpin)
    {
        Vector2 sideways = new Vector2(-wheel.wheelVelocityLocalSpace.x * downForce, 0f);
        Vector2 forward = new Vector2(0f, accelerationForce + brakeForce);
        Vector2 rawForce = forward + sideways;

        //Normal force | No accelation or brake influence on sideforce
        float distance = Vector2.Distance(Vector2.zero, rawForce);
        float time = Mathf.Clamp(distance / downForce, 0f, 25f);
        time = float.IsNaN(time) ? 25f : time;

        float gripPercentage = 0F;
        if(time > wheel.currentSurface.SlipValue && !slip)
            slip = true;
        if (time < wheel.currentSurface.UnSlipValue && slip)
            slip = false;

        gripPercentage = slip ? wheel.currentSurface.unGripped.Evaluate(Mathf.Abs(time)) : wheel.currentSurface.gripped.Evaluate(Mathf.Abs(time));
        float gripForce = downForce * gripPercentage;
        Vector3 clampedGripForce = ClampForce(rawForce, gripForce);

        Vector2 slipForces = new Vector2(Mathf.Abs(brakeForce / downForce), Mathf.Abs(accelerationForce / downForce));
        Vector2 spinLockForce = wheel.RotateWheelModel(slipForces, suspensionPosition);
        wheelSpin = spinLockForce.y;

        wheel.grip_UI = Mathf.Max(.01f, gripPercentage);
        float horizontalForce = clampedGripForce.x / downForce;
        horizontalForce = float.IsNaN(horizontalForce) ? 0f : horizontalForce;
        wheel.steeringWheelForce = horizontalForce;
        return clampedGripForce;
    }

    private Vector3 ClampForce(Vector2 rawForce, float absoluteClampValue)
    {
        return new Vector3(Mathf.Clamp(rawForce.x, -absoluteClampValue, absoluteClampValue), 0f, Mathf.Clamp(rawForce.y, -absoluteClampValue, absoluteClampValue));
    }

    private Vector3 CalculateSuspensionForce(RaycastHit hit, out float suspensionCompresssion)
    {
        //Rollbar calculation
        float rollTravel = hit.distance / (springLenght + (wheel.wheelRadius));
        rollbar.UpdateSpringValue(rollTravel, suspensionPosition);
        float rollbarForce = rollbar.CalculateRollForce(suspensionPosition);

        //Spring compression calculation
        float springCompression = Mathf.Clamp(1f - (hit.distance / (springLenght + wheel.wheelRadius)),0f,1f);
        float springForce = springConstant * springCompression;
        suspensionCompresssion = springCompression;

        //Spring velocity calculation
        float compressionVelocity = Mathf.Clamp((lastSpringCompression - springCompression) / Time.fixedDeltaTime, 0, 1f);
        lastSpringCompression = springCompression;

        //Spring damper calculation
        float damperForce = -damperStiffness * compressionVelocity;

        Debug.DrawLine(wheel.transform.position, wheel.transform.position + (-this.transform.up * (hit.distance + wheel.wheelRadius)), Color.green);

        return wheel.transform.up * Mathf.Max(0f,(springForce + damperForce + rollbarForce));
    }

    private float CalculateBrakingForce(float brakeInput)
    {
        return (Mathf.Clamp(wheel.wheelVelocityLocalSpace.z, -1, 1) * (-VehicleConstants.BRAKE_FORCE * brakeBias)) * brakeInput;
    }

    public void OnReset()
    {
        wheel.enabled = true;
        wheel.OnReset();
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
