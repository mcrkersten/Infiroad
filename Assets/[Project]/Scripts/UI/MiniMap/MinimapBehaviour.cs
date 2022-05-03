using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapBehaviour : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float minZoom;
    [SerializeField] private float maxZoom;
    [SerializeField] private Transform minimapCamera;
    private Vector3 localStartPosition;

    private void Start()
    {
        rb = transform.root.GetComponent<Rigidbody>();
        localStartPosition = minimapCamera.localPosition;
    }
    // Update is called once per frame
    void Update()
    {
        Vector3 local = new Vector3(localStartPosition.x, Mathf.Lerp(minZoom, maxZoom, (rb.velocity.magnitude) / maxSpeed), localStartPosition.z);
        minimapCamera.transform.localPosition = local;
    }
}
