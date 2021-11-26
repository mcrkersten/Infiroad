using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
[System.Serializable]
public class Engine
{
    public bool EngineOn = true;
    private int currentGear;
    public int CurrentGear { get { return currentGear; } }
    private float maxRPM;
    private float[] gearRatio;
    public AnimationCurve torqueCurve;
    private List<Wheel> driveWheels;
    public float horsePower;
    private float differentialRatio;
    private float engineDelay = .1f;

    private float lastFrameInput;

    [HideInInspector]
    public float engineRPM;
    private float idleRPM;
    [HideInInspector]
    public float wheelInputRPM;
    [HideInInspector]
    public float pullMultiplier;

    public float EnergyOutput { get { return energyOutput; } }
    private float energyOutput;
    public Engine(float[] gears, float differentialRatio, float idleRPM, float maxRPM, float horsePower, List<Wheel> driveWheels, AnimationCurve torqueCurve)
    {
        this.gearRatio = gears;
        this.idleRPM = idleRPM;
        this.maxRPM = maxRPM;
        this.horsePower = horsePower;
        this.differentialRatio = differentialRatio;
        this.torqueCurve = torqueCurve;
        this.driveWheels = driveWheels;
    }

    public void SimulateEngine(float throttle, float clutch, DriveType type)
    {
        wheelInputRPM = Mathf.Max(idleRPM, WheelRadPerSecond(type) * gearRatio[currentGear] * differentialRatio * 60 / (Mathf.PI * 2));
        float desiredEngineRPM = Mathf.Max(idleRPM, maxRPM * throttle);

        engineRPM = Mathf.Lerp(wheelInputRPM, desiredEngineRPM, engineDelay);

        pullMultiplier = Mathf.Max(0f, engineRPM - wheelInputRPM);
        energyOutput = torqueCurve.Evaluate(Mathf.Clamp(engineRPM / maxRPM, 0, 1f)) * (pullMultiplier/100f) * clutch;
    }


    public void ShiftUp(InputAction.CallbackContext obj)
    {
        if (currentGear < gearRatio.Length - 1)
        {
            Debug.Log("ShiftUp");
            currentGear++;
        }
    }

    public void ShiftDown(InputAction.CallbackContext obj)
    {
        if (currentGear > 0)
        {
            currentGear--;
        }
    }

    private float WheelRadPerSecond(DriveType type)
    {
        float rad = 0;
        foreach (Wheel w in driveWheels)
        {
            switch (type)
            {
                case DriveType.rearWheelDrive:
                    if (w.wheelType == WheelPosition.RearLeft || w.wheelType == WheelPosition.RearRight)
                        if (w.RPM > rad)
                            rad = w.RPM;
                    break;  
                case DriveType.frontWheelDrive:
                    if (w.wheelType == WheelPosition.FrontLeft || w.wheelType == WheelPosition.FrontRight)
                        if (w.RPM > rad)
                            rad = w.RPM;
                    break;
                case DriveType.allWheelDrive:
                    if (w.RPM > rad)
                        rad = w.RPM;
                    break;
            }
        }
        return rad;
    }
}