using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

[System.Serializable]
public class Dashboard
{
    [SerializeField] private List<DashboardMeter> dashboardMeters = new List<DashboardMeter>();
    public void UpdateMeter(float percentage, DashboardMeter.MeterType meter)
    {
        DashboardMeter dbm = dashboardMeters.First(m => m.meterType == meter);
        dbm.UpdateMeter(percentage);
    }

    public void UpdateGear(int gear, DashboardMeter.MeterType meter)
    {
        DashboardMeter dbm = dashboardMeters.First(m => m.meterType == meter);
        dbm.UpdateGear(gear);
    }
}
[System.Serializable]
public class DashboardMeter
{
    [SerializeField] private GameObject meterDile;
    [SerializeField] private RevLights revLights;
    [SerializeField] private List<TextMeshProUGUI> meterText = new List<TextMeshProUGUI>();
    public MeterType meterType;
    [SerializeField] private int minAngle;
    [SerializeField] private int maxAngle;
    [SerializeField] private float maxValue;

    public void UpdateMeter(float value)
    {
        float angle;
        value = float.IsNaN(value) ? maxValue : value;
        value = float.IsInfinity(value) ? maxValue : value;
        switch (meterType)
        {
            case MeterType.Speedometer:
                angle = Mathf.Lerp(minAngle, maxAngle, value/maxValue);
                meterDile.transform.localEulerAngles = new Vector3(0f, angle, 0f);
                if (meterText.Count != 0)
                {
                    string s = (Mathf.Abs((int)value)).ToString();
                    while (s.Length < meterText.Count)
                        s = "0" + s;
                    for (int i = 0; i < s.Length; i++)
                        meterText[i].text = s[i].ToString();
                }
                break;
            case MeterType.Tacheometer:
                angle = Mathf.Lerp(minAngle, maxAngle, value);
                meterDile.transform.localEulerAngles = new Vector3(0f, angle, 0f);
                revLights.UpdateRevLights(value);
                break;
            case MeterType.OilPressureMeter:
                break;
        }
    }

    public void UpdateGear(int gear)
    {
        if(gear == 0)
            meterText[0].text = "N";
        else
            meterText[0].text = gear.ToString();
    }
    
    public enum MeterType
    {
        Speedometer = 0,
        Tacheometer,
        OilPressureMeter,
    }
}
[System.Serializable]
public class RevLights
{
    [SerializeField] private List<RevLight> revLights = new List<RevLight>();
    public void UpdateRevLights(float percentage)
    {
        for (int i = 0; i < revLights.Count; i++)
        {
            if (percentage * 9f > i)
                revLights[i].LitLight();
            else
                revLights[i].UnlitLight();
        }
    }
}