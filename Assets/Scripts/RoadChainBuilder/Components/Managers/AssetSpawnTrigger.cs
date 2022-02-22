using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AssetSpawnTrigger : MonoBehaviour
{
    public bool singleAsset;
    [Range(0f, 10f)]
    public int spawnProbability;

    public int amountOfAssetsOnPoint;
    public AssetTypeTag poolTag;

    private ObjectPooler objectPooler;

    public float spawnRadius;

    private void Start()
    {
        this.transform.DOScale(Vector3.one, 3f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (objectPooler == null)
            objectPooler = ObjectPooler.Instance;

        AssetSpawnPointTag aspt = other.GetComponent<AssetSpawnPointTag>();
        if (aspt == null)
            return;
        if (aspt.poolTag.Contains(poolTag))
        {
            if (singleAsset)
                SpawnSingle(aspt);
            else
                SpawnGroup(aspt);
        }
    }

    private void SpawnGroup(AssetSpawnPointTag aspt)
    {
        int randomAmount = Random.Range(0, amountOfAssetsOnPoint);
        for (int i = 0; i < randomAmount; i++)
        {
            string assetSpawnTag = aspt.gameObject.name;
            Vector3 position = aspt.gameObject.transform.position;
            Vector2 random = Random.insideUnitCircle * spawnRadius;
            Vector3 pos = new Vector3(random.x, 0, random.y);
            objectPooler.SpawnFromPool(poolTag, assetSpawnTag, position + pos, Quaternion.identity);
        }
    }

    private void SpawnSingle(AssetSpawnPointTag aspt)
    {
        int i = Random.Range(0, 10);
        if (i < spawnProbability)
        {
            string assetSpawnTag = aspt.gameObject.name;
            Vector3 position = aspt.transform.position;
            objectPooler.SpawnFromPool(poolTag, assetSpawnTag, position, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
        }
    }
}


public enum AssetTypeTag
{
    Grass = 0,
    Trees,
    Wheat
}