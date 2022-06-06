using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BindingMenuLogic : MonoBehaviour
{
    [SerializeField] private BindingManager bindingManager;

    [SerializeField] private GameObject bindingMenuWarning;
    [SerializeField] private GameObject saveBindingButton;
    [SerializeField] private BindingButton remapButton;
    [SerializeField] private Buttons inputTypeSelectionButtons;
    [SerializeField] private Buttons navigateInputButtons;
    [SerializeField] private Buttons startRemapButton;
    [SerializeField] private BlibManager blibManager;

    public void EnableBindingMenuLogicButtons()
    {
        Enable_NavigateInputSettings();
        Enable_InputTypeSetButtons();
        Enable_StartRemapProcessButtons();
    }

    public void EnableSaveButton()
    {
        saveBindingButton.gameObject.SetActive(true);
    }

    public void DisableSaveButton()
    {
        saveBindingButton.gameObject.SetActive(false);
    }


    #region UsedBySceneAssignedButtons
    public void StartRemapProcessOnError()
    {
        navigateInputButtons.buttons[0].transform.parent.gameObject.SetActive(true);
        inputTypeSelectionButtons.buttons[0].transform.parent.gameObject.SetActive(false);
        blibManager.InstantiateBlibs(GetEnumCount(bindingManager.selectedInputType) + 1);
        blibManager.ActivateBlib(0);
        SetRemapKeyNameText(bindingManager.selectedInputType);
        SetRemapButtonText(bindingManager.selectedInputType);
    }

    public void SaveNewBinding()
    {
        bindingManager.SaveActionmap(bindingManager.vehicleInputActions);
    }

    public void CancelBindingMenuLogic()
    {
        bindingManager.selectedKeybinding = 0;
        blibManager.DestroyAllBLibs();
        this.gameObject.SetActive(false);
    }

    #endregion

    #region EnableCodeAssignedButtons
    private void Enable_InputTypeSetButtons()
    {
        inputTypeSelectionButtons.buttons[0].onClick.AddListener(() => SetInputType(0));
        inputTypeSelectionButtons.buttons[1].onClick.AddListener(() => SetInputType(1));
        inputTypeSelectionButtons.buttons[2].onClick.AddListener(() => SetInputType(2));
    }

    private void Enable_StartRemapProcessButtons()
    {
        startRemapButton.buttons[0].onClick.AddListener(() => StartRemapProcess(InputType.Keyboard));
        startRemapButton.buttons[1].onClick.AddListener(() => StartRemapProcess(InputType.Gamepad));
        startRemapButton.buttons[2].onClick.AddListener(() => StartRemapProcess(InputType.Wheel));
        navigateInputButtons.transform.parent.gameObject.SetActive(true);
    }

    private void Enable_NavigateInputSettings()
    {
        navigateInputButtons.buttons[0].onClick.AddListener(() => NextBinding(bindingManager.selectedInputType));
        navigateInputButtons.buttons[1].onClick.AddListener(() => PreviousBinding(bindingManager.selectedInputType));
        navigateInputButtons.buttons[2].onClick.AddListener(() => CancelBindingMenuLogic());
        navigateInputButtons.buttons[3].onClick.AddListener(() => StartRemappingKey(bindingManager.selectedKeybinding, bindingManager.selectedInputType));
        navigateInputButtons.buttons[4].onClick.AddListener(() => SetToDefaultInputType(bindingManager.selectedInputType));
    }
    #endregion

    #region UsedToNavigate

    private void NextBinding(InputType inputType)
    {
        int x = GetEnumCount(inputType);
        //if last jump to first
        if (bindingManager.selectedKeybinding == x)
            bindingManager.selectedKeybinding = 0;
        else
            bindingManager.selectedKeybinding += 1;
        blibManager.ActivateBlib((int)bindingManager.selectedKeybinding);
        SetRemapKeyNameText(inputType);
        SetRemapButtonText(bindingManager.selectedInputType);
    }

    private void PreviousBinding(InputType inputType)
    {
        int x = GetEnumCount(inputType);
        //if first jump to last
        if (bindingManager.selectedKeybinding == 0)
            bindingManager.selectedKeybinding = x;
        else
            bindingManager.selectedKeybinding -= 1;
        blibManager.ActivateBlib((int)bindingManager.selectedKeybinding);
        SetRemapKeyNameText(inputType);
        SetRemapButtonText(bindingManager.selectedInputType);
    }

    #endregion

    #region Remap
    private void RemapWheelBinding(WheelBinding key)
    {
        VehicleInputActions vehicleInputActions = bindingManager.vehicleInputActions;
        switch (key)
        {
            case WheelBinding.Steering:
                bindingManager.RemapButton(vehicleInputActions.SteeringWheel.Steering);
                break;
            case WheelBinding.Shift_Up:
                bindingManager.RemapButton(vehicleInputActions.SteeringWheel.ShiftUP);
                break;
            case WheelBinding.Shift_Down:
                bindingManager.RemapButton(vehicleInputActions.SteeringWheel.ShiftDOWN);
                break;
            case WheelBinding.Brake:
                bindingManager.RemapButton(vehicleInputActions.SteeringWheel.Braking);
                break;
            case WheelBinding.Accelerate:
                bindingManager.RemapButton(vehicleInputActions.SteeringWheel.Acceleration);
                break;
            case WheelBinding.Reset:
                bindingManager.RemapButton(vehicleInputActions.SteeringWheel.Reset);
                break;
            case WheelBinding.Clutch:
                bindingManager.RemapButton(vehicleInputActions.SteeringWheel.Clutch);
                break;
            case WheelBinding.StartEngine:
                bindingManager.RemapButton(vehicleInputActions.SteeringWheel.StartEngine);
                break;
        }
    }

    private void RemapKeyboardBinding(KeyboardBinding key)
    {
        VehicleInputActions vehicleInputActions = bindingManager.vehicleInputActions;
        switch (key)
        {
            case KeyboardBinding.SteerLeft:
                bindingManager.RemapAxis(vehicleInputActions.Keyboard.Steering, false);
                break;
            case KeyboardBinding.SteerRight:
                bindingManager.RemapAxis(vehicleInputActions.Keyboard.Steering, true);
                break;
            case KeyboardBinding.Shift_Up:
                bindingManager.RemapButton(vehicleInputActions.Keyboard.ShiftUP);
                break;
            case KeyboardBinding.Shift_Down:
                bindingManager.RemapButton(vehicleInputActions.Keyboard.ShiftDOWN);
                break;
            case KeyboardBinding.Brake:
                bindingManager.RemapButton(vehicleInputActions.Keyboard.Braking);
                break;
            case KeyboardBinding.Accelerate:
                bindingManager.RemapButton(vehicleInputActions.Keyboard.Acceleration);
                break;
            case KeyboardBinding.Reset:
                bindingManager.RemapButton(vehicleInputActions.Keyboard.Reset);
                break;
            case KeyboardBinding.Clutch:
                bindingManager.RemapButton(vehicleInputActions.Keyboard.Clutch);
                break;
            case KeyboardBinding.StartEngine:
                bindingManager.RemapButton(vehicleInputActions.Keyboard.StartEngine);
                break;
        }
    }

    private void RemapGamepadBinding(GamepadBinding key)
    {
        VehicleInputActions vehicleInputActions = bindingManager.vehicleInputActions;
        switch (key)
        {
            case GamepadBinding.SteerLeft:
                bindingManager.RemapAxis(vehicleInputActions.Gamepad.Steering, false);
                break;
            case GamepadBinding.SteerRight:
                bindingManager.RemapAxis(vehicleInputActions.Gamepad.Steering, true);
                break;
            case GamepadBinding.Shift_Up:
                bindingManager.RemapButton(vehicleInputActions.Gamepad.ShiftUP);
                break;
            case GamepadBinding.Shift_Down:
                bindingManager.RemapButton(vehicleInputActions.Gamepad.ShiftDOWN);
                break;
            case GamepadBinding.Brake:
                bindingManager.RemapButton(vehicleInputActions.Gamepad.Braking);
                break;
            case GamepadBinding.Accelerate:
                bindingManager.RemapButton(vehicleInputActions.Gamepad.Acceleration);
                break;
            case GamepadBinding.Reset:
                bindingManager.RemapButton(vehicleInputActions.Gamepad.Reset);
                break;
            case GamepadBinding.Clutch:
                bindingManager.RemapButton(vehicleInputActions.Gamepad.Clutch);
                break;
            case GamepadBinding.StartEngine:
                bindingManager.RemapButton(vehicleInputActions.Gamepad.StartEngine);
                break;
        }
    }
    #endregion

    #region UI_UpdateFunctions
    public void SetRemapButtonToInputAction(InputAction actionToRebind, int i)
    {
        remapButton.SetKeyText(actionToRebind.bindings[i]);
        Debug.Log(actionToRebind.bindings[i]);
        navigateInputButtons.buttons[3].onClick.AddListener(() => StartRemappingKey(bindingManager.selectedKeybinding, bindingManager.selectedInputType));
    }

    private void SetRemapButtonText(InputType inputType)
    {
        string k;
        InputBinding b;
        switch (inputType)
        {
            case InputType.Keyboard:
                k = GetBinding(true, out b);
                if (k != "") { remapButton.SetKeyText(b); return; }
                break;
            case InputType.Gamepad:
                k = GetBinding(true, out b);
                if (k != "") { remapButton.SetKeyText(b); return; }
                break;
            case InputType.Wheel:
                k = GetBinding(false, out b);
                if (k != "") { remapButton.SetKeyText(b); return; }
                break;
        }
        remapButton.UnBound();
    }

    private void SetRemapKeyNameText(InputType inputType)
    {
        switch (inputType)
        {
            case InputType.Keyboard:
                SetKeyboardBindingNameText();
                break;
            case InputType.Gamepad:
                SetGamepadBindingNameText();
                break;
            case InputType.Wheel:
                SetWheelBindingNameText();
                break;
            default:
                break;
        }
    }

    private void SetKeyboardBindingNameText()
    {
        switch ((KeyboardBinding)bindingManager.selectedKeybinding)
        {
            case KeyboardBinding.SteerLeft:
                remapButton.bindingName.text = "Steer left";
                break;
            case KeyboardBinding.SteerRight:
                remapButton.bindingName.text = "Steer right";
                break;
            case KeyboardBinding.Shift_Up:
                remapButton.bindingName.text = "Shift up";
                break;
            case KeyboardBinding.Shift_Down:
                remapButton.bindingName.text = "Shift down";
                break;
            case KeyboardBinding.Brake:
                remapButton.bindingName.text = "Brake";
                break;
            case KeyboardBinding.Accelerate:
                remapButton.bindingName.text = "Accelerate";
                break;
            case KeyboardBinding.Reset:
                remapButton.bindingName.text = "Reset vehicle";
                break;
            case KeyboardBinding.Clutch:
                remapButton.bindingName.text = "Clutch";
                break;
            case KeyboardBinding.StartEngine:
                remapButton.bindingName.text = "Start engine";
                break;
            default:
                break;
        }
    }

    private void SetGamepadBindingNameText()
    {
        switch ((GamepadBinding)bindingManager.selectedKeybinding)
        {
            case GamepadBinding.SteerLeft:
                remapButton.bindingName.text = "Steer left";
                break;
            case GamepadBinding.SteerRight:
                remapButton.bindingName.text = "Steer right";
                break;
            case GamepadBinding.Shift_Up:
                remapButton.bindingName.text = "Shift up";
                break;
            case GamepadBinding.Shift_Down:
                remapButton.bindingName.text = "Shift down";
                break;
            case GamepadBinding.Brake:
                remapButton.bindingName.text = "Brake";
                break;
            case GamepadBinding.Accelerate:
                remapButton.bindingName.text = "Accelerate";
                break;
            case GamepadBinding.Reset:
                remapButton.bindingName.text = "Reset vehicle";
                break;
            case GamepadBinding.Clutch:
                remapButton.bindingName.text = "Clutch";
                break;
            case GamepadBinding.StartEngine:
                remapButton.bindingName.text = "Start engine";
                break;
            default:
                break;
        }
    }

    private void SetWheelBindingNameText()
    {
        switch ((WheelBinding)bindingManager.selectedKeybinding)
        {
            case WheelBinding.Steering:
                remapButton.bindingName.text = "Steering";
                break;
            case WheelBinding.Shift_Up:
                remapButton.bindingName.text = "Shift up";
                break;
            case WheelBinding.Shift_Down:
                remapButton.bindingName.text = "Shift down";
                break;
            case WheelBinding.Brake:
                remapButton.bindingName.text = "Brake";
                break;
            case WheelBinding.Accelerate:
                remapButton.bindingName.text = "Accelerate";
                break;
            case WheelBinding.Reset:
                remapButton.bindingName.text = "Reset vehicle";
                break;
            case WheelBinding.Clutch:
                remapButton.bindingName.text = "Clutch";
                break;
            case WheelBinding.StartEngine:
                remapButton.bindingName.text = "Start engine";
                break;
            default:
                break;
        }
    }
    #endregion

    #region UI_ButtonFunctions
    private void StartRemapProcess(InputType type)
    {
        bindingManager.selectedInputType = type;
        bindingManager.currentSelectedInputActionMap = bindingManager.vehicleInputActions.asset.actionMaps[(int)type];
        navigateInputButtons.buttons[0].transform.parent.gameObject.SetActive(true);
        inputTypeSelectionButtons.buttons[0].transform.parent.gameObject.SetActive(false);
        blibManager.InstantiateBlibs(GetEnumCount(type) + 1);
        blibManager.ActivateBlib(0);
        SetRemapKeyNameText(type);
        SetRemapButtonText(bindingManager.selectedInputType);
    }
    private void SetInputType(int i)
    {
        bindingManager.selectedInputType = (InputType)i;
        bindingManager.currentSelectedInputActionMap = bindingManager.vehicleInputActions.asset.actionMaps[i];
        if (bindingManager.LoadActionmap(bindingManager.vehicleInputActions))
        {
            Debug.Log("START GAME WITH CUSTOM BINDING");
            SceneManager.LoadScene(1);
            return;
        }
        else if((InputType)i != InputType.Wheel)
        {
            Debug.Log("START GAME WITH DEFAULT BINDING");
            SceneManager.LoadScene(1);
            return;
        }
        else
        {
            Debug.Log("BINDING MENU WARNING");
            bindingMenuWarning.SetActive(true);
        }
    }

    private void SetToDefaultInputType(InputType i)
    {
        bindingManager.ResetToDefaultActionmap(bindingManager.vehicleInputActions);
        CancelBindingMenuLogic();
        EnableBindingMenuLogicButtons();
        Debug.Log("Test");
    }

    private void StartRemappingKey(int key, InputType type)
    {
        navigateInputButtons.buttons[3].onClick.RemoveAllListeners();
        remapButton.Listening();
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

    private void DisableButtons(Buttons buttons)
    {
        foreach (var b in buttons.buttons)
        {
            b.onClick.RemoveAllListeners();
        }
    }
    #endregion


    private string GetBinding(bool hasAxis, out InputBinding binding)
    {
        if (hasAxis && bindingManager.selectedKeybinding == 0)
            binding = bindingManager.currentSelectedInputActionMap.actions[0].bindings[1];
        else if (hasAxis && bindingManager.selectedKeybinding == 1)
            binding = bindingManager.currentSelectedInputActionMap.actions[0].bindings[2];
        else if(hasAxis)
            binding = bindingManager.currentSelectedInputActionMap.actions[bindingManager.selectedKeybinding - 1].bindings[0];
        else
            binding = bindingManager.currentSelectedInputActionMap.actions[bindingManager.selectedKeybinding].bindings[0];
        string k = InputControlPath.ToHumanReadableString(binding.effectivePath);
        return k;
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

    private void OnDisable()
    {
        DisableButtons(inputTypeSelectionButtons);
        DisableButtons(navigateInputButtons);
        SetRemapKeyNameText(0);
        navigateInputButtons.buttons[0].transform.parent.gameObject.SetActive(false);
        inputTypeSelectionButtons.buttons[0].transform.parent.gameObject.SetActive(true);
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
    Brake,
    Accelerate,
    Clutch,
    Shift_Up,
    Shift_Down,
    Reset,
    StartEngine
}

public enum GamepadBinding
{
    SteerLeft = 0,
    SteerRight,
    Brake,
    Accelerate,
    Clutch,
    Shift_Up,
    Shift_Down,
    Reset,
    StartEngine
}

public enum KeyboardBinding
{
    SteerLeft = 0,
    SteerRight,
    Brake,
    Accelerate,
    Clutch,
    Shift_Up,
    Shift_Down,
    Reset,
    StartEngine
}
