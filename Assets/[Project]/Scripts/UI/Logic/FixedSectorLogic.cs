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
    [SerializeField] private Camera trackRenderCamera;
    [SerializeField] private SegmentChainBuilder roadChainBuilder;
    [SerializeField] private int resolution;
    private List<Sector> sectors = new List<Sector>();
    private Sector createdSector;

    [Header("Prefabs")]
    [SerializeField] private GameObject roadLinerenderPrefab;
    [SerializeField] private GameObject entryExitSpritePrefab;
    [SerializeField] private GameObject sectorButtonPrefab;

    [Header("UI Updatables")]
    [SerializeField] private TextMeshProUGUI sectorLenght;
    [SerializeField] private Transform sectorList;
    private bool connectedViewActive;
    [SerializeField] private LineRendererOrthoFramer lineRendererOrthoFramer;

    [SerializeField] private TextMeshProUGUI startGuide;
    [SerializeField] private TextMeshProUGUI generateGuide;

    private void Awake()
    {
        SectorButton.sectorDeleted += DeleteSector;
        SectorButton.sectorSelected += OnSectorSelection;
        SectorButton.sectorDeselected += OnSectorDeselected;

        buttons.buttons[0].onClick.AddListener(() => AddSectorToListButton());
        buttons.buttons[1].onClick.AddListener(() => GenerateSectorButton());
        buttons.buttons[2].onClick.AddListener(() => StartButton());
        buttons.buttons[3].onClick.AddListener(() => ConnectView());
    }

    private void AddSectorToListButton()
    {
        createdSector.sectorUI_Element = Instantiate(sectorButtonPrefab, sectorList);
        createdSector.sectorUI_Element.GetComponent<SectorButton>().sector = createdSector;
        sectors.Add(createdSector);
        createdSector.sectorUI_Element.GetComponent<SectorButton>().sectorName.text = ("Sector: " + sectors.Count);
        createdSector = null;
        buttons.buttons[0].interactable = false;


        if (sectors.Count > 1)
        {
            startGuide.gameObject.SetActive(false);
            buttons.buttons[2].interactable = true;
            buttons.buttons[2].gameObject.SetActive(true);
            buttons.buttons[3].gameObject.SetActive(true);
        }
    }

    private void GenerateSectorButton()
    {
        if (connectedViewActive)
            ResetFromConnectedView();
        if (createdSector != null)
            Destroy(createdSector.segmentChain.gameObject);

        createdSector = roadChainBuilder.GenerateTimingSector();
        CreateSectorUI_Elements(createdSector);

        if(sectors.Count <= 2)
            buttons.buttons[0].interactable = true;
    }

    private void StartButton()
    {
        GameModeManager.Instance.fixedSectors = sectors;
        foreach (Sector sector in sectors)
            DontDestroyOnLoad(sector.segmentChain.gameObject);
        MainMenuLogic.Instance.ActivateMenu(MenuType.InputSelection); 
    }

    private void CreateSectorUI_Elements(Sector sector)
    {
        DrawSector(sector);
        CreateEntryAndExitSprite(sector);
        PositionCameraOnSector(sector);
        UpdateUI_Elements((int)sector.sectorLenght);
    }

    private void DrawSector(Sector sector)
    {
        //Create line renderer
        LineRenderer line = Instantiate(roadLinerenderPrefab).GetComponent<LineRenderer>();
        line.material = roadChainBuilder.road.roadSettings[sector.segmentChain.ChainIndex].UIMaterial;
        sector.segmentChain.line = line;
        line.transform.parent = sector.segmentChain.transform;
        line.transform.localPosition = Vector3.zero;
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
                //Debug.Log(sector.beziers[i].GetPoint(x / (float)x / (float)resolution));
                line.SetPosition((i * resolution) + x, sector.beziers[i].GetOrientedPoint((float)x / (float)resolution, Ease.Linear).pos);
            }
        }
        lineRendererOrthoFramer.FitNow(line);
    }

    private void CreateEntryAndExitSprite(Sector sector)
    {
        //Create entry and exit sprites
        GameObject entry = Instantiate(entryExitSpritePrefab, sector.segmentChain.organizedSegments[0].transform);
        entry.transform.localPosition = Vector3.zero;
        entry.transform.localRotation = Quaternion.identity;

        GameObject exit = Instantiate(entryExitSpritePrefab, sector.segmentChain.organizedSegments[sector.segmentChain.organizedSegments.Count - 1].transform);
        exit.transform.localPosition = Vector3.zero;
        exit.transform.localRotation = Quaternion.identity;
    }

    private void PositionCameraOnSector(Sector sector)
    {
        if (lineRendererOrthoFramer == null || sector == null || sector.segmentChain == null)
        {
            return;
        }

        lineRendererOrthoFramer.FitNow(sector.segmentChain.line);
    }

    private void PositionCameraOnConnectedView()
    {
        if (lineRendererOrthoFramer == null)
        {
            return;
        }

        List<LineRenderer> targetLines = new List<LineRenderer>(sectors.Count);
        for (int i = 0; i < sectors.Count; i++)
        {
            if (sectors[i] == null || sectors[i].segmentChain == null || sectors[i].segmentChain.line == null)
            {
                continue;
            }

            targetLines.Add(sectors[i].segmentChain.line);
        }

        if (targetLines.Count == 0)
        {
            return;
        }

        if (!TryGetConnectedViewAnchors(targetLines, out Vector3 startAnchor, out Vector3 endAnchor))
        {
            return;
        }

        lineRendererOrthoFramer.FitNow(targetLines.ToArray(), startAnchor, endAnchor);
    }

    private void OnSectorSelection(Sector sector)
    {
        if (connectedViewActive)
            ResetFromConnectedView();

        UpdateUI_Elements((int)sector.sectorLenght);
        PositionCameraOnSector(sector);
        sector.segmentChain.gameObject.SetActive(true);
        if (createdSector != null)
            createdSector.segmentChain.gameObject.SetActive(false);
    }

    private void OnSectorDeselected()
    {
        if (createdSector != null)
        {
            createdSector.segmentChain.gameObject.SetActive(false);
            UpdateUI_Elements((int)createdSector.sectorLenght);
            PositionCameraOnSector(createdSector);
        }
    }

    private void UpdateUI_Elements(int lenght) 
    {
        sectorLenght.text = (lenght).ToString();
    }

    private void DeleteSector(Sector sector)
    {
        Destroy(sector.segmentChain.gameObject);
        Destroy(sector.sectorUI_Element);
        sectors.Remove(sector);

        if (sectors.Count < 2)
        {
            startGuide.gameObject.SetActive(true);
            buttons.buttons[2].gameObject.SetActive(false);
            buttons.buttons[3].gameObject.SetActive(false);
        }

        buttons.buttons[1].Select();
    }

    private void ConnectView()
    {
        if (createdSector != null)
            createdSector.segmentChain.gameObject.SetActive(false);

        float totalLenght = 0;
        for (int i = 0; i < sectors.Count; i++)
        {
            totalLenght += sectors[i].sectorLenght;
            Transform root = sectors[i].segmentChain.organizedSegments[0].transform.root;
            root.gameObject.SetActive(true);
            if (i == 0)
                continue;

            root.rotation = sectors[i - 1].segmentChain.organizedSegments[sectors[i - 1].segmentChain.organizedSegments.Count - 1].transform.rotation;

            GameObject temp = new GameObject("Temp");
            temp.transform.position = sectors[i].segmentChain.organizedSegments[0].transform.position;
            root.transform.parent = temp.transform;
            temp.transform.position = sectors[i - 1].segmentChain.organizedSegments[sectors[i - 1].segmentChain.organizedSegments.Count - 1].transform.position;
            root.transform.parent = null;
            Destroy(temp);
        }
        PositionCameraOnConnectedView();
        UpdateUI_Elements((int)totalLenght);
        connectedViewActive = true;
    }

    private void ResetFromConnectedView()
    {
        for (int i = 0; i < sectors.Count; i++)
        {
            if(sectors[i].segmentChain == null) continue;
            Transform root = sectors[i].segmentChain.transform;
            root.rotation = Quaternion.identity;
            root.position = Vector3.zero;
            root.gameObject.SetActive(false);
        }
        connectedViewActive = false;

    }

    private bool TryGetConnectedViewAnchors(List<LineRenderer> targetLines, out Vector3 startAnchor, out Vector3 endAnchor)
    {
        startAnchor = Vector3.zero;
        endAnchor = Vector3.zero;
        bool hasStartAnchor = false;
        bool hasEndAnchor = false;

        if (targetLines == null || targetLines.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < targetLines.Count; i++)
        {
            if (TryGetLinePointWorld(targetLines[i], 0, out startAnchor))
            {
                hasStartAnchor = true;
                break;
            }
        }

        for (int i = targetLines.Count - 1; i >= 0; i--)
        {
            LineRenderer line = targetLines[i];
            if (line == null)
            {
                continue;
            }

            if (TryGetLinePointWorld(line, line.positionCount - 1, out endAnchor))
            {
                hasEndAnchor = true;
                break;
            }
        }

        return hasStartAnchor && hasEndAnchor;
    }

    private bool TryGetLinePointWorld(LineRenderer line, int pointIndex, out Vector3 worldPoint)
    {
        worldPoint = Vector3.zero;

        if (line == null || line.positionCount < 2 || pointIndex < 0 || pointIndex >= line.positionCount)
        {
            return false;
        }

        worldPoint = line.GetPosition(pointIndex);
        if (!line.useWorldSpace)
        {
            worldPoint = line.transform.TransformPoint(worldPoint);
        }

        return true;
    }

    private void OnDestroy()
    {
        SectorButton.sectorDeleted -= DeleteSector;
        SectorButton.sectorSelected -= OnSectorSelection;
        SectorButton.sectorDeselected -= OnSectorDeselected;

        buttons.buttons[0].onClick.RemoveAllListeners();
        buttons.buttons[1].onClick.RemoveAllListeners();
        buttons.buttons[2].onClick.RemoveAllListeners();
        buttons.buttons[3].onClick.RemoveAllListeners();
    }
}
