using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class Engine2
{
    public int currentGear;
    public int maxRPM;
    public AnimationCurve engineTorqueProfile;
    public AnimationCurve engineBrakeProfile;
    public AnimationCurve throttleProfile;
    public Dashboard dashboard;

    public float[] gearRatios =  { 4.23f, 2.47f, 1.77f, 1.43f, 1.15f, 0.99f };
    private float finalDriveRatio = 5.91f;

    public List<WheelRaycast> driveWheels = new List<WheelRaycast>();
    private float driveWheelRadiusInMeter;
    private float rollingCircumference;

    [SerializeField] private float ShiftTime;
    private bool isShifting;
    private float shiftTime;

    public void InitializeEngine()
    {
        driveWheelRadiusInMeter = driveWheels[0].wheelRadius;
        rollingCircumference = driveWheelRadiusInMeter * 2f * Mathf.PI;
    }

    public float Run(float currentForwardSpeed, float throttle)
    {
        if (isShifting)
        {
            shiftTime -= Time.deltaTime;
            if (shiftTime < 0)
                isShifting = false;
            return 0f;
        }
            

        float mechanicalForce = CalculateMechanicalForce(currentForwardSpeed, throttle);

        float effectiveGearRatio = gearRatios[currentGear] * finalDriveRatio;
        float wheelRPM = currentForwardSpeed / (rollingCircumference / 60f);
        float enginewRPM = wheelRPM * effectiveGearRatio;
        float engineTorque = mechanicalForce + (CalculateEnginePullTorque(enginewRPM) * throttle * effectiveGearRatio);
        float engineForce = engineTorque / driveWheelRadiusInMeter;

        dashboard.UpdateTechometerDile(enginewRPM / maxRPM);
        return engineForce;
    }

    float CalculateMechanicalForce(float currentForwardSpeed, float throttle)
    {
        float effectiveGearRatio = gearRatios[currentGear] * finalDriveRatio;
        float wheelRPM = currentForwardSpeed / (rollingCircumference / 60f);
        float currentEngineRPM = wheelRPM * effectiveGearRatio;

        float idealEngineRPM = (3500f) * (1f - throttle);
        if(currentEngineRPM < idealEngineRPM)
            return idealEngineRPM == 0f ? 0f : CalculateEnginePullTorque(idealEngineRPM);
        return CalculateEngineBrakeTorque(idealEngineRPM, currentEngineRPM);
    }

    private float CalculateEnginePullTorque(float rpm)
    {
        float time = rpm / maxRPM;
        return engineTorqueProfile.Evaluate(time);
    }

    private float CalculateEngineBrakeTorque(float rpm, float overshootRMP)
    {
        float time = rpm / maxRPM;
        float overshoot = overshootRMP / rpm;
        overshoot = Mathf.Clamp(overshoot, 0f, 10f);
        return -engineTorqueProfile.Evaluate(time) * engineBrakeProfile.Evaluate(overshoot);
    }


    public void ShiftUp(InputAction.CallbackContext obj)
    {
        StartAutomaticShift();
        currentGear = Mathf.Min(gearRatios.Length - 1, currentGear + 1);
    }

    public void ShiftDown(InputAction.CallbackContext obj)
    {
        StartAutomaticShift();
        currentGear = Mathf.Max(0, currentGear - 1);
    }

    private void StartAutomaticShift()
    {
        isShifting = true;
        shiftTime = ShiftTime;
    }

}
