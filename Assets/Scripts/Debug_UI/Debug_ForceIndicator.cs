using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Debug_ForceIndicator : MonoBehaviour
{
    public List<Wheel> wheelsToDebug = new List<Wheel>();
    private List<Indicator> indicators = new List<Indicator>();
    public GameObject indicatorPrefab;

    private void Start()
    {
        foreach (Wheel wheel in wheelsToDebug)
        {
            indicators.Add(new Indicator(Instantiate(indicatorPrefab, this.transform).GetComponent<Image>(), wheel));
        }
    }

    private void FixedUpdate()
    {
        int i = 0;
        foreach (Indicator indicator in indicators)
        {

            Vector2 offset = GetOffset(indicator);
            indicator.image.transform.localScale = Vector3.one * indicator.wheel.gripDebug;
            indicator.image.transform.localPosition = new Vector3(-indicator.wheel.forceDirectionDebug.x + offset.x, -indicator.wheel.forceDirectionDebug.z + offset.y, 0) / 20f;
            i++;
        }
    }

    private Vector2 GetOffset(Indicator indicator)
    {
        Vector2 offset = Vector2.zero;
        switch (indicator.wheel.wheelPosition)
        {
            case WheelPosition.FrontLeft:
                offset = new Vector2(-200, 400);
                break;
            case WheelPosition.FrontRight:
                offset = new Vector2(200, 400);
                break;
            case WheelPosition.RearLeft:
                offset = new Vector2(-200, -400);
                break;
            case WheelPosition.RearRight:
                offset = new Vector2(200, -400);
                break;
        }
        return offset;
    }

    protected class Indicator
    {
        public Image image;
        public Wheel wheel;
        public Indicator(Image image, Wheel wheel)
        {
            this.image = image;
            this.wheel = wheel;
        }
    }
}

