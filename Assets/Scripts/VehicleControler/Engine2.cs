using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine2
{
    public int maxRPM;
    public AnimationCurve engineTorqueProfile;
    public AnimationCurve throttleProfile;

    public float[] gearRatios =  { 4.23f, 2.47f, 1.67f, 1.23f, 1.0f, 0.79f };
    private float finalDriveRatio = 3.91f;

    public List<Wheel> driveWheels = new List<Wheel>();
    private float driveWheelRadiusInMeter;
    private float rollingCircumference;
    private float vehicleMass;

    public void InitializeEngine(float vehicleMass)
    {
        this.vehicleMass = vehicleMass;
        driveWheelRadiusInMeter = driveWheels[0].wheelRadius;
        rollingCircumference = driveWheelRadiusInMeter * 2f * Mathf.PI;
    }

    public float Run(float currentForwardSpeed, int currentGear, float throttle)
    {
        Wheel main = driveWheels[0];
        float wheelRPM = currentForwardSpeed / (rollingCircumference / 60f);
        float engineRPM = wheelRPM * (gearRatios[currentGear] * finalDriveRatio);
        float engineTorque = CalculateEngineTorque(engineRPM);
        float engineForce = engineTorque / driveWheelRadiusInMeter;
        float acceleration = engineForce / vehicleMass;
        return acceleration;
    }

    private float CalculateEngineTorque(float engineRPM)
    {
        float time = engineRPM / maxRPM;
        return engineTorqueProfile.Evaluate(time) * maxRPM;
    }
}
