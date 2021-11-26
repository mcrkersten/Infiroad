using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadChainBuilder : MonoBehaviour
{
    public RoadChainSettings settings;
    [Header("")]
    public GameObject segmentPrefab;
    public GameObject roadChainPrefab;

    [Header("")]
    public List<RoadChain> createdRoadChains = new List<RoadChain>();

    private Transform currentRoadChainObject;
    private EdgePoint lastEdgePoint;
    public GameObject startSegment;

    [ContextMenu("Test")]
    public void CreateFirstSegment()
    {
        createdRoadChains.Clear();
        CreateNextRoadChain(new EdgePoint(EdgeLocation.none, 3, startSegment));
    }
    
    [ContextMenu("Test_2")]
    public void CreateNextRoadChainManual()
    {
        CreateNextRoadChain(lastEdgePoint);
    }

    public void CreateNextRoadChain(EdgePoint lastExitPoint)
    {
        //Instantiate
        GameObject roadChainObject = Instantiate(roadChainPrefab);
        RoadChain roadChain = roadChainObject.GetComponent<RoadChain>();
        roadChainObject.name = "RoadBlock-" + createdRoadChains.Count;
        currentRoadChainObject = roadChain.transform;
        createdRoadChains.Add(roadChain);

        //Determine and set main position of the roadChainObject
        Vector3 position = CalculateRoadChainObjectPosition(lastExitPoint);
        roadChainObject.transform.position = position;

        //Create entry segment and set entry on edge of the roadChain
        GameObject entrySegment = Instantiate(segmentPrefab, roadChainObject.transform);
        EdgeLocation inverse = InverseEdgeLocation(lastExitPoint.edgeLocation);
        EdgePoint entryPoint = new EdgePoint(inverse, lastExitPoint.edgePointPositionIndex, entrySegment);
        entrySegment.transform.localPosition = GetEdgePointLocalLocationPosition(entryPoint);
        entrySegment.transform.rotation = lastExitPoint.rotation;
        entrySegment.name = "EntrySegment";

        //Create exit segment and set on random position on edge of roadChain
        GameObject exitSegment = Instantiate(segmentPrefab, roadChainObject.transform);
        EdgeLocation nextExit = GetRandomExitLocation(entryPoint);
        EdgePoint exitPoint = new EdgePoint(nextExit, GetRandomExitPointIndex(inverse, nextExit), exitSegment);

        exitSegment.transform.localPosition = GetEdgePointLocalLocationPosition(exitPoint);
        exitSegment.transform.rotation = exitPoint.rotation;
        exitSegment.name = "ExitSegment";

        //Set created exitpoint to roadchain.
        //So the next created roadChain knows where to continue from
        roadChain.entryPoint = entryPoint;
        roadChain.exitPoint = exitPoint;

        //Create random points between entry and exit
        int nPoints = GetPointAmount(lastExitPoint, exitPoint);
        List<Transform> segments = CreatePointsbetween(lastExitPoint, exitPoint, nPoints);
        OrientSegments(segments);
        lastEdgePoint = exitPoint;
    }

    [System.Serializable]
    public class RoadChainSettings
    {
        [Tooltip("Amount of points on the side of a RoadchainBlock")]
        public int sidePointAmount;
        public int gridSize;//Unity UnitSize

        public int straight_NpointsBetween;
        public int corner_NpointsBetween;

        [Header("New segment settings")]
        [Tooltip("The center-distance of the spawncircle from last segment")]
        public float centerDistance;
        [Tooltip("The spawn radius for new segments")]
        public float randonSpawnRadius;
    }

    private EdgeLocation GetRandomExitLocation(EdgePoint entryPoint)
    {
        List<EdgeLocation> possibility = new List<EdgeLocation> { (EdgeLocation)0, (EdgeLocation)1, (EdgeLocation)2, (EdgeLocation)3 };
        possibility.Remove(entryPoint.edgeLocation);

        if (entryPoint.edgeLocation == EdgeLocation.none)
            possibility.Remove(EdgeLocation.Bottom);

        if(entryPoint.edgeLocation == EdgeLocation.Left || entryPoint.edgeLocation == EdgeLocation.Right)
        {
            if (entryPoint.edgePointPositionIndex == 0)
                possibility.Remove(EdgeLocation.Bottom);
            if (entryPoint.edgePointPositionIndex == 4)
                possibility.Remove(EdgeLocation.Top);
        }

        if (entryPoint.edgeLocation == EdgeLocation.Top || entryPoint.edgeLocation == EdgeLocation.Bottom)
        {
            if (entryPoint.edgePointPositionIndex == 0)
                possibility.Remove(EdgeLocation.Left);
            if (entryPoint.edgePointPositionIndex == 4)
                possibility.Remove(EdgeLocation.Right);
        }

        return possibility[Random.Range(0, possibility.Count)];
    }

    private int GetPointAmount(EdgePoint entry, EdgePoint exit)
    {
        int nOfPoints = 0;
        switch (entry.edgeLocation)
        {
            case EdgeLocation.Left:
                if (exit.edgeLocation == EdgeLocation.Right) { nOfPoints = settings.straight_NpointsBetween; }
                else { nOfPoints = nOfPoints = settings.corner_NpointsBetween; }
                break;
            case EdgeLocation.Right:
                if (exit.edgeLocation == EdgeLocation.Left) { nOfPoints = settings.straight_NpointsBetween; }
                else { nOfPoints = nOfPoints = settings.corner_NpointsBetween; }
                break;
            case EdgeLocation.Top:
                if (exit.edgeLocation == EdgeLocation.Bottom) { nOfPoints = settings.straight_NpointsBetween; }
                else { nOfPoints = nOfPoints = settings.corner_NpointsBetween; }
                break;
            case EdgeLocation.Bottom:
                if (exit.edgeLocation == EdgeLocation.Top) { nOfPoints = settings.straight_NpointsBetween; }
                else { nOfPoints = nOfPoints = settings.corner_NpointsBetween; }
                break;
            case EdgeLocation.none:
                if (exit.edgeLocation == EdgeLocation.Top) { nOfPoints = settings.straight_NpointsBetween; }
                else { nOfPoints = nOfPoints = settings.corner_NpointsBetween; }
                break;
        }
        return nOfPoints;
    }

    private Vector3 GetEdgePointLocalLocationPosition(EdgePoint exitPoint)
    {
        float side = ((settings.gridSize) / 2f);

        float offset = (float)settings.gridSize / settings.sidePointAmount;
        float normalize = -((settings.gridSize - offset)/ 2f);

        float addition = offset * exitPoint.edgePointPositionIndex;
        float pointPosition = normalize + addition;
        Debug.Log(pointPosition);

        switch (exitPoint.edgeLocation)
        {
            case EdgeLocation.Left:
                return new Vector3(-side, 0, pointPosition);
            case EdgeLocation.Right:
                return new Vector3(side, 0, pointPosition);
            case EdgeLocation.Top:
                return new Vector3(pointPosition, 0, side);
            case EdgeLocation.Bottom:
                return new Vector3(pointPosition, 0, -side);
            case EdgeLocation.none:
                return new Vector3(0, 0, -side);
        }
        return Vector3.zero;
    }

    private Vector3 CalculateRoadChainObjectPosition(EdgePoint exitPoint)
    {
        Vector3 p = exitPoint.segment.transform.root.transform.position;
        if (exitPoint.edgeLocation == EdgeLocation.none)
            p = Vector3.zero;

        switch (exitPoint.edgeLocation)
        {
            case EdgeLocation.Left:
                p[0] -= settings.gridSize;
                break;
            case EdgeLocation.Right:
                p[0] += settings.gridSize;
                break;
            case EdgeLocation.Top:
                p[2] += settings.gridSize;
                break;
            case EdgeLocation.Bottom:
                p[2] -= settings.gridSize;
                break;
            case EdgeLocation.none:
                break;
        }
        return p;
    }

    private int GetRandomExitPointIndex(EdgeLocation entry, EdgeLocation exit)
    {
        int max = settings.sidePointAmount;
        int min = 0;
        switch (entry)
        {
            case EdgeLocation.Left:
                if (exit != EdgeLocation.Right)
                    max = 3;
                break;
            case EdgeLocation.Right:
                if (exit != EdgeLocation.Left)
                    min = 2;
                break;
            case EdgeLocation.Top:
                if (exit != EdgeLocation.Bottom)
                    max = 3;
                break;
            case EdgeLocation.Bottom:
                if (exit != EdgeLocation.Top)
                    min = 2;
                break;
            case EdgeLocation.none:
                min = 2;
                break;
            default:
                break;
        }
        return Random.Range(min, max);
    }

    private List<Transform> CreatePointsbetween(EdgePoint entry, EdgePoint exit, int nOfPoints)
    {
        GameObject lastFromEntrySegemnt = entry.segment;
        GameObject lastFromExitSegemnt = exit.segment;
        List<GameObject> segementsFromEntry = new List<GameObject>();
        List<GameObject> segementsFromExit = new List<GameObject>();
        for (int i = 0; i < nOfPoints; i++)
        {
            if(i%2f == 0f) //from EnytyPoint
            {
                Vector2 rand = Random.insideUnitCircle;
                Vector3 randFlat = new Vector3(rand.x, 0, rand.y);
                Vector3 centerPoint = lastFromEntrySegemnt.transform.position + (lastFromEntrySegemnt.transform.forward * settings.centerDistance);
                Vector3 randomPoint = randFlat * settings.randonSpawnRadius;
                Vector3 point = centerPoint + randomPoint;
                GameObject ins = Instantiate(segmentPrefab, currentRoadChainObject);
                lastFromEntrySegemnt = ins;
                segementsFromEntry.Add(ins);
                ins.transform.position = point;
                ins.name = "Segment_" + i;
                ins.transform.SetSiblingIndex(i);
            }
            else //From ExitPoint
            {
                Vector2 rand = Random.insideUnitCircle;
                Vector3 randFlat = new Vector3(rand.x, 0, rand.y);
                Vector3 centerPoint = lastFromExitSegemnt.transform.position + (-lastFromExitSegemnt.transform.forward * settings.centerDistance);
                Vector3 randomPoint = randFlat * settings.randonSpawnRadius;
                Vector3 point = centerPoint + randomPoint;
                GameObject ins = Instantiate(segmentPrefab, currentRoadChainObject);
                lastFromExitSegemnt = ins;
                segementsFromExit.Add(ins);
                ins.transform.position = point;
                ins.name = "Segment_" + i;
            }
        }

        return SetPointsInOrder(segementsFromEntry, segementsFromExit);
    }

    private List<Transform> SetPointsInOrder(List<GameObject> segementsFromEntry, List<GameObject> segementsFromExit)
    {
        List<Transform> segments = new List<Transform>();
        GameObject ent = GetChildWithName(currentRoadChainObject.gameObject, "EntrySegment");
        GameObject ex = GetChildWithName(currentRoadChainObject.gameObject, "ExitSegment");

        int index = 0;
        foreach (var item in segementsFromEntry)
        {
            item.transform.SetSiblingIndex(index);
            segments.Add(item.transform);
            index++;
        }
        foreach (var item in segementsFromExit)
        {
            item.transform.SetSiblingIndex(index);
            segments.Add(item.transform);
            index++;
        }

        segments.Insert(0, ent.transform);
        segments.Add(ex.transform);

        ex.transform.SetSiblingIndex(index);
        ent.transform.SetAsFirstSibling();

        return segments;
    }

    private void OrientSegments(List<Transform> segments)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            if(i > 0 && i < segments.Count - 1)
            {
                Vector3 infront = new Vector3(segments[i + 1].position.x, 0, segments[i + 1].position.z);
                Vector3 behind = new Vector3(segments[i - 1].position.x, 0, segments[i - 1].position.z);
                Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, infront - behind);
                segments[i].rotation = rotation;
            }
        }
    }

    private GameObject GetChildWithName(GameObject obj, string name)
    {
        Transform trans = obj.transform;
        Transform childTrans = trans.Find(name);
        if (childTrans != null)
        {
            return childTrans.gameObject;
        }
        else
        {
            return null;
        }
    }

    private EdgeLocation InverseEdgeLocation(EdgeLocation toInverse)
    {
        EdgeLocation inverse = EdgeLocation.none;
        switch (toInverse)
        {
            case EdgeLocation.Left:
                inverse = EdgeLocation.Right;
                break;
            case EdgeLocation.Right:
                inverse = EdgeLocation.Left;
                break;
            case EdgeLocation.Top:
                inverse = EdgeLocation.Bottom;
                break;
            case EdgeLocation.Bottom:
                inverse = EdgeLocation.Top;
                break;
            case EdgeLocation.none:
                break;
        }
        return inverse;
    }
}

[System.Serializable]
public class EdgePoint
{
    public EdgeLocation edgeLocation;
    public int edgePointPositionIndex; //Point on side.
    public Quaternion rotation { get { return GetEdgeRotation(); } }
    public GameObject segment { get { return Segment; } }
    private GameObject Segment;
    public EdgePoint(EdgeLocation edge ,int index, GameObject segment)
    {
        this.edgePointPositionIndex = index;
        this.edgeLocation = edge;
        this.Segment = segment;
    }

    private Quaternion GetEdgeRotation()
    {
        Quaternion rotation = Quaternion.identity;
        switch (edgeLocation)
        {
            case EdgeLocation.Left:
                rotation = Quaternion.Euler(0, -90, 0);
                break;
            case EdgeLocation.Right:
                rotation = Quaternion.Euler(0, 90, 0);
                break;
            case EdgeLocation.Top:
                rotation = Quaternion.Euler(0, 0, 0);
                break;
            case EdgeLocation.Bottom:
                rotation = Quaternion.Euler(0, 180, 0);
                break;
            case EdgeLocation.none:
                rotation = Quaternion.Euler(0, 0, 0);
                break;
            default:
                break;
        }
        return rotation;
    }
}

public enum EdgeLocation
{
    Left = 0,
    Right,
    Top,
    Bottom,
    none //automatically becomes bottom entry
}
