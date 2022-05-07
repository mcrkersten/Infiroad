using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace GameSystems
{
    public class CountdownClock : MonoBehaviour
    {
        private float timerSpeed;
        private float currentTime = -1f;
        private int count;

        [SerializeField] private ClockType clockType;
        [SerializeField] private CounterType counterType;
        [SerializeField] private TextMeshProUGUI numbersText;
        [SerializeField] private TextMeshProUGUI goText;
        [SerializeField] private List<TextMeshProUGUI> allExtraText = new List<TextMeshProUGUI>();

        public delegate void OnTimerFinished(CountdownClock clock);
        public static event OnTimerFinished timerFinished;

        [SerializeField] private AK.Wwise.Event CountDown_Audio;
        [SerializeField] private AK.Wwise.Event GO_Audio;

        private void Start()
        {
            DeactivateNumbers();
            DeactivateGO();
            foreach (TextMeshProUGUI item in allExtraText)
                item.gameObject.SetActive(false);
        }
        public void StartCountdown(int count, float timerSpeed)
        {
            this.count = count;
            currentTime = count + 5;
            this.timerSpeed = timerSpeed;
        }

        public void StartGameCountdown(int count)
        {
            this.count = count;
            currentTime = count;
            this.timerSpeed = 1f;
        }

        [ContextMenu("DebugTimer")]
        public void DebugTimer()
        {
            StartCountdown(3, 1.8f);
        }

        private void ActivateNumbers()
        {
            numbersText.gameObject.SetActive(true);
        }

        public void FixedUpdate()
        {
            if(currentTime >= 0)
            {
                switch (clockType)
                {
                    case ClockType.StartCountdown:
                        StartCountdownLoop();
                        break;
                    case ClockType.GameTime:
                        foreach (TextMeshProUGUI item in allExtraText)
                            item.gameObject.SetActive(true);
                        GameTimeCountdownLoop();
                        break;
                }
            }
        }

        private void GameTimeCountdownLoop()
        {
            if(currentTime > 0f)
            {
                currentTime -= Time.fixedDeltaTime * timerSpeed;
                UpdateNumbers_UI(currentTime);
            }

            if(currentTime < 0f)
            {
                TriggerGameEnd();
                this.enabled = false;
            }
        }

        private void StartCountdownLoop()
        {
            if (currentTime > 1)
            {
                if (currentTime < count)
                    UpdateNumbers_UI(currentTime);

                currentTime -= Time.fixedDeltaTime * timerSpeed;
                if (currentTime < 1)
                    DeactivateNumbers();
                return;
            }

            if (currentTime > 0)
            {
                TriggerGO_UI();
                this.enabled = false;
            }
        }

        private void DeactivateNumbers()
        {
            numbersText.gameObject.SetActive(false);
        }

        private void UpdateNumbers_UI(float currentTime)
        {
            switch (counterType)
            {
                case CounterType.Digital:
                    ActivateNumbers();
                    if (numbersText.text != ((int)currentTime).ToString() && currentTime > 1)
                        CountDown_Audio.Post(this.gameObject);
                    numbersText.text = ((int)currentTime).ToString();
                    break;
                case CounterType.Analog:
                    ActivateNumbers();
                    numbersText.text = System.Math.Round(currentTime, 2).ToString();
                    break;
            }
        }

        private void TriggerGO_UI()
        {
            timerFinished?.Invoke(this);
            goText.gameObject.SetActive(true);
            goText.gameObject.transform.localScale = Vector3.one;
            goText.gameObject.transform.DOScaleX(2f, .4f).SetEase(DG.Tweening.Ease.OutCubic).OnComplete(DeactivateGO);
            goText.DOColor(new Color(255, 255, 255, 0), .4f);
            GO_Audio.Post(this.gameObject);
        }

        private void TriggerGameEnd()
        {
            timerFinished.Invoke(this);
            goText.gameObject.SetActive(true);
            DeactivateNumbers();
        }

        private void DeactivateGO()
        {
            goText.gameObject.SetActive(false);
        }
    }

    public enum CounterType
    {
        Digital = 0,
        Analog
    }

    public enum ClockType
    {
        StartCountdown = 0,
        GameTime
    }
}
