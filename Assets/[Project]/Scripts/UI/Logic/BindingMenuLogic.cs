using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class BindingMenuLogic : MonoBehaviour
{
    [SerializeField] private GameObject bindingButtonPrefab;
    [SerializeField] private RectTransform bindingButtonParent;
    [SerializeField] private List<BindingButton> bindingButtons = new List<BindingButton>();
    [SerializeField] private Buttons startRemapButton;
    [SerializeField] private Buttons inputTypeSelectionButtons;
    [SerializeField] private Button returnToDefaultButton;
    [SerializeField] private ReturnButton returnButton;
    [SerializeField] private BindingManager bindingManager;

    [SerializeField] private GameObject bindingMenuWarning;

    [Header("Animation")]
    [SerializeField] private List<Ui_AnimationObject> keyboardPanel = new List<Ui_AnimationObject>();
    [SerializeField] private List<Ui_AnimationObject> gamepadPanel = new List<Ui_AnimationObject>();
    [SerializeField] private List<Ui_AnimationObject> wheelPanel = new List<Ui_AnimationObject>();
    private List<Ui_AnimationObject> currentAnimated = new List<Ui_AnimationObject>();

    private void Start()
    {
        ReturnButton.returnPressed += OnReturnButton;
        foreach (Ui_AnimationObject item in keyboardPanel)
            item.Init();
        foreach (Ui_AnimationObject item in gamepadPanel)
            item.Init();
        foreach (Ui_AnimationObject item in wheelPanel)
            item.Init();
    }

    public void EnableBindingMenuLogicButtons()
    {
        Enable_InputTypeSetButtons();
        Enable_StartRemapProcessButtons();
    }

    #region UsedBySceneAssignedButtons
    public void ReturnToDefaultButton()
    {
        bindingManager.ResetToDefaultActionmap(bindingManager.vehicleInputActions);
        CreateBindingKeyList(bindingManager.selectedInputType);

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
    }

    #endregion

    #region Remap

    private void RemapBinding(BindingButton button)
    {
        switch (button.isPositive)
        {
            case -1:
                bindingManager.RemapButton(button);
                break;
            case 0:
                bindingManager.RemapAxis(button, false);
                break;
            case 1:
                bindingManager.RemapAxis(button, true);
                break;
        }
    }

    #endregion

    #region UI_UpdateFunctions

    private void CreateBindingKeyList(InputType inputType)
    {
        foreach (BindingButton b in bindingButtons)
            Destroy(b.gameObject);
        bindingButtons.Clear();

        switch (inputType)
        {
            case InputType.Keyboard:
                CreateKeyboardList();
                break;
            case InputType.Gamepad:
                CreateGamepadList();
                break;
            case InputType.Wheel:
                CreateWheelList();
                break;
            default:
                break;
        }

        bindingButtons[1].button.Select();
    }

    private void CreateKeyboardList()
    {
        for (int i = 0; i < GetEnumCount(InputType.Keyboard); i++)
        {
            BindingButton button = Instantiate(bindingButtonPrefab, bindingButtonParent).GetComponent<BindingButton>();
            button.keyIndex = i;
            button.button.onClick.AddListener(() => StartRemappingKey(button));
            bindingButtons.Add(button);

            VehicleInputActions.KeyboardActions keyboard = bindingManager.vehicleInputActions.Keyboard;
            switch ((KeyboardBinding)i)
            {
                case KeyboardBinding.SteerLeft:
                    button.bindingName.text = "Steer left";
                    button.inputAction = keyboard.Steering;
                    button.isPositive = 0;
                    break;
                case KeyboardBinding.SteerRight:
                    button.bindingName.text = "Steer right";
                    button.inputAction = keyboard.Steering;
                    button.isPositive = 1;
                    break;
                case KeyboardBinding.Shift_Up:
                    button.bindingName.text = "Shift up";
                    button.inputAction = keyboard.ShiftUP;
                    break;
                case KeyboardBinding.Shift_Down:
                    button.bindingName.text = "Shift down";
                    button.inputAction = keyboard.ShiftDOWN;
                    break;
                case KeyboardBinding.Brake:
                    button.bindingName.text = "Brake";
                    button.inputAction = keyboard.Braking;
                    break;
                case KeyboardBinding.Accelerate:
                    button.bindingName.text = "Accelerate";
                    button.inputAction = keyboard.Acceleration;
                    break;
                case KeyboardBinding.Reset:
                    button.bindingName.text = "Reset vehicle";
                    button.inputAction = keyboard.Reset;
                    break;
                case KeyboardBinding.Clutch:
                    button.bindingName.text = "Clutch";
                    button.inputAction = keyboard.Clutch;
                    break;
                case KeyboardBinding.StartEngine:
                    button.bindingName.text = "Start engine";
                    button.inputAction = keyboard.StartEngine;
                    break;
                default:
                    break;
            }
            button.SetKeyText(button.inputAction.bindings[button.isPositive + 1]);
        }
    }

    private void CreateGamepadList()
    {
        for (int i = 0; i < GetEnumCount(InputType.Gamepad); i++)
        {
            BindingButton button = Instantiate(bindingButtonPrefab, bindingButtonParent).GetComponent<BindingButton>();
            button.keyIndex = i;
            button.button.onClick.AddListener(() => StartRemappingKey(button));
            bindingButtons.Add(button);

            VehicleInputActions.GamepadActions gamepad = bindingManager.vehicleInputActions.Gamepad;

            switch ((GamepadBinding)i)
            {
                case GamepadBinding.SteerLeft:
                    button.bindingName.text = "Steer left";
                    button.inputAction = gamepad.Steering;
                    button.isPositive = 0;
                    break;
                case GamepadBinding.SteerRight:
                    button.bindingName.text = "Steer right";
                    button.inputAction = gamepad.Steering;
                    button.isPositive = 1;
                    break;
                case GamepadBinding.Shift_Up:
                    button.bindingName.text = "Shift up";
                    button.inputAction = gamepad.ShiftUP;
                    break;
                case GamepadBinding.Shift_Down:
                    button.bindingName.text = "Shift down";
                    button.inputAction = gamepad.ShiftDOWN;
                    break;
                case GamepadBinding.Brake:
                    button.bindingName.text = "Brake";
                    button.inputAction = gamepad.Braking;
                    break;
                case GamepadBinding.Accelerate:
                    button.bindingName.text = "Accelerate";
                    button.inputAction = gamepad.Acceleration;
                    break;
                case GamepadBinding.Reset:
                    button.bindingName.text = "Reset vehicle";
                    button.inputAction = gamepad.Reset;
                    break;
                case GamepadBinding.Clutch:
                    button.bindingName.text = "Clutch";
                    button.inputAction = gamepad.Clutch;
                    break;
                case GamepadBinding.StartEngine:
                    button.bindingName.text = "Start engine";
                    button.inputAction = gamepad.StartEngine;
                    break;
                default:
                    break;
            }
            button.SetKeyText(button.inputAction.bindings[button.isPositive + 1]);
        }
    }

    private void CreateWheelList()
    {
        for (int i = 0; i < GetEnumCount(InputType.Wheel); i++)
        {
            BindingButton button = Instantiate(bindingButtonPrefab, bindingButtonParent).GetComponent<BindingButton>();
            button.keyIndex = i;
            button.button.onClick.AddListener(() => StartRemappingKey(button));
            bindingButtons.Add(button);
            VehicleInputActions.SteeringWheelActions gamepad = bindingManager.vehicleInputActions.SteeringWheel;
            switch ((WheelBinding)i)
            {
                case WheelBinding.Steering:
                    button.bindingName.text = "Steering";
                    button.inputAction = gamepad.Steering;
                    break;
                case WheelBinding.Shift_Up:
                    button.bindingName.text = "Shift up";
                    button.inputAction = gamepad.ShiftUP;
                    break;
                case WheelBinding.Shift_Down:
                    button.bindingName.text = "Shift down";
                    button.inputAction = gamepad.ShiftDOWN;
                    break;
                case WheelBinding.Brake:
                    button.bindingName.text = "Brake";
                    button.inputAction = gamepad.Braking;
                    break;
                case WheelBinding.Accelerate:
                    button.bindingName.text = "Accelerate";
                    button.inputAction = gamepad.Acceleration;
                    break;
                case WheelBinding.Reset:
                    button.bindingName.text = "Reset vehicle";
                    button.inputAction = gamepad.Reset;
                    break;
                case WheelBinding.Clutch:
                    button.bindingName.text = "Clutch";
                    button.inputAction = gamepad.Clutch;
                    break;
                case WheelBinding.StartEngine:
                    button.bindingName.text = "Start engine";
                    button.inputAction = gamepad.StartEngine;
                    break;
                default:
                    break;
            }
            button.SetKeyText(button.inputAction.bindings[0]);
        }

    }
    #endregion

    #region UI_ButtonFunctions
    private void StartRemapProcess(InputType type)
    {
        bindingManager.LoadActionmap(bindingManager.vehicleInputActions);
        switch (type)
        {
            case InputType.Keyboard:
                ExecuteAnimationObjects(keyboardPanel);
                break;
            case InputType.Gamepad:
                ExecuteAnimationObjects(gamepadPanel);
                break;
            case InputType.Wheel:
                ExecuteAnimationObjects(wheelPanel);
                break;
        }

        CreateBindingKeyList(type);
        returnToDefaultButton.onClick.AddListener(() => ReturnToDefaultButton());
        bindingManager.selectedInputType = type;
        bindingManager.currentSelectedInputActionMap = bindingManager.vehicleInputActions.asset.actionMaps[(int)type];
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

    private void ExecuteAnimationObjects(List<Ui_AnimationObject> animationObjects)
    {
        returnButton.returnTo = ReturnButton.ReturnTo.InputSelection;
        foreach (Ui_AnimationObject item in animationObjects)
            item.AnimateAll_To();
        currentAnimated = animationObjects;
        returnButton.GetComponent<Button>().Select();
    }

    private void OnReturnButton(ReturnButton.ReturnTo returnTo)
    {
        returnButton.returnTo = ReturnButton.ReturnTo.GamemodeSelection;
        switch (returnTo)
        {
            case ReturnButton.ReturnTo.InputSelection:
                foreach (Ui_AnimationObject item in currentAnimated)
                    item.AnimateAll_Start();
                break;
            default:
                break;
        }
    }

    private void StartRemappingKey(BindingButton selectedButton)
    {
        selectedButton.Listening();
        RemapBinding(selectedButton);
    }

    private void DisableButtons(Buttons buttons)
    {
        foreach (var b in buttons.buttons)
        {
            b.onClick.RemoveAllListeners();
        }
    }
    #endregion

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
