using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class KeyboardInputBindings : PlayerInputBindings
{

}
public enum KeyboardBinding
{
    SteerLeft = 0,
    SteerRight,
    Shift_Up,
    Shift_Down,
    Brake,
    Accelerate,
    Reset,
    Clutch
}