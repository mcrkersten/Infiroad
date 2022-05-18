using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class RoadDecoration : ScriptableObject
{
    [Tooltip("The segment on where this roadDecoration will spawn")]
    public int wholeUnitsInPool;
    public int segmentIndex;
    public float mainCurveTime;
    public List<Decoration> decor = new List<Decoration>();
    public int poolIndex;
}

[System.Serializable]
public class Decoration
{
    [Header("# When used for Meshtasks")]
    public MeshtaskPoolType meshtaskPoolType;
    public int unitsInPool;
    public GameObject prefab;

    [Header("As Set prop")]
    public Vector2 position;
    public float curveTime;
    public int noiseChannel;
}
