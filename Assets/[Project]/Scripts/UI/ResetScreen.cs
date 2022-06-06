using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.UI;
using GameSystems;
public class ResetScreen : MonoBehaviour
{
    public VehicleInputActions vehicleInputActions;
    public Image blackImage;

    public delegate void OnResetVehicle();
    public static event OnResetVehicle resetVehicle;

    // Start is called before the first frame update
    void Start()
    {
        vehicleInputActions = BindingManager.Instance.vehicleInputActions;
        switch (BindingManager.Instance.selectedInputType)
        {
            case InputType.Keyboard:
                vehicleInputActions.Keyboard.Reset.Enable();
                vehicleInputActions.Keyboard.Reset.started += ActivateResetScreen;
                break;
            case InputType.Gamepad:
                vehicleInputActions.Gamepad.Reset.Enable();
                vehicleInputActions.Gamepad.Reset.started += ActivateResetScreen;
                break;
            case InputType.Wheel:
                vehicleInputActions.SteeringWheel.Reset.Enable();
                vehicleInputActions.SteeringWheel.Reset.started += ActivateResetScreen;
                break;
        }
    }

    public void ActivateResetScreen(InputAction.CallbackContext obj)
    {
        Debug.Log("WORK");
        blackImage.DOColor(new Color(0, 0, 0, 255), .5f).SetEase(DG.Tweening.Ease.InCubic).OnComplete(DeactivateResetScreen);
    }

    private void DeactivateResetScreen()
    {
        resetVehicle?.Invoke();
        blackImage.DOColor(new Color(0, 0, 0, 0), .5f).SetEase(DG.Tweening.Ease.OutCubic).SetDelay(.2f);
    }

    // Update is called once per frame
    void OnDestroy()
    {
        switch (BindingManager.Instance.selectedInputType)
        {
            case InputType.Keyboard:
                vehicleInputActions.Keyboard.Reset.Disable();
                vehicleInputActions.Keyboard.Reset.started -= ActivateResetScreen;
                break;
            case InputType.Gamepad:
                vehicleInputActions.Gamepad.Reset.Disable();
                vehicleInputActions.Gamepad.Reset.started -= ActivateResetScreen;
                break;
            case InputType.Wheel:
                vehicleInputActions.SteeringWheel.Reset.Disable();
                vehicleInputActions.SteeringWheel.Reset.started -= ActivateResetScreen;
                break;
        }
    }
}
