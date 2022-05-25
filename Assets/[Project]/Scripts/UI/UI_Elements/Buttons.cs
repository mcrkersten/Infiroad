using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Buttons : MonoBehaviour
{
    public bool removeListenersOnDeactivate;
    public List<Button> buttons = new List<Button>();
    private void OnDisable()
    {
        if (removeListenersOnDeactivate)
        {
            foreach (Button b in buttons)
                b.onClick.RemoveAllListeners();
        }
    }
}
