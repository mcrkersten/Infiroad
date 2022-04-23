using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuLogic : MonoBehaviour
{
    [SerializeField] private Buttons mainMenuButtons;
    [SerializeField] private BindingMenuLogic bindingMenu;

    private void Start()
    {
        mainMenuButtons.buttons[0].onClick.AddListener(() => StartGameButton());
        mainMenuButtons.buttons[1].onClick.AddListener(() => HighscoreButton());
        mainMenuButtons.buttons[2].onClick.AddListener(() => SettingsButton());
        mainMenuButtons.buttons[3].onClick.AddListener(() => QuitGameButton());
    }

    private void StartGameButton()
    {
        bindingMenu.gameObject.SetActive(true);
        bindingMenu.StartBIndingMenuLogic();
    }

    private void HighscoreButton()
    {

    }

    private void SettingsButton()
    {

    }

    private void QuitGameButton()
    {

    }
}
