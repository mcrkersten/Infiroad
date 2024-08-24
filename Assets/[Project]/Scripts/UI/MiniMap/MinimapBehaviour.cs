using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DG.Tweening;

public class MinimapBehaviour : MonoBehaviour
{
    [Header("Content")]
    [SerializeField] private Camera minimapCamera;
    private Rigidbody rb;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float minZoom;
    [SerializeField] private float maxZoom;
    private Vector3 localStartPosition;
    [SerializeField] private Ui_AnimationObject minimapScaleAnimation; 

    public GameObject roadLinerenderPrefab;
    public int roadLinerenderResolution;

    private bool isZoomed;

    [SerializeField] private Material material;

    private void Start()
    {
        minimapScaleAnimation.Init();
        rb = transform.root.GetComponent<Rigidbody>();
        localStartPosition = minimapCamera.transform.localPosition;

        if(GameModeManager.Instance.fixedSectors.Count != 0) {
            foreach (Sector item in GameModeManager.Instance.fixedSectors)
            {
                Destroy(item.roadChain.organizedSegments[0].transform.GetChild(0).gameObject);
                Destroy(item.roadChain.organizedSegments[item.roadChain.organizedSegments.Count - 1].transform.GetChild(0).gameObject);
                item.roadChain.line.material = material;
                item.roadChain.line.widthCurve.keys[0].value = 5;
            }
        }
    }

    public void GenerateMinimapRoadSegment(SegmentChain roadChain, RoadSegment segment)
    {
                //Create line renderer
        LineRenderer line = Instantiate(roadLinerenderPrefab).GetComponent<LineRenderer>();
        roadChain.line = line;
        line.transform.parent = segment.transform;
        line.transform.position = Vector3.zero;
        line.positionCount = roadLinerenderResolution;

        for (int x = 0; x < roadLinerenderResolution; x++)
        {
            if (x == 0)
            {
                if(segment.bezier.Equals(0)) { Debug.Log("LOL"); }
                Vector3 pos0 = segment.bezier.GetOrientedPoint(.0001f, Ease.Linear).pos;
                line.SetPosition(0, segment.transform.TransformPoint(pos0));
                continue;
            }
            if (x == roadLinerenderResolution - 1)
            {
                Vector3 pos1 = segment.bezier.GetOrientedPoint(.9999f, Ease.Linear).pos;
                line.SetPosition(roadLinerenderResolution - 1, segment.transform.TransformPoint(pos1));
                continue;
            }

            Vector3 pos = segment.bezier.GetOrientedPoint((float)x / (float)roadLinerenderResolution, Ease.Linear).pos;
            line.SetPosition(x, segment.transform.TransformPoint(pos));
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 local = new Vector3(localStartPosition.x, Mathf.Lerp(minZoom, maxZoom, (rb.velocity.magnitude) / maxSpeed), localStartPosition.z);
        minimapCamera.transform.localPosition = local;
        minimapCamera.orthographicSize = Mathf.Lerp(minZoom, maxZoom, (rb.velocity.magnitude) / maxSpeed);
    }

    public void ScaleMinimap(InputAction.CallbackContext obj)
    {
        Debug.Log("SCALE");
        if (isZoomed)
        {
            minimapScaleAnimation.Animate_ToStartScale();
            isZoomed = false;
        }
        else
        {
            minimapScaleAnimation.Animate_ToScale();
            isZoomed = true;
        }
    }
}
