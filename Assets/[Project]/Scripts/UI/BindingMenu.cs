using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BindingMenu : MonoBehaviour
{
    public BlibManager blibManager;
    public VehicleInputActions inputAction;
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    private int bindingCount;

    private void Start()
    {

    }
    public void StartBinding(KeyBinding key, ControlType type)
    {
        switch (type)
        {
            case ControlType.Keyboard:
                DefaultBinding(key);
                break;
            case ControlType.Gamepad:
                DefaultBinding(key);
                break;
            case ControlType.Wheel:
                WheelBinding(key);
                break;
            default:
                break;
        }
    }  

    void RemapButtonClicked(InputAction actionToRebind)
    {
        rebindingOperation = actionToRebind.PerformInteractiveRebinding()
                    // To avoid accidental input from mouse motion
                    .WithControlsExcluding("Mouse")
                    .OnMatchWaitForAnother(0.1f)
                    .OnComplete(operation => OnComplete())
                    .Start();
    }

    private void OnComplete()
    {
        rebindingOperation.Dispose();
    }

    private void WheelBinding(KeyBinding key)
    {
        switch (key)
        {
            case KeyBinding.Shift_Up:
                RemapButtonClicked(inputAction.SteeringWheel.ShiftUP);
                break;
            case KeyBinding.Shift_Down:
                RemapButtonClicked(inputAction.SteeringWheel.ShiftDOWN);
                break;
            case KeyBinding.Brake:
                RemapButtonClicked(inputAction.SteeringWheel.Braking);
                break;
            case KeyBinding.Accelerate:
                RemapButtonClicked(inputAction.SteeringWheel.Acceleration);
                break;
            case KeyBinding.Reset:
                RemapButtonClicked(inputAction.SteeringWheel.Reset);
                break;
            case KeyBinding.Clutch:
                RemapButtonClicked(inputAction.SteeringWheel.Clutch);
                break;
        }
    }

    private void DefaultBinding(KeyBinding key)
    {
        switch (key)
        {
            case KeyBinding.Shift_Up:
                RemapButtonClicked(inputAction.Default.ShiftUP);
                break;
            case KeyBinding.Shift_Down:
                RemapButtonClicked(inputAction.Default.ShiftDOWN);
                break;
            case KeyBinding.Brake:
                RemapButtonClicked(inputAction.Default.Braking);
                break;
            case KeyBinding.Accelerate:
                RemapButtonClicked(inputAction.Default.Acceleration);
                break;
            case KeyBinding.Reset:
                RemapButtonClicked(inputAction.Default.Reset);
                break;
            case KeyBinding.Clutch:
                RemapButtonClicked(inputAction.Default.Clutch);
                break;
        }
    }
}

public enum KeyBinding
{
    Shift_Up = 0,
    Shift_Down,
    Brake,
    Accelerate,
    Reset,
    Clutch
}

public enum ControlType
{
    Keyboard = 0,
    Gamepad,
    Wheel
}


