using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class BindingButton : MonoBehaviour
{
    public Button button;
    [HideInInspector] public int keyIndex;
    [HideInInspector] public InputAction inputAction;
    [HideInInspector] public int isPositive = -1;

    [SerializeField] private Image selectionOutline;
    public TextMeshProUGUI bindingName;
    public TextMeshProUGUI boundButtonName;
    [SerializeField] private TextMeshProUGUI toolTip;

    [SerializeField] private Color unboundColor;
    [SerializeField] private Color listenColor;
    [SerializeField] private Color boundColor;

    public delegate void OnButtonSelection(int index);
    public static event OnButtonSelection buttonSelected;

    public void OnSelection()
    {
        toolTip.gameObject.SetActive(true);
        toolTip.text = "Select to rebind";
        selectionOutline.color = Color.white;

        int childCount = transform.parent.childCount;
        buttonSelected?.Invoke(keyIndex);
        if (keyIndex < 1)
            return;

        float x = Mathf.Clamp(keyIndex, 0, childCount - 2);
        transform.parent.GetComponent<RectTransform>().DOAnchorPosY(40 * (x - 1), .25f);
    }

    public void OnDeselection()
    {
        toolTip.gameObject.SetActive(false);
        selectionOutline.color = Color.clear;
    }

    public void Listening()
    {
        boundButtonName.text = "Listening";
        boundButtonName.color = listenColor;
        toolTip.text = "Awaiting input";
    }

    public void SetKeyText(InputBinding binding)
    {
        var str = InputControlPath.ToHumanReadableString(binding.effectivePath);
        var newStr = "";
        var idx = str.LastIndexOf(" ");
        if (idx > -1)
            newStr = str.Remove(idx);
        else
            newStr = str;

        if (newStr != "")
        {
            boundButtonName.text = newStr;
            boundButtonName.color = boundColor;
        }
        else
            UnBound();
        toolTip.text = "Select to rebind";
    }

    public void UnBound()
    {
        boundButtonName.text = "Unbound";
        boundButtonName.color = unboundColor;
    }
}
