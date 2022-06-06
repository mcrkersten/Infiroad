using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehiclePlatform : MonoBehaviour
{
    private float dragDistance;
    private Vector3 positionLastFrame;
    [SerializeField] private Transform platform;
    [SerializeField] private Transform vehicle;
    bool isDragging;

    private void Start()
    {
        vehicle.transform.parent = platform;
    }

    private void Update()
    {
        if (!isDragging)
        {
            dragDistance = Mathf.Lerp(dragDistance, 0f, Time.fixedDeltaTime);
            platform.Rotate(Vector3.up, -dragDistance / 4f);
        }
    }

    private void OnMouseDown()
    {
        isDragging = true;
        positionLastFrame = Input.mousePosition;
    }

    private void OnMouseDrag()
    {
        dragDistance = Input.mousePosition.x - positionLastFrame.x;
        positionLastFrame = Input.mousePosition;
        platform.Rotate(Vector3.up, -dragDistance/4f);
    }

    private void OnMouseUp()
    {
        isDragging = false;
    }
}
