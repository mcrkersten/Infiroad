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
    private bool gameIsStarted = false;

    [SerializeField] private Ui_AnimationGameObject onScreenTiming;

    [Header("Improvement Colours")]
    [SerializeField] private Color sectorImprovement;
    [SerializeField] private Color lapImprovement;
    [SerializeField] private Color slowerColor;


    private void Start()
    {
        SectorTiming.sectorTimingUpdate += OnScreenTiming;
        gameMode = GameModeManager.Instance.gameMode;
        EventTriggerManager.timerTrigger += UpdateTimer;
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
                currentSector = sectors.Dequeue();
                currentSector.SelectSector();
                Debug.Log(currentSector);
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

    private void UpdateTimer(GameObject timerObject)
    {
        currentSector.UnselectSector();

        switch (gameMode)
        {
            case GameMode.Relaxed:
                //UpdateSector(currentSector);
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
        currentSector = sectors.Dequeue();
        currentSector.SelectSector();
    }

    private void UpdateSector(SectorTiming index)
    {
        index.UpdateSectorTiming(currentSectorTime, currenFullLapTime);
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
            OnLapTiming(interval);
            lapIntervalText.transform.DOMove(lapIntervalText.transform.position, 0f).SetDelay(3f).OnComplete(DeactivateLapInteval);
        }
        else
        {
            bestLapTime = currenFullLapTime;
            bestLapTimeText.text = SectorTiming.GenTimeSpanFromSeconds(currenFullLapTime);
        }
        currenFullLapTime = 0f;
    }

    private void OnLapTiming(float interval)
    {
        string timing = "";
        if (interval < 0f)
        {
            lapIntervalText.color = lapImprovement;
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
        EventTriggerManager.timerTrigger -= UpdateTimer;
    }

    private void OnScreenTiming(float sectorInterval, float lap, SectorTiming.LapType laptype)
    {
        onScreenTiming.gameObject.SetActive(true);
        onScreenTiming.gameObject.GetComponent<RectTransform>().DOSizeDelta(onScreenTiming.ui_Animation.animateScaleTo, .5f).OnComplete(UnscaleOnScreenTiming);
        onScreenTiming.textElements[0].text = SectorTiming.GenTimeSpanFromSeconds(lap);

        string sign = "+";
        if (sectorInterval < 0f)
            sign = "-";

        switch (laptype)
        {
            case SectorTiming.LapType.firstLap:
                return;
            case SectorTiming.LapType.flyingLap:
                onScreenTiming.textElements[1].color = lapImprovement;
                onScreenTiming.textElements[1].text = sign + SectorTiming.GenTimeSpanFromSeconds(sectorInterval);
                break;
            case SectorTiming.LapType.slower:
                onScreenTiming.textElements[1].color = slowerColor;
                onScreenTiming.textElements[1].text = sign + SectorTiming.GenTimeSpanFromSeconds(sectorInterval);
                break;
            case SectorTiming.LapType.fastLap:
                onScreenTiming.textElements[1].color = lapImprovement;
                onScreenTiming.textElements[1].text = sign + SectorTiming.GenTimeSpanFromSeconds(sectorInterval);
                break;
            case SectorTiming.LapType.fastSector:
                onScreenTiming.textElements[1].color = sectorImprovement;
                onScreenTiming.textElements[1].text = sign + SectorTiming.GenTimeSpanFromSeconds(sectorInterval);
                break;
        }
    }

    private void UnscaleOnScreenTiming()
    {
        onScreenTiming.gameObject.GetComponent<RectTransform>().DOSizeDelta(onScreenTiming.ui_Animation.startSize, .5f).SetDelay(3f);
    }
}
