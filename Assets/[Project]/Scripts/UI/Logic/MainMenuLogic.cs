using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;

public class MainMenuLogic : MonoBehaviour
{
    public static MainMenuLogic Instance { get { return instance; } }
    private static MainMenuLogic instance;

    private MenuType selectedMenu = MenuType.Null;
    [SerializeField] private Buttons mainMenuButtons;
    public List<Ui_AnimationObject> menuAnimationObjects = new List<Ui_AnimationObject>();

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        ReturnButton.returnPressed += OnReturnButton;
        foreach (Ui_AnimationObject item in menuAnimationObjects)
            item.Init();

        mainMenuButtons.buttons[0].onClick.AddListener(() => StartGameButton());
        mainMenuButtons.buttons[1].onClick.AddListener(() => HighscoreButton());
        mainMenuButtons.buttons[2].onClick.AddListener(() => QuitGameButton());
        ActivateMenu(MenuType.Main);
    }

    private void OnReturnButton(MenuType returnTo)
    {
        foreach (Ui_AnimationObject t in menuAnimationObjects.Where(ao => ao.menuType == returnTo))
            t.AnimateAll_To();
        foreach (Ui_AnimationObject t in menuAnimationObjects.Where(ao => ao.menuType == selectedMenu))
            t.AnimateAll_Start();
        selectedMenu = returnTo;
    }

    public void ActivateMenu(MenuType type)
    {
        foreach (Ui_AnimationObject t in menuAnimationObjects.Where(ao => ao.menuType == type))
            t.AnimateAll_To();
        foreach (Ui_AnimationObject t in menuAnimationObjects.Where(ao => ao.menuType == selectedMenu))
            t.AnimateAll_Start();
        selectedMenu = type;
    }

    private void StartGameButton()
    {
        ActivateMenu(MenuType.GamemodeSelection);
        menuAnimationObjects[1].gameObject.GetComponent<GameModeMenuLogic>().gameModeButtons.SelectButton(0);
    }

    private void HighscoreButton()
    {

    }

    private void QuitGameButton()
    {

    }

    private void OnDisable()
    {
        mainMenuButtons.buttons[0].onClick.RemoveAllListeners();
        mainMenuButtons.buttons[1].onClick.RemoveAllListeners();
        mainMenuButtons.buttons[2].onClick.RemoveAllListeners();
    }
}

public enum MenuType
{
    Main = 0,
    GamemodeSelection,
    InputSelection,
    InputType,
    FixedSectorCreator,
    Null
}