using UnityEngine;

[CreateAssetMenu(fileName = "ProGenTheme", menuName = "ProGen/CreateProGenTheme", order = 1)]
public class ProGenThemeScriptableObject : ScriptableObject
{
    [Header("Wall Options")]    
    public GameObject wallPrefab;
    public GameObject wallTopPrefab;

    public bool keepInsideWalls = true;

    [Header("Window Options")]
    public GameObject[] windowPrefabs;

    public bool randomizeWindowSelection = false;

    [Range(0.0f,1.0f)]
    public float windowPercentChance = 0.3f;

    [Header("Roof Options")]
    public GameObject[] roofPrefabs;

    public bool randomizeRoofSelection = false;

    public bool includeRoof = true;

    [Header("Wall Corner Options 2x2 Min")]
    [Space(1.0f)]
    public GameObject cornerPrefab;
    public GameObject topCornerPrefab;
    public GameObject bottomCornerPrefab;
    public GameObject cornerRoofPrefab;
    
    public bool allowCornerWalls = false;

    [Header("Door Options")]
    public GameObject doorPrefab;

    [Range(0.0f,1.0f)]
    public float doorPercentChance = 0.2f;

    [Header("Grid Options")]
    [Range(1,20)]
    public int rows = 3;

    [Range(1,20)]
    public int columns = 3;
    
    public bool randomizeRows = false;

    public bool randomizeColumns = false;

    [Range(0.0f,20.0f)]
    public float cellUnitSize = 1;

    [Range(1,50)]
    public int numberOfFloors = 1;
}
