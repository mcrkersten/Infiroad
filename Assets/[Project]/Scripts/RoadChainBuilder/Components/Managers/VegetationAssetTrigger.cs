using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class VegetationAssetTrigger : MonoBehaviour
{
    [Range(0f, 10f)]
    public int spawnProbability;
    public bool singleAssetOnPoint;
    public int amountOfAssetsOnPoint;
    public float spawnRadius;

    public VegetationAssetTypeTag poolTag;

    private ObjectPooler objectPooler;

    private void Start()
    {
        objectPooler = ObjectPooler.Instance;
        this.transform.DOScale(Vector3.one, 3f);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Get tag from the gameObject that triggered this function
        VegetationTriggerAsset assetTag = other.GetComponent<VegetationTriggerAsset>();
        if (assetTag == null)
            return;

        //Search in pool for sub-pool with tag
        if (assetTag.tags.Contains(poolTag))
        {
            if (singleAssetOnPoint)
                ActivateSingleFromAssetPool(assetTag);
            else
                ActivateGroupFromAssetPool(assetTag, Random.Range(0, amountOfAssetsOnPoint));
        }
    }

    //Activate a group of assets from the assetpool
    private void ActivateGroupFromAssetPool(VegetationTriggerAsset aspt, int groupSize)
    {
        for (int i = 0; i < groupSize; i++)
        {
            string roadTag = aspt.transform.name;
            Vector3 position = aspt.transform.position;
            Vector2 random = Random.insideUnitCircle * spawnRadius;
            Vector3 pos = new Vector3(random.x, 0, random.y);
            objectPooler.ActivateVegetationAssetFromPool(poolTag, roadTag, position + pos, Quaternion.identity);
        }
    }

    //Activate a single asset from the assetpool
    private void ActivateSingleFromAssetPool(VegetationTriggerAsset aspt)
    {
        int i = Random.Range(0, 10);
        if (i < spawnProbability)
        {
            string assetSpawnTag = aspt.gameObject.name;
            Vector3 position = aspt.transform.position;
            objectPooler.ActivateVegetationAssetFromPool(poolTag, assetSpawnTag, position, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
        }
    }
}