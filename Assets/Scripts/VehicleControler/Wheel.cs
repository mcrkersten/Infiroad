using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    public WheelPosition wheelType;
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

    private float wheelForce;
    public float WheelForce { get { return wheelForce; } }

    private Vector3 wheelVelocityLocalSpace;
    public float RPM;

    [Header("Wheel")]
    public float wheelRadius;
    public float steerAngle;

    [Header("General Slip")]
    public AnimationCurve slipCurve;

    [Header("Braking Slip")]
    public AnimationCurve speedCurve;
    public AnimationCurve brakePedalCurve;
    public AnimationCurve brakeSlipCurve;

    [Header("Acceleration Slip")]
    public float maxAccelerationPull;
    public AnimationCurve accelerationSlipCurve;

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
        float wheelBrakeSlip = 0f;
        float wheelTractionSlip = 0f;
        if (Physics.SphereCast(transform.position, wheelRadius, -transform.up, out RaycastHit hit, maxLength))
        {
            Debug.DrawLine(this.transform.position, this.transform.position + rb.GetPointVelocity(hit.point).normalized);
            Debug.DrawLine(this.transform.position, this.transform.position + this.transform.forward, Color.green);
            wheelModel.transform.position = transform.position + (-this.transform.up * (hit.distance));

            wheelVelocityLocalSpace = transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));
            if (isDriveWheel)
            {
                Vector3 accelerationForce = AccelerateWheel(engine, out wheelTractionSlip);
                rb.AddForceAtPosition(accelerationForce, hit.point + new Vector3(0, wheelRadius, 0));
            }

            Vector3 suspensionForce = CalculateSuspensionForce(hit, out float springforce);
            rb.AddForceAtPosition(suspensionForce, hit.point);

            Vector3 brakeSlipForce = BrakeWheel(brakeInput, out wheelBrakeSlip);
            rb.AddForceAtPosition(brakeSlipForce, hit.point);

            Vector3 sidewaysGripForce = CalculateSidewaysGripForce(springforce, wheelTractionSlip, wheelBrakeSlip);
            rb.AddForceAtPosition(sidewaysGripForce, hit.point);


            //Vector3 brakingForce = 
            Debug.DrawRay(transform.position, -this.transform.up * hit.distance, Color.red);
        }
        else
        {
            wheelForce = (1);
        }
        RotateWheel(wheelBrakeSlip);
    }

    private Vector3 CalculateSuspensionForce(RaycastHit hit, out float springForce)
    {
        lastLength = springLength;

        springLength = hit.distance;

        springLength = Mathf.Clamp(springLength, minLength, maxLength);
        float springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;
        springForce = springtStiffness * (restLength - springLength);
        float damperForce = damperStiffness * springVelocity;
        if (springForce + damperForce > 0f)
        {
            Debug.DrawLine(this.transform.position, this.transform.position + this.transform.up * ((springForce + damperForce) / 2000f), Color.cyan);
            return (springForce + damperForce) * this.transform.up;
        }
        else
        {
            springForce = 0;
            return Vector3.zero;
        }
    }

    private Vector3 CalculateSidewaysGripForce(float springForce, float wheelTractionSlip, float wheelBrakeSlip)
    {
        springForce = Mathf.Clamp(springForce, 0, 7000f);
        float yawSpeed = VehicleConstants.WHEEL_BASE * .5f * rb.angularVelocity.y;
        float rot_angle = Mathf.Atan2(yawSpeed, wheelVelocityLocalSpace.z);
        float sideSlip = Mathf.Atan2(wheelVelocityLocalSpace.x, wheelVelocityLocalSpace.z) * Mathf.Rad2Deg;

        //Slip angles front and rear
        float slipAngle;
        if (wheelType == WheelPosition.FrontLeft || wheelType == WheelPosition.FrontRight)
            slipAngle = sideSlip + rot_angle - steerAngle;
        else
            slipAngle = sideSlip - rot_angle;

        float sidewaysForce = Mathf.Clamp(wheelVelocityLocalSpace.x/30F, -1f, 1f); //Sideways force of vehicle
        float slip = 1f -  (Mathf.Abs(slipCurve.Evaluate(sidewaysForce)) * speedCurve.Evaluate(rb.velocity.magnitude * 3.6f));

        float brakeSlipPercentage = 1f - (wheelBrakeSlip / brakeSlipCurve.keys[2].value);
        wheelForce = 1f - slipCurve.Evaluate(sidewaysForce) * (speedCurve.Evaluate(rb.velocity.magnitude));
        Vector3 output = (wheelVelocityLocalSpace.x * springForce) * (-transform.right * slip) * brakeSlipPercentage * ( 1f - wheelTractionSlip );
        return output;
    }

    private Vector3 AccelerateWheel(Engine engine, out float wheelTractionSlip)
    {
        wheelTractionSlip = accelerationSlipCurve.Evaluate(engine.pullMultiplier / maxAccelerationPull);
        if (float.IsNaN(wheelTractionSlip))
            wheelTractionSlip = 0f;
        Vector3 rawForce = transform.forward * (engine.horsePower) * engine.EnergyOutput * (1f - wheelTractionSlip);
        return rawForce;
    }

    private Vector3 BrakeWheel(float brakeInput, out float wheelBrakeSlip)
    {
        float brakeEnergy = VehicleConstants.BRAKE_FORCE * brakePedalCurve.Evaluate(brakeInput); //Energy of brakes
        wheelBrakeSlip = brakeSlipCurve.Evaluate(brakePedalCurve.Evaluate(brakeInput));

        Vector3 brakeVelocity;
        if (Mathf.Abs(wheelVelocityLocalSpace.z) < 7f)
        {
            if (wheelVelocityLocalSpace.z > .01f)
                brakeVelocity = -transform.forward * brakeEnergy;
            else
                brakeVelocity = transform.forward * brakeEnergy;
        }
        else
        {
            if (wheelVelocityLocalSpace.z > .01f)
                brakeVelocity = (-transform.forward * brakeEnergy) * (1f - wheelBrakeSlip);
            else
                brakeVelocity = (transform.forward * brakeEnergy) * (1f - wheelBrakeSlip);
        }
        return brakeVelocity;
    }

    public float CalculateBaseRPM()
    {
        float circumference = (Mathf.PI * 2f) * wheelRadius;
        Vector3 velocity = wheelVelocityLocalSpace;
        float meterPerHour = (velocity.z * 3.6f) * 1000f;
        float meterPerMinute = meterPerHour / 60f;
        return meterPerMinute / circumference;
    }

    private void RotateWheel(float wheelBrakeSlip)
    {
        float circumference = (Mathf.PI * 2f) * wheelRadius;
        Vector3 velocity = wheelVelocityLocalSpace * 3.6f;
        float speed = velocity.z / circumference;

        if (wheelBrakeSlip > 0f)
        {
            speed = 0;
        }

        if (wheelType == WheelPosition.FrontRight || wheelType == WheelPosition.RearRight)
            wheelModel.transform.GetChild(0).Rotate(Vector3.left, speed);
        else 
            wheelModel.transform.GetChild(0).Rotate(Vector3.left, -speed);

        if(speed > 7f)
        {
            wheelModel.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
            wheelModel.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            wheelModel.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
            wheelModel.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
        }

        float meterPerSecond = wheelVelocityLocalSpace.z;
        RPM = meterPerSecond / wheelRadius;
    }
}

public enum WheelPosition { FrontLeft, FrontRight, RearLeft, RearRight };
