using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VR_Manager : MonoBehaviour
{
    public VehicleInputActions vehicleInputActions;
    private InputAction resetCamera;
    private InputAction toggleVR;

    [SerializeField] private GameObject vrOrigin;
    [SerializeField] private Camera vrCamera;
    [SerializeField] private Transform vrResetPoint;

    [SerializeField] private GameObject monitorCameraSystem;
    [SerializeField] private bool isMainMenu;

    private void Awake()
    {
        //XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        if ( isMainMenu ) return;

        if (BindingManager.Instance != null)
            vehicleInputActions = BindingManager.Instance.vehicleInputActions;

        resetCamera = vehicleInputActions.SteeringWheel.VR_ResetCamera;
        vehicleInputActions.SteeringWheel.VR_ResetCamera.Enable();
        vehicleInputActions.SteeringWheel.VR_ResetCamera.started += RecenterVR;

        toggleVR = vehicleInputActions.SteeringWheel.VR_Toggle;
        vehicleInputActions.SteeringWheel.VR_Toggle.Enable();
        vehicleInputActions.SteeringWheel.VR_Toggle.started += ToggleVR;

        monitorCameraSystem.SetActive(true);
    }

    private void ToggleVR(InputAction.CallbackContext obj)
    {
        //if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        //{
        //    XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
        //    XRGeneralSettings.Instance.Manager.StartSubsystems();
        //    vrOrigin.SetActive(true);
        //    monitorCameraSystem.SetActive(false);
        //}
        //else
        //{
        //    XRGeneralSettings.Instance.Manager.StopSubsystems();
        //    XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        //    vrOrigin.SetActive(false);
        //    monitorCameraSystem.SetActive(true);
        //}
    }

    private void RecenterVR(InputAction.CallbackContext obj)
    {
        //Rotatiom
        float yRotation = vrResetPoint.transform.rotation.eulerAngles.y -vrOrigin.transform.rotation.eulerAngles.y;
        vrCamera.transform.Rotate(0, yRotation, 0);

        //Position
        Vector3 distanceDiff = vrResetPoint.position - vrCamera.transform.position;
        vrCamera.transform.position += distanceDiff;
    }
}
