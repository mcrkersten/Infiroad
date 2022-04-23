using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class GamepadInputBindings : PlayerInputBindings
{

}
public enum GamepadBinding
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