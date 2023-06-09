using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sector
{
    public float sectorLenght;
    public List<OrientedCubicBezier3D> beziers = new List<OrientedCubicBezier3D>();
    [HideInInspector]public GameObject sectorUI_Element;
    public SegmentChain roadChain;

    public Sector(SegmentChain chain)
    {
        roadChain = chain;
        sectorLenght = 0f;
        GenerateBeziers();
    }
    private void GenerateBeziers()
    {
        foreach (RoadSegment segment in roadChain.organizedSegments)
        {
            if (segment.HasValidNextPoint)
                beziers.Add(segment.GetBezierRepresentation(Space.World));
        }
    }
}
