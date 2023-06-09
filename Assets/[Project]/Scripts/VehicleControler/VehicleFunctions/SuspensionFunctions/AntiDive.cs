using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiDive : MonoBehaviour
{
    public float antiDive;
    float frontSpring = 1f;
    float rearSpring = 1f;

    public void UpdateSpringValue(float value, SuspensionPosition suspensionPosition)
    {
        if (suspensionPosition == SuspensionPosition.FrontLeft || suspensionPosition == SuspensionPosition.FrontRight)
            frontSpring = value;
        if (suspensionPosition == SuspensionPosition.RearLeft || suspensionPosition == SuspensionPosition.RearRight)
            rearSpring = value;
    }

    public float CalculateAntiDiveForce(SuspensionPosition suspensionPosition)
    {
        float antiRollForce = (frontSpring - rearSpring) * antiDive;
        if (suspensionPosition == SuspensionPosition.FrontLeft || suspensionPosition == SuspensionPosition.FrontRight)
            return antiRollForce * antiDive;
        if (suspensionPosition == SuspensionPosition.RearLeft || suspensionPosition == SuspensionPosition.RearRight)
            return -antiRollForce * antiDive;
        return 0f;
    }
}
