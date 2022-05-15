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
    [Tooltip("Distance spring can travel in y axis")]
    public float springTravel;

    public float springtStiffness;
    public float damperStiffness;

    private float minLength;
    private float maxLength;
    private float lastLength;

    //For audio;
    [HideInInspector] public float stressSuspensionAudio;

    private FeedbackComponent feedbackComponent;
    public AnimationCurve Slip_feedbackCurve;

    private void Start()
    {
        rb = transform.root.GetComponent<Rigidbody>();

        minLength = springLenght - springTravel;
        maxLength = springLenght + springTravel/10f;

        feedbackComponent = new FeedbackComponent("Suspension", .25f);
        FeedbackSystem.instance.RegisterFeedbackComponent(feedbackComponent);
    }

    public float SimulatePhysics(float brakeInput, float engineForce)
    {
        if (wheel.broken)
            return 0f;

        float downForce = 0f;
        float physicsWobble = 0f;
        if (wheel.Raycast(maxLength, layerMask, out RaycastHit hit))
        {
            Debug.DrawLine(this.transform.position, this.transform.position + rb.GetPointVelocity(hit.point).normalized);
            Debug.DrawLine(wheel.transform.position, wheel.transform.position + wheel.transform.forward, Color.green);
            //Debug.DrawRay(transform.position, -this.transform.up * hit.distance, Color.red);

            wheel.wheelVelocityLocalSpace = wheel.transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));

            float suspensionVelocity;
            Vector3 suspensionForce = CalculateSuspensionForce(hit, out suspensionVelocity);
            physicsWobble = suspensionVelocity;

            rb.AddForceAtPosition(suspensionForce, hit.point);
            downForce = suspensionForce.y + (rb.mass);

            float accelerationForce = engineForce;
            float brakeForce = CalculateBrakingForce(brakeInput);

            Vector3 localForceDirection = CalculateForces(accelerationForce, brakeForce, downForce);
            wheel.forceDirectionDebug = localForceDirection;
            Vector3 worldForceDirection = wheel.transform.TransformDirection(localForceDirection);
            //Check if valid
            worldForceDirection = float.IsNaN(worldForceDirection.y) ? Vector3.zero : worldForceDirection;
            rb.AddForceAtPosition(worldForceDirection, hit.point);

            Debug.DrawRay(wheel.transform.position, -wheel.transform.up * hit.distance, Color.red);

            stressSuspensionAudio = Mathf.Lerp(stressSuspensionAudio, physicsWobble, Time.deltaTime/2f);
        }
        return physicsWobble;
    }

    private Vector3 CalculateForces(float accelerationForce, float brakeForce, float downForce)
    {
        Vector2 forward = new Vector2(0f, accelerationForce + brakeForce);
        float time2 = -forward.y / downForce;
        float brakeSlip = wheel.RotateWheelModel(time2, suspensionPosition);
        Vector3 brake = this.transform.root.InverseTransformDirection(rb.GetPointVelocity(transform.position));

        float calc = Mathf.Lerp(-wheel.wheelVelocityLocalSpace.x, -brake.x, 1f - brakeSlip);
        Vector2 sideways = new Vector2(calc * downForce, 0f);

        Vector2 rawForce = forward + sideways;

        //Normal force | No accelation or brake influence on sideforce
        float distance = Vector2.Distance(Vector2.zero, rawForce);
        float time = distance / downForce;

        float gripPercentage = 0;
        if (wheel.currentSurface != null)
            gripPercentage = wheel.currentSurface.slip.Evaluate(Mathf.Abs(time));

        float gripForce = downForce * gripPercentage;
        Vector3 clampedGripForce =  ClampForce(rawForce, gripForce);

        wheel.gripDebug = Mathf.Max(.01f, gripPercentage);
        float horizontalForce = Mathf.Clamp(clampedGripForce.x/downForce, -1f, 1f) * -Mathf.Abs(gripPercentage);
        wheel.steeringWheelForce = horizontalForce;
        return clampedGripForce;
    }

    private Vector3 ClampForce(Vector2 rawForce, float absoluteClampValue)
    {
        return new Vector3(Mathf.Clamp(rawForce.x, -absoluteClampValue, absoluteClampValue), 0f, Mathf.Clamp(rawForce.y, -absoluteClampValue, absoluteClampValue));
    }

    private Vector3 CalculateSuspensionForce(RaycastHit hit, out float suspensionCompresssion)
    {
        float springCompressedLength = Mathf.Clamp(hit.distance, minLength, maxLength);
        float springVelocity = (lastLength - springCompressedLength) / Time.fixedDeltaTime;
        float springForce = springtStiffness * (this.springLenght - springCompressedLength);
        float damperForce = damperStiffness * springVelocity;
        lastLength = springCompressedLength;
        suspensionCompresssion = springVelocity;

        Debug.DrawLine(this.transform.position, this.transform.position + this.transform.up * ((springForce + damperForce) / 2000f), Color.cyan);
        return this.transform.up * Mathf.Max(0f,(springForce + damperForce));
    }

    private float CalculateBrakingForce(float brakeInput)
    {
        return Mathf.Clamp(wheel.wheelVelocityLocalSpace.z, -1, 1) * (-VehicleConstants.BRAKE_FORCE * brakeBias) * brakeInput;
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
