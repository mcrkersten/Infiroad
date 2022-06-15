using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameModeMenuLogic : MonoBehaviour
{
    public Buttons gameModeButtons;
    [SerializeField] private MainMenuLogic mainMenuLogic;
    [SerializeField] private BindingMenuLogic bindingMenu;
    [SerializeField] private Image extensionPanel;

    public void ActivateExtensionPanel()
    {
        extensionPanel.GetComponent<RectTransform>().DOAnchorPos(new Vector2(362f, 200f), 1f).SetEase(DG.Tweening.Ease.OutCubic);
    }

    public void DeactivateExtensionPanel()
    {
        extensionPanel.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0f, 0f), 1f).SetEase(DG.Tweening.Ease.OutCubic);
    }

    private void Start()
    {
        gameModeButtons.buttons[0].onClick.AddListener(() => SelectMode(GameMode.Relaxed));
        gameModeButtons.buttons[1].onClick.AddListener(() => SelectMode(GameMode.TimeTrial));
        gameModeButtons.buttons[2].onClick.AddListener(() => SelectMode(GameMode.RandomSectors));
        gameModeButtons.buttons[3].onClick.AddListener(() => SelectMode(GameMode.FixedSectors));
    }

    private void SelectMode(GameMode mode)
    {
        GameModeManager.Instance.gameMode = mode;
        switch (mode)
        {
            case GameMode.Relaxed:
                mainMenuLogic.ActivateMenu(MenuType.InputSelection);
                break;
            case GameMode.TimeTrial:
                mainMenuLogic.ActivateMenu(MenuType.InputSelection);
                break;
            case GameMode.RandomSectors:
                mainMenuLogic.ActivateMenu(MenuType.InputSelection);
                break;
            case GameMode.FixedSectors:
                mainMenuLogic.ActivateMenu(MenuType.FixedSectorCreator);
                break;
        }
    }
}
