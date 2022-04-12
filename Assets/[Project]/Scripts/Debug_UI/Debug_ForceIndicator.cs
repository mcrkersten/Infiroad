using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Debug_ForceIndicator : MonoBehaviour
{
    public List<Suspension> suspensionToDebug = new List<Suspension>();
    private List<Indicator> indicators = new List<Indicator>();

    public GameObject indicatorPrefab;
    public Gradient colorGradient;

    private void Start()
    {
        foreach (Suspension s in suspensionToDebug)
        {
            indicators.Add(new Indicator(Instantiate(indicatorPrefab, this.transform).GetComponent<Image>(), s));
        }
    }

    private void FixedUpdate()
    {
        int i = 0;
        foreach (Indicator indicator in indicators)
        {
            indicator.wheel.gripDebug = float.IsNaN(indicator.wheel.gripDebug) ? 0f : indicator.wheel.gripDebug;
            Vector2 offset = GetOffset(indicator);
            indicator.image.transform.localScale = Vector3.one * indicator.wheel.gripDebug;
            indicator.image.color = colorGradient.Evaluate(1f - indicator.wheel.gripDebug);

            indicator.wheel.forceDirectionDebug = float.IsNaN(indicator.wheel.forceDirectionDebug.z) ? Vector3.zero : indicator.wheel.forceDirectionDebug;
            indicator.image.transform.localPosition = new Vector3(-indicator.wheel.forceDirectionDebug.x + offset.x, -indicator.wheel.forceDirectionDebug.z + offset.y, 0) / 20f;
            i++;
        }
    }

    private Vector2 GetOffset(Indicator indicator)
    {
        Vector2 offset = Vector2.zero;
        switch (indicator.suspension.suspensionPosition)
        {
            case SuspensionPosition.FrontLeft:
                offset = new Vector2(-200, 400);
                break;
            case SuspensionPosition.FrontRight:
                offset = new Vector2(200, 400);
                break;
            case SuspensionPosition.RearLeft:
                offset = new Vector2(-200, -400);
                break;
            case SuspensionPosition.RearRight:
                offset = new Vector2(200, -400);
                break;
        }
        return offset;
    }

    protected class Indicator
    {
        public Image image;
        public Suspension suspension;
        public Wheel_Raycast wheel;
        public Indicator(Image image, Suspension suspension)
        {
            this.image = image;
            this.suspension = suspension;
            wheel = suspension.wheel;
        }
    }
}

