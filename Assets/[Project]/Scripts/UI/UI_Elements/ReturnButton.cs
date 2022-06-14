using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ReturnButton : MonoBehaviour
{
    public delegate void OnReturn(ReturnTo index);
    public static event OnReturn returnPressed;

    [SerializeField] private Button button;
    public ReturnTo returnTo;

    private void Start()
    {
        button.onClick.AddListener(() => OnButtonPress(returnTo));
    }

    private void OnButtonPress(ReturnTo index)
    {
        returnPressed?.Invoke(index);
    }

    public enum ReturnTo
    {
        MainMenu = 0,
        GamemodeSelection,
        InputSelection
    }
}
