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

    public PlayerInputBindings selectedBinding;
    public List<PlayerInputBindings> playerInputBindings = new List<PlayerInputBindings>();
    [SerializeField] private List<PlayerInputBindings> defaultPlayerInputBindings = new List<PlayerInputBindings>();

    private void Start()
    {
        vehicleInputActions = new VehicleInputActions();
        instance = this;
        DontDestroyOnLoad(this.transform);
    }

    public List<InputType> CheckBindingFiles()
    {
        List<InputType> bindingErrors = new List<InputType>();
        List<PlayerInputBindings> bindings = new List<PlayerInputBindings>();
        foreach(InputType inputType in Enum.GetValues(typeof(InputType)))
        {
            PlayerInputBindings b = BindingFileManager.LoadBindingData(inputType);
            if (b != null)
                bindings.Add(b);
            else
                bindingErrors.Add(inputType);
        }

        List<InputType> fixedErrors = new List<InputType>();
        foreach (InputType inputType in bindingErrors)
        {
            //PlayerInputBindings selectedBinding = defaultPlayerInputBindings.Where(i => i.inputBindings.inputType == inputType).FirstOrDefault();
            //if(selectedBinding != null)
            //{
            //    bindings.Add(selectedBinding);
            //    fixedErrors.Add(inputType);
            //}
        }

        foreach (InputType inputType in fixedErrors)
        {
            bindingErrors.Remove(inputType);
        }

        foreach (PlayerInputBindings binding in bindings)
        {
            PlayerInputBindings selectedBinding = playerInputBindings.Where(i => i.inputBindings.inputType == binding.inputBindings.inputType).First();
            var index = bindings.IndexOf(selectedBinding);

            if (index != -1)
                playerInputBindings[index] = binding;
        }
        return bindingErrors;
    }

    public void InitializeSelectedBinding()
    {
        switch (selectedBinding.inputBindings.inputType)
        {
            case InputType.Keyboard:
                InitializeKeyboard();
                break;
            case InputType.Gamepad:
                InitializeGamepad();
                break;
            case InputType.Wheel:
                InitializeWheel();
                break;
        }
    }

    private void InitializeKeyboard()
    {
        foreach (BindingObject binding in selectedBinding.inputBindings.bindings)
        {
            KeyboardBinding key = (KeyboardBinding)binding.key;
            Debug.Log(vehicleInputActions);
            switch (key)
            {
                case KeyboardBinding.SteerLeft:
                    int negative = vehicleInputActions.Keyboard.Steering.bindings.IndexOf(b => b.name == "negative");
                    vehicleInputActions.Keyboard.Steering.ApplyBindingOverride(negative, binding.inputBinding);
                    break;
                case KeyboardBinding.SteerRight:
                    int positive = vehicleInputActions.Keyboard.Steering.bindings.IndexOf(b => b.name == "positive");
                    vehicleInputActions.Keyboard.Steering.ApplyBindingOverride(positive, binding.inputBinding);
                    break;
                case KeyboardBinding.Shift_Up:
                    vehicleInputActions.Keyboard.ShiftUP.ApplyBindingOverride(binding.inputBinding);
                    break;
                case KeyboardBinding.Shift_Down:
                    vehicleInputActions.Keyboard.ShiftDOWN.ApplyBindingOverride(binding.inputBinding);
                    break;
                case KeyboardBinding.Brake:
                    vehicleInputActions.Keyboard.Braking.ApplyBindingOverride(binding.inputBinding);
                    break;
                case KeyboardBinding.Accelerate:
                    vehicleInputActions.Keyboard.Acceleration.ApplyBindingOverride(binding.inputBinding);
                    break;
                case KeyboardBinding.Reset:
                    vehicleInputActions.Keyboard.Reset.ApplyBindingOverride(binding.inputBinding);
                    break;
                case KeyboardBinding.Clutch:
                    vehicleInputActions.Keyboard.Clutch.ApplyBindingOverride(binding.inputBinding);
                    break;
            }
        }
    }

    private void InitializeGamepad()
    {
        foreach (BindingObject binding in selectedBinding.inputBindings.bindings)
        {
            GamepadBinding key = (GamepadBinding)binding.key;
            switch (key)
            {
                case GamepadBinding.SteerLeft:
                    var negative = vehicleInputActions.Gamepad.Steering.bindings.IndexOf(b => b.name == "negative");
                    vehicleInputActions.Gamepad.Steering.ApplyBindingOverride(negative, binding.inputBinding);
                    break;
                case GamepadBinding.SteerRight:
                    var positive = vehicleInputActions.Gamepad.Steering.bindings.IndexOf(b => b.name == "positive");
                    vehicleInputActions.Gamepad.Steering.ApplyBindingOverride(positive, binding.inputBinding);
                    break;
                case GamepadBinding.Shift_Up:
                    vehicleInputActions.Gamepad.ShiftUP.ApplyBindingOverride(binding.inputBinding);
                    break;
                case GamepadBinding.Shift_Down:
                    vehicleInputActions.Gamepad.ShiftDOWN.ApplyBindingOverride(binding.inputBinding);
                    break;
                case GamepadBinding.Brake:
                    vehicleInputActions.Gamepad.Braking.ApplyBindingOverride(binding.inputBinding);
                    break;
                case GamepadBinding.Accelerate:
                    vehicleInputActions.Gamepad.Acceleration.ApplyBindingOverride(binding.inputBinding);
                    break;
                case GamepadBinding.Reset:
                    vehicleInputActions.Gamepad.Reset.ApplyBindingOverride(binding.inputBinding);
                    break;
                case GamepadBinding.Clutch:
                    vehicleInputActions.Gamepad.Clutch.ApplyBindingOverride(binding.inputBinding);
                    break;
            }
        }
    }

    private void InitializeWheel()
    {
        foreach (BindingObject binding in selectedBinding.inputBindings.bindings)
        {
            WheelBinding key = (WheelBinding)binding.key;
            switch (key)
            {
                case WheelBinding.Steering:
                    vehicleInputActions.SteeringWheel.Steering.ApplyBindingOverride(binding.inputBinding);
                    break;
                case WheelBinding.Shift_Up:
                    vehicleInputActions.SteeringWheel.ShiftUP.ApplyBindingOverride(binding.inputBinding);
                    break;
                case WheelBinding.Shift_Down:
                    vehicleInputActions.SteeringWheel.ShiftDOWN.ApplyBindingOverride(binding.inputBinding);
                    break;
                case WheelBinding.Brake:
                    vehicleInputActions.SteeringWheel.Braking.ApplyBindingOverride(binding.inputBinding);
                    break;
                case WheelBinding.Accelerate:
                    vehicleInputActions.SteeringWheel.Acceleration.ApplyBindingOverride(binding.inputBinding);
                    break;
                case WheelBinding.Reset:
                    vehicleInputActions.SteeringWheel.Reset.ApplyBindingOverride(binding.inputBinding);
                    break;
                case WheelBinding.Clutch:
                    vehicleInputActions.SteeringWheel.Clutch.ApplyBindingOverride(binding.inputBinding);
                    break;
            }
        }
    }
}
