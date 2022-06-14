using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BindingIcon : MonoBehaviour
{
    private BindingManager bindingManager;
    [SerializeField] List<Sprite> icons = new List<Sprite>();
    [SerializeField] private Image iconImageRenderer;

    private void OnEnable()
    {
        bindingManager = BindingManager.Instance;
        BindingButton.buttonSelected += UpdateIcon;
    }

    private void UpdateIcon(int index)
    {
        switch (bindingManager.selectedInputType)
        {
            case InputType.Keyboard:
                iconImageRenderer.sprite = icons[index + 1];
                break;
            case InputType.Gamepad:
                iconImageRenderer.sprite = icons[index + 1];
                break;
            case InputType.Wheel:
                if(index == 0)
                    iconImageRenderer.sprite = icons[index];
                else
                    iconImageRenderer.sprite = icons[index + 2];
                break;
        }
    }

    private void OnDisable()
    {
        
    }
}
