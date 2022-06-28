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
        if (!isPaused)
        {
            Debug.Log("GAME PAUSED");
            pauseMenuPlane.gameObject.SetActive(true);
            isPaused = true;
            buttons.buttons[0].Select();
            Time.timeScale = 0;
        }
        else
        {
            Debug.Log("GAME UN-PAUSED");
            pauseMenuPlane.gameObject.SetActive(false);
            Time.timeScale = 1;
            isPaused = false;
        }
    }

    private void OnResume()
    {
        Debug.Log("GAME UN-PAUSED");
        pauseMenuPlane.gameObject.SetActive(false);
        isPaused = false;
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
