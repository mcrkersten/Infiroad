using System.Collections.Generic;
using UnityEngine;

public class LineRendererOrthoFramer : MonoBehaviour
{
    private const float MinOrthoSize = 0.0001f;
    private const float MinAxisMagnitude = 0.0001f;

    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private RenderTexture referenceTexture;

    [Header("Fallback Render Size")]
    [SerializeField] private int fallbackWidth = 2200;
    [SerializeField] private int fallbackHeight = 1020;

    [Header("Behaviour")]
    // Preserved for serialized scene compatibility; framing now always recenters on the rotated fit result.
    [SerializeField] private bool centerCameraOnBounds = true;
    [SerializeField] private float padding = 1.05f;


    public void FitNow(LineRenderer targetLineRenderer)
    {
        if (targetCamera == null || targetLineRenderer == null)
        {
            return;
        }

        if (targetLineRenderer.positionCount < 2)
        {
            return;
        }

        if (!TryGetLineEndpointsWorld(targetLineRenderer, out Vector3 startAnchor, out Vector3 endAnchor))
        {
            return;
        }

        FitNow(new[] { targetLineRenderer }, startAnchor, endAnchor);
    }

    public void FitNow(LineRenderer[] targetLines, Vector3 startAnchor, Vector3 endAnchor)
    {
        if (targetCamera == null || targetLines == null || targetLines.Length == 0)
        {
            return;
        }

        if (!TryCollectWorldPoints(targetLines, out List<Vector3> worldPoints))
        {
            return;
        }

        AlignCameraYaw(startAnchor, endAnchor);
        float aspect = GetTargetAspect();
        targetCamera.orthographic = true;
        targetCamera.aspect = aspect;

        if (!TryCalculateFrame(worldPoints, out Vector3 worldCenter, out float halfWidth, out float halfHeight))
        {
            return;
        }

        Vector3 camPos = targetCamera.transform.position;
        camPos.x = worldCenter.x;
        camPos.z = worldCenter.z;
        targetCamera.transform.position = camPos;

        float requiredOrthoSize = Mathf.Max(halfHeight, halfWidth / Mathf.Max(aspect, MinOrthoSize));
        targetCamera.orthographicSize = Mathf.Max(MinOrthoSize, requiredOrthoSize * Mathf.Max(1f, padding));
    }

    private float GetTargetAspect()
    {
        if (referenceTexture != null && referenceTexture.height > 0)
        {
            return (float)referenceTexture.width / referenceTexture.height;
        }

        int height = Mathf.Max(1, fallbackHeight);
        int width = Mathf.Max(1, fallbackWidth);
        return (float)width / height;
    }

    private void AlignCameraYaw(Vector3 startAnchor, Vector3 endAnchor)
    {
        Vector3 flattenedAnchor = endAnchor - startAnchor;
        flattenedAnchor.y = 0f;

        if (flattenedAnchor.sqrMagnitude <= MinAxisMagnitude * MinAxisMagnitude)
        {
            return;
        }

        flattenedAnchor.Normalize();
        bool alignToHorizontalAxis = GetTargetAspect() >= 1f;
        float targetYaw = alignToHorizontalAxis
            ? Mathf.Atan2(-flattenedAnchor.z, flattenedAnchor.x) * Mathf.Rad2Deg
            : Mathf.Atan2(flattenedAnchor.x, flattenedAnchor.z) * Mathf.Rad2Deg;
        targetYaw += 180f;

        Vector3 currentEuler = targetCamera.transform.rotation.eulerAngles;
        currentEuler.y = targetYaw;
        targetCamera.transform.rotation = Quaternion.Euler(currentEuler);
    }

    private bool TryCalculateFrame(List<Vector3> worldPoints, out Vector3 worldCenter, out float halfWidth, out float halfHeight)
    {
        worldCenter = Vector3.zero;
        halfWidth = 0f;
        halfHeight = 0f;

        if (worldPoints == null || worldPoints.Count < 2)
        {
            return false;
        }

        Vector3 horizontalAxis = Vector3.ProjectOnPlane(targetCamera.transform.right, Vector3.up);

        if (horizontalAxis.sqrMagnitude <= MinAxisMagnitude * MinAxisMagnitude)
        {
            horizontalAxis = Vector3.right;
        }
        else
        {
            horizontalAxis.Normalize();
        }

        Vector3 verticalAxis = Vector3.ProjectOnPlane(targetCamera.transform.up, Vector3.up);
        if (verticalAxis.sqrMagnitude <= MinAxisMagnitude * MinAxisMagnitude)
        {
            verticalAxis = new Vector3(-horizontalAxis.z, 0f, horizontalAxis.x);
        }
        else
        {
            verticalAxis.Normalize();
        }

        float minHorizontal = float.PositiveInfinity;
        float maxHorizontal = float.NegativeInfinity;
        float minVertical = float.PositiveInfinity;
        float maxVertical = float.NegativeInfinity;

        for (int i = 0; i < worldPoints.Count; i++)
        {
            Vector3 worldPoint = worldPoints[i];
            worldPoint.y = 0f;

            float horizontal = Vector3.Dot(worldPoint, horizontalAxis);
            float vertical = Vector3.Dot(worldPoint, verticalAxis);

            minHorizontal = Mathf.Min(minHorizontal, horizontal);
            maxHorizontal = Mathf.Max(maxHorizontal, horizontal);
            minVertical = Mathf.Min(minVertical, vertical);
            maxVertical = Mathf.Max(maxVertical, vertical);
        }

        float centerHorizontal = (minHorizontal + maxHorizontal) * 0.5f;
        float centerVertical = (minVertical + maxVertical) * 0.5f;

        Vector3 centerOnPlane = (horizontalAxis * centerHorizontal) + (verticalAxis * centerVertical);
        worldCenter = new Vector3(centerOnPlane.x, targetCamera.transform.position.y, centerOnPlane.z);
        halfWidth = (maxHorizontal - minHorizontal) * 0.5f;
        halfHeight = (maxVertical - minVertical) * 0.5f;
        return true;
    }

    private bool TryCollectWorldPoints(LineRenderer[] targetLines, out List<Vector3> worldPoints)
    {
        worldPoints = new List<Vector3>();

        for (int i = 0; i < targetLines.Length; i++)
        {
            LineRenderer targetLine = targetLines[i];
            if (targetLine == null || targetLine.positionCount <= 0)
            {
                continue;
            }

            for (int pointIndex = 0; pointIndex < targetLine.positionCount; pointIndex++)
            {
                worldPoints.Add(GetWorldPoint(targetLine, pointIndex));
            }
        }

        return worldPoints.Count >= 2;
    }

    private bool TryGetLineEndpointsWorld(LineRenderer targetLineRenderer, out Vector3 startPoint, out Vector3 endPoint)
    {
        startPoint = Vector3.zero;
        endPoint = Vector3.zero;

        if (targetLineRenderer == null || targetLineRenderer.positionCount < 2)
        {
            return false;
        }

        startPoint = GetWorldPoint(targetLineRenderer, 0);
        endPoint = GetWorldPoint(targetLineRenderer, targetLineRenderer.positionCount - 1);
        return true;
    }

    private Vector3 GetWorldPoint(LineRenderer lineRenderer, int index)
    {
        Vector3 point = lineRenderer.GetPosition(index);
        if (lineRenderer.useWorldSpace)
        {
            return point;
        }

        return lineRenderer.transform.TransformPoint(point);
    }
}
