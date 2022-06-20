using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using DG.Tweening;
using GameSystems;

public class SectorTimeManager : MonoBehaviour
{
    private float currentSectorTime = 0f;
    private float currenFullLapTime = 0f;
    private float bestLapTime = 0f;
    private SectorTiming currentSector;
    private Queue<SectorTiming> sectors = new Queue<SectorTiming>();
    private GameMode gameMode;

    [SerializeField] private GameObject sectorTimingPrefab;
    [SerializeField] private Transform sectorTimingParent;

    [SerializeField] private TextMeshProUGUI currentLapTimeText;
    [SerializeField] private TextMeshProUGUI bestLapTimeText;
    [SerializeField] private TextMeshProUGUI lapIntervalText;
    [SerializeField] private Color imporovementColor;
    [SerializeField] private Color slowerColor;
    private bool gameIsStarted = false;



    private void Start()
    {
        gameMode = GameModeManager.Instance.gameMode;
        EventTriggerManager.sectorTrigger += UpdateCurrentSector;
        GameManager.onStartGame += StartTimer;

        switch (gameMode)
        {
            case GameMode.Relaxed:
                break;
            case GameMode.TimeTrial:
                break;
            case GameMode.RandomSectors:
                break;
            case GameMode.FixedSectors:
                int index = 1;
                foreach (Sector sector in GameModeManager.Instance.fixedSectors)
                {
                    SectorTiming s = Instantiate(sectorTimingPrefab, sectorTimingParent).GetComponent<SectorTiming>();
                    s.UpdateSectorIndex(index++);
                    s.ui_Animation.Init();
                    sectors.Enqueue(s);
                }
                break;
        }
    }

    private void StartTimer()
    {
        gameIsStarted = true;
    }

    private void Update()
    {
        if (gameIsStarted)
        {
            currentSectorTime += Time.deltaTime;
            currenFullLapTime += Time.deltaTime;
            currentLapTimeText.text = SectorTiming.GenTimeSpanFromSeconds(currenFullLapTime);
        }
    }

    private void UpdateCurrentSector()
    {
        currentSector = sectors.Dequeue();
        switch (gameMode)
        {
            case GameMode.Relaxed:
                break;
            case GameMode.TimeTrial:
                break;
            case GameMode.RandomSectors:
                break;
            case GameMode.FixedSectors:
                UpdateSector(currentSector);
                break;
        }
        sectors.Enqueue(currentSector);
    }

    private void UpdateSector(SectorTiming index)
    {
        index.UpdateSectorTiming(currentSectorTime);
        currentSectorTime = 0f;
        if (index.sectorIndex == GameModeManager.Instance.fixedSectors.Count)
            UpdateLapTime();
    }

    private void UpdateLapTime()
    {
        if (bestLapTime != 0f)
        {
            float interval = currenFullLapTime - bestLapTime;
            if (currenFullLapTime < bestLapTime)
                bestLapTime = currenFullLapTime;
            OnTiming(interval);
            lapIntervalText.transform.DOMove(lapIntervalText.transform.position, 0f).SetDelay(3f).OnComplete(DeactivateLapInteval);
        }
        else {
            bestLapTime = currenFullLapTime;
            bestLapTimeText.text = SectorTiming.GenTimeSpanFromSeconds(currenFullLapTime);
        }
        currenFullLapTime = 0f;
    }

    private void OnTiming(float interval)
    {
        string timing = "";
        if (interval < 0f)
        {
            lapIntervalText.color = imporovementColor;
            timing = "-";
        }
        else
        {
            lapIntervalText.color = slowerColor;
            timing = "+";
        }
        lapIntervalText.text = timing + SectorTiming.GenTimeSpanFromSeconds(interval);
        lapIntervalText.gameObject.SetActive(true);
    }

    private void DeactivateLapInteval()
    {
        lapIntervalText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        EventTriggerManager.sectorTrigger -= UpdateCurrentSector;
    }
}
