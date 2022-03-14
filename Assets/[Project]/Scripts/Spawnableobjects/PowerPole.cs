using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerPole : MonoBehaviour
{
    [HideInInspector] public PowerPole connectedPole;
    [SerializeField] private Transform frontTop;
    [SerializeField] private Transform frontBottom;
    [SerializeField] private Transform rearTop;
    [SerializeField] private Transform rearBottom;

    public Connectors frontTopConnectors;
    public Connectors frontBottomConnectors;

    public Connectors rearTopConnectors;
    public Connectors rearBottomConnectors;

    public void OnInstantiation()
    {
        frontTopConnectors = new Connectors(frontTop.GetChild(0), frontTop.GetChild(1), frontTop.GetChild(2));
        frontBottomConnectors = new Connectors(frontBottom.GetChild(0), frontBottom.GetChild(1), frontBottom.GetChild(2));
        rearTopConnectors = new Connectors(rearTop.GetChild(0), rearTop.GetChild(1), rearTop.GetChild(2));
        rearBottomConnectors = new Connectors(rearBottom.GetChild(0), rearBottom.GetChild(1), rearBottom.GetChild(2));
    }
}
[System.Serializable]
public class Connectors
{
    public Connectors(Transform L, Transform M, Transform R)
    {
        this.L = L;
        this.M = M;
        this.R = R;
    }

    public Transform L;
    public Transform M;
    public Transform R;
}

