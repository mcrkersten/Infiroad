using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class VegetationAssetScanner : MonoBehaviour
{
    [Range(0f, 10f)]
    public int spawnProbability;
    public bool singleAssetOnPoint;
    public int amountOfAssetsOnPoint;
    public float spawnRadius;

    public VegetationScannerTypeTag scannerType;

    private ObjectPooler objectPooler;

    private void Awake()
    {
        objectPooler = ObjectPooler.Instance;
        this.transform.DOScale(Vector3.one, 3f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (objectPooler == null)
            objectPooler = ObjectPooler.Instance;
        //Get tag from the gameObject that triggered this function
        AssetTrigger assetTag = other.GetComponent<AssetTrigger>();
        if (assetTag == null)
            return;

        if (assetTag.scannedBy.Contains(scannerType))
            return;

        //Search in pool for sub-pool with tag
        if (assetTag.scanableByScannerType.Contains(scannerType))
        {
            if (singleAssetOnPoint)
                ActivateSingleFromAssetPool(assetTag);
            else
                ActivateGroupFromAssetPool(assetTag, Random.Range(0, amountOfAssetsOnPoint));

            assetTag.scannedBy.Add(scannerType);
            other.gameObject.SetActive(false);
        }
    }

    //Activate a group of assets from the assetpool
    private void ActivateGroupFromAssetPool(AssetTrigger aspt, int groupSize)
    {
        for (int i = 0; i < groupSize; i++)
        {
            Vector3 position = aspt.transform.position;
            Vector2 random = Random.insideUnitCircle * spawnRadius;
            Vector3 pos = new Vector3(random.x, 0, random.y);
            objectPooler.ActivateAssetFromAssetQueue(scannerType.ToString(), aspt.transform.name, position + pos, Quaternion.identity);
        }
    }

    //Activate a single asset from the assetpool
    private void ActivateSingleFromAssetPool(AssetTrigger aspt)
    {
        int i = Random.Range(0, 10);
        if (i < spawnProbability)
        {
            Vector3 position = aspt.transform.position;
            objectPooler.ActivateAssetFromAssetQueue(scannerType.ToString(), aspt.gameObject.name, position, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
        }
    }
}