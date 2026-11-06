using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

public class PauseMenuBehaviour : MonoBehaviour
{
    [SerializeField] private Buttons buttons;
    [SerializeField] private RectTransform pauseMenuPlane;
    private bool isPaused;
    private void Start()
    {
        buttons.buttons[0].onClick.AddListener(() => OnResume());
        buttons.buttons[1].onClick.AddListener(() => LoadMainMenu());
        buttons.buttons[2].onClick.AddListener(() => OnQuitGame());
    }

    public void OnStartMenu(InputAction.CallbackContext obj)
    {
        SetPaused(!isPaused);
    }

    private void OnResume()
    {
        SetPaused(false);
    }

    private void SetPaused(bool paused)
    {
        isPaused = paused;
        pauseMenuPlane.gameObject.SetActive(paused);
        Time.timeScale = paused ? 0 : 1;
        if (paused)
            buttons.buttons[0].Select();
    }

    private void OnDestroy()
    {
        // Never leave the next scene frozen if this menu is destroyed while paused
        if (isPaused)
            Time.timeScale = 1;
    }

    private void LoadMainMenu()
    {
        Time.timeScale = 1;
        GameModeManager.Instance.LoadMainMenuScene();
    }

    private void OnQuitGame()
    {
        Application.Quit();
    }
}
