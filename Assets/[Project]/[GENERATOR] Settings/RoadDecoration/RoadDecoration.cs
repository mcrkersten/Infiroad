using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class RoadDecoration : ScriptableObject
{
    public float mainCurveTime;
    public List<Decoration> decor = new List<Decoration>();
}

[System.Serializable]
public class Decoration
{
    public GameObject prefab;
    public Vector2 position;
    public float curveTime;
    public int noiseChannel;
}
