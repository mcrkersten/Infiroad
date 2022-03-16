using System;
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

    private FeedbackComponent feedbackComponent;
    public AnimationCurve Slip_feedbackCurve;

    private void Start()
    {
        rb = transform.root.GetComponent<Rigidbody>();

        minLength = restLength - springTravel;
        maxLength = restLength + springTravel;

        feedbackComponent = new FeedbackComponent("Suspension", .25f);
        FeedbackSystem.instance.RegisterFeedbackComponent(feedbackComponent);
    }

    public float SimulatePhysics(float brakeInput, float engineForce)
    {
        float downForce = 0f;
        float wheelSlipAnimate = 0f;
        float physicsWobble = 0f;
        if (wheel.Raycast(maxLength, layerMask, out RaycastHit hit))
        {
            Debug.DrawLine(this.transform.position, this.transform.position + rb.GetPointVelocity(hit.point).normalized);
            Debug.DrawLine(wheel.transform.position, wheel.transform.position + wheel.transform.forward, Color.green);
            //Debug.DrawRay(transform.position, -this.transform.up * hit.distance, Color.red);

            wheel.wheelVelocityLocalSpace = wheel.transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));

            float suspensionCompression = 0;
            Vector3 suspensionForce = CalculateSuspensionForce(hit, out suspensionCompression);
            physicsWobble = suspensionCompression;
            rb.AddForceAtPosition(suspensionForce, hit.point);
            downForce = suspensionForce.y + (rb.mass/4f);

            float accelerationForce = engineForce;
            float brakeForce = CalculateBrakingForce(brakeInput);
            float sidewaysForce = -wheel.wheelVelocityLocalSpace.x * downForce;

            Vector3 localForceDirection = CalculateForces(accelerationForce, brakeForce, sidewaysForce, downForce);
            wheel.forceDirectionDebug = localForceDirection;
            Vector3 worldForceDirection = wheel.transform.TransformDirection(localForceDirection);
            //Check if valid
            worldForceDirection = float.IsNaN(worldForceDirection.y) ? Vector3.zero : worldForceDirection;
            rb.AddForceAtPosition(worldForceDirection, hit.point);

            Debug.DrawRay(wheel.transform.position, -wheel.transform.up * hit.distance, Color.red);
        }
        else
        {
            wheel.steeringWheelForce = 0;
        }

        //Technical debt
        wheel.RotateWheelModel(wheelSlipAnimate, suspensionPosition);
        return physicsWobble;
    }

    private Vector3 CalculateForces(float accelerationForce, float brakeForce, float sidewaysForce, float downForce)
    {
        Vector2 forward = new Vector2(0f, accelerationForce + brakeForce);
        Vector2 sideways = new Vector2(sidewaysForce, 0f);
        Vector2 rawForce = forward + sideways;

        float distance = Vector2.Distance(Vector2.zero, sideways + forward);
        float time = distance / downForce;
        float force = wheel.slipCurve.Evaluate(Mathf.Abs(time));
        float maxForce = downForce * force;

        Vector3 cleanedForce =  ReturnCleanedForce(maxForce, rawForce);

        float scaledForce = cleanedForce.x / maxForce;
        wheel.steeringWheelForce = -(scaledForce) * 100f;
        wheel.gripDebug = Mathf.Max(.01f, force);

        switch (suspensionPosition)
        {
            case SuspensionPosition.FrontLeft:
                feedbackComponent.UpdateHighFrequencyRumble(Slip_feedbackCurve.Evaluate(Mathf.Abs(sideways.x / downForce)));
                break;
            case SuspensionPosition.FrontRight:
                feedbackComponent.UpdateHighFrequencyRumble(Slip_feedbackCurve.Evaluate(Mathf.Abs(sideways.x / downForce)));
                break;
            case SuspensionPosition.RearLeft:
                feedbackComponent.UpdateLowFrequencyRumble(Slip_feedbackCurve.Evaluate(Mathf.Abs(sideways.x / downForce)));
                break;
            case SuspensionPosition.RearRight:
                Debug.Log(time);
                feedbackComponent.UpdateLowFrequencyRumble(Slip_feedbackCurve.Evaluate(Mathf.Abs(sideways.x / downForce)));
                break;
            default:
                break;
        }
        return cleanedForce;
    }

    private Vector3 ReturnCleanedForce(float maxForce, Vector2 rawForce)
    {
        return new Vector3(Mathf.Clamp(rawForce.x, -maxForce, maxForce), 0f, Mathf.Clamp(rawForce.y, -maxForce, maxForce));
    }

    private Vector3 CalculateSuspensionForce(RaycastHit hit, out float suspensionCompresssion)
    {
        float springLength = Mathf.Clamp(hit.distance, minLength, maxLength);
        float springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;
        float springForce = springtStiffness * (restLength - springLength);
        float damperForce = damperStiffness * springVelocity;
        lastLength = springLength;
        suspensionCompresssion = springVelocity;
        if (springForce + damperForce > 0f)
        {
            Debug.DrawLine(this.transform.position, this.transform.position + this.transform.up * ((springForce + damperForce) / 2000f), Color.cyan);
            return this.transform.up * (springForce + damperForce);
        }

        return Vector3.zero;
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
