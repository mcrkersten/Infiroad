using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BindingMenuLogic : MonoBehaviour
{
    [SerializeField] private BindingButton bindingButton;

    [SerializeField] private Buttons inputTypeSelectionButtons;
    [SerializeField] private Buttons navigateInputButtons;
    [SerializeField] private BlibManager blibManager;

    [SerializeField] private VehicleInputActions inputAction;
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    [SerializeField] private List<PlayerInputSettings> playerInputSettings = new List<PlayerInputSettings>();


    private InputType selectedInputType;
    private KeyBinding selectedKeybinding = 0;

    private void Start()
    {
        blibManager.InstantiateBlibs(Enum.GetValues(typeof(KeyBinding)).Length - 1);
        blibManager.ActivateBlib(0);
    }

    private void OnEnable()
    {
        inputAction = new VehicleInputActions();
        Enable_NavigateInputSettings();
        Enable_InputTypeSetButtons();
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
        navigateInputButtons.buttons[0].transform.parent.gameObject.SetActive(true);
        inputTypeSelectionButtons.buttons[0].transform.parent.gameObject.SetActive(false);
    }

    private void Enable_NavigateInputSettings()
    {
        navigateInputButtons.buttons[0].onClick.AddListener(() => NextBinding());
        navigateInputButtons.buttons[1].onClick.AddListener(() => PreviousBinding());
        navigateInputButtons.buttons[2].onClick.AddListener(() => CancelBinding());
        navigateInputButtons.buttons[3].onClick.AddListener(() => StartBindingKey(selectedKeybinding, selectedInputType));

    }

    private void StartBindingKey(KeyBinding key, InputType type)
    {
        navigateInputButtons.buttons[3].onClick.RemoveAllListeners();
        bindingButton.Listening();
        switch (type)
        {
            case InputType.Keyboard:
                KeyboardBinding(key);
                break;
            case InputType.Gamepad:
                GamepadBinding(key);
                break;
            case InputType.Wheel:
                WheelBinding(key);
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

    private void OnRebindComplete(InputAction actionToRebind)
    {
        actionToRebind.Enable();
        bindingButton.Bound(InputControlPath.ToHumanReadableString(actionToRebind.bindings[0].effectivePath));
        rebindingOperation.Dispose();
        navigateInputButtons.buttons[3].onClick.AddListener(() => StartBindingKey(selectedKeybinding, selectedInputType));
    }

    private void WheelBinding(KeyBinding key)
    {
        switch (key)
        {
            case KeyBinding.Steering:
                RemapButton(inputAction.SteeringWheel.Steering);
                break;
            case KeyBinding.Shift_Up:
                RemapButton(inputAction.SteeringWheel.ShiftUP);
                break;
            case KeyBinding.Shift_Down:
                RemapButton(inputAction.SteeringWheel.ShiftDOWN);
                break;
            case KeyBinding.Brake:
                RemapButton(inputAction.SteeringWheel.Braking);
                break;
            case KeyBinding.Accelerate:
                RemapButton(inputAction.SteeringWheel.Acceleration);
                break;
            case KeyBinding.Reset:
                RemapButton(inputAction.SteeringWheel.Reset);
                break;
            case KeyBinding.Clutch:
                RemapButton(inputAction.SteeringWheel.Clutch);
                break;
        }
    }

    private void KeyboardBinding(KeyBinding key)
    {
        switch (key)
        {
            case KeyBinding.Steering:
                RemapButton(inputAction.Keyboard.Steering);
                break;
            case KeyBinding.Shift_Up:
                RemapButton(inputAction.Keyboard.ShiftUP);
                break;
            case KeyBinding.Shift_Down:
                RemapButton(inputAction.Keyboard.ShiftDOWN);
                break;
            case KeyBinding.Brake:
                RemapButton(inputAction.Keyboard.Braking);
                break;
            case KeyBinding.Accelerate:
                RemapButton(inputAction.Keyboard.Acceleration);
                break;
            case KeyBinding.Reset:
                RemapButton(inputAction.Keyboard.Reset);
                break;
            case KeyBinding.Clutch:
                RemapButton(inputAction.Keyboard.Clutch);
                break;
        }
    }

    private void GamepadBinding(KeyBinding key)
    {
        switch (key)
        {
            case KeyBinding.Steering:
                RemapButton(inputAction.Gamepad.Steering);
                break;
            case KeyBinding.Shift_Up:
                RemapButton(inputAction.Gamepad.ShiftUP);
                break;
            case KeyBinding.Shift_Down:
                RemapButton(inputAction.Gamepad.ShiftDOWN);
                break;
            case KeyBinding.Brake:
                RemapButton(inputAction.Gamepad.Braking);
                break;
            case KeyBinding.Accelerate:
                RemapButton(inputAction.Gamepad.Acceleration);
                break;
            case KeyBinding.Reset:
                RemapButton(inputAction.Gamepad.Reset);
                break;
            case KeyBinding.Clutch:
                RemapButton(inputAction.Gamepad.Clutch);
                break;
        }
    }

    private void NextBinding()
    {
        //if last jump to first
        if (selectedKeybinding == (KeyBinding)Enum.GetValues(typeof(KeyBinding)).Length - 2)
            selectedKeybinding = (KeyBinding)0;
        else
            selectedKeybinding += 1;
        blibManager.ActivateBlib((int)selectedKeybinding);
        SetBindingText();
    }

    private void PreviousBinding()
    {
        //if first jump to last
        if (selectedKeybinding == 0)
            selectedKeybinding = (KeyBinding)Enum.GetValues(typeof(KeyBinding)).Length - 2;
        else
            selectedKeybinding -= 1;
        blibManager.ActivateBlib((int)selectedKeybinding);
        SetBindingText();
    }

    private void SetBindingText()
    {
        switch (selectedKeybinding)
        {
            case KeyBinding.Steering:
                bindingButton.bindingName.text = "Steering";
                break;
            case KeyBinding.Shift_Up:
                bindingButton.bindingName.text = "Shift up";
                break;
            case KeyBinding.Shift_Down:
                bindingButton.bindingName.text = "Shift down";
                break;
            case KeyBinding.Brake:
                bindingButton.bindingName.text = "Brake";
                break;
            case KeyBinding.Accelerate:
                bindingButton.bindingName.text = "Accelerate";
                break;
            case KeyBinding.Reset:
                bindingButton.bindingName.text = "Reset vehicle";
                break;
            case KeyBinding.Clutch:
                bindingButton.bindingName.text = "Clutch";
                break;
            default:
                break;
        }
    }
    public void CancelBinding()
    {
        selectedInputType = 0;
        selectedKeybinding = 0;
        this.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        DisableButtons(inputTypeSelectionButtons);
        DisableButtons(navigateInputButtons);
        blibManager.ActivateBlib(0);
        SetBindingText();
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

public enum KeyBinding
{
    Steering = 0,
    Shift_Up,
    Shift_Down,
    Brake,
    Accelerate,
    Reset,
    Clutch
}

public enum InputType
{
    Keyboard = 0,
    Gamepad,
    Wheel
}


