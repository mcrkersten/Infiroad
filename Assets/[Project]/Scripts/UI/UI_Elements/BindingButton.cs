using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class BindingButton : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI bindingName;
    [SerializeField] private TextElement unbound;
    [SerializeField] private TextElement listening;

    public void SetNewBinding()
    {
        listening.gameObject.SetActive(false);
        unbound.gameObject.SetActive(true);
    }

    public void Listening()
    {
        listening.UpdateTextElement(0, "Listening...");
        listening.UpdateTextElement(1, "Click prefered button");

        unbound.gameObject.SetActive(false);
        listening.gameObject.SetActive(true);
    }

    public void Bound(string keyName)
    {
        listening.UpdateTextElement(0, keyName);
        listening.UpdateTextElement(1, "Click to rebind");
    }

    public void UnBound()
    {
        listening.gameObject.SetActive(false);
        unbound.gameObject.SetActive(true);
    }
}
