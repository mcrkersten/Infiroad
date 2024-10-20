using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sector
{
    public float sectorLenght;
    public List<OrientedCubicBezier3D> beziers = new List<OrientedCubicBezier3D>();
    [HideInInspector]public GameObject sectorUI_Element;
    public SegmentChain segmentChain;

    public Sector(SegmentChain chain)
    {
        segmentChain = chain;
        sectorLenght = 0f;
        GenerateBeziers();
    }
    private void GenerateBeziers()
    {
        foreach (RoadSegment segment in segmentChain.organizedSegments)
        {
            if (segment.HasValidNextPoint)
                beziers.Add(segment.GetBezierRepresentation(Space.World));
        }
    }
}
