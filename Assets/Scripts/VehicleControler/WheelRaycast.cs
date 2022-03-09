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

    private Vector3 wheelModelLocalStartPosition;
    private Vector3 suspensionLocalStartPosition;

    [HideInInspector] public Vector3 wheelVelocityLocalSpace;
    [HideInInspector] public float steeringWheelForce;

    public float WheelForce { get { return steeringWheelForce; } }
    public float RPM;

    [Header("Wheel")]
    public float wheelRadius;
    public float steerAngle;


    [HideInInspector] public Vector3 forceDirectionDebug = Vector3.zero;
    public float gripDebug = 0.01f;

    private void Start()
    {
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
        if (Physics.SphereCast(transform.position, wheelRadius, -transform.up, out RaycastHit hitPoint, maxLenght, layerMask))
        {
            wheelModel.transform.localPosition = wheelModelLocalStartPosition + (-this.transform.up * (hitPoint.distance));
            suspension.transform.localPosition = suspensionLocalStartPosition + (-this.transform.up * (hitPoint.distance));
            hit = hitPoint;
            return true;
        }
        hit = new RaycastHit();
        return false;
    }

    public void RotateWheelModel(float wheelBrakeSlip, SuspensionPosition suspensionPosition)
    {
        float circumference = (Mathf.PI * 2f) * wheelRadius;
        Vector3 velocity = wheelVelocityLocalSpace * 3.6f;
        float speed = velocity.z / circumference;

        if (wheelBrakeSlip > 0f)
            speed = 0;

        if (suspensionPosition == SuspensionPosition.FrontRight || suspensionPosition == SuspensionPosition.RearRight)
            wheelModel.transform.Rotate(Vector3.left, -speed);
        else
            wheelModel.transform.Rotate(Vector3.left, speed);

        float meterPerSecond = wheelVelocityLocalSpace.z;
        RPM = meterPerSecond / wheelRadius;
    }
}
