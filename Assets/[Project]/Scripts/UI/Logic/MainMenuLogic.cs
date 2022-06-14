using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class MainMenuLogic : MonoBehaviour
{
    [SerializeField] private Buttons mainMenuButtons;
    public List<Ui_AnimationObject> menuAnimationObjects = new List<Ui_AnimationObject>();
    [SerializeField] private GameObject lastMenu;

    private void Start()
    {
        foreach (Ui_AnimationObject item in menuAnimationObjects)
            item.Init();

        mainMenuButtons.buttons[0].onClick.AddListener(() => StartGameButton());
        mainMenuButtons.buttons[1].onClick.AddListener(() => HighscoreButton());
        mainMenuButtons.buttons[2].onClick.AddListener(() => QuitGameButton());
        ReturnButton.returnPressed += OnReturnButton;
        AnimateFromTo(null, menuAnimationObjects[0]);
    }

    public void ModeSelected()
    {
        AnimateFromTo(menuAnimationObjects[1], menuAnimationObjects[2]);
    }

    private void OnReturnButton(ReturnButton.ReturnTo returnTo)
    {
        switch (returnTo)
        {
            case ReturnButton.ReturnTo.MainMenu:
                AnimateFromTo(menuAnimationObjects[1], menuAnimationObjects[0]);
                break;
            case ReturnButton.ReturnTo.GamemodeSelection:
                AnimateFromTo(menuAnimationObjects[2], menuAnimationObjects[1]);
                break;
            default:
                break;
        }
    }

    private void StartGameButton()
    {
        AnimateFromTo(menuAnimationObjects[0], menuAnimationObjects[1]);
        menuAnimationObjects[1].gameObject.GetComponent<GameModeMenuLogic>().gameModeButtons.SelectButton(0);
    }

    private void HighscoreButton()
    {

    }

    private void QuitGameButton()
    {

    }

    private void AnimateFromTo(Ui_AnimationObject from, Ui_AnimationObject to)
    {
        to.Animate_ToPosition();
        Buttons b = to.rectTransform.GetComponent<Buttons>();
        if (b != null && b.selectFirstOnAnimateFinished)
            b.SelectButton(0);

        if (from != null)
        {
            from?.Animate_ToStartPosition();
            lastMenu = from?.gameObject;

            GameModeMenuLogic gml = from?.gameObject.GetComponent<GameModeMenuLogic>();
            if (gml != null)
                gml.DeactivateExtensionPanel();
        }
    }

    private void DisableLastMenu()
    {
        lastMenu.SetActive(false);
    }
    private void OnDisable()
    {
        mainMenuButtons.buttons[0].onClick.RemoveAllListeners();
        mainMenuButtons.buttons[1].onClick.RemoveAllListeners();
        mainMenuButtons.buttons[2].onClick.RemoveAllListeners();
        ReturnButton.returnPressed -= OnReturnButton;
    }
}
