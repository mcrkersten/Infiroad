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
    public Engine2 engine;
    private float engineForce;
    public DriveType driveType;
    public Transform centerOfMass;
    public Transform steeringWheel;

    private float ackermannAngleLeft;
    private float ackermannAngleRight;
    private Rigidbody rb;
    private float slideDirection;

    public List<Suspension> suspensions = new List<Suspension>();
    public List<DownForceWing> downforceWing = new List<DownForceWing>();

    public UserInputType userInputType;
    public AnimationCurve controllerSteering;

    public VehicleUserInterfaceData userInterface;
    void Awake()
    {
        Input.ResetInputAxes();
        steeringInput = new SteeringWheelInput();
        steeringInput.Init();

        rb = this.GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass.localPosition;

        vehicleInputActions = new VehicleInputActions();
        engine.InitializeEngine();
        userInterface = new VehicleUserInterfaceData(engine, this);
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
            rb.AddForceAtPosition(downForceVector * 300f, wing.transform.position);
            Debug.DrawLine(wing.transform.position, wing.transform.position + downForceVector, Color.green);
        }

        Vector3 airResistance = CalculateAirResistance();
        rb.AddForce(airResistance);

        float clutchInput = 1f - clutch.ReadValue<float>();
        float accelerationInput = ReadAccelerationInput(userInputType);
        accelerationInput = SmoothInput(accelerationInput, .25f);
        float brakeInput = ReadBrakeInput(userInputType);
        float steerPosition = ReadSteeringInput(userInputType);

         float physicsWobble = ApplyForceToWheels(brakeInput);

        float localForwardVelocity = Vector3.Dot(rb.velocity, transform.forward);
        engineForce = engine.Run(localForwardVelocity, accelerationInput, physicsWobble);

        SetUserInterface(accelerationInput, brakeInput);

        CalculateSteeringInput(steerPosition, userInputType);
        SetYrotationFrontWheels();

        slideDirection = (CalculateSlideVector(suspensions));
        steeringInput.SetWheelForce(-Mathf.RoundToInt(slideDirection));
    }

    float lastInput;
    private float SmoothInput(float accelerationInput, float smoothSpeed)
    {
        lastInput = Mathf.Lerp(lastInput, accelerationInput, smoothSpeed);
        return lastInput;
    }

    private Vector3 CalculateAirResistance()
    {
        return Vector3.zero;
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

    private void CalculateSteeringInput(float steerPosition, UserInputType inputType)
    {
        float steering = steerPosition;
        if(inputType == UserInputType.Controller)
            steering = controllerSteering.Evaluate(steerPosition);

        steeringWheel.transform.localEulerAngles = new Vector3(steeringWheel.transform.localEulerAngles.x, steering * (float)wheelInputAngle, steeringWheel.transform.localEulerAngles.z);
        float steerForce = Mathf.Clamp(steering * 2.25f, -steeringRatio, steeringRatio);

        if (steering > 0)//right
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(VehicleConstants.WHEEL_BASE / (VehicleConstants.TURN_RADIUS + (VehicleConstants.REAR_TRACK / 2))) * steerForce;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(VehicleConstants.WHEEL_BASE / (VehicleConstants.TURN_RADIUS - (VehicleConstants.REAR_TRACK / 2))) * steerForce;
        }
        else if (steering < 0)//left
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
        foreach (Suspension w in suspensions)
        {
            if (w.suspensionPosition == SuspensionPosition.FrontLeft)
                w.wheel.steerAngle = ackermannAngleLeft;
            if (w.suspensionPosition == SuspensionPosition.FrontRight)
                w.wheel.steerAngle = ackermannAngleRight;
        }
    }

    private float ApplyForceToWheels(float brakeInput)
    {
        float physicsWobble = 0f;
        foreach (Suspension w in suspensions)
        {
            switch (driveType)
            {
                case DriveType.rearWheelDrive:
                    if (w.suspensionPosition == SuspensionPosition.RearLeft || w.suspensionPosition == SuspensionPosition.RearRight)
                        physicsWobble += w.SimulatePhysics(brakeInput, engineForce);
                    else
                        physicsWobble += w.SimulatePhysics(brakeInput, 0);
                    break;
                case DriveType.frontWheelDrive:
                    if (w.suspensionPosition == SuspensionPosition.FrontLeft || w.suspensionPosition == SuspensionPosition.FrontRight)
                        physicsWobble += w.SimulatePhysics(brakeInput, engineForce);
                    else
                        physicsWobble += w.SimulatePhysics(brakeInput, 0);
                    break;
                case DriveType.allWheelDrive:
                    physicsWobble += w.SimulatePhysics(brakeInput, engineForce);
                    break;
                default:
                    break;
            }
        }
        return physicsWobble / suspensions.Count;
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

    private float CalculateSlideVector(List<Suspension> frontSuspensions)
    {
        float value = 0;
        foreach (Suspension w in suspensions)
        {
            if(w.suspensionPosition == SuspensionPosition.FrontLeft || w.suspensionPosition == SuspensionPosition.FrontRight)
                value += w.wheel.WheelForce;
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


