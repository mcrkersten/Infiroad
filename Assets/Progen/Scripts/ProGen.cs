using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ProGen : MonoBehaviour
{   
    [SerializeField]
    private ProGenThemeScriptableObject theme;

    private Floor[] floors;

    private List<GameObject> rooms = new List<GameObject>();

    private int prefabCounter = 0;

    public ProGenThemeScriptableObject Theme => theme;


    private bool CanFlagCornerWalls
    {
        get 
        {
            return theme.rows >= 2 && theme.columns >= 2 
                && theme.cornerPrefab != null 
                && theme.allowCornerWalls;
        }
    }

    private void Awake() 
    { 
        Generate();
    }

    public void Generate()
    {
        // Validate ProGen Before Proceeding
        if(!this.IsValid())
        {
            Debug.LogWarning("There parameters not set in ProGen. Make sure all prefab type parameters are set...");
            return;
        }

        prefabCounter = 0;

        // Clear 
        Clear();

        // Create Data Structure
        BuildDataStructure();

        // Generate prefabs
        Render();

        // Removes inside walls
        if(!theme.keepInsideWalls)
        {
            RemoveInsideWalls();
        }
    }

    void BuildDataStructure()
    {
        floors = new Floor[theme.numberOfFloors];

        int floorCount = 0;
        
        int initialRows = theme.rows;
        int initialColumns = theme.columns;

        foreach(Floor floor in floors)
        {
            Room[,] rooms = new Room[initialRows,initialColumns];

            for(int row = 0; row < initialRows; row++)
            {
                for(int column = 0; column < initialColumns; column++)
                {
                    var roomPosition = new Vector3(row * theme.cellUnitSize, floorCount, column * theme.cellUnitSize);
                    rooms[row,column] = new Room(roomPosition, theme.includeRoof ? (floorCount == floors.Length - 1) : false);

                    rooms[row, column].Walls[0] = new Wall(roomPosition, Quaternion.Euler(0, 0, 0));
                    rooms[row, column].Walls[1] = new Wall(roomPosition, Quaternion.Euler(0, 90, 0));
                    rooms[row, column].Walls[2] = new Wall(roomPosition, Quaternion.Euler(0, 180, 0));
                    rooms[row, column].Walls[3] = new Wall(roomPosition, Quaternion.Euler(0, -90, 0));
                    
                    FlagCornerWalls(rooms, roomPosition, row, column);
                    
                    if(theme.randomizeRows || theme.randomizeColumns)
                        rooms[row,column].HasRoof = true;
                }
            }
            floors[floorCount] = new Floor(floorCount++, rooms);

            // Rule: if random column or rows let's experiment with different values
            if(theme.randomizeRows)
                initialRows = UnityEngine.Random.Range(1, theme.rows);
            if(theme.randomizeColumns)
                initialColumns = UnityEngine.Random.Range(1, theme.columns);
        }
    }

    void FlagCornerWalls(Room[,] rooms, Vector3 roomPosition, int row, int column)
    {
        if(CanFlagCornerWalls)
        {
            // for corner walls
            // if row == 0 && column == 0 -> left bottom corner
            // if row == 0 && column == max -> left upper corner
            // if row == max && column == 0 -> right bottom corner
            // if row == max && column == max -> right upper corner
            if(row == 0 && column == 0) // left bottom corner
                rooms[row, column].Walls[0].WallCornerTypeSelected = Wall.WallCornerType.LeftBottom;
            else if(row == 0 && column == theme.columns - 1) // left upper corner
                rooms[row, column].Walls[1].WallCornerTypeSelected = Wall.WallCornerType.LeftUpper;
            else if(row == theme.rows - 1 && column == 0) // right bottom corner
                rooms[row, column].Walls[2].WallCornerTypeSelected = Wall.WallCornerType.RightBottom;
            else if(row == theme.rows - 1 && column == theme.columns - 1) //right upper corner
                rooms[row, column].Walls[3].WallCornerTypeSelected = Wall.WallCornerType.RightUpper;
        }
    }

    void Render()
    {
        foreach(Floor floor in floors)
        {
            for(int row = 0; row < floor.Rows; row++)
            {
                for(int column = 0; column < floor.Columns; column++)
                {
                    Room room = floor.rooms[row, column];
                    room.FloorNumber = floor.FloorNumber;
                    GameObject roomGo = new GameObject($"Room_{row}_{column}");
                    rooms.Add(roomGo);
                    roomGo.transform.parent = transform;
                    
                    // corner logic takes presedence
                    if(room.HasRoundedCorner)
                    {
                        RoomPlacementWithRoundedCorners(room, roomGo);
                    }
                    else
                    {
                        if(floor.FloorNumber == 0)
                            RoomPlacement(UnityEngine.Random.Range(0.0f, 1.0f) <= theme.doorPercentChance ? theme.doorPrefab : theme.wallPrefab, room, roomGo);
                        else
                        {
                            // Rule: if window coverage percent is within threshold add a window
                            // otherwise add a basic wall
                            if(UnityEngine.Random.Range(0.0f, 1.0f) <= theme.windowPercentChance)
                            {
                                if(theme.randomizeWindowSelection)
                                {
                                    int windowIndex = UnityEngine.Random.Range(0, theme.windowPrefabs.Length);
                                    RoomPlacement(theme.windowPrefabs[windowIndex], room, roomGo);
                                }
                                else 
                                    RoomPlacement(theme.windowPrefabs[0], room, roomGo);
                            }
                            else 
                                RoomPlacement(theme.wallPrefab, room, roomGo);
                        }
                    }
                }
            }
        }
    }

    private void RoomPlacementWithRoundedCorners(Room room, GameObject roomGo)
    {
        if(room.Walls.Any(w => w.WallCornerTypeSelected == Wall.WallCornerType.LeftBottom))
        {
            SpawnPrefab(theme.cornerPrefab, roomGo.transform, room.Walls[0].Position, room.Walls[0].Rotation);
            SpawnPrefab(theme.wallPrefab, roomGo.transform, room.Walls[2].Position, room.Walls[2].Rotation);
            SpawnPrefab(theme.wallPrefab, roomGo.transform, room.Walls[3].Position, room.Walls[3].Rotation);
        }
        else if(room.Walls.Any(w => w.WallCornerTypeSelected == Wall.WallCornerType.LeftUpper))
        {
            SpawnPrefab(theme.cornerPrefab, roomGo.transform, room.Walls[1].Position, room.Walls[1].Rotation);
            SpawnPrefab(theme.wallPrefab, roomGo.transform, room.Walls[0].Position, room.Walls[0].Rotation);
            SpawnPrefab(theme.wallPrefab, roomGo.transform, room.Walls[3].Position, room.Walls[3].Rotation);
        }
        else if(room.Walls.Any(w => w.WallCornerTypeSelected == Wall.WallCornerType.RightUpper))
        {
            SpawnPrefab(theme.cornerPrefab, roomGo.transform, room.Walls[2].Position, room.Walls[2].Rotation);
            SpawnPrefab(theme.wallPrefab, roomGo.transform, room.Walls[0].Position, room.Walls[0].Rotation);
            SpawnPrefab(theme.wallPrefab, roomGo.transform, room.Walls[1].Position, room.Walls[1].Rotation);
        }
        else if(room.Walls.Any(w => w.WallCornerTypeSelected == Wall.WallCornerType.RightBottom))
        {
            SpawnPrefab(theme.cornerPrefab, roomGo.transform, room.Walls[3].Position, room.Walls[3].Rotation);
            SpawnPrefab(theme.wallPrefab, roomGo.transform, room.Walls[2].Position, room.Walls[2].Rotation);
            SpawnPrefab(theme.wallPrefab, roomGo.transform, room.Walls[1].Position, room.Walls[1].Rotation);
        }
    }

    private void RoomPlacement(GameObject prefab, Room room, GameObject roomGo)
    {
        SpawnPrefab(prefab, roomGo.transform, room.Walls[0].Position, room.Walls[0].Rotation);
        SpawnPrefab(prefab, roomGo.transform, room.Walls[1].Position, room.Walls[1].Rotation);
        SpawnPrefab(prefab, roomGo.transform, room.Walls[2].Position, room.Walls[2].Rotation);
        SpawnPrefab(prefab, roomGo.transform, room.Walls[3].Position, room.Walls[3].Rotation);

        AddRoof(room, roomGo);
    }

    private void AddRoof(Room room, GameObject roomGo)
    {
        if (room.HasRoof)
        {
            // Rule: if we need to randomize roof and we're at the top
            if (theme.randomizeRoofSelection && room.FloorNumber == floors.Count() - 1)
            {
                int roofIndex = UnityEngine.Random.Range(0, theme.roofPrefabs.Length);
                SpawnPrefab(theme.roofPrefabs[roofIndex], roomGo.transform, room.Walls[0].Position, room.Walls[0].Rotation);
            }
            else
                SpawnPrefab(theme.roofPrefabs[0], roomGo.transform, room.Walls[0].Position, room.Walls[0].Rotation);
        }
    }

    private void SpawnPrefab(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
    {
        var gameObject = Instantiate(prefab, transform.position + position, rotation);
        gameObject.transform.parent = parent;
        gameObject.AddComponent<WallComponent>();
        gameObject.name = $"{gameObject.name}_{prefabCounter}";
        prefabCounter++;
    }

    void RemoveInsideWalls()
    {
        var wallComponents = GameObject.FindObjectsOfType<WallComponent>();
        var childs = wallComponents.Select(c => c.transform.GetChild(0).position.ToString()).ToList();

        var dupPositions = childs.GroupBy(c => c)
            .Where(c => c.Count() > 1)
            .Select(grp => grp.Key)
            .ToList();  

        foreach(WallComponent w in wallComponents)
        {
            var childTransform = w.transform.GetChild(0);
            if(dupPositions.Contains(childTransform.position.ToString())){
                    DestroyImmediate(childTransform.gameObject);
            }
        }
    }

    void Clear()
    {
        for(int i = 0;i < rooms.Count;i++)
        {
            DestroyImmediate(rooms[i]);
        }
        rooms.Clear();

        // in case we have left over rooms
        if(rooms.Count() == 0 && transform.childCount > 0)
        {
            foreach(Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
}