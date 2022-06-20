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

    private bool isZoomed;

    [SerializeField] private Material material;

    private void Start()
    {
        minimapScaleAnimation.Init();
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
