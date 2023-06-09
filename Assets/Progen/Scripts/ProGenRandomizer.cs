using UnityEngine;

[RequireComponent(typeof(ProGen))]
public class ProGenRandomizer : MonoBehaviour
{
    [SerializeField]
    [Range(0, 20)]
    private float randomizeInSeconds = 5.0f;

    [SerializeField]
    [Range(1,20)]
    private int minRows = 1;

    [SerializeField]
    [Range(1,20)]
    private int maxRows = 1;

    [SerializeField]
    [Range(1,20)]
    private int minColumns = 1;

    [SerializeField]
    [Range(1,20)]
    private int maxColumns = 1;

    [SerializeField]
    [Range(1,20)]
    private int minFloors = 1;

    [SerializeField]
    [Range(1,20)]
    private int maxFloors = 1;

    [SerializeField]
    [Range(0,20)]
    private int minCellUnitSize = 1;

    [SerializeField]
    [Range(0,20)]
    private int maxCellUnitSize = 1;

    [SerializeField]
    private bool randomizeRoofInclusion = false;

    private float randomizerTimer = 0;

    private ProGen proGen;

    void Awake()
    {
        proGen = GetComponent<ProGen>();
    }

    void Update()
    {
        if(randomizerTimer >= randomizeInSeconds)
        {
            proGen.Theme.rows = Random.Range(minRows, maxRows);
            proGen.Theme.columns = Random.Range(minColumns, maxColumns);
            proGen.Theme.numberOfFloors = Random.Range(minFloors, maxFloors);
            proGen.Theme.cellUnitSize = Random.Range(minCellUnitSize, maxCellUnitSize);
            if(randomizeRoofInclusion)
            {
                proGen.Theme.includeRoof = !proGen.Theme.includeRoof;
            }
            proGen.Generate();
            randomizerTimer = 0;
        }

        randomizerTimer += Time.deltaTime * 1.0f;
    }
}
