using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class FixedSectorLogic : MonoBehaviour
{
    [SerializeField] private Buttons buttons;

    [Header("Track generator")]
    [SerializeField] private Transform trackRenderCamera;
    [SerializeField] private RoadChainBuilder roadChainBuilder;
    [SerializeField] private int segmtentAmount;
    [SerializeField] private int resolution;
    private List<Sector> sectors = new List<Sector>();
    private Sector selectedSector;
    [Header("Prefabs")]
    [SerializeField] private GameObject roadLinerenderPrefab;
    [SerializeField] private GameObject entryExitSpritePrefab;
    [SerializeField] private GameObject sectorButtonPrefab;
    [Header("UI Updatables")]
    [SerializeField] private TextMeshProUGUI sectorLenght;
    [SerializeField] private Transform sectorList;

    private void Awake()
    {
        SectorButton.sectorDeleted += DeleteSector;
        SectorButton.sectorSelected += OnSectorSelection;
        SectorButton.sectorDeselected += OnSectorDeselected;

        buttons.buttons[0].onClick.AddListener(() => AddSectorToListButton());
        buttons.buttons[1].onClick.AddListener(() => GenerateSectorButton());
        buttons.buttons[2].onClick.AddListener(() => StartButton());
    }

    private void AddSectorToListButton()
    {
        selectedSector.sectorUI_Element = Instantiate(sectorButtonPrefab, sectorList);
        selectedSector.sectorUI_Element.GetComponent<SectorButton>().sector = selectedSector;
        sectors.Add(selectedSector);
        selectedSector.sectorUI_Element.GetComponent<SectorButton>().sectorName.text = ("Sector: " + sectors.Count);
        selectedSector = null;
        buttons.buttons[0].interactable = false;
        buttons.buttons[2].interactable = true;
    }

    private void GenerateSectorButton()
    {
        if (selectedSector != null)
            Destroy(selectedSector.segments[0].transform.parent.gameObject);

        selectedSector = roadChainBuilder.GenerateSector(segmtentAmount);
        CreateSectorUI_Elements(selectedSector);
        buttons.buttons[0].interactable = true;
    }

    private void StartButton()
    {

    }

    private void CreateSectorUI_Elements(Sector sector)
    {
        DrawSector(sector);
        CreateEntryAndExitSprite(sector);
        PositionCamera(sector);
        UpdateUI_Elements(sector);
    }

    private void DrawSector(Sector sector)
    {
        //Create line renderer
        LineRenderer line = Instantiate(roadLinerenderPrefab).GetComponent<LineRenderer>();
        line.transform.parent = sector.segments[0].transform.parent;
        sector.line = line;
        line.positionCount = resolution * sector.beziers.Count;

        for (int i = 0; i < sector.beziers.Count; i++)
        {
            sector.sectorLenght += sector.beziers[i].GetArcLength();
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
    }

    private void CreateEntryAndExitSprite(Sector sector)
    {
        //Create entry and exit sprites
        GameObject entry = Instantiate(entryExitSpritePrefab, sector.segments[0].transform);
        entry.transform.localPosition = Vector3.zero;
        entry.transform.localRotation = Quaternion.identity;

        GameObject exit = Instantiate(entryExitSpritePrefab, sector.segments[sector.segments.Count - 1].transform);
        exit.transform.localPosition = Vector3.zero;
        exit.transform.localRotation = Quaternion.identity;
    }

    private void PositionCamera(Sector sector)
    {
        Vector3 first = sector.segments[0].transform.position;
        Vector3 last = sector.segments[sector.segments.Count - 1].transform.position;
        Vector3 position = Vector3.zero;
        position = Vector3.Lerp(first, last, .5f);
        trackRenderCamera.position = position + (Vector3.up * 50f);
        trackRenderCamera.rotation = Quaternion.LookRotation(first - last) * Quaternion.Euler(new Vector3(90, 0, 90));
    }

    private void OnSectorSelection(Sector sector)
    {
        UpdateUI_Elements(sector);
        PositionCamera(sector);
        if (selectedSector != null)
            selectedSector.segments[0].transform.parent.gameObject.SetActive(false);
    }

    private void OnSectorDeselected()
    {
        if (selectedSector != null)
        {
            selectedSector.segments[0].transform.parent.gameObject.SetActive(true);
            UpdateUI_Elements(selectedSector);
            PositionCamera(selectedSector);
        }
    }

    private void UpdateUI_Elements(Sector sector) 
    {
        sectorLenght.text = ((int)sector.sectorLenght).ToString();
    }

    private void DeleteSector(Sector sector)
    {
        Destroy(sector.segments[0].transform.parent.gameObject);
        Destroy(sector.sectorUI_Element);
        sectors.Remove(sector);
        if(sectors.Count == 0)
            buttons.buttons[2].interactable = false;
        buttons.buttons[1].Select();
    }
}
[System.Serializable]
public class Sector
{
    public float sectorLenght;
    public LineRenderer line;
    public List<RoadSegment> segments = new List<RoadSegment>();
    public List<OrientedCubicBezier3D> beziers = new List<OrientedCubicBezier3D>();
    public GameObject sectorUI_Element;

    public Sector(List<RoadSegment> segments)
    {
        sectorLenght = 0f;
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
