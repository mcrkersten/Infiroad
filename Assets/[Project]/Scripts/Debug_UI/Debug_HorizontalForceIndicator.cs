using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Debug_HorizontalForceIndicator : MonoBehaviour
{
    public VehicleController vehicle;
    public Slider slider;

    public void Update()
    {
        slider.value = vehicle.steerWeight;
    }
}
