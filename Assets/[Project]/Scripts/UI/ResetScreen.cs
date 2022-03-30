using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.UI;

public class ResetScreen : MonoBehaviour
{
    public VehicleInputActions vehicleInputActions;
    public Image blackImage;

    public delegate void OnResetVehicle();
    public static event OnResetVehicle resetVehicle;

    // Start is called before the first frame update
    void Start()
    {
        vehicleInputActions = new VehicleInputActions();
        vehicleInputActions.Default.Reset.Enable();
        vehicleInputActions.Default.Reset.started += ActivateResetScreen;
    }

    public void ActivateResetScreen(InputAction.CallbackContext obj)
    {
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
        vehicleInputActions.Default.Reset.started -= ActivateResetScreen;
    }
}
