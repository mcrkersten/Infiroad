using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class RoadDecoration : ScriptableObject
{
    [Tooltip("The segment on where this roadDecoration will spawn")]
    public int segmentIndex;
    public int wholeUnitsInPool;
    public float mainCurveTime;
    public List<Decoration> decor = new List<Decoration>();
    public int poolIndex;
}

[System.Serializable]
public class Decoration
{
    public GameObject prefab;
    public Vector2 position;
    public float curveTime;
    public int noiseChannel;
}
