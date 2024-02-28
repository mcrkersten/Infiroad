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
    private GameModeManager gameModeManager;
    [SerializeField] private GameObject bindingButtonPrefab;
    [SerializeField] private RectTransform bindingButtonParent;
    [SerializeField] private List<BindingButton> bindingButtons = new List<BindingButton>();
    [SerializeField] private Buttons startRemapButton;
    [SerializeField] private Buttons inputTypeSelectionButtons;
    [SerializeField] private Button returnToDefaultButton;
    [SerializeField] private BindingManager bindingManager;

    [SerializeField] private GameObject bindingMenuWarning;

    [Header("Animation")]
    [SerializeField] private List<Ui_AnimationObject> keyboardPanel = new List<Ui_AnimationObject>();
    [SerializeField] private List<Ui_AnimationObject> gamepadPanel = new List<Ui_AnimationObject>();
    [SerializeField] private List<Ui_AnimationObject> wheelPanel = new List<Ui_AnimationObject>();
    private List<Ui_AnimationObject> currentAnimated = new List<Ui_AnimationObject>();

    private void OnEnable()
    {
        EnableBindingMenuLogicButtons();
    }
    private void Start()
    {
        bindingManager = BindingManager.Instance;
        gameModeManager = GameModeManager.Instance;
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
        inputTypeSelectionButtons.buttons[0].onClick.AddListener(() => StartGame(0));
        inputTypeSelectionButtons.buttons[1].onClick.AddListener(() => StartGame(1));
        inputTypeSelectionButtons.buttons[2].onClick.AddListener(() => StartGame(2));
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
        switch (button.isPositiveOnAxis)
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
        for (int i = 0; i <= GetEnumCount(InputType.Keyboard); i++)
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
                    button.isPositiveOnAxis = 0;
                    break;
                case KeyboardBinding.SteerRight:
                    button.bindingName.text = "Steer right";
                    button.inputAction = keyboard.Steering;
                    button.isPositiveOnAxis = 1;
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
                case KeyboardBinding.Pause:
                    button.bindingName.text = "Pause game";
                    button.inputAction = keyboard.PauseGame;
                    break;
                case KeyboardBinding.ToggleMinimap:
                    button.bindingName.text = "Minimap rescale";
                    button.inputAction = keyboard.MinimapRescale;
                    break;
                case KeyboardBinding.NextSong:
                    button.bindingName.text = "Next song";
                    button.inputAction = keyboard.NextRadioSong;
                    break;
                case KeyboardBinding.PauseRadio:
                    button.bindingName.text = "Pause radio";
                    button.inputAction = keyboard.PauseRadio;
                    break;
            }
            button.SetKeyText(button.inputAction.bindings[button.isPositiveOnAxis + 1]);
        }
    }

    private void CreateGamepadList()
    {
        for (int i = 0; i <= GetEnumCount(InputType.Gamepad); i++)
        {
            BindingButton button = Instantiate(bindingButtonPrefab, bindingButtonParent).GetComponent<BindingButton>();
            button.keyIndex = i;
            button.button.onClick.AddListener(() => StartRemappingKey(button));
            bindingButtons.Add(button);

            VehicleInputActions.GamepadActions gamepad = bindingManager.vehicleInputActions.Gamepad;

            switch ((GamepadBinding)i)
            {
                case GamepadBinding.Steering:
                    button.bindingName.text = "Steering";
                    button.inputAction = gamepad.Steering;
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
                case GamepadBinding.Pause:
                    button.bindingName.text = "Pause game";
                    button.inputAction = gamepad.PauseGame;
                    break;
                case GamepadBinding.ToggleMinimap:
                    button.bindingName.text = "Minimap rescale";
                    button.inputAction = gamepad.MinimapRescale;
                    break;
                case GamepadBinding.NextSong:
                    button.bindingName.text = "Next song";
                    button.inputAction = gamepad.NextRadioSong;
                    break;
                case GamepadBinding.PauseRadio:
                    button.bindingName.text = "Pause radio";
                    button.inputAction = gamepad.PauseRadio;
                    break;
            }
            button.SetKeyText(button.inputAction.bindings[0]);
        }
    }

    private void CreateWheelList()
    {
        for (int i = 0; i <= GetEnumCount(InputType.Wheel); i++)
        {
            BindingButton button = Instantiate(bindingButtonPrefab, bindingButtonParent).GetComponent<BindingButton>();
            button.keyIndex = i;
            button.button.onClick.AddListener(() => StartRemappingKey(button));
            bindingButtons.Add(button);
            VehicleInputActions.SteeringWheelActions wheel = bindingManager.vehicleInputActions.SteeringWheel;
            switch ((WheelBinding)i)
            {
                case WheelBinding.Steering:
                    button.bindingName.text = "Steering";
                    button.inputAction = wheel.Steering;
                    break;
                case WheelBinding.Brake:
                    button.bindingName.text = "Brake";
                    button.inputAction = wheel.Braking;
                    break;
                case WheelBinding.Accelerate:
                    button.bindingName.text = "Accelerate";
                    button.inputAction = wheel.Acceleration;
                    break;
                case WheelBinding.Clutch:
                    button.bindingName.text = "Clutch";
                    button.inputAction = wheel.Clutch;
                    break;
                case WheelBinding.Shift_Up:
                    button.bindingName.text = "Shift up";
                    button.inputAction = wheel.ShiftUP;
                    break;
                case WheelBinding.Shift_Down:
                    button.bindingName.text = "Shift down";
                    button.inputAction = wheel.ShiftDOWN;
                    break;
                case WheelBinding.Reset:
                    button.bindingName.text = "Reset vehicle";
                    button.inputAction = wheel.Reset;
                    break;
                case WheelBinding.Pause:
                    button.bindingName.text = "Pause game";
                    button.inputAction = wheel.PauseGame;
                    break;
                case WheelBinding.ToggleMinimap:
                    button.bindingName.text = "Minimap rescale";
                    button.inputAction = wheel.MinimapRescale;
                    break;
                case WheelBinding.NextSong:
                    button.bindingName.text = "Next song";
                    button.inputAction = wheel.NextRadioSong;
                    break;
                case WheelBinding.PauseRadio:
                    button.bindingName.text = "Pause radio";
                    button.inputAction = wheel.PauseRadio;
                    break;
                case WheelBinding.VR_RecenterCamera:
                    button.bindingName.text = "Recenter VR camera";
                    button.inputAction = wheel.VR_ResetCamera;
                    break;
                case WheelBinding.VR_Toggle:
                    button.bindingName.text = "Toggle VR";
                    button.inputAction = wheel.VR_Toggle;
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
    private void StartGame(int i)
    {
        bindingManager.selectedInputType = (InputType)i;
        bindingManager.currentSelectedInputActionMap = bindingManager.vehicleInputActions.asset.actionMaps[i];
        if (bindingManager.LoadActionmap(bindingManager.vehicleInputActions))
        {
            Debug.Log("START GAME WITH CUSTOM BINDING");
            gameModeManager.LoadGameScene();
            return;
        }
        else if((InputType)i != InputType.Wheel)
        {
            Debug.Log("START GAME WITH DEFAULT BINDING");
            gameModeManager.LoadGameScene();
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
        foreach (Ui_AnimationObject item in animationObjects)
            item.AnimateAll_To();
        currentAnimated = animationObjects;
    }

    public void OnReturnButton()
    {
        foreach (Ui_AnimationObject item in currentAnimated)
            item.AnimateAll_Start();
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
    Pause,
    ToggleMinimap,
    NextSong,
    PauseRadio,
    VR_RecenterCamera,
    VR_Toggle
}

public enum GamepadBinding
{
    Steering = 0,
    Brake,
    Accelerate,
    Clutch,
    Shift_Up,
    Shift_Down,
    Reset,
    Pause,
    ToggleMinimap, 
    NextSong,
    PauseRadio
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
    Pause,
    ToggleMinimap,
    NextSong,
    PauseRadio,
}
