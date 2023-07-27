using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SegmentChainBuilder;
using static UnityEngine.Rendering.HableCurve;

public class WorldBuilder : MonoBehaviour
{
    [SerializeField] private float seaLevel;
    [SerializeField] private Vector3 cubeSize;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private int roadLinerenderResolution;
    private Camera bakeCamera;
    public bool generateCubes;
    [SerializeField] private List<GameObject> buildings = new List<GameObject>();

    public void GenerateWorldGrid(SegmentChainSettings settings, SegmentChain chain)
    {
        if(!generateCubes)return;

        int size = settings.sidePointAmount * 3;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                GameObject cube = Instantiate(buildings[Random.Range(0, buildings.Count)], chain.transform);
                cube.SetActive(true);
                float offset = (float)settings.gridSize / size;
                float normalize = -((settings.gridSize - offset) / 2f);
                float pointPosition = normalize;
                cube.transform.eulerAngles = new Vector3(0,Random.Range(0,180),0); 
                cube.transform.position = chain.organizedSegments[0].transform.TransformPoint( new Vector3(pointPosition + offset * x, seaLevel, offset * y));
                cube.transform.localScale = cubeSize;
            }
        }
    }

    public void PopulateWorld(SegmentChainSettings settings, SegmentChain chain)
    {
        GameObject lineObject = Instantiate(linePrefab);
        LineRenderer line = lineObject.transform.GetChild(0).GetComponent<LineRenderer>();
        MeshCollider collider = lineObject.transform.GetChild(1).GetComponent<MeshCollider>();
        bakeCamera = lineObject.transform.GetChild(2).GetComponent<Camera>();

        int amount = chain.organizedSegments.Count - 1;
        line.positionCount = roadLinerenderResolution * amount;

        int count = 0;
        for (int i = 0; i < amount; i++) {

            RoadSegment segment = chain.organizedSegments[i];
            segment.CreateBezier();
            line.transform.parent = segment.transform;
            for (int x = 0; x < roadLinerenderResolution; x++) {
                if (x == 0)
                {
                    Vector3 pos0 = segment.bezier.GetOrientedPoint(.01f, Ease.Linear).pos + (Vector3.up * seaLevel);
                    line.SetPosition(0 + count, segment.transform.TransformPoint(pos0));
                    continue;
                }
                if (x == roadLinerenderResolution - 1)
                {
                    Vector3 pos1 = segment.bezier.GetOrientedPoint(.99f, Ease.Linear).pos + (Vector3.up * seaLevel);
                    line.SetPosition(roadLinerenderResolution - 1 + count, segment.transform.TransformPoint(pos1));
                    continue;
                }

                Vector3 pos = segment.bezier.GetOrientedPoint((float)x / (float)roadLinerenderResolution, Ease.Linear).pos + (Vector3.up * seaLevel);
                line.SetPosition(x + count, segment.transform.TransformPoint(pos));
            }
            count += roadLinerenderResolution;
        }
        line.transform.parent = lineObject.transform;
        CreateCollider(line, collider);

        GenerateWorldGrid(settings, chain);
    }

    public void CreateCollider(LineRenderer line, MeshCollider meshCollider)
    {
        Mesh mesh = new Mesh();
        line.BakeMesh(mesh, bakeCamera, false);
        meshCollider.sharedMesh = mesh;
    }
}
