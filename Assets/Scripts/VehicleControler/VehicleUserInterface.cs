using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VehicleUserInterface : MonoBehaviour
{
    public TextMeshProUGUI speedometer;
    public TextMeshProUGUI RPMometer;
    public TextMeshProUGUI gear;
    public Image RPM_Mask;
    public Image acceleration_Mask;
    public Image braking_Mask;

    private VehicleUserInterfaceData data;

    private Vector2 RPM_MaskSize;
    private Vector2 acceleration_MaskSize;
    private Vector2 braking_MaskSize;
    private void Start()
    {
        RPM_MaskSize = RPM_Mask.rectTransform.rect.size;
        acceleration_MaskSize = acceleration_Mask.rectTransform.rect.size;
        braking_MaskSize = braking_Mask.rectTransform.rect.size;

        data = transform.root.GetComponent<VehicleController>().data;
    }

    private void Update()
    {
        speedometer.text = data.GetCurrentSpeed().ToString();
        RPMometer.text = data.GetRPM().ToString();
        gear.text = (1 + data.GetCurrentGear()).ToString();
        RPM_Mask.rectTransform.sizeDelta = new Vector2(data.GetRPMPercentage() * RPM_MaskSize.x, RPM_MaskSize.y);
        acceleration_Mask.rectTransform.sizeDelta = new Vector2(acceleration_MaskSize.x, data.acceleration * acceleration_MaskSize.y);
        braking_Mask.rectTransform.sizeDelta = new Vector2(braking_MaskSize.x, data.brake * braking_MaskSize.y);
    }
}
