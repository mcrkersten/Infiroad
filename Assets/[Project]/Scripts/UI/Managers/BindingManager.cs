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
    public int selectedKeybinding = 0;

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
                Debug.Log(keyboard);
                if (keyboard == "") return false;
                Debug.Log("not aborted");
                ((InputActionMap)inputAction.Keyboard).LoadBindingOverridesFromJson(keyboard);
                break;
            case InputType.Gamepad:
                string gamepad = PlayerPrefs.GetString(selectedInputType.ToString());
                if (gamepad == "") return false;
                ((InputActionMap)inputAction.Keyboard).LoadBindingOverridesFromJson(gamepad);
                break;
            case InputType.Wheel:
                string wheel = PlayerPrefs.GetString(selectedInputType.ToString());
                if (wheel == "") return false;
                ((InputActionMap)inputAction.Keyboard).LoadBindingOverridesFromJson(wheel);
                break;
        }
        return true;
    }

    public void RemapButton(InputAction actionToRebind)
    {
        Debug.Log(actionToRebind);
        actionToRebind.Disable();
        rebindingOperation = actionToRebind.PerformInteractiveRebinding()
                    // To avoid accidental input from mouse motion
                    .WithControlsExcluding("Mouse")
                    .WithControlsExcluding("<Keyboard>/printScreen")
                    .OnMatchWaitForAnother(0.1f)
                    .OnComplete(operation => OnRemapButtonComplete(actionToRebind))
                    .Start();

    }

    public void RemapAxis(InputAction actionToRebind, bool isPositive)
    {
        var newAction = new InputAction(binding: "/*/<button>");
        actionToRebind.Disable();
        rebindingOperation = newAction.PerformInteractiveRebinding()
                    // To avoid accidental input from mouse motion
                    .WithControlsExcluding("Mouse")
                    .WithControlsExcluding("<Keyboard>/printScreen")
                    .OnMatchWaitForAnother(0.1f)
                    .OnComplete(operation => OnRemapAxisComplete(actionToRebind, newAction, isPositive))
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

    private void OnRemapButtonComplete(InputAction actionToRebind)
    {
        actionToRebind.Enable();
        rebindingOperation.Dispose();
        bindingMenuLogic.SetRemapButtonToInputAction(actionToRebind, 0);
        if (CheckIfSelectedActionmapIsFullyBound())
            bindingMenuLogic.EnableSaveButton();
        else bindingMenuLogic.DisableSaveButton();

    }

    private void OnRemapAxisComplete(InputAction actionToRebind, InputAction newAction, bool isPositive)
    {
        int i;
        if (!isPositive)
        {
            var negative = actionToRebind.bindings.IndexOf(b => b.name == "negative");
            actionToRebind.ApplyBindingOverride(negative, newAction.bindings[0]);
            i = 1;
        }
        else
        {
            var positive = actionToRebind.bindings.IndexOf(b => b.name == "positive");
            actionToRebind.ApplyBindingOverride(positive, newAction.bindings[0]);
            i = 2;
            axisSkip = 1;
        }

        actionToRebind.Enable();
        rebindingOperation.Dispose();
        bindingMenuLogic.SetRemapButtonToInputAction(actionToRebind, i);
        if (CheckIfSelectedActionmapIsFullyBound())
            bindingMenuLogic.EnableSaveButton();
        else bindingMenuLogic.DisableSaveButton();
    }
}
