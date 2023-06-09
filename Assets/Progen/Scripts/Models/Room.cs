using System.Linq;
using UnityEngine;

public class Room
{
    public Wall[] Walls { get; set; } = new Wall[4];
    
    private Vector2 position;

    public bool HasRoof { get; set; }
    
    public RoomRay RoomRay { get; private set; }

    public int FloorNumber { get; set; }

    public bool HasRoundedCorner
    {
        get 
        {
            return Walls.Where(w => w.WallCornerTypeSelected != Wall.WallCornerType.Normal).Any();
        }
    }

    public Room(Vector2 position, bool hasRoof = false, RoomRay roomRay = null)
    {
        this.position = position;
        this.HasRoof = hasRoof;
        this.RoomRay = roomRay;
    }

    public Vector2 RoomPosition
    {
        get 
        {
            return this.position;
        }
    }
}