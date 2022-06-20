using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using DG.Tweening;
using UnityEngine.UI;

public class SectorTiming : MonoBehaviour
{
    public int sectorIndex;
    [SerializeField] private float bestTime = 0f;
    [SerializeField] private TextMeshProUGUI timingText;
    [SerializeField] private TextMeshProUGUI timingImprovement;
    [SerializeField] private TextMeshProUGUI sectorIndexText;
    [SerializeField] public Ui_AnimationObject ui_Animation;
    private RectTransform rectTransform;

    [SerializeField] private Color imporovementColor;
    [SerializeField] private Color slowerColor;

    public void UpdateSectorTiming(float time)
    {
        if (rectTransform == null)
            rectTransform = this.GetComponent<RectTransform>();

        if (bestTime != 0f)
        {
            float interval = time - bestTime;
            if (time < bestTime)
                bestTime = time;
            OnTiming(interval);
        }
        else {
            bestTime = time;
        }
        timingText.text = GenTimeSpanFromSeconds(time);
    }

    public void UpdateSectorIndex(int index)
    {
        sectorIndexText.text = index.ToString();
        sectorIndex = index;
    }

    private void OnTiming(float interval)
    {
        string timing = "";
        if (interval < 0f)
        {
            timingImprovement.color = imporovementColor;
            timing = "-";
        }
        else
        {
            timingImprovement.color = slowerColor;
            timing = "+";
        }
        timingImprovement.text = timing + GenTimeSpanFromSeconds(interval);
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

}