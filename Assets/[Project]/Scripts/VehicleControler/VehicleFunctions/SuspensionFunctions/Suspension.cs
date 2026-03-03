using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Suspension : MonoBehaviour
{
    [SerializeField] private Rollbar rollbar;
    [SerializeField] private AntiDive antiDive;
    public Wheel_Raycast wheel;

    public SuspensionPosition suspensionPosition;
    public LayerMask layerMask;
    private Rigidbody rb;
    [HideInInspector] public float brakeBias;

    [Header("Suspension")]
    [Tooltip("Position of spring under standard vehicle load")]
    public float springLenght;
    public float maxSpringStretch;

    public float springConstant;
    public float damperStiffness;

    private float lastSpringCompression;

    //For audio;
    [HideInInspector] public float stressSuspensionAudio;

    private FeedbackComponent feedbackComponent;
    public AnimationCurve Slip_feedbackCurve;

    private void Awake()
    {
        rb = transform.root.GetComponentInChildren<Rigidbody>();
    }

    private void Start()
    {
        feedbackComponent = new FeedbackComponent("Suspension", .25f);
        FeedbackSystem.instance.RegisterFeedbackComponent(feedbackComponent);
    }

    public Vector3 SimulatePhysics(float brakeInput, float engineForce, out float wheelSpin, out float engineWobble, out RaycastHit? hitCast)
    {
        hitCast = null;
        Vector3 result = Vector3.zero;
        wheelSpin = 0f;
        engineWobble = 0f;
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
            hitCast = hit;
        }
        return result;
    }
    bool slip;
    [Header("Grip Tuning")]
    [SerializeField, Range(0.1f, 2f)] private float lateralDemandScale = 0.45f;

    private Vector3 CalculateForces(float accelerationForce, float brakeForce, float downForce, out float wheelSpin)
    {
        float safeDownForce = Mathf.Max(Mathf.Abs(downForce), 0.001f);
        Vector2 sideways = new Vector2(-wheel.wheelVelocityLocalSpace.x * downForce * lateralDemandScale, 0f);
        Vector2 forward = new Vector2(0f, accelerationForce + brakeForce);
        Vector2 rawForce = forward + sideways;

        //Normal force | No accelation or brake influence on sideforce
        float distance = Vector2.Distance(Vector2.zero, rawForce);
        float forceDemandRatio = Mathf.Clamp(distance / safeDownForce, 0f, 25f);
        forceDemandRatio = float.IsNaN(forceDemandRatio) ? 25f : forceDemandRatio;

        bool useSurfaceSlipCurve = wheel.currentSurface != null && wheel.currentSurface.useSlip;
        float gripPercentage;

        if (useSurfaceSlipCurve)
        {
            if (forceDemandRatio > wheel.currentSurface.SlipValue && !slip)
                slip = true;
            if (forceDemandRatio < wheel.currentSurface.UnSlipValue && slip)
                slip = false;

            float surfaceGrip = slip
                ? wheel.currentSurface.unGripped.Evaluate(Mathf.Abs(forceDemandRatio))
                : wheel.currentSurface.gripped.Evaluate(Mathf.Abs(forceDemandRatio));
            // Surface curves own grip behavior when enabled; avoid double-penalizing with fallback curve.
            float minSurfaceGrip = slip ? 0.75f : 0.9f;
            gripPercentage = Mathf.Clamp(Mathf.Clamp01(surfaceGrip), minSurfaceGrip, 1f);
        }
        else
        {
            slip = false;
            // When no surface slip curve is active, keep neutral grip scaling.
            gripPercentage = 1f;
        }

        gripPercentage = Mathf.Clamp01(gripPercentage);

        float gripForce = downForce * gripPercentage;
        Vector3 clampedGripForce = ClampForce(rawForce, gripForce);

        Vector2 slipForces = new Vector2(Mathf.Abs(brakeForce / safeDownForce), Mathf.Abs(accelerationForce / safeDownForce));
        Vector2 spinLockForce = wheel.RotateWheelModel(slipForces);
        wheelSpin = spinLockForce.y;

        wheel.grip_UI = Mathf.Max(.01f, gripPercentage);
        wheel.gripTime_UI = forceDemandRatio;
        float horizontalForce = clampedGripForce.x / safeDownForce;
        horizontalForce = float.IsNaN(horizontalForce) ? 0f : horizontalForce;
        wheel.steeringWheelForce = horizontalForce;
        return clampedGripForce;
    }

    private Vector3 ClampForce(Vector2 rawForce, float absoluteClampValue)
    {
        // Prioritize lateral stability, then spend remaining grip on longitudinal force.
        // This reduces snap oversteer when throttle is applied at speed.
        float maxMagnitude = Mathf.Max(0f, absoluteClampValue);
        if (maxMagnitude <= 0.0001f)
            return Vector3.zero;

        float maxLateral = maxMagnitude * 0.9f;
        float lateral = Mathf.Clamp(rawForce.x, -maxLateral, maxLateral);
        float longitudinalBudget = Mathf.Sqrt(Mathf.Max(0f, (maxMagnitude * maxMagnitude) - (lateral * lateral)));
        float longitudinal = Mathf.Clamp(rawForce.y, -longitudinalBudget, longitudinalBudget);

        return new Vector3(lateral, 0f, longitudinal);
    }

    private Vector3 CalculateSuspensionForce(RaycastHit hit, out float suspensionCompresssion)
    {
        //Rollbar calculation
        float compression = hit.distance / (springLenght + (wheel.wheelRadius));
        rollbar.UpdateSpringValue(compression, suspensionPosition);
        antiDive.UpdateSpringValue(compression, suspensionPosition);

        float rollbarForce = rollbar.CalculateRollForce(suspensionPosition);
        float antiDiveForce = antiDive.CalculateAntiDiveForce(suspensionPosition);

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

        return wheel.transform.up * Mathf.Max(0f,(springForce + damperForce + rollbarForce + antiDiveForce));
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
