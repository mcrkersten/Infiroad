using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleController : MonoBehaviour
{
    //Input
    private SteeringWheelInput steeringInput;
    public VehicleInputActions vehicleInputActions;
    private InputAction steering;
    private InputAction braking;
    private InputAction acceleration;
    private InputAction clutch;

    [Header("Vehicle configuration")]
    public Engine engine;
    public Transform centerOfMass;
    public Transform steeringWheel;

    private float ackermannAngleLeft;
    private float ackermannAngleRight;
    private Rigidbody rb;
    private float slideDirection;

    public List<Wheel> wheels = new List<Wheel>();
    public List<DownForceWing> downforceWing = new List<DownForceWing>();

    [Header("Gears")]
    public DriveType driveType;
    public float[] gearRatios;
    public AnimationCurve torqueCurve;
    public float differentialRatio;
    public float idleRPM;
    public float maxRPM;
    public float enginePower;

    public VehicleUserInterfaceData data;
    void Awake()
    {
        vehicleInputActions = new VehicleInputActions();
        steeringInput = new SteeringWheelInput();
        engine = new Engine(gearRatios, differentialRatio, idleRPM, maxRPM, enginePower, wheels, torqueCurve);
        data = new VehicleUserInterfaceData(engine, this);
        steeringInput.Init();
        rb = this.GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass.localPosition;
    }

    private void OnEnable()
    {
        steering = vehicleInputActions.Vehicle.Steering;
        braking = vehicleInputActions.Vehicle.Braking;
        acceleration = vehicleInputActions.Vehicle.Acceleration;
        clutch = vehicleInputActions.Vehicle.Clutch;
        steering.Enable();
        braking.Enable();
        acceleration.Enable();
        clutch.Enable();

        vehicleInputActions.Vehicle.ShiftUP.Enable();
        vehicleInputActions.Vehicle.ShiftUP.started += engine.ShiftUp;
        vehicleInputActions.Vehicle.ShiftDOWN.Enable();
        vehicleInputActions.Vehicle.ShiftDOWN.started += engine.ShiftDown;
    }

    // Update is called once per frame
    void Update()
    {
        steeringInput.Update();
        SteeringInput();
        RotateTires();

        slideDirection = (1f - CalculateSlideVector(wheels));
        Debug.DrawLine(centerOfMass.position, centerOfMass.transform.position + rb.velocity.normalized, Color.red);

        steeringInput.SetWheelForce(-Mathf.RoundToInt(slideDirection * 500f));
    }

    private void FixedUpdate()
    {
        foreach (DownForceWing wing in downforceWing)
        {
            float downforce = wing.CalculateLiftforce(1.1455f);
            Vector3 downForceVector = downforce * -wing.transform.up;
            rb.AddForceAtPosition(downForceVector * 1000f, wing.transform.position);
            Debug.DrawLine(wing.transform.position, wing.transform.position + downForceVector, Color.green);
        }

        float accelerationInput = (acceleration.ReadValue<float>() + 1f) / 2;
        float clutchInput = 1f - (clutch.ReadValue<float>() + 1f) / 2f;
        float brakeInput = 1f - (braking.ReadValue<float>() + 1f) / 2f;
        engine.SimulateEngine(accelerationInput, clutchInput, driveType);
        ApplyForceToWheels(brakeInput);

        data.acceleration = accelerationInput;
        data.brake = brakeInput;
    }
    private void SteeringInput()
    {
        float steerPosition = Mathf.Clamp(steeringInput.GetSteeringWheelPosition(), -.6f, .6f);
        steeringWheel.transform.localEulerAngles = new Vector3(14.289f, 0, -steerPosition * 445f);

        float steerForce = Mathf.Clamp(steerPosition * 2f,-1.23f,1.23f);
        if (steerPosition > 0)//right
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(VehicleConstants.WHEEL_BASE / (VehicleConstants.TURN_RADIUS + (VehicleConstants.REAR_TRACK / 2))) * steerForce;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(VehicleConstants.WHEEL_BASE / (VehicleConstants.TURN_RADIUS - (VehicleConstants.REAR_TRACK / 2))) * steerForce;
        }
        else if (steerPosition < 0)//left
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(VehicleConstants.WHEEL_BASE / (VehicleConstants.TURN_RADIUS - (VehicleConstants.REAR_TRACK / 2))) * steerForce;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(VehicleConstants.WHEEL_BASE / (VehicleConstants.TURN_RADIUS + (VehicleConstants.REAR_TRACK / 2))) * steerForce;
        }
        else
        {
            ackermannAngleLeft = 0;
            ackermannAngleRight = 0;
        }
    }
    private void RotateTires()
    {
        foreach (Wheel w in wheels)
        {
            if (w.wheelType == WheelPosition.FrontLeft)
                w.steerAngle = ackermannAngleLeft;
            if (w.wheelType == WheelPosition.FrontRight)
                w.steerAngle = ackermannAngleRight;
        }
    }
    private void ApplyForceToWheels(float brakeForce)
    {
        foreach (Wheel w in wheels)
        {
            switch (driveType)
            {
                case DriveType.rearWheelDrive:
                    if (w.wheelType == WheelPosition.RearLeft || w.wheelType == WheelPosition.RearRight)
                        w.SimulatePhysics(brakeForce, engine, true);
                    else
                        w.SimulatePhysics(brakeForce, engine, false);
                    break;
                case DriveType.frontWheelDrive:
                    if (w.wheelType == WheelPosition.FrontLeft || w.wheelType == WheelPosition.FrontRight)
                        w.SimulatePhysics(brakeForce, engine, true);
                    else
                        w.SimulatePhysics(brakeForce, engine, false);
                    break;
                case DriveType.allWheelDrive:
                    w.SimulatePhysics(brakeForce, engine, true);
                    break;
                default:
                    break;
            }
        }
    }
    private void OnDisable()
    {
        steering.Disable();
        braking.Disable();
        acceleration.Disable();
        clutch.Disable();

        vehicleInputActions.Vehicle.ShiftUP.started -= engine.ShiftUp;
        vehicleInputActions.Vehicle.ShiftUP.Disable();
        vehicleInputActions.Vehicle.ShiftDOWN.started -= engine.ShiftDown;
        vehicleInputActions.Vehicle.ShiftDOWN.Disable();
    }

    private float CalculateSlideVector(List<Wheel> frontWheels)
    {
        float value = 0;
        foreach (Wheel w in wheels)
        {
            if(w.wheelType == WheelPosition.FrontLeft || w.wheelType == WheelPosition.FrontRight)
                value += w.WheelForce;
        }
        value = value / 2f;
        return value;
    }
}

public enum DriveType
{
    rearWheelDrive = 0,
    frontWheelDrive,
    allWheelDrive,
}



