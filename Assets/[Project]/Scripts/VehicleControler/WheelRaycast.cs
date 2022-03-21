using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelRaycast : MonoBehaviour
{
    [SerializeField] private GameObject wheelModel;
    [SerializeField] private Transform suspension;
    [SerializeField] private List<SuspensionPointer> suspensionPointers = new List<SuspensionPointer>();

    [Header("General Slip")]
    public AnimationCurve slipCurve;
    [SerializeField] private AnimationCurve brakeLock;
    [SerializeField] private AnimationCurve wheelSpin;

    private Vector3 wheelModelLocalStartPosition;
    private Vector3 suspensionLocalStartPosition;

    [HideInInspector] public Vector3 wheelVelocityLocalSpace;
    [HideInInspector] public float steeringWheelForce;
    public float RPM;

    [Header("Wheel")]
    public float wheelRadius;
    private float tireCircumference;
    public float steerAngle;


    [HideInInspector] public Vector3 forceDirectionDebug = Vector3.zero;
    public float gripDebug = 0.01f;

    private void Start()
    {
        tireCircumference = (Mathf.PI * 2f) * wheelRadius;
        wheelModelLocalStartPosition = new Vector3(wheelModel.transform.localPosition.x, 0, wheelModel.transform.localPosition.z);
        suspensionLocalStartPosition = new Vector3(suspension.transform.localPosition.x, 0, suspension.transform.localPosition.z);
    }

    private void Update()
    {
        transform.localEulerAngles = new Vector3(0, steerAngle, 0);

        foreach (SuspensionPointer s in suspensionPointers)
        {
            s.UpdatePointer();
        }
    }

    public bool Raycast(float maxLenght, LayerMask layerMask, out RaycastHit hit)
    {
        if (Physics.SphereCast(transform.position, wheelRadius, -transform.root.up, out RaycastHit hitPoint, maxLenght, layerMask))
        {
            wheelModel.transform.localPosition = wheelModelLocalStartPosition + (-Vector3.up * (hitPoint.distance));
            suspension.transform.localPosition = suspensionLocalStartPosition + (-Vector3.up * (hitPoint.distance));
            hit = hitPoint;
            
            return true;
        }
        wheelModel.transform.localPosition = wheelModelLocalStartPosition + (-Vector3.up * maxLenght);
        suspension.transform.localPosition = suspensionLocalStartPosition + (-Vector3.up * maxLenght);
        hit = new RaycastHit();
        return false;
    }

    public float RotateWheelModel(float verticalForce, SuspensionPosition suspensionPosition)
    {
        float wheelLockPercentage = brakeLock.Evaluate(verticalForce);
        float wheelSpinPercentage = wheelSpin.Evaluate(verticalForce);
        float combinedPercentage = 1f - Mathf.Clamp((1f - wheelLockPercentage) + (1f - wheelSpinPercentage), 0f, 1f);

        float rotationSpeed = (wheelVelocityLocalSpace.z * 3.6f) / tireCircumference;
        if (suspensionPosition == SuspensionPosition.FrontRight || suspensionPosition == SuspensionPosition.RearRight)
            wheelModel.transform.Rotate(Vector3.right, rotationSpeed * combinedPercentage);
        else
            wheelModel.transform.Rotate(Vector3.left, rotationSpeed * combinedPercentage);

        float meterPerSecond = wheelVelocityLocalSpace.z;
        RPM = meterPerSecond / wheelRadius;
        return combinedPercentage;
    }
}
