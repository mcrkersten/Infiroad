using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Buttons : MonoBehaviour
{
    public bool removeListenersOnDeactivate;
    public List<Button> buttons = new List<Button>();

    [Header("Hover behaviour")]
    [SerializeField] private RestState restState;
    [SerializeField] private Sprite solid;
    [SerializeField] private Sprite unSolid;
    [SerializeField] private Color textColor;
    private void OnDisable()
    {
        foreach (Button b in buttons)
            OnHoverEnd(b.gameObject);

        if (removeListenersOnDeactivate)
            foreach (Button b in buttons)
                b.onClick.RemoveAllListeners();
    }

    public void OnHoverStart(GameObject button)
    {
        Image image = button.GetComponent<Image>();
        switch (restState)
        {
            case RestState.Solid:
                image.sprite = unSolid;
                button.GetComponentInChildren<TextMeshProUGUI>().color = image.color;
                break;
            case RestState.UnSolid:
                image.sprite = solid;
                button.GetComponentInChildren<TextMeshProUGUI>().color = textColor;
                break;
            case RestState.NotSet:
                break;
        }
    }

    public void OnHoverEnd(GameObject button)
    {
        Image image = button.GetComponent<Image>();
        switch (restState)
        {
            case RestState.Solid:
                image.sprite = solid;
                button.GetComponentInChildren<TextMeshProUGUI>().color = textColor;
                break;
            case RestState.UnSolid:
                image.sprite = unSolid;
                button.GetComponentInChildren<TextMeshProUGUI>().color = image.color;
                break;
            case RestState.NotSet:
                break;
        }
    }

    private enum RestState
    {
        NotSet = default,
        Solid,
        UnSolid,
    }
}
