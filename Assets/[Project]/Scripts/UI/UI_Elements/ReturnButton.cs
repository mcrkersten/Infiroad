using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ReturnButton : MonoBehaviour
{
    public delegate void OnReturn(MenuType index);
    public static event OnReturn returnPressed;

    [SerializeField] private Button button;
    [SerializeField] private MenuType returnTo;

    private void Start()
    {
        button.onClick.AddListener(() => OnButtonPress(returnTo));
    }

    private void OnButtonPress(MenuType index)
    {
        returnPressed?.Invoke(index);
    }

    public void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
}
