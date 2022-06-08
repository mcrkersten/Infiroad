using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rollbar : MonoBehaviour
{
    public float antiRoll;
    float leftSpring = 1f;
    float rightSpring = 1f;

    public void UpdateSpringValue(float value, SuspensionPosition suspensionPosition)
    {
        if (suspensionPosition == SuspensionPosition.FrontLeft || suspensionPosition == SuspensionPosition.RearLeft)
            leftSpring = value;
        if (suspensionPosition == SuspensionPosition.FrontRight || suspensionPosition == SuspensionPosition.RearRight)
            rightSpring = value;
    }

    public float CalculateRollForce(SuspensionPosition suspensionPosition)
    {
        float antiRollForce = (leftSpring - rightSpring) * antiRoll;
        if (suspensionPosition == SuspensionPosition.FrontLeft || suspensionPosition == SuspensionPosition.RearLeft)
            return -antiRollForce * antiRoll;
        if (suspensionPosition == SuspensionPosition.FrontRight || suspensionPosition == SuspensionPosition.RearRight)
            return antiRollForce * antiRoll;
        return 0f;
    }
}
