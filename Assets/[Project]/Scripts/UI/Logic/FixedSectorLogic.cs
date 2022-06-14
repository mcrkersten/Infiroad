using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class FixedSectorLogic : MonoBehaviour
{
    [SerializeField] private Transform camera;
    [SerializeField] private int segmtentAmount;
    [SerializeField] private Buttons buttons;
    [SerializeField] private RoadChainBuilder roadChainBuilder;
    [SerializeField] private List<Sector> sectors = new List<Sector>();
    [SerializeField] private Sector selectedSector;

    [SerializeField] private int resolution;
    [SerializeField] private GameObject roadLinerenderPrefab;
    [SerializeField] private List<LineRenderer> lines = new List<LineRenderer>();

    [SerializeField] private Transform sectorButtonParent;
    [SerializeField] private GameObject sectorButtonPrefab;
    private void Awake()
    {
        buttons.buttons[0].onClick.AddListener(() => AddSectorToListButton());
        buttons.buttons[1].onClick.AddListener(() => GenerateSectorButton());
        buttons.buttons[2].onClick.AddListener(() => AutoGenerateButton());
        buttons.buttons[3].onClick.AddListener(() => StartButton());
    }

    private void AddSectorToListButton()
    {

    }

    private void GenerateSectorButton()
    {
        if (selectedSector != null)
            Destroy(selectedSector.roadChain);

        selectedSector = roadChainBuilder.GenerateSector(segmtentAmount);
        DrawSector(selectedSector);
    }

    private void AutoGenerateButton()
    {

    }

    private void StartButton()
    {

    }

    private void DrawSector(Sector sector)
    {
        LineRenderer line = Instantiate(roadLinerenderPrefab).GetComponent<LineRenderer>();
        line.positionCount = resolution * sector.beziers.Count;
        for (int i = 0; i < sector.beziers.Count; i++)
        {
            for (int x = 0; x < resolution; x++)
            {
                if (x == 0)
                {
                    line.SetPosition((i * resolution) + 0, sector.beziers[i].GetOrientedPoint(.0001f, Ease.Linear).pos);
                    continue;
                }
                if (x == resolution - 1)
                {
                    line.SetPosition((i * resolution) + resolution - 1, sector.beziers[i].GetOrientedPoint(.9999f, Ease.Linear).pos);
                    continue;
                }
                Debug.Log(sector.beziers[i].GetPoint(x / (float)x / (float)resolution));
                line.SetPosition((i * resolution) + x, sector.beziers[i].GetOrientedPoint((float)x / (float)resolution, Ease.Linear).pos);
            }
        }

        Vector3 cameraPosition = Vector3.Lerp(sector.segments[0].transform.position, sector.segments[sector.segments.Count - 1].transform.position, .5f);
        camera.position = cameraPosition
    }
}

public class Sector
{
    public RoadChain roadChain;
    public List<RoadSegment> segments = new List<RoadSegment>();
    public List<OrientedCubicBezier3D> beziers = new List<OrientedCubicBezier3D>();

    public Sector(List<RoadSegment> segments)
    {
        this.segments = segments;
        GenerateBeziers();
    }
    private void GenerateBeziers()
    {
        foreach (RoadSegment segment in segments)
        {
            if(segment.HasValidNextPoint)
                beziers.Add(segment.GetBezierRepresentation(Space.World));
        }
    }
}
