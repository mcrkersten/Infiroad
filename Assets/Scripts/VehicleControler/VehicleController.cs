using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleController : MonoBehaviour
{

    public bool useWheel;
    public int wheelInputAngle;
    public float steeringRatio;
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

    public UserInputType userInputType;

    public VehicleUserInterfaceData userInterface;
    void Awake()
    {
        Input.ResetInputAxes();
        steeringInput = new SteeringWheelInput();
        steeringInput.Init();

        vehicleInputActions = new VehicleInputActions();
        engine = new Engine(gearRatios, differentialRatio, idleRPM, maxRPM, enginePower, wheels, torqueCurve);
        userInterface = new VehicleUserInterfaceData(engine, this);

        rb = this.GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass.localPosition;
    }

    private void OnEnable()
    {

        switch (userInputType)
        {
            case UserInputType.Wheels:
                ActivateWheelControls();
                break;
            default:
                ActivateDefaultControls();
                break;
        };

        steering.Enable();
        braking.Enable();
        acceleration.Enable();
        clutch.Enable();
    }

    private void ActivateWheelControls()
    {
        steering = vehicleInputActions.SteeringWheel.Steering;
        braking = vehicleInputActions.SteeringWheel.Braking;
        acceleration = vehicleInputActions.SteeringWheel.Acceleration;
        clutch = vehicleInputActions.SteeringWheel.Clutch;

        vehicleInputActions.SteeringWheel.ShiftUP.Enable();
        vehicleInputActions.SteeringWheel.ShiftUP.started += engine.ShiftUp;
        vehicleInputActions.SteeringWheel.ShiftDOWN.Enable();
        vehicleInputActions.SteeringWheel.ShiftDOWN.started += engine.ShiftDown;
    }

    private void ActivateDefaultControls()
    {
        steering = vehicleInputActions.Default.Steering;
        braking = vehicleInputActions.Default.Braking;
        acceleration = vehicleInputActions.Default.Acceleration;
        clutch = vehicleInputActions.Default.Clutch;

        vehicleInputActions.Default.ShiftUP.Enable();
        vehicleInputActions.Default.ShiftUP.started += engine.ShiftUp;
        vehicleInputActions.Default.ShiftDOWN.Enable();
        vehicleInputActions.Default.ShiftDOWN.started += engine.ShiftDown;
    }

    // Update is called once per frame
    void Update()
    {
        LogitechGSDK.LogiUpdate();
        Debug.DrawLine(centerOfMass.position, centerOfMass.transform.position + rb.velocity.normalized, Color.red);
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

        float clutchInput = 1f - clutch.ReadValue<float>();

        float accelerationInput = ReadAccelerationInput(userInputType);
        float brakeInput = ReadBrakeInput(userInputType);
        float steerPosition = ReadSteeringInput(userInputType);

        engine.SimulateEngine(accelerationInput, clutchInput, driveType);
        ApplyForceToWheels(brakeInput);

        SetUserInterface(accelerationInput, brakeInput);

        CalculateSteeringInput(steerPosition);
        SetYrotationFrontWheels();

        slideDirection = (CalculateSlideVector(wheels));
        steeringInput.SetWheelForce(-Mathf.RoundToInt(slideDirection));
    }


    private float ReadAccelerationInput(UserInputType inputType)
    {
        switch (inputType)
        {
            case UserInputType.Wheels:
                return 1f - acceleration.ReadValue<float>();
            default:
                return acceleration.ReadValue<float>();
        }
    }

    private float ReadSteeringInput(UserInputType inputType)
    {
        switch (inputType)
        {
            case UserInputType.Wheels:
                return -.5f + steering.ReadValue<float>();
            default:
                return steering.ReadValue<float>();
        }
    }

    private float ReadBrakeInput(UserInputType inputType)
    {
        switch (inputType)
        {
            case UserInputType.Wheels:
                return 1f - braking.ReadValue<float>();
            default:
                return braking.ReadValue<float>();
        }
    }

    private void SetUserInterface(float accelerationInput, float brakeInput)
    {

        userInterface.acceleration = accelerationInput;
        userInterface.brake = brakeInput;
    }
    private void CalculateSteeringInput(float steerPosition)
    {
        steeringWheel.transform.localEulerAngles = new Vector3(14.289f, 0, -steerPosition * (float)wheelInputAngle);
        float steerForce = Mathf.Clamp(steerPosition * 2f, -steeringRatio, steeringRatio);

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
    private void SetYrotationFrontWheels()
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

        switch (userInputType)
        {
            case UserInputType.Wheels:
                vehicleInputActions.SteeringWheel.ShiftUP.started -= engine.ShiftUp;
                vehicleInputActions.SteeringWheel.ShiftUP.Disable();
                vehicleInputActions.SteeringWheel.ShiftDOWN.started -= engine.ShiftDown;
                vehicleInputActions.SteeringWheel.ShiftDOWN.Disable();
                break;
            default:
                vehicleInputActions.Default.ShiftUP.started -= engine.ShiftUp;
                vehicleInputActions.Default.ShiftUP.Disable();
                vehicleInputActions.Default.ShiftDOWN.started -= engine.ShiftDown;
                vehicleInputActions.Default.ShiftDOWN.Disable();
                break;
        }
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

public enum UserInputType
{
    Keyboard = 0,
    Controller,
    Wheels
}


