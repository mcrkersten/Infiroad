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
            gameModeManager = GameModeManager.Instance;
            instance = this;

            bestRun = FileManager.LoadRaceData();
            currentRun = new RaceData(0, gameTime);
        }

        private void StartGameMode()
        {
            startCountdownClock.StartCountdown(3, 2);
            CountdownClock.timerFinished += GO_Timer;
            switch (gameModeManager.gameMode)
            {
                case GameMode.Relaxed:
                    break;
                case GameMode.TimeTrial:
                    CountdownClock.timerFinished += Game_Timer;
                    break;
            }
        }

        private void Start()
        {
            StartGameMode();
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
