using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

[System.Serializable]
public class Dashboard
{
    [SerializeField] private GearLights gearLights;
    [SerializeField] private List<DashboardMeter> dashboardMeters = new List<DashboardMeter>();
    public void UpdateMeter(float percentage, DashboardMeter.MeterType meter)
    {
        DashboardMeter dbm = dashboardMeters.First(m => m.meterType == meter);
        dbm.UpdateMeter(percentage);
    }

    public void OnChangeGear(int gear)
    {
        gearLights.UpdateGear(gear);
    }
}
[System.Serializable]
public class DashboardMeter
{
    [SerializeField] private GameObject meterDile;
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
                break;
            case MeterType.Tacheometer:
                angle = Mathf.Lerp(minAngle, maxAngle, value);
                meterDile.transform.localEulerAngles = new Vector3(0f, angle, 0f);
                break;
            case MeterType.OilPressureMeter:
                break;
        }

        if (meterText.Count != 0)
        {
            string s = (Mathf.Abs((int)value)).ToString();
            while (s.Length < meterText.Count)
                s = "0" + s;
            Debug.Log(s[0]);
            for (int i = 0; i < s.Length; i++)
                meterText[i].text = s[i].ToString();
        }
    }
    
    public enum MeterType
    {
        Speedometer = 0,
        Tacheometer,
        OilPressureMeter,
    }
}
[System.Serializable]
public class GearLights
{
    [SerializeField] private Material lit;
    [SerializeField] private Material unlit;
    [SerializeField] private List<MeshRenderer> gearLights = new List<MeshRenderer>();
    public void UpdateGear(int gear)
    {
        for (int i = 0; i < gearLights.Count; i++)
        {
            if (i == gear)
                gearLights[i].material = lit;
            else
                gearLights[i].material = unlit;
        }
    }
}