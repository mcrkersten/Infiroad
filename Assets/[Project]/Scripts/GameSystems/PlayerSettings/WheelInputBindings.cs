using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class WheelInputBindings : PlayerInputBindings
{

}
public enum WheelBinding
{
    Steering = 0,
    Shift_Up,
    Shift_Down,
    Brake,
    Accelerate,
    Reset,
    Clutch
}