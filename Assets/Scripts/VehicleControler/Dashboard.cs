using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dashboard
{
    [SerializeField] private GameObject tachometerDile;
    [SerializeField] private int minAngle;
    [SerializeField] private int maxAngle;

    public void UpdateTechometerDile(float percentage)
    {
        float angle = Mathf.Lerp(minAngle, maxAngle, percentage);
        tachometerDile.transform.localEulerAngles = new Vector3(0f, angle, 0f);
    }
}
