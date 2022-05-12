using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class MeshtaskSettings : ScriptableObject
{
    public MeshTaskType meshTaskType;
    public VertexPosition[] points;
    public bool meshtaskContinues;
    public int noiseChannel;
    public int PointCount => points.Length;

    [Space]
    public Material material;

    public bool meshIsClosed;
}
