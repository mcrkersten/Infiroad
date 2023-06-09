using UnityEngine;

public class RoomRay
{
    [SerializeField]
    public float RayDistance { get; private set; }
    
    public Vector3 RayPosition { get; private set; }

    public RoomRay(Vector3 rayPosition, float distance)
    {
        this.RayPosition = rayPosition;
        this.RayDistance = distance;
    }
}