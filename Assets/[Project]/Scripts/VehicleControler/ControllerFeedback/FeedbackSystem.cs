using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;

public class FeedbackSystem
{
    public static FeedbackSystem instance 
    { 
        get 
        { 
            if (Instance != null) 
            {
                return Instance;
            }
            else
            {
                Debug.LogError("NO FEEDBACK SYSTEM CREATED | check feedback system for PlayerInput");
                return null;
            }
        } 
    }
    private static FeedbackSystem Instance;

    private List<FeedbackComponent> feedbackComponents = new List<FeedbackComponent>();

    private PlayerInput _playerInput;
    public Gamepad activeGamepad;


    public FeedbackSystem(PlayerInput playerInput)
    {
        _playerInput = playerInput;
        activeGamepad = GetActiveGamepad();
        Instance = this;
    }

    public bool RegisterFeedbackComponent(FeedbackComponent component)
    {
        if (!feedbackComponents.Contains(component))
            feedbackComponents.Add(component);
        return feedbackComponents.Contains(component);
    }

    public void GamepadFeedbackLoop()
    {
        float highFrequency = 0f;
        float lowFrequency = 0f;
        foreach (FeedbackComponent component in feedbackComponents)
        {
            highFrequency += component.highFrequency;
            highFrequency = Mathf.Clamp(highFrequency, 0f, 1f);

            lowFrequency += component.lowFrequency;
            lowFrequency = Mathf.Clamp(lowFrequency, 0f, 1f);
        }

        if(activeGamepad != null)
            activeGamepad.SetMotorSpeeds(lowFrequency, highFrequency);
        else
            activeGamepad = GetActiveGamepad();
    }

    private Gamepad GetActiveGamepad()
    {
        return Gamepad.all.FirstOrDefault(g => _playerInput.devices.Any(d => d.deviceId == g.deviceId));
    }
}

public class FeedbackComponent
{
    readonly private float intensity;
    readonly public string sourceName;
    public float highFrequency;
    public float lowFrequency;

    public FeedbackComponent(string sourceName, float intensity)
    {
        this.sourceName = sourceName;
        this.intensity = intensity;
    }

    /// <summary>
    /// Set the Right motor speed of controller | i = 0 - 1
    /// </summary>
    /// <param name="i">0f - 1f</param>
    public void UpdateHighFrequencyRumble(float i)
    {
        i = float.IsNaN(i) ? 0 : i;
        i = float.IsInfinity(i) ? intensity : i;
        highFrequency = Mathf.Clamp(i, 0f, intensity);
    }

    /// <summary>
    /// Set the Left motor speed of controller | i = 0 - 1
    /// </summary>
    /// <param name="i">0f - 1f</param>
    public void UpdateLowFrequencyRumble(float i)
    {
        i = float.IsNaN(i) ? 0 : i;
        i = float.IsInfinity(i) ? intensity : i;
        lowFrequency = Mathf.Clamp(i, 0f, intensity);
    }

}
