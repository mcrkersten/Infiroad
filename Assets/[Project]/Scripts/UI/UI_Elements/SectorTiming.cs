using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using DG.Tweening;
using UnityEngine.UI;

public class SectorTiming : MonoBehaviour
{
    public delegate void OnSectorTimingUpdate(float interval, float lap, LapType laptype);
    public static event OnSectorTimingUpdate sectorTimingUpdate;

    public int sectorIndex;
    [SerializeField] private float bestSectorTime = 0f;
    [SerializeField] private float bestLapSectorTime = 0f;
    [SerializeField] private TextMeshProUGUI improvementTimeText;
    [SerializeField] private TextMeshProUGUI sectorTimeText;
    [SerializeField] private TextMeshProUGUI sectorIndexText;
    [SerializeField] public Ui_AnimationObject ui_Animation;
    private RectTransform rectTransform;
    private Image image;

    [Header("Improvement Colours")]
    [SerializeField] private Color sectorImprovement;
    [SerializeField] private Color lapImprovement;
    [SerializeField] private Color slowerColor;

    [Header("Sector selection")]
    [SerializeField] private Color selectedSectorColor;
    [SerializeField] private Color unselectedSectorColor;

    private void Awake()
    {
        if (image == null)
            image = this.GetComponent<Image>();
        if (rectTransform == null)
            rectTransform = this.GetComponent<RectTransform>();

    }
    public void UpdateSectorTiming(float sector, float lap)
    {
        float sectorInterval = sector - bestSectorTime;
        float lapInterval = lap - bestLapSectorTime;

        if (bestLapSectorTime == 0f || bestSectorTime == 0f)
            OnTiming(lapInterval, sector, lap, LapType.firstLap);
        else if (lap < bestLapSectorTime && sector < bestSectorTime)
            OnTiming(lapInterval, sector, lap, LapType.flyingLap);
        else if (sector < bestSectorTime)
            OnTiming(sectorInterval, sector, lap, LapType.fastSector);
        else
            OnTiming(lapInterval, sector, lap, LapType.slower);
    }

    public void UpdateSectorIndex(int index)
    {
        sectorIndexText.text = index.ToString();
        sectorIndex = index;
    }

    private void OnTiming(float interval, float sector, float lap, LapType lapType)
    {
        sectorTimingUpdate?.Invoke(interval, lap, lapType);
        switch (lapType)
        {
            case LapType.firstLap:
                bestLapSectorTime = lap;
                bestSectorTime = sector;
                sectorTimeText.text = GenTimeSpanFromSeconds(bestSectorTime);
                return;
            case LapType.flyingLap:
                bestLapSectorTime = lap;
                bestSectorTime = sector;
                improvementTimeText.color = lapImprovement;
                break;
            case LapType.slower:
                improvementTimeText.color = slowerColor;
                break;
            case LapType.fastLap:
                bestLapSectorTime = lap;
                improvementTimeText.color = lapImprovement;
                break;
            case LapType.fastSector:
                bestSectorTime = sector;
                improvementTimeText.color = sectorImprovement;
                break;
        }

        string sign = "+";
        if (interval < 0f)
            sign = "-";
        improvementTimeText.text = sign + GenTimeSpanFromSeconds(interval);
        sectorTimeText.text = GenTimeSpanFromSeconds(bestSectorTime);
        OnTimingAnimation();
    }

    private void OnTimingAnimation()
    {
        rectTransform.DOSizeDelta(ui_Animation.animateScaleTo, .5f).OnComplete(OnTimingAnimationComplete);
    }

    private void OnTimingAnimationComplete()
    {
        rectTransform.DOSizeDelta(ui_Animation.startSize, .5f).SetDelay(3f);
    }

    public static string GenTimeSpanFromSeconds(float seconds)
    {
        // Create a TimeSpan object and TimeSpan string from 
        // a number of seconds.
        TimeSpan interval = TimeSpan.FromSeconds(seconds);
        string timeInterval = interval.ToString();

        // Pad the end of the TimeSpan string with spaces if it 
        // does not contain milliseconds.
        int pIndex = timeInterval.IndexOf(':');
        pIndex = timeInterval.IndexOf('.', pIndex);
        if (pIndex < 0) timeInterval += "        ";


        return String.Format(@"{0:mm\:ss\.fff}", interval);
    }

    public void SelectSector()
    {
        image.color = selectedSectorColor;
    }
    public void UnselectSector()
    {
        image.color = unselectedSectorColor;
    }

    public enum LapType
    {
        firstLap,
        flyingLap,
        slower,
        fastLap,
        fastSector
    }
}