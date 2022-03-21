using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Dashboard
{
    [SerializeField] private List<DashboardMeter> dashboardMeters = new List<DashboardMeter>();
    public void UpdateMeter(float percentage, DashboardMeter.MeterType meter)
    {
        DashboardMeter dbm = dashboardMeters.First(m => m.meterType == meter);
        dbm.UpdateMeter(percentage);
    }
}
[System.Serializable]
public class DashboardMeter
{
    [SerializeField] private GameObject meterDile;
    public MeterType meterType;
    [SerializeField] private int minAngle;
    [SerializeField] private int maxAngle;
    [SerializeField] private float maxValue;

    public void UpdateMeter(float value)
    {
        float angle;
        switch (meterType)
        {
            case MeterType.Speedometer:
                angle = Mathf.Lerp(minAngle, maxAngle, value/maxValue);
                meterDile.transform.localEulerAngles = new Vector3(0f, angle, 0f);
                break;
            case MeterType.Tacheometer:
                angle = Mathf.Lerp(minAngle, maxAngle, value);
                meterDile.transform.localEulerAngles = new Vector3(0f, angle, 0f);
                break;
            case MeterType.OilPressureMeter:
                break;
        }
    }

    public enum MeterType
    {
        Speedometer = 0,
        Tacheometer,
        OilPressureMeter,
    }
}
