using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringWheelInput
{
    LogitechGSDK.DIJOYSTATE2ENGINES rec;
    public void Init()
    {
        Debug.Log("Steering Init: " + LogitechGSDK.LogiSteeringInitialize(false));
    }
    // Update is called once per frame
    public void Update()
    {
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
            rec = LogitechGSDK.LogiGetStateUnity(0);
        }
    }

    public void SetInputWheelForce(int slideVector)
    {
        LogitechGSDK.LogiPlayConstantForce(0, slideVector);
    }
}
