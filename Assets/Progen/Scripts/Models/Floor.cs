
using UnityEngine;

[System.Serializable]
public class Floor
{
    public int FloorNumber { get; private set; }

    [SerializeField]
    public Room[,] rooms;

    public int Rows { get; private set; }

    public int Columns { get; private set; }

    public Floor(int floorNumber, Room[,] rooms)
    {
        FloorNumber = floorNumber;
        this.rooms = rooms;
        Rows = rooms.GetLength(0);
        Columns = rooms.GetLength(1);
    }
}