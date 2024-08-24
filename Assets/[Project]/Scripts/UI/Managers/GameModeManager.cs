using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get { return instance; } }
    private static GameModeManager instance;

    public GameMode gameMode;
    public List<Sector> fixedSectors = new List<Sector>();

    private void Start()
    {
        if (GameModeManager.instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.transform);
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene(1);
    }

    public void LoadMainMenuScene()
    {
        foreach (Sector sector in fixedSectors)
            Destroy(sector.roadChain.gameObject);
        fixedSectors.Clear();
        SceneManager.LoadScene(0);
        SceneManager.UnloadSceneAsync(1);
    }
}

public enum GameMode
{
    Relaxed = 0,
    TimeTrial,
    RandomSectors,
    FixedSectors
}