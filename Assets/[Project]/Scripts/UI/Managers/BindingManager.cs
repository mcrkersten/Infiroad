using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.InputSystem;

public class BindingManager : MonoBehaviour
{
    public static BindingManager Instance { get { return instance; } }
    private static BindingManager instance;

    public VehicleInputActions vehicleInputActions;

    public InputType selectedInputType;

    private void Start()
    {
        vehicleInputActions = new VehicleInputActions();
        instance = this;
        DontDestroyOnLoad(this.transform);
    }

    public void SaveActionmap(InputType inputType, VehicleInputActions inputAction)
    {
        switch (inputType)
        {
            case InputType.Keyboard:
                var keyboard = ((InputActionMap)inputAction.Keyboard).SaveBindingOverridesAsJson();
                PlayerPrefs.SetString(inputType.ToString(), keyboard);
                break;
            case InputType.Gamepad:
                var gamepad = ((InputActionMap)inputAction.Gamepad).SaveBindingOverridesAsJson();
                PlayerPrefs.SetString(inputType.ToString(), gamepad); break;
            case InputType.Wheel:
                var wheel = ((InputActionMap)inputAction.SteeringWheel).SaveBindingOverridesAsJson();
                PlayerPrefs.SetString(inputType.ToString(), wheel); break;
        }
    }

    public void LoadInputActionmap(InputType inputType, VehicleInputActions inputAction)
    {
        switch (inputType)
        {
            case InputType.Keyboard:
                var keyboard = PlayerPrefs.GetString(inputType.ToString());
                ((InputActionMap)inputAction.Keyboard).LoadBindingOverridesFromJson(keyboard);
                break;
            case InputType.Gamepad:
                var gamepad = PlayerPrefs.GetString(inputType.ToString());
                ((InputActionMap)inputAction.Keyboard).LoadBindingOverridesFromJson(gamepad);
                break;
            case InputType.Wheel:
                var wheel = PlayerPrefs.GetString(inputType.ToString());
                ((InputActionMap)inputAction.Keyboard).LoadBindingOverridesFromJson(wheel);
                break;
        }
    }
}
