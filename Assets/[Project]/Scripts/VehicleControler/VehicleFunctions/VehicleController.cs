using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class VehicleController : MonoBehaviour
{
    public bool useBindingManager;

    [HideInInspector] public float distanceTraveled = 0f;
    private bool physicsLocked;
    public PlayerInput playerInput;
    public bool useWheel;
    public float steeringRatio;
    private SteeringWheelInput steeringInput;

    [SerializeField] MinimapBehaviour minimapBehaviour;
    [SerializeField] PauseMenuBehaviour pauseMenuBehaviour;

    public VehicleInputActions vehicleInputActions;
    private InputAction steering;
    private InputAction braking;
    private InputAction acceleration;
    private InputAction clutch;
    [SerializeField] private RadioSystem radio;

    [Header("Vehicle configuration")]
    [Range(0f,1f)] public float brakeBias;
    public Engine2 engine;
    private float engineForce;

    [Header("Vehicle physics")]
    [Range (0f,1f)] public float dragReduction;
    public float donwforceincreasement;

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
    public float steeringStrenght;

    public VehicleUserInterfaceData userInterface;
    private FeedbackSystem feedbackSystem;

    [Header("Reset system")]
    [SerializeField] private ResetScreen resetScreen;
    private Transform resetPosition;
    private Vector3 lastFramePosition = new Vector3();
    private InputType userInputType = 0;

    public int wheelInputAngle;
    [SerializeField] AnimationCurve gamepadInputWeakener;
    public List<VegetationAssetScanner> vegetationAssetScanners = new List<VegetationAssetScanner>();

    void Awake()
    {
        if (useBindingManager && BindingManager.Instance != null)
            vehicleInputActions = BindingManager.Instance.vehicleInputActions;

        rb = this.GetComponent<Rigidbody>();
        feedbackSystem = new FeedbackSystem(playerInput);
        ResetScreen.resetVehicle += ResetVehicle;
        EventTriggerManager.resetPoint += NewResetPoint;

        Input.ResetInputAxes();
        steeringInput = new SteeringWheelInput();
        steeringInput.Init();

        rb.centerOfMass = centerOfMass.localPosition;

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
        if (useBindingManager && BindingManager.Instance != null)
        {
            switch (BindingManager.Instance.selectedInputType)
            {
                case InputType.Wheel:
                    ActivateWheelControls();
                    break;
                case InputType.Keyboard:
                    ActivateKeyboardControls();
                    break;
                case InputType.Gamepad:
                    ActivateGamepadControls();
                    break;
            };
            steering.Enable();
            braking.Enable();
            acceleration.Enable();
            clutch.Enable();
            userInputType = BindingManager.Instance.selectedInputType;
        }
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

        vehicleInputActions.SteeringWheel.MinimapRescale.Enable();
        vehicleInputActions.SteeringWheel.MinimapRescale.started += minimapBehaviour.ScaleMinimap;
        vehicleInputActions.SteeringWheel.PauseGame.Enable();
        vehicleInputActions.SteeringWheel.PauseGame.started += pauseMenuBehaviour.OnStartMenu;

        vehicleInputActions.SteeringWheel.PauseRadio.Enable();
        vehicleInputActions.SteeringWheel.PauseRadio.started += radio.PauseSong;
        vehicleInputActions.SteeringWheel.NextRadioSong.Enable();
        vehicleInputActions.SteeringWheel.NextRadioSong.started += radio.NextSong;
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

        vehicleInputActions.Keyboard.MinimapRescale.Enable();
        vehicleInputActions.Keyboard.MinimapRescale.started += minimapBehaviour.ScaleMinimap;
        vehicleInputActions.Keyboard.PauseGame.Enable();
        vehicleInputActions.Keyboard.PauseGame.started += pauseMenuBehaviour.OnStartMenu;

        vehicleInputActions.Keyboard.PauseRadio.Enable();
        vehicleInputActions.Keyboard.PauseRadio.started += radio.PauseSong;
        vehicleInputActions.Keyboard.NextRadioSong.Enable();
        vehicleInputActions.Keyboard.NextRadioSong.started += radio.NextSong;
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

        vehicleInputActions.Gamepad.MinimapRescale.Enable();
        vehicleInputActions.Gamepad.MinimapRescale.started += minimapBehaviour.ScaleMinimap;
        vehicleInputActions.Gamepad.PauseGame.Enable();
        vehicleInputActions.Gamepad.PauseGame.started += pauseMenuBehaviour.OnStartMenu;

        vehicleInputActions.Gamepad.PauseRadio.Enable();
        vehicleInputActions.Gamepad.PauseRadio.started += radio.PauseSong;
        vehicleInputActions.Gamepad.NextRadioSong.Enable();
        vehicleInputActions.Gamepad.NextRadioSong.started += radio.NextSong;
    }
    // Update is called once per frame
    void Update()
    {
        LogitechGSDK.LogiUpdate();
        Debug.DrawLine(centerOfMass.position, centerOfMass.transform.position + rb.velocity.normalized, Color.red);

        if(userInputType == InputType.Gamepad)
            feedbackSystem.GamepadFeedbackLoop();
    }

    private void FixedUpdate()
    {
        distanceTraveled += Vector3.Distance(transform.position, lastFramePosition);
        lastFramePosition = transform.position;
        foreach (DownForceWing wing in downforceWing)
        {
            float downforce = wing.CalculateLiftforce(29.92f);
            Vector3 downForceVector = (downforce + donwforceincreasement) * -wing.transform.up;
            Vector3 dragForceVector = (downforce * dragReduction) * -wing.transform.forward;
            rb.AddForceAtPosition(downForceVector, wing.transform.position);
            rb.AddForceAtPosition(dragForceVector, wing.transform.position);
            Debug.DrawLine(wing.transform.position, wing.transform.position + downForceVector, Color.green);
        }
        rb.AddForce(CalculateAirResistance());

        float accelerationInput = ReadAccelerationInput(userInputType);
        accelerationInput = SmoothInput(accelerationInput, .25f);
        float clutchInput = ReadClutchInput(userInputType);
        float brakeInput = ReadBrakeInput(userInputType);
        float steerInput = ReadSteeringInput(userInputType);

        if(BindingManager.Instance.selectedInputType != InputType.Wheel)
            steerInput = steerInput * gamepadInputWeakener.Evaluate(Mathf.Abs(steerInput));

        float wheelSlip  = 0f;
        float physicsWobble = ApplyForceToWheels(brakeInput, out wheelSlip);

        float localForwardVelocity = transform.InverseTransformDirection(rb.velocity).z;
        engineForce = engine.Run(localForwardVelocity, accelerationInput, clutchInput, brakeInput, physicsWobble, wheelSlip);

        SetUserInterface(accelerationInput, brakeInput);

        float fibrationCompensation = localForwardVelocity * Time.deltaTime;
        float playerSteeringForce = CalculateSteeringInputForce(steerInput, userInputType, fibrationCompensation);
        ApplySteeringDirection(playerSteeringForce);
        SetSteeringWheelModelRotation(-playerSteeringForce);
        SetSteeringFrontWheels();

        radio.SetRadioQuality((Mathf.Clamp(rb.velocity.magnitude * 3f, 0f, 200f) / 200f) * 100f);
    }

    private void SetSteeringWheelModelRotation(float rotation)
    {
        switch (wheelModelRotation)
        {
            case WheelModelRotation.X:
                steeringWheel.transform.localEulerAngles = new Vector3(rotation, steeringWheel.transform.localEulerAngles.y, steeringWheel.transform.localEulerAngles.z);
                break;
            case WheelModelRotation.Y:
                steeringWheel.transform.localEulerAngles = new Vector3(steeringWheel.transform.localEulerAngles.x, rotation, steeringWheel.transform.localEulerAngles.z);
                break;
            case WheelModelRotation.Z:
                steeringWheel.transform.localEulerAngles = new Vector3(steeringWheel.transform.localEulerAngles.x, steeringWheel.transform.localEulerAngles.y, .00001f + (rotation * (float)wheelInputAngle));
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
    private float ReadAccelerationInput(InputType inputType)
    {
        if (!useBindingManager)
            return 0f;
        switch (inputType)
        {
            case InputType.Wheel:
                return 1f - acceleration.ReadValue<float>();
            default:
                return acceleration.ReadValue<float>();
        }
    }

    private float ReadSteeringInput(InputType inputType)
    {
        if (!useBindingManager)
            return 0f;
        switch (inputType)
        {
            case InputType.Wheel:
                return Mathf.Lerp(-1f, 1f, steering.ReadValue<float>());
            case InputType.Gamepad:
                return steering.ReadValue<Vector2>().x;
            default:
                return steering.ReadValue<float>();
        }
    }

    private float ReadBrakeInput(InputType inputType)
    {
        if (!useBindingManager)
            return 0f;
        switch (inputType)
        {
            case InputType.Wheel:
                return 1f - braking.ReadValue<float>();
            default:
                return braking.ReadValue<float>();
        }
    }

    private float ReadClutchInput(InputType inputType)
    {
        if (!useBindingManager)
            return 0f;
        switch (inputType)
        {
            case InputType.Wheel:
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

    private float gamePadSteering = 0f;
    private float CalculateSteeringInputForce(float steerInput, InputType inputType, float power)
    {
        steerWeight = CalculateSteerWeight() / steeringStrenght;
        switch (inputType)
        {
            case InputType.Keyboard:
                gamePadSteering = Mathf.Lerp(gamePadSteering, steerInput - (steerWeight), Time.fixedDeltaTime);
                return gamePadSteering;
            case InputType.Gamepad:
                gamePadSteering = Mathf.Lerp(gamePadSteering, steerInput - (steerWeight), Time.fixedDeltaTime);
                return gamePadSteering;
            case InputType.Wheel:
                //Steering wheel input
                steeringInput.SetInputWheelForce(Mathf.RoundToInt(steerWeight * 100f));
                return steerInput;
            default:
                return steerInput;
        }
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
                w.wheel.SetSteerAngle(ackermannAngleLeft);
            if (w.suspensionPosition == SuspensionPosition.FrontRight)
                w.wheel.SetSteerAngle(ackermannAngleRight);
        }
    }

    private float ApplyForceToWheels(float brakeInput, out float wheelSlip)
    {
        float physicsWobble = 0f;
        wheelSlip = 0f;
        List<Vector3> physics = new List<Vector3>();
        List<RaycastHit?> hits = new List<RaycastHit?>();
        foreach (Suspension s in suspensions)
        {
            RaycastHit? hit = null;
            switch (driveType)
            {
                case DriveType.rearWheelDrive:
                    if (s.suspensionPosition == SuspensionPosition.RearLeft || s.suspensionPosition == SuspensionPosition.RearRight)
                        physics.Add(s.SimulatePhysics(brakeInput, ApplyTorqueWithLimitedSlip(s, engineForce), out wheelSlip, out physicsWobble, out hit));
                    else
                    {
                        float d;
                        physics.Add(s.SimulatePhysics(brakeInput, 0, out d, out physicsWobble, out hit));
                    }
                    break;
                case DriveType.frontWheelDrive:
                    if (s.suspensionPosition == SuspensionPosition.FrontLeft || s.suspensionPosition == SuspensionPosition.FrontRight)
                        physics.Add(s.SimulatePhysics(brakeInput, ApplyTorqueWithLimitedSlip(s, engineForce), out wheelSlip, out physicsWobble, out hit));
                    else
                    {
                        float d;
                        physics.Add(s.SimulatePhysics(brakeInput, 0, out d, out physicsWobble, out hit));
                    }
                    break;
                case DriveType.allWheelDrive:
                    physics.Add(s.SimulatePhysics(brakeInput, ApplyTorqueWithLimitedSlip(s, engineForce), out wheelSlip, out physicsWobble, out hit));
                    break;
                default:
                    break;
            }
            hits.Add(hit);
        }


        int i = 0;
        foreach (Suspension s in suspensions)
        {
            if(hits[i] != null)
                rb.AddForceAtPosition(physics[i], Vector3.Lerp(s.transform.position, hits[i].Value.point, 1f));
            i++;
        }
        return physicsWobble / suspensions.Count;
    }

    private float maxSpeedDifference = 50f; // Max speed difference for limited-slip effect
    private float limitedSlipCoefficient = 0.8f; // Coefficient for limited-slip effect
    private float ApplyTorqueWithLimitedSlip(Suspension suspension, float torque)
    {
        // Calculate slip ratio
        float slipRatio = suspension.wheel.RPM * 2 * Mathf.PI * suspension.wheel.wheelRadius / suspension.wheel.wheelVelocityLocalSpace.magnitude;

        // Apply limited-slip effect
        float limitedSlipEffect = 1f - Mathf.Clamp01(Mathf.Abs(slipRatio) / maxSpeedDifference);
        float limitedSlipTorque = limitedSlipCoefficient * limitedSlipEffect * torque;

        // Apply torque to the wheel
        return torque - limitedSlipTorque;
    }

    private void OnDisable()
    {
        ResetScreen.resetVehicle -= ResetVehicle;
        EventTriggerManager.resetPoint -= NewResetPoint;
        steering?.Disable();
        braking?.Disable();
        acceleration?.Disable();
        clutch?.Disable();

        if (vehicleInputActions == null) return;

        switch (BindingManager.Instance.selectedInputType)
        {
            case InputType.Wheel:
                vehicleInputActions.SteeringWheel.ShiftUP.started -= engine.ShiftUp;
                vehicleInputActions.SteeringWheel.ShiftUP.Disable();
                vehicleInputActions.SteeringWheel.ShiftDOWN.started -= engine.ShiftDown;
                vehicleInputActions.SteeringWheel.ShiftDOWN.Disable();

                vehicleInputActions.SteeringWheel.MinimapRescale.Disable();
                vehicleInputActions.SteeringWheel.MinimapRescale.started -= minimapBehaviour.ScaleMinimap;
                vehicleInputActions.SteeringWheel.PauseGame.Disable();
                vehicleInputActions.SteeringWheel.PauseGame.started -= pauseMenuBehaviour.OnStartMenu;

                vehicleInputActions.SteeringWheel.PauseRadio.Disable();
                vehicleInputActions.SteeringWheel.PauseRadio.started -= radio.PauseSong;
                vehicleInputActions.SteeringWheel.NextRadioSong.Disable();
                vehicleInputActions.SteeringWheel.NextRadioSong.started -= radio.NextSong;
                break;
            case InputType.Keyboard:
                vehicleInputActions.Keyboard.ShiftUP.started -= engine.ShiftUp;
                vehicleInputActions.Keyboard.ShiftUP.Disable();
                vehicleInputActions.Keyboard.ShiftDOWN.started -= engine.ShiftDown;
                vehicleInputActions.Keyboard.ShiftDOWN.Disable();

                vehicleInputActions.Keyboard.MinimapRescale.Disable();
                vehicleInputActions.Keyboard.MinimapRescale.started -= minimapBehaviour.ScaleMinimap;
                vehicleInputActions.Keyboard.PauseGame.Disable();
                vehicleInputActions.Keyboard.PauseGame.started -= pauseMenuBehaviour.OnStartMenu;

                vehicleInputActions.Keyboard.PauseRadio.Disable();
                vehicleInputActions.Keyboard.PauseRadio.started -= radio.PauseSong;
                vehicleInputActions.Keyboard.NextRadioSong.Disable();
                vehicleInputActions.Keyboard.NextRadioSong.started -= radio.NextSong;
                break;
            case InputType.Gamepad:
                vehicleInputActions.Gamepad.ShiftUP.started -= engine.ShiftUp;
                vehicleInputActions.Gamepad.ShiftUP.Disable();
                vehicleInputActions.Gamepad.ShiftDOWN.started -= engine.ShiftDown;
                vehicleInputActions.Gamepad.ShiftDOWN.Disable();

                vehicleInputActions.Gamepad.MinimapRescale.Disable();
                vehicleInputActions.Gamepad.MinimapRescale.started -= minimapBehaviour.ScaleMinimap;
                vehicleInputActions.Gamepad.PauseGame.Disable();
                vehicleInputActions.Gamepad.PauseGame.started -= pauseMenuBehaviour.OnStartMenu;

                vehicleInputActions.Gamepad.PauseRadio.Disable();
                vehicleInputActions.Gamepad.PauseRadio.started -= radio.PauseSong;
                vehicleInputActions.Gamepad.NextRadioSong.Disable();
                vehicleInputActions.Gamepad.NextRadioSong.started -= radio.NextSong;
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
        Debug.Log("Reset");
        LockPhysicsLock();
        foreach (Suspension sus in suspensions)
            sus.OnReset();
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

