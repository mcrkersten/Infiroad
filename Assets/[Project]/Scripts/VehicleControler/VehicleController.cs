using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class VehicleController : MonoBehaviour
{
    [HideInInspector] public float distanceTraveled = 0f;
    private bool physicsLocked;
    public PlayerInput playerInput;
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
    [Range(0f,1f)] public float brakeBias;
    public Engine2 engine;
    private float engineForce;

    [Space]
    public DriveType driveType;
    public Transform centerOfMass;

    private float ackermannAngleLeft;
    private float ackermannAngleRight;

    private Rigidbody rb;
    [HideInInspector] public float steerWeight;

    public List<Suspension> suspensions = new List<Suspension>();
    public List<DownForceWing> downforceWing = new List<DownForceWing>();

    [Header("Steering wheel settings")]
    public Transform steeringWheel;
    [SerializeField] private WheelModelRotation wheelModelRotation;

    [Header("Controll settings")]
    public UserInputType userInputType;
    public float steeringStrenght;

    public VehicleUserInterfaceData userInterface;
    private FeedbackSystem feedbackSystem;

    [Header("Reset system")]
    [SerializeField] private ResetScreen resetScreen;
    private Transform resetPosition;
    private Vector3 lastFramePosition = new Vector3();

    void Awake()
    {
        ResetScreen.resetVehicle += ResetVehicle;
        EventTriggerManager.resetPoint += NewResetPoint;

        Input.ResetInputAxes();
        steeringInput = new SteeringWheelInput();
        steeringInput.Init();
        feedbackSystem = new FeedbackSystem(playerInput);

        rb = this.GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass.localPosition;

        vehicleInputActions = new VehicleInputActions();
        engine.InitializeEngine();
        userInterface = new VehicleUserInterfaceData(engine, this);

        SetBrakeBias();
    }

    private void NewResetPoint(Transform transform)
    {
        resetPosition = transform;
    }

    private void SetBrakeBias()
    {
        foreach (Suspension sus in suspensions)
        {
            switch (sus.suspensionPosition)
            {
                case SuspensionPosition.FrontLeft:
                    sus.brakeBias = brakeBias;
                    break;
                case SuspensionPosition.FrontRight:
                    sus.brakeBias = brakeBias;
                    break;
                case SuspensionPosition.RearLeft:
                    sus.brakeBias = 1f - brakeBias;  
                    break;
                case SuspensionPosition.RearRight:
                    sus.brakeBias = 1f - brakeBias;
                    break;
            }
        }
    }

    private void OnEnable()
    {

        switch (userInputType)
        {
            case UserInputType.Wheels:
                ActivateWheelControls();
                break;
            case UserInputType.Keyboard:
                ActivateKeyboardControls();
                break;
            case UserInputType.Gamepad:
                ActivateGamepadControls();
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
    private void ActivateKeyboardControls()
    {
        steering = vehicleInputActions.Keyboard.Steering;
        braking = vehicleInputActions.Keyboard.Braking;
        acceleration = vehicleInputActions.Keyboard.Acceleration;
        clutch = vehicleInputActions.Keyboard.Clutch;

        vehicleInputActions.Keyboard.ShiftUP.Enable();
        vehicleInputActions.Keyboard.ShiftUP.started += engine.ShiftUp;
        vehicleInputActions.Keyboard.ShiftDOWN.Enable();
        vehicleInputActions.Keyboard.ShiftDOWN.started += engine.ShiftDown;
    }
    private void ActivateGamepadControls()
    {
        steering = vehicleInputActions.Gamepad.Steering;
        braking = vehicleInputActions.Gamepad.Braking;
        acceleration = vehicleInputActions.Gamepad.Acceleration;
        clutch = vehicleInputActions.Gamepad.Clutch;

        vehicleInputActions.Gamepad.ShiftUP.Enable();
        vehicleInputActions.Gamepad.ShiftUP.started += engine.ShiftUp;
        vehicleInputActions.Gamepad.ShiftDOWN.Enable();
        vehicleInputActions.Gamepad.ShiftDOWN.started += engine.ShiftDown;
    }
    // Update is called once per frame
    void Update()
    {
        LogitechGSDK.LogiUpdate();
        Debug.DrawLine(centerOfMass.position, centerOfMass.transform.position + rb.velocity.normalized, Color.red);

        if(userInputType == UserInputType.Gamepad)
            feedbackSystem.GamepadFeedbackLoop();
    }

    private void FixedUpdate()
    {
        distanceTraveled += Vector3.Distance(transform.position, lastFramePosition);
        lastFramePosition = transform.position;
        foreach (DownForceWing wing in downforceWing)
        {
            float downforce = wing.CalculateLiftforce(1.1455f);
            Vector3 downForceVector = downforce * -wing.transform.up;
            rb.AddForceAtPosition(downForceVector * 300f, wing.transform.position);
            Debug.DrawLine(wing.transform.position, wing.transform.position + downForceVector, Color.green);
        }
        Vector3 airResistance = CalculateAirResistance();
        rb.AddForce(airResistance);

        float accelerationInput = ReadAccelerationInput(userInputType);
        accelerationInput = SmoothInput(accelerationInput, .25f);
        float clutchInput = ReadClutchInput(userInputType);
        float brakeInput = ReadBrakeInput(userInputType);
        float steerInput = ReadSteeringInput(userInputType);

        float physicsWobble = ApplyForceToWheels(brakeInput);

        float localForwardVelocity = Vector3.Dot(rb.velocity, transform.forward);
        engineForce = engine.Run(localForwardVelocity, accelerationInput, clutchInput, physicsWobble);

        SetUserInterface(accelerationInput, brakeInput);

        steerWeight = CalculateSteerWeight();

        float fibrationCompensation = localForwardVelocity * Time.deltaTime;
        float playerSteeringForce = CalculateSteeringInputForce(steerInput, steerWeight, userInputType, fibrationCompensation);

        ApplySteeringDirection(playerSteeringForce);
        SetWheelModelRotation(-playerSteeringForce);
        SetSteeringFrontWheels();

        //Steering wheel input
        steeringInput.SetInputWheelForce(Mathf.RoundToInt(-steerWeight * 100f));
    }

    private void SetWheelModelRotation(float rotation)
    {
        switch (wheelModelRotation)
        {
            case WheelModelRotation.X:
                steeringWheel.transform.localEulerAngles = new Vector3(rotation * (float)wheelInputAngle, steeringWheel.transform.localEulerAngles.y, steeringWheel.transform.localEulerAngles.z);
                break;
            case WheelModelRotation.Y:
                steeringWheel.transform.localEulerAngles = new Vector3(steeringWheel.transform.localEulerAngles.x, rotation * (float)wheelInputAngle, steeringWheel.transform.localEulerAngles.z);
                break;
            case WheelModelRotation.Z:
                steeringWheel.transform.localEulerAngles = new Vector3(steeringWheel.transform.localEulerAngles.x, steeringWheel.transform.localEulerAngles.y, rotation * (float)wheelInputAngle);
                break;
        }
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

    #region ReadInput
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
                return Mathf.Lerp(-1f, 1f, steering.ReadValue<float>());
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

    private float ReadClutchInput(UserInputType inputType)
    {
        switch (inputType)
        {
            case UserInputType.Wheels:
                return clutch.ReadValue<float>();
            default:
                return 1f - clutch.ReadValue<float>();
        }
    }
    #endregion

    private void SetUserInterface(float accelerationInput, float brakeInput)
    {
        userInterface.acceleration = accelerationInput;
        userInterface.brake = brakeInput;
    }

    private float steeringForce = 0f;
    private float CalculateSteeringInputForce(float steerInput, float steerWeight, UserInputType inputType, float power)
    {
        float steering = steeringForce;
        if(inputType == UserInputType.Gamepad)
        {
            steering += Mathf.Lerp(0f, steerInput + (steerWeight/(1.5f)), Time.deltaTime);
            steering = Mathf.Clamp(steering, -1f, 1f);
            steeringForce = steering;
            return steeringForce;
        }
        return steerInput;
    }

    /// <summary>
    /// Apply direction 
    /// -1f = full left rotation
    /// 1f = full right rotation
    /// </summary>
    /// <param name="rotation"></param>
    private void ApplySteeringDirection(float rotation)
    {
        float steerDirection = Mathf.Clamp(rotation, -steeringRatio, steeringRatio);

        if (rotation > 0)//right
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(VehicleConstants.WHEEL_BASE / (VehicleConstants.TURN_RADIUS + (VehicleConstants.REAR_TRACK / 2))) * steerDirection;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(VehicleConstants.WHEEL_BASE / (VehicleConstants.TURN_RADIUS - (VehicleConstants.REAR_TRACK / 2))) * steerDirection;
        }
        else if (rotation < 0)//left
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(VehicleConstants.WHEEL_BASE / (VehicleConstants.TURN_RADIUS - (VehicleConstants.REAR_TRACK / 2))) * steerDirection;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(VehicleConstants.WHEEL_BASE / (VehicleConstants.TURN_RADIUS + (VehicleConstants.REAR_TRACK / 2))) * steerDirection;
        }
        else
        {
            ackermannAngleLeft = 0;
            ackermannAngleRight = 0;
        }
    }

    private void SetSteeringFrontWheels()
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
        ResetScreen.resetVehicle -= ResetVehicle;
        EventTriggerManager.resetPoint -= NewResetPoint;
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
            case UserInputType.Keyboard:
                vehicleInputActions.Keyboard.ShiftUP.started -= engine.ShiftUp;
                vehicleInputActions.Keyboard.ShiftUP.Disable();
                vehicleInputActions.Keyboard.ShiftDOWN.started -= engine.ShiftDown;
                vehicleInputActions.Keyboard.ShiftDOWN.Disable();
                break;
            case UserInputType.Gamepad:
                vehicleInputActions.Gamepad.ShiftUP.started -= engine.ShiftUp;
                vehicleInputActions.Gamepad.ShiftUP.Disable();
                vehicleInputActions.Gamepad.ShiftDOWN.started -= engine.ShiftDown;
                vehicleInputActions.Gamepad.ShiftDOWN.Disable();
                break;
        }
    }

    /// <summary>
    /// From -1f, to 1f
    /// </summary>
    /// <returns></returns>
    private float CalculateSteerWeight()
    {
        float value = 0;
        foreach (Suspension w in suspensions)
            if(w.suspensionPosition == SuspensionPosition.FrontLeft || w.suspensionPosition == SuspensionPosition.FrontRight)
                value += w.wheel.steeringWheelForce;
        return value/2f;
    }

    private void ResetVehicle()
    {
        LockPhysicsLock();
        transform.DOMove(resetPosition.position, .2f).SetEase(DG.Tweening.Ease.InOutCubic).OnComplete(UnlockPhysicsLock);
        transform.DORotate(resetPosition.eulerAngles, .2f).SetEase(DG.Tweening.Ease.InOutCubic);
    }

    public void LockPhysicsLock()
    {
        rb.isKinematic = true;
        physicsLocked = true;
    }
    public void UnlockPhysicsLock()
    {
        rb.isKinematic = false;
        physicsLocked = false;
    }

    private enum WheelModelRotation
    {
        X = 0,
        Y,
        Z,
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
    Gamepad,
    Wheels
}


