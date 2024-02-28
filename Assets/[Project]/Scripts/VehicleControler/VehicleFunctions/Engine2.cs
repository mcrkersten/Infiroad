using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class Engine2
{
    private int currentSelectedGear = 0;

    public int maxRPM;
    public AnimationCurve engineTorqueProfile;
    public AnimationCurve engineBrakeProfile;
    public AnimationCurve throttleProfile;
    public AnimationCurve AntiStallProfile;

    public Dashboard dashboard;
    public float lastRPM = 0f;

    public float[] gearRatios;
    public float finalDriveRatio = 6.49f;

    public List<Wheel_Raycast> driveWheels = new List<Wheel_Raycast>();
    private float driveWheelRadiusInMeter;
    private float rollingCircumference;

    [SerializeField] private float ShiftTime;
    private float shiftTime;
    private bool isShifting;

    public float physicsImpact;

    [Header("Audio and feedback")]
    public AK.Wwise.RTPC engineRPMAudio;
    public AK.Wwise.RTPC windAudio;
    public AK.Wwise.RTPC throttleAudio;
    public AK.Wwise.RTPC GearAudio;

    private FeedbackComponent feedbackComponent;
    public AnimationCurve RPM_feedbackCurve;
    public void InitializeEngine()
    {
        //General settings
        driveWheelRadiusInMeter = driveWheels[0].wheelRadius;
        rollingCircumference = driveWheelRadiusInMeter * 2f * Mathf.PI;

        //Audio
        windAudio.SetGlobalValue(0);
        engineRPMAudio.SetGlobalValue(0);
        throttleAudio.SetGlobalValue(0);
        GearAudio.SetGlobalValue(1);

        //Rumble
        feedbackComponent = new FeedbackComponent("Engine", 5f);
        FeedbackSystem.instance.RegisterFeedbackComponent(feedbackComponent);
    }

    public float Run(float currentForwardVelocity, float throttle, float clutch, float brake, float physicsWobble, float wheelSpin)
    {
        clutch = SemiAutomaticClutch(clutch);
        clutch *= AntiStall(brake);

        // Calculate mechanical force
        float mechanicalForce = CalculateMechanicalFriction(currentForwardVelocity, throttle, clutch);
        mechanicalForce = 0;
        // Calculate effective gear ratio and wheel RPM
        float effectiveGearRatio = GetCurrentGearRatio();
        float wheelRPM = (currentForwardVelocity / (rollingCircumference / 60f)) * wheelSpin;

        // Calculate engine RPM and velocity
        float enginewRPM = (wheelRPM * effectiveGearRatio) - (physicsWobble * physicsImpact);
        float deltaRPM = (enginewRPM * clutch) + (Mathf.Lerp(1000f, maxRPM, throttle) * (1f - clutch)) - lastRPM;
        lastRPM = Mathf.Lerp(lastRPM + deltaRPM, enginewRPM, clutch);
        lastRPM = Mathf.Clamp(lastRPM, 0, maxRPM);
        float velocityRPM = lastRPM - enginewRPM;
        lastRPM += velocityRPM;

        if(float.IsNaN(velocityRPM))
        { 
            lastRPM = 1f;
            return 0f; 
        }

        // Calculate wheel torque and force to apply
        float engineTorque = CalculateEngineTorque(lastRPM);
        float throttleTorque = engineTorque * throttle * effectiveGearRatio;
        float wheelTorque = (mechanicalForce + throttleTorque) * clutch;
        float forceToApply = wheelTorque / driveWheelRadiusInMeter;

        engineRPMAudio.SetGlobalValue((lastRPM / maxRPM) * 10000);
        windAudio.SetGlobalValue(currentForwardVelocity);

        dashboard.UpdateMeter(lastRPM / maxRPM, DashboardMeter.MeterType.Tacheometer);
        dashboard.UpdateMeter(currentForwardVelocity * 3.6f, DashboardMeter.MeterType.Speedometer);

        feedbackComponent.UpdateHighFrequencyRumble( RPM_feedbackCurve.Evaluate(lastRPM / maxRPM));
        feedbackComponent.UpdateLowFrequencyRumble(velocityRPM/100f);

        throttleAudio.SetGlobalValue(throttle);
        return forceToApply;
    }

    private float GetCurrentGearRatio()
    {
        if (currentSelectedGear == -1)
            return 0f; // neutral gear, output 0 gear ratio
        else if (currentSelectedGear >= 0 && currentSelectedGear < gearRatios.Length)
            return gearRatios[currentSelectedGear] * finalDriveRatio;
        else
        {
            Debug.LogError("Invalid gear selected: " + currentSelectedGear);
            return 0f;
        }
    }

    private float AntiStall(float brake)
    {
        return 1f;// - AntiStallProfile.Evaluate(brake);
    }

    private float SemiAutomaticClutch(float clutch)
    {
        if (isShifting)
        {
            Mathf.Clamp(clutch = shiftTime / ShiftTime, 0f, 1f);
            shiftTime -= Time.deltaTime / 2f;
            if (shiftTime < 0)
            {
                isShifting = false;
                //GEAR CHANGE NEEDS TO HAPPEN WHEN CLUTCH DISENGAGED
                GearAudio.SetGlobalValue(currentSelectedGear);
            }
        }
        else
        {
            if (shiftTime < ShiftTime)
            {
                Mathf.Clamp(clutch = shiftTime / ShiftTime, 0f, 1f);
                shiftTime += Time.deltaTime / 2f;
            }
        }
        return clutch;
    }

    float CalculateMechanicalFriction(float currentForwardSpeed, float throttle, float clutch)
    {
        if (currentSelectedGear == -1)
            return 0f; // neutral gear, output 0 gear ratio

        float effectiveGearRatio = gearRatios[currentSelectedGear] * finalDriveRatio;
        float wheelRPM = currentForwardSpeed / (rollingCircumference / 60f);
        float currentEngineRPM = (wheelRPM * effectiveGearRatio);

        float idealEngineRPM = (3500f) * (1f - throttle);
        if(currentEngineRPM < idealEngineRPM)
            return idealEngineRPM == 0f ? 0f : CalculateEngineTorque(idealEngineRPM);
        return CalculateEngineBrakeForce(idealEngineRPM, throttle) * clutch;
    }

    private float CalculateEngineTorque(float rpm)
    {
        float time = rpm / maxRPM;
        return engineTorqueProfile.Evaluate(time);
    }

    private float CalculateEngineBrakeForce(float currentRPM, float throttle)
    {
        float normalizedRPM = currentRPM / maxRPM; // Normalize the current RPM
        float brakeForce = -engineBrakeProfile.Evaluate(normalizedRPM); // Evaluate the engine brake curve
    
        // Apply throttle modulation
        brakeForce *= Mathf.Lerp(1f, 0f, throttle);
    
        return brakeForce;
    }


    public void ShiftUp(InputAction.CallbackContext obj)
    {
        if (!isShifting)
        {
            StartSemiAutomaticShift();
            currentSelectedGear++;
            currentSelectedGear = Mathf.Clamp(currentSelectedGear, -1, gearRatios.Length - 1);
            dashboard.UpdateGear(currentSelectedGear + 1, DashboardMeter.MeterType.Tacheometer);
        }
    }

    public void ShiftDown(InputAction.CallbackContext obj)
    {
        if (!isShifting)
        {
            StartSemiAutomaticShift();
            currentSelectedGear--;
            currentSelectedGear = Mathf.Clamp(currentSelectedGear, -1, gearRatios.Length - 1);
            dashboard.UpdateGear(currentSelectedGear + 1, DashboardMeter.MeterType.Tacheometer);
        }
    }

    private void StartSemiAutomaticShift()
    {
        isShifting = true;
        shiftTime = ShiftTime;
    }

}
