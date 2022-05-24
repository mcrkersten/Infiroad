using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BindingMenuLogic : MonoBehaviour
{
    [SerializeField] private GameObject bindingMenuWarning;
    [SerializeField] private GameObject saveBindingButton;
    [SerializeField] private BindingManager bindingManager;
    [SerializeField] private BindingButton bindingButton;
    [SerializeField] private Buttons inputTypeSelectionButtons;
    [SerializeField] private Buttons navigateInputButtons;
    [SerializeField] private BlibManager blibManager;

    [SerializeField] private VehicleInputActions inputAction;
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    private InputType selectedInputType = InputType.Gamepad;
    private int selectedKeybinding = 0;
    private List<InputType> bindingErrors = new List<InputType>();

    public void StartBindingMenuLogic()
    {
        Enable_NavigateInputSettings();
        Enable_InputTypeSetButtons();
        inputAction = bindingManager.vehicleInputActions;
    }

    public void SaveNewBinding()
    {
        bindingManager.SaveActionmap(selectedInputType, inputAction);
    }

    public void CancelBindingMenuLogic()
    {
        selectedInputType = 0;
        selectedKeybinding = 0;
        blibManager.DestroyAllBLibs();
        this.gameObject.SetActive(false);
    }

    private void Enable_InputTypeSetButtons()
    {
        inputTypeSelectionButtons.buttons[0].onClick.AddListener(() => SetInputType(0));
        inputTypeSelectionButtons.buttons[1].onClick.AddListener(() => SetInputType(1));
        inputTypeSelectionButtons.buttons[2].onClick.AddListener(() => SetInputType(2));
    }

    private void SetInputType(int i)
    {
        selectedInputType = (InputType)i;
        if (bindingErrors.Contains(selectedInputType))
        {
            Debug.Log("BINDING MENU WARNING");
            bindingMenuWarning.SetActive(true);
        }
        else
        {
            Debug.Log("START GAME");
            SceneManager.LoadScene(1);
        }
    }

    public void ActivateRebinding()
    {
        navigateInputButtons.buttons[0].transform.parent.gameObject.SetActive(true);
        inputTypeSelectionButtons.buttons[0].transform.parent.gameObject.SetActive(false);
        blibManager.InstantiateBlibs(GetEnumCount(selectedInputType) + 1);
        blibManager.ActivateBlib(0);
        SetBindingText(selectedInputType);
        SetBindingButtonText();
    }

    private void Enable_NavigateInputSettings()
    {
        navigateInputButtons.buttons[0].onClick.AddListener(() => NextBinding(selectedInputType));
        navigateInputButtons.buttons[1].onClick.AddListener(() => PreviousBinding(selectedInputType));
        navigateInputButtons.buttons[2].onClick.AddListener(() => CancelBindingMenuLogic());
        navigateInputButtons.buttons[3].onClick.AddListener(() => StartBindingKey(selectedKeybinding, selectedInputType));

    }

    private void StartBindingKey(int key, InputType type)
    {
        navigateInputButtons.buttons[3].onClick.RemoveAllListeners();
        bindingButton.Listening();
        switch (type)
        {
            case InputType.Keyboard:
                RemapKeyboardBinding((KeyboardBinding)key);
                break;
            case InputType.Gamepad:
                RemapGamepadBinding((GamepadBinding)key);
                break;
            case InputType.Wheel:
                RemapWheelBinding((WheelBinding)key);
                break;
        }
    }  

    private void RemapButton(InputAction actionToRebind)
    {
        actionToRebind.Disable();
        rebindingOperation = actionToRebind.PerformInteractiveRebinding()
                    // To avoid accidental input from mouse motion
                    .WithControlsExcluding("Mouse")
                    .WithControlsExcluding("<Keyboard>/printScreen")
                    .OnMatchWaitForAnother(0.1f)
                    .OnComplete(operation => OnRebindComplete(actionToRebind))
                    .Start();

    }

    private void RemapAxis(InputAction actionToRebind, bool isPositive)
    {
        var newAction = new InputAction(binding: "/*/<button>");
        actionToRebind.Disable();
        rebindingOperation = newAction.PerformInteractiveRebinding()
                    // To avoid accidental input from mouse motion
                    .WithControlsExcluding("Mouse")
                    .WithControlsExcluding("<Keyboard>/printScreen")
                    .OnMatchWaitForAnother(0.1f)
                    .OnComplete(operation => OnRebindAxisComplete(actionToRebind, newAction, isPositive))
                    .Start();
    }

    private void OnRebindComplete(InputAction actionToRebind)
    {
        actionToRebind.Enable();
        bindingButton.SetKeyText(actionToRebind.bindings[0]);
        rebindingOperation.Dispose();
        navigateInputButtons.buttons[3].onClick.AddListener(() => StartBindingKey(selectedKeybinding, selectedInputType));
    }

    private void OnRebindAxisComplete(InputAction actionToRebind, InputAction newAction, bool isPositive)
    {
        if (!isPositive)
        {
            var negative = actionToRebind.bindings.IndexOf(b => b.name == "negative");
            actionToRebind.ApplyBindingOverride(negative, newAction.bindings[0]);
            bindingButton.SetKeyText(actionToRebind.bindings[1]);
        }
        else
        {
            var positive = actionToRebind.bindings.IndexOf(b => b.name == "positive");
            actionToRebind.ApplyBindingOverride(positive, newAction.bindings[0]);
            bindingButton.SetKeyText(actionToRebind.bindings[2]);
        }

        actionToRebind.Enable();
        rebindingOperation.Dispose();
        navigateInputButtons.buttons[3].onClick.AddListener(() => StartBindingKey(selectedKeybinding, selectedInputType));
    }

    private void RemapWheelBinding(WheelBinding key)
    {
        switch (key)
        {
            case WheelBinding.Steering:
                RemapButton(inputAction.SteeringWheel.Steering);
                break;
            case WheelBinding.Shift_Up:
                RemapButton(inputAction.SteeringWheel.ShiftUP);
                break;
            case WheelBinding.Shift_Down:
                RemapButton(inputAction.SteeringWheel.ShiftDOWN);
                break;
            case WheelBinding.Brake:
                RemapButton(inputAction.SteeringWheel.Braking);
                break;
            case WheelBinding.Accelerate:
                RemapButton(inputAction.SteeringWheel.Acceleration);
                break;
            case WheelBinding.Reset:
                RemapButton(inputAction.SteeringWheel.Reset);
                break;
            case WheelBinding.Clutch:
                RemapButton(inputAction.SteeringWheel.Clutch);
                break;
        }
    }

    private void RemapKeyboardBinding(KeyboardBinding key)
    {
        switch (key)
        {
            case KeyboardBinding.SteerLeft:
                RemapAxis(inputAction.Keyboard.Steering, false);
                break;
            case KeyboardBinding.SteerRight:
                RemapAxis(inputAction.Keyboard.Steering, true);
                break;
            case KeyboardBinding.Shift_Up:
                RemapButton(inputAction.Keyboard.ShiftUP);
                break;
            case KeyboardBinding.Shift_Down:
                RemapButton(inputAction.Keyboard.ShiftDOWN);
                break;
            case KeyboardBinding.Brake:
                RemapButton(inputAction.Keyboard.Braking);
                break;
            case KeyboardBinding.Accelerate:
                RemapButton(inputAction.Keyboard.Acceleration);
                break;
            case KeyboardBinding.Reset:
                RemapButton(inputAction.Keyboard.Reset);
                break;
            case KeyboardBinding.Clutch:
                RemapButton(inputAction.Keyboard.Clutch);
                break;
        }
    }

    private void RemapGamepadBinding(GamepadBinding key)
    {
        switch (key)
        {
            case GamepadBinding.SteerLeft:
                RemapAxis(inputAction.Keyboard.Steering, false);
                break;
            case GamepadBinding.SteerRight:
                RemapAxis(inputAction.Keyboard.Steering, true);
                break;
            case GamepadBinding.Shift_Up:
                RemapButton(inputAction.Gamepad.ShiftUP);
                break;
            case GamepadBinding.Shift_Down:
                RemapButton(inputAction.Gamepad.ShiftDOWN);
                break;
            case GamepadBinding.Brake:
                RemapButton(inputAction.Gamepad.Braking);
                break;
            case GamepadBinding.Accelerate:
                RemapButton(inputAction.Gamepad.Acceleration);
                break;
            case GamepadBinding.Reset:
                RemapButton(inputAction.Gamepad.Reset);
                break;
            case GamepadBinding.Clutch:
                RemapButton(inputAction.Gamepad.Clutch);
                break;
        }
    }

    private void NextBinding(InputType inputType)
    {
        int x = GetEnumCount(inputType);
        //if last jump to first
        if (selectedKeybinding == x)
            selectedKeybinding = 0;
        else
            selectedKeybinding += 1;
        blibManager.ActivateBlib((int)selectedKeybinding);
        SetBindingText(inputType);
        SetBoundButtonText(selectedKeybinding);
    }

    private void PreviousBinding(InputType inputType)
    {
        int x = GetEnumCount(inputType);
        //if first jump to last
        if (selectedKeybinding == 0)
            selectedKeybinding = x;
        else
            selectedKeybinding -= 1;
        blibManager.ActivateBlib((int)selectedKeybinding);
        SetBindingText(inputType);
        SetBoundButtonText(selectedKeybinding);
    }

    private void SetBoundButtonText(int action, InputAction currentInputAction)
    {
        switch (selectedInputType)
        {
            case InputType.Keyboard:
                if(currentInputAction.GetBindingDisplayString() != "")
                    bindingButton.UnBound();
                break;
            case InputType.Gamepad:
                    bindingButton.UnBound();
                break;
            case InputType.Wheel:
                    bindingButton.UnBound();
                break;
        }
    }

    private int GetEnumCount(InputType inputType)
    {
        int x = 0;
        switch (inputType)
        {
            case InputType.Keyboard:
                x = Enum.GetValues(typeof(KeyboardBinding)).Length - 1;
                break;
            case InputType.Gamepad:
                x = Enum.GetValues(typeof(GamepadBinding)).Length - 1;
                break;
            case InputType.Wheel:
                x = Enum.GetValues(typeof(WheelBinding)).Length - 1;
                break;
        }
        return x;
    }

    private void SetBindingText(InputType inputType)
    {
        switch (inputType)
        {
            case InputType.Keyboard:
                SetKeyboardBindingText();
                break;
            case InputType.Gamepad:
                SetGamepadBindingText();
                break;
            case InputType.Wheel:
                SetWheelBindingText();
                break;
            default:
                break;
        }
    }

    private void SetKeyboardBindingText()
    {
        switch ((KeyboardBinding)selectedKeybinding)
        {
            case KeyboardBinding.SteerLeft:
                bindingButton.bindingName.text = "Steer left";
                break;
            case KeyboardBinding.SteerRight:
                bindingButton.bindingName.text = "Steer right";
                break;
            case KeyboardBinding.Shift_Up:
                bindingButton.bindingName.text = "Shift up";
                break;
            case KeyboardBinding.Shift_Down:
                bindingButton.bindingName.text = "Shift down";
                break;
            case KeyboardBinding.Brake:
                bindingButton.bindingName.text = "Brake";
                break;
            case KeyboardBinding.Accelerate:
                bindingButton.bindingName.text = "Accelerate";
                break;
            case KeyboardBinding.Reset:
                bindingButton.bindingName.text = "Reset vehicle";
                break;
            case KeyboardBinding.Clutch:
                bindingButton.bindingName.text = "Clutch";
                break;
            default:
                break;
        }
    }

    private void SetGamepadBindingText() {
        switch ((GamepadBinding)selectedKeybinding)
        {
            case GamepadBinding.SteerLeft:
                bindingButton.bindingName.text = "Steer left";
                break;
            case GamepadBinding.SteerRight:
                bindingButton.bindingName.text = "Steer right";
                break;
            case GamepadBinding.Shift_Up:
                bindingButton.bindingName.text = "Shift up";
                break;
            case GamepadBinding.Shift_Down:
                bindingButton.bindingName.text = "Shift down";
                break;
            case GamepadBinding.Brake:
                bindingButton.bindingName.text = "Brake";
                break;
            case GamepadBinding.Accelerate:
                bindingButton.bindingName.text = "Accelerate";
                break;
            case GamepadBinding.Reset:
                bindingButton.bindingName.text = "Reset vehicle";
                break;
            case GamepadBinding.Clutch:
                bindingButton.bindingName.text = "Clutch";
                break;
            default:
                break;
        }
    }

    private void SetWheelBindingText()
    {
        switch ((WheelBinding)selectedKeybinding)
        {
            case WheelBinding.Steering:
                bindingButton.bindingName.text = "Steering";
                break;
            case WheelBinding.Shift_Up:
                bindingButton.bindingName.text = "Shift up";
                break;
            case WheelBinding.Shift_Down:
                bindingButton.bindingName.text = "Shift down";
                break;
            case WheelBinding.Brake:
                bindingButton.bindingName.text = "Brake";
                break;
            case WheelBinding.Accelerate:
                bindingButton.bindingName.text = "Accelerate";
                break;
            case WheelBinding.Reset:
                bindingButton.bindingName.text = "Reset vehicle";
                break;
            case WheelBinding.Clutch:
                bindingButton.bindingName.text = "Clutch";
                break;
            default:
                break;
        }
    }

    private void OnDisable()
    {
        DisableButtons(inputTypeSelectionButtons);
        DisableButtons(navigateInputButtons);
        SetBindingText(selectedInputType);
        navigateInputButtons.buttons[0].transform.parent.gameObject.SetActive(false);
        inputTypeSelectionButtons.buttons[0].transform.parent.gameObject.SetActive(true);
    }

    private void DisableButtons(Buttons buttons)
    {
        foreach (var b in buttons.buttons)
        {
            b.onClick.RemoveAllListeners();
        }
    }
}

public enum InputType
{
    Keyboard = 0,
    Gamepad,
    Wheel
}

public enum WheelBinding
{
    Steering = 0,
    Shift_Up,
    Shift_Down,
    Brake,
    Accelerate,
    Reset,
    Clutch
}

public enum GamepadBinding
{
    SteerLeft = 0,
    SteerRight,
    Shift_Up,
    Shift_Down,
    Brake,
    Accelerate,
    Reset,
    Clutch
}

public enum KeyboardBinding
{
    SteerLeft = 0,
    SteerRight,
    Shift_Up,
    Shift_Down,
    Brake,
    Accelerate,
    Reset,
    Clutch
}
