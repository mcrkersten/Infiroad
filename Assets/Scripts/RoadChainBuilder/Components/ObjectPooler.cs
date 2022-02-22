using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler
{

    public Dictionary<string, Dictionary<AssetTypeTag, Queue<GameObject>>> poolDictionary = new Dictionary<string, Dictionary<AssetTypeTag, Queue<GameObject>>>();
    public Queue<AssetSpawnPointTag> assetSpawnPointPool = new Queue<AssetSpawnPointTag>();
    private GameObject parent;

    #region Singleton
    public static ObjectPooler Instance;

    public ObjectPooler()
    {
        Instance = this;
    }

    #endregion

    public void InstantiateAssetSpawnPointPool(int assetPointAmount, GameObject assetPointPrefab)
    {
        CheckParent();
        for (int i = 0; i < assetPointAmount; i++)
        {
            GameObject obj = GameObject.Instantiate(assetPointPrefab);
            obj.transform.parent = parent.transform;
            obj.SetActive(false);
            assetSpawnPointPool.Enqueue(obj.GetComponent<AssetSpawnPointTag>());
        }
    }

    public void InstantiatePool(List<Pool> pools, string tag)
    {

        if (poolDictionary.ContainsKey(tag))
            return;

        CheckParent();
        Dictionary<AssetTypeTag, Queue<GameObject>> tempDictionary = new Dictionary<AssetTypeTag, Queue<GameObject>>();
        foreach (Pool pool in pools)
        {
            if (pool.prefabs.Count == 0)
                return;

            int spawnIndex = 0;
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                if (pool.prefabs.Count != 1)
                    spawnIndex = Random.Range(0, pool.prefabs.Count);

                GameObject obj = GameObject.Instantiate(pool.prefabs[spawnIndex]);
                obj.transform.parent = parent.transform;
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            tempDictionary.Add(pool.tag, objectPool);
        }

        poolDictionary.Add(tag, tempDictionary);
    }

    public GameObject SpawnFromPool(AssetTypeTag typeTag, string roadTag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(roadTag)) { return null; }
        if (!poolDictionary[roadTag].ContainsKey(typeTag)) { return null; }

        GameObject objectToSpawn = poolDictionary[roadTag][typeTag].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        poolDictionary[roadTag][typeTag].Enqueue(objectToSpawn);
        return objectToSpawn;
    }

    public AssetSpawnPointTag SpawnFromAssetSpawnPointPool(Vector3 position, List<AssetTypeTag> tags)
    {
        AssetSpawnPointTag objectToSpawn = assetSpawnPointPool.Dequeue();
        objectToSpawn.poolTag.Clear();
        foreach (AssetTypeTag at in tags)
        {
            objectToSpawn.poolTag.Add(at);
        }

        objectToSpawn.gameObject.SetActive(true);
        objectToSpawn.transform.position = position;
        assetSpawnPointPool.Enqueue(objectToSpawn);
        return objectToSpawn;
    }

    private void CheckParent()
    {
        if (parent == null)
        {
            parent = new GameObject();
            parent.name = "ObjectPool";
        }
    }
}

[System.Serializable]
public class Pool
{
    public AssetTypeTag tag;
    public List<GameObject> prefabs = new List<GameObject>();
    public int size;
}