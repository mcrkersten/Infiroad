using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class Buttons : MonoBehaviour
{
    public bool selectFirstOnAnimateFinished;
    public bool removeListenersOnDeactivate;
    public bool usesSprites = false;
    public bool colorBasedOnInputType;
    public List<Button> buttons = new List<Button>();

    [Header("Hover behaviour")]
    [SerializeField] private RestState restState;
    [SerializeField] private Sprite solid;
    [SerializeField] private Sprite unSolid;
    [SerializeField] private Color textColor;
    [SerializeField] private List<Color> colors = new List<Color>();
    public void SelectButton(int index)
    {
        buttons[index].Select();
    }

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
                SetToHollow(button);
                break;
            case RestState.UnSolid:
                SetToSolid(button);
                break;
            case RestState.NotSet:
                break;
        }
    }

    public void OnHoverEnd(GameObject button)
    {
        switch (restState)
        {
            case RestState.Solid:
                SetToSolid(button);
                break;
            case RestState.UnSolid:
                SetToHollow(button);
                break;
            case RestState.NotSet:
                break;
        }
    }

    private void SetToSolid(GameObject button)
    {
        Image image = button.GetComponent<Image>();
        image.sprite = solid;

        //TEXT
        if (colors.Count == 0)
            foreach (TextMeshProUGUI t in button.GetComponentsInChildren<TextMeshProUGUI>())
                t.color = textColor;
        else if(!colorBasedOnInputType)
            foreach (TextMeshProUGUI t in button.GetComponentsInChildren<TextMeshProUGUI>())
                t.color = colors[buttons.IndexOf(button.GetComponent<Button>())];
        //IMAGE
        if (colors.Count == 0)
            foreach (Image t in button.GetComponentsInChildren<Image>().Where(go => go.gameObject != button.gameObject))
                t.color = textColor;
        else if (!colorBasedOnInputType)
            foreach (Image t in button.GetComponentsInChildren<Image>().Where(go => go.gameObject != button.gameObject))
                t.color = colors[buttons.IndexOf(button.GetComponent<Button>())];

        if (colorBasedOnInputType)
        {
            foreach (TextMeshProUGUI t in button.GetComponentsInChildren<TextMeshProUGUI>())
                t.color = colors[(int)BindingManager.Instance.selectedInputType];
            foreach (Image t in button.GetComponentsInChildren<Image>().Where(go => go.gameObject != button.gameObject))
                t.color = colors[(int)BindingManager.Instance.selectedInputType];
        }
    }

    public void SetToHollow(GameObject button)
    {
        Image image = button.GetComponent<Image>();
        image.sprite = unSolid;
        foreach (TextMeshProUGUI t in button.GetComponentsInChildren<TextMeshProUGUI>())
            t.color = image.color;
        foreach (Image t in button.GetComponentsInChildren<Image>().Where(go => go.gameObject != button.gameObject))
            t.color = image.color;
    }

    private enum RestState
    {
        NotSet = default,
        Solid,
        UnSolid,
    }
}
