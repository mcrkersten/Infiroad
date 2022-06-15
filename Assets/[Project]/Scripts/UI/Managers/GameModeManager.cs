using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get { return instance; } }
    private static GameModeManager instance;

    public GameMode gameMode;
    public List<Sector> fixedSectors = new List<Sector>();

    private void Start()
    {
        instance = this;
        DontDestroyOnLoad(this.transform);
    }
}

public enum GameMode
{
    Relaxed = 0,
    TimeTrial,
    RandomSectors,
    FixedSectors
}