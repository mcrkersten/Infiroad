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

    public BindingMenuLogic bindingMenuLogic;

    public VehicleInputActions vehicleInputActions;
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    public InputType selectedInputType;
    public InputActionMap currentSelectedInputActionMap;

    private int axisSkip = 0;

    private void Start()
    {
        vehicleInputActions = new VehicleInputActions();
        instance = this;
        DontDestroyOnLoad(this.transform);
    }

    public void SaveActionmap(VehicleInputActions inputAction)
    {
        switch (selectedInputType)
        {
            case InputType.Keyboard:
                var keyboard = ((InputActionMap)inputAction.Keyboard).SaveBindingOverridesAsJson();
                PlayerPrefs.SetString(selectedInputType.ToString(), keyboard);
                break;
            case InputType.Gamepad:
                var gamepad = ((InputActionMap)inputAction.Gamepad).SaveBindingOverridesAsJson();
                PlayerPrefs.SetString(selectedInputType.ToString(), gamepad); 
                break;
            case InputType.Wheel:
                var wheel = ((InputActionMap)inputAction.SteeringWheel).SaveBindingOverridesAsJson();
                PlayerPrefs.SetString(selectedInputType.ToString(), wheel); 
                break;
        }
    }

    public bool LoadActionmap(VehicleInputActions inputAction)
    {
        switch (selectedInputType)
        {
            case InputType.Keyboard:
                string keyboard = PlayerPrefs.GetString(selectedInputType.ToString());
                if (keyboard == "") return false;
                ((InputActionMap)inputAction.Keyboard).LoadBindingOverridesFromJson(keyboard);
                break;
            case InputType.Gamepad:
                string gamepad = PlayerPrefs.GetString(selectedInputType.ToString());
                if (gamepad == "") return false;
                ((InputActionMap)inputAction.Gamepad).LoadBindingOverridesFromJson(gamepad);
                break;
            case InputType.Wheel:
                string wheel = PlayerPrefs.GetString(selectedInputType.ToString());
                if (wheel == "") return false;
                ((InputActionMap)inputAction.SteeringWheel).LoadBindingOverridesFromJson(wheel);
                break;
        }
        return true;
    }

    public bool ResetToDefaultActionmap(VehicleInputActions inputAction)
    {
        switch (selectedInputType)
        {
            case InputType.Keyboard:
                ((InputActionMap)inputAction.Keyboard).RemoveAllBindingOverrides();
                break;
            case InputType.Gamepad:
                ((InputActionMap)inputAction.Gamepad).RemoveAllBindingOverrides();
                break;
            case InputType.Wheel:
                ((InputActionMap)inputAction.SteeringWheel).RemoveAllBindingOverrides();
                break;
        }
        PlayerPrefs.SetString(selectedInputType.ToString(), "");
        return true;
    }

    public void RemapButton(BindingButton button)
    {
        button.inputAction.Disable();
        rebindingOperation = button.inputAction.PerformInteractiveRebinding()
                    // To avoid accidental input from mouse motion
                    .WithControlsExcluding("Mouse")
                    .WithControlsExcluding("<Keyboard>/printScreen")
                    .OnMatchWaitForAnother(0.1f)
                    .OnComplete(operation => OnRemapButtonComplete(button))
                    .Start();

    }

    public void RemapAxis(BindingButton button, bool isPositive)
    {
        var newAction = new InputAction(binding: "/*/<button>");
        button.inputAction.Disable();
        rebindingOperation = newAction.PerformInteractiveRebinding()
                    // To avoid accidental input from mouse motion
                    .WithControlsExcluding("Mouse")
                    .WithControlsExcluding("<Keyboard>/printScreen")
                    .OnMatchWaitForAnother(0.1f)
                    .OnComplete(operation => OnRemapAxisComplete(button, newAction, isPositive))
                    .Start();
    }

    private bool CheckIfSelectedActionmapIsFullyBound()
    {
        foreach (InputBinding item in vehicleInputActions.asset.actionMaps[(int)selectedInputType].bindings)
        {
            string k = InputControlPath.ToHumanReadableString(item.effectivePath);
            if (k == "") return false;
        }
        return true;
    }

    private void OnRemapButtonComplete(BindingButton button)
    {
        button.inputAction.Enable();
        rebindingOperation.Dispose();
        button.SetKeyText(button.inputAction.bindings[0]);
        SaveActionmap(vehicleInputActions);
    }

    private void OnRemapAxisComplete(BindingButton button, InputAction newAction, bool isPositive)
    {
        int i;
        if (!isPositive)
        {
            var negative = button.inputAction.bindings.IndexOf(b => b.name == "negative");
            button.inputAction.ApplyBindingOverride(negative, newAction.bindings[0]);
            i = 1;
        }
        else
        {
            var positive = button.inputAction.bindings.IndexOf(b => b.name == "positive");
            button.inputAction.ApplyBindingOverride(positive, newAction.bindings[0]);
            i = 2;
            axisSkip = 1;
        }

        button.inputAction.Enable();
        rebindingOperation.Dispose();
        button.SetKeyText(button.inputAction.bindings[i]);
        SaveActionmap(vehicleInputActions);
    }
}
