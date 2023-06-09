using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystems {
    public class GameManager : MonoBehaviour
    {
        public int gameTime;
        private float currentGameTime;

        public delegate void StartGame();
        public static event StartGame onStartGame;

        [SerializeField] private VehicleController vehicleController;
        [SerializeField] private SegmentChainBuilder roadChainBuilder;

        [Header("Game and UI systems")]
        [SerializeField] private CountdownClock startCountdownClock;
        [SerializeField] private CountdownClock gameTimeCountdownClock;
        [SerializeField] private DistanceClock distanceDifferenceClock;
        [SerializeField] private DistanceClock distanceTotalClock;

        private RaceData currentRun;
        private RaceData bestRun;
        private int frame;
        private bool isRunning;

        public static GameManager Instance { get { return instance; } }
        private static GameManager instance;
        private GameModeManager gameModeManager;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            gameModeManager = GameModeManager.Instance;
            roadChainBuilder = SegmentChainBuilder.instance;
            if(GameManager.instance != null)
                StartGameMode();
        }


        private void StartGameMode()
        {
            foreach (VegetationAssetScanner item in vehicleController.vegetationAssetScanners)
                item.multiPool = true;

            roadChainBuilder.GenerateRoadForGamemode(gameModeManager);

            startCountdownClock.StartCountdown(3, 2);
            CountdownClock.timerFinished += GO_Timer;

            StartTimerForMode(gameModeManager.gameMode);
        }

        private void FixedUpdate()
        {
            int distance = 0;
            if (isRunning)
            {
                if (bestRun != null)
                    distance = bestRun.ReadDistance(frame);
                currentRun.AddDistance((int)vehicleController.distanceTraveled);
                currentGameTime -= Time.deltaTime;
                frame++;

                int dist = (int)vehicleController.distanceTraveled - distance;
                distanceDifferenceClock.UpdateDistance(dist);
                distanceTotalClock.UpdateDistance((int)vehicleController.distanceTraveled);
            }
        }

        private void StartTimerForMode(GameMode gameMode)
        {
            switch (gameMode)
            {
                case GameMode.Relaxed:
                    break;
                case GameMode.TimeTrial:
                    bestRun = FileManager.LoadRaceData();
                    currentRun = new RaceData(0, gameTime);
                    CountdownClock.timerFinished += Game_Timer;
                    break;
                case GameMode.RandomSectors:
                    break;
                case GameMode.FixedSectors:
                    break;
            }
        }

        private void GO_Timer(CountdownClock clock) {
            if(startCountdownClock == clock)
            {
                switch (gameModeManager.gameMode)
                {
                    case GameMode.Relaxed:
                        break;
                    case GameMode.TimeTrial:
                        gameTimeCountdownClock.StartGameCountdown(gameTime);
                        currentGameTime = gameTime;
                        vehicleController.distanceTraveled = 0f;
                        distanceDifferenceClock.Activate();
                        distanceTotalClock.Activate();
                        isRunning = true;
                        break;
                }
                vehicleController.UnlockPhysicsLock();
                onStartGame?.Invoke();
            }
        }

        private void Game_Timer(CountdownClock clock)
        {
            if(gameTimeCountdownClock == clock)
            {
                GameOver();
            }
        }

        private void GameOver()
        {
            isRunning = false;
            Debug.Log("GAME OVER");
            if (bestRun != null)
            {
                if (vehicleController.distanceTraveled > bestRun.distance)
                {
                    FileManager.SaveRaceData(currentRun);
                }
            }
            else
            {
                FileManager.SaveRaceData(currentRun);
            }
        }

        private void OnDestroy()
        {
            CountdownClock.timerFinished -= GO_Timer;
        }
    }
}
