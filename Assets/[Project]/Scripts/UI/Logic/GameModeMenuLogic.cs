using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeMenuLogic : MonoBehaviour
{
    [SerializeField] private Buttons gameModeButtons;
    [SerializeField] private BindingMenuLogic bindingMenu;

    private void Start()
    {
        gameModeButtons.buttons[0].onClick.AddListener(() => SelectMode(GameMode.Relaxed));
        gameModeButtons.buttons[1].onClick.AddListener(() => SelectMode(GameMode.TimeTrial));
    }

    private void SelectMode(GameMode mode)
    {
        GameModeManager.Instance.gameMode = mode;
        bindingMenu.gameObject.SetActive(true);
        bindingMenu.EnableBindingMenuLogicButtons();
        this.gameObject.SetActive(false);
    }
}
