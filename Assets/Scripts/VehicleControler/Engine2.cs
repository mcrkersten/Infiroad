using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class Engine2
{
    public int currentGear;
    public int maxRPM;
    public AnimationCurve engineTorqueProfile;
    public AnimationCurve throttleProfile;

    public float[] gearRatios =  { 4.23f, 2.47f, 1.67f, 1.23f, 1.0f, 0.79f };
    private float finalDriveRatio = 3.91f;

    public List<WheelRaycast> driveWheels = new List<WheelRaycast>();
    private float driveWheelRadiusInMeter;
    private float rollingCircumference;
    private float vehicleMass;

    public void InitializeEngine(float vehicleMass)
    {
        this.vehicleMass = vehicleMass;
        driveWheelRadiusInMeter = driveWheels[0].wheelRadius;
        rollingCircumference = driveWheelRadiusInMeter * 2f * Mathf.PI;
    }

    public float Run(float currentForwardSpeed, float throttle)
    {
        float effectiveGearRatio = gearRatios[currentGear] * finalDriveRatio;
        float wheelRPM = currentForwardSpeed / (rollingCircumference / 60f);
        float rpm = wheelRPM * effectiveGearRatio;
        float engineTorque = CalculateEngineTorque(rpm) * throttle * effectiveGearRatio;
        float engineForce = engineTorque / driveWheelRadiusInMeter;
        float acceleration = engineForce / vehicleMass;
        Debug.Log(rpm);

        return engineForce;
    }

    private float CalculateEngineTorque(float rpm)
    {
        float time = rpm / maxRPM;
        return engineTorqueProfile.Evaluate(time);
    }

    public void ShiftUp(InputAction.CallbackContext obj)
    {
        currentGear = Mathf.Min(gearRatios.Length - 1, currentGear + 1);
    }

    public void ShiftDown(InputAction.CallbackContext obj)
    {
        currentGear = Mathf.Max(0, currentGear - 1);

    }
}
