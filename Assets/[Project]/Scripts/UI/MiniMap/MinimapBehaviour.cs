using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapBehaviour : MonoBehaviour
{
    [SerializeField] private Camera minimapCamera;
    private Rigidbody rb;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float minZoom;
    [SerializeField] private float maxZoom;
    private Vector3 localStartPosition;

    [SerializeField] private Material material;

    private void Start()
    {
        rb = transform.root.GetComponent<Rigidbody>();
        localStartPosition = minimapCamera.transform.localPosition;

        foreach (Sector item in GameModeManager.Instance.fixedSectors)
        {
            Destroy(item.roadChain.organizedSegments[0].transform.GetChild(0).gameObject);
            Destroy(item.roadChain.organizedSegments[item.roadChain.organizedSegments.Count - 1].transform.GetChild(0).gameObject);
            item.roadChain.line.material = material;
            item.roadChain.line.widthCurve.keys[0].value = 5;
        }

    }
    // Update is called once per frame
    void Update()
    {

        Vector3 local = new Vector3(localStartPosition.x, Mathf.Lerp(minZoom, maxZoom, (rb.velocity.magnitude) / maxSpeed), localStartPosition.z);
        minimapCamera.transform.localPosition = local;
        minimapCamera.orthographicSize = Mathf.Lerp(minZoom, maxZoom, (rb.velocity.magnitude) / maxSpeed);
    }
}
