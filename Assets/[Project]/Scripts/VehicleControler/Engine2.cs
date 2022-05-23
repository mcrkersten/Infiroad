using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class Engine2
{
    private int currentSelectedGear;
    private int currentEngagedGear;

    public int maxRPM;
    public AnimationCurve engineTorqueProfile;
    public AnimationCurve engineBrakeProfile;
    public AnimationCurve throttleProfile;
    public AnimationCurve AntiStallProfile;

    public Dashboard dashboard;
    private float lastRPM = 0f;

    public float[] gearRatios =  { 2.36f, 1.88f, 1.5f, 1.19f, .97f };
    private float finalDriveRatio = 6.49f;

    public List<Wheel_Raycast> driveWheels = new List<Wheel_Raycast>();
    private float driveWheelRadiusInMeter;
    private float rollingCircumference;

    [SerializeField] private float ShiftTime;
    private float shiftTime;
    private bool isShifting;

    public float physicsImpact;

    [Header("Audio and feedback")]
    public AK.Wwise.RTPC engineRPMAudio;
    public AK.Wwise.RTPC velocityAudio;
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
        velocityAudio.SetGlobalValue(0);
        engineRPMAudio.SetGlobalValue(0);
        throttleAudio.SetGlobalValue(0);
        GearAudio.SetGlobalValue(1);

        //Rumble
        feedbackComponent = new FeedbackComponent("Engine", 5f);
        FeedbackSystem.instance.RegisterFeedbackComponent(feedbackComponent);
    }

    public float Run(float currentForwardVelocity, float throttle, float clutch, float brake, float physicsWobble, float wheelSlip)
    {
        clutch = SemiAutomaticClutch(clutch);
        clutch *= AntiStall(brake);

        float mechanicalForce = CalculateMechanicalFriction(currentForwardVelocity, throttle, clutch);

        float effectiveGearRatio = gearRatios[currentEngagedGear] * finalDriveRatio;
        float wheelRPM = (currentForwardVelocity / (rollingCircumference / 60f)) * wheelSlip;
        float enginewRPM = ((wheelRPM * effectiveGearRatio) - (physicsWobble * physicsImpact));
        float velocity = (enginewRPM * clutch) + (Mathf.Lerp(3500f, maxRPM, throttle ) * (1f - clutch)) - lastRPM;
        lastRPM = Mathf.Lerp(lastRPM + velocity, enginewRPM, clutch);
        float wheelTorque = (mechanicalForce + (CalculateEngineTorque(lastRPM) * throttle * effectiveGearRatio)) * clutch;
        float forceToApply = wheelTorque / driveWheelRadiusInMeter;


        engineRPMAudio.SetGlobalValue(enginewRPM);
        velocityAudio.SetGlobalValue(velocity);

        dashboard.UpdateMeter(lastRPM / maxRPM, DashboardMeter.MeterType.Tacheometer);
        dashboard.UpdateMeter(currentForwardVelocity * 3.6f, DashboardMeter.MeterType.Speedometer);

        feedbackComponent.UpdateHighFrequencyRumble( RPM_feedbackCurve.Evaluate(lastRPM / maxRPM));
        feedbackComponent.UpdateLowFrequencyRumble(velocity/100f);

        throttleAudio.SetGlobalValue(throttle);
        return forceToApply;
    }

    private float AntiStall(float brake)
    {
        return 1f - AntiStallProfile.Evaluate(brake);
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
                currentEngagedGear = currentSelectedGear;
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
        float effectiveGearRatio = gearRatios[currentSelectedGear] * finalDriveRatio;
        float wheelRPM = currentForwardSpeed / (rollingCircumference / 60f);
        float currentEngineRPM = (wheelRPM * effectiveGearRatio);

        float idealEngineRPM = (3500f) * (1f - throttle);
        if(currentEngineRPM < idealEngineRPM)
            return idealEngineRPM == 0f ? 0f : CalculateEngineTorque(idealEngineRPM);
        return CalculateEngineBrakeTorque(idealEngineRPM, currentEngineRPM) * clutch;
    }

    private float CalculateEngineTorque(float rpm)
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
        if (!isShifting)
        {
            StartAutomaticShift();
            currentSelectedGear = Mathf.Min(gearRatios.Length - 1, currentSelectedGear + 1);
        }
    }

    public void ShiftDown(InputAction.CallbackContext obj)
    {
        if (!isShifting)
        {
            StartAutomaticShift();
            currentSelectedGear = Mathf.Max(0, currentSelectedGear - 1);
        }
    }

    private void StartAutomaticShift()
    {
        isShifting = true;
        shiftTime = ShiftTime;
    }

}
