using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler
{

    public Dictionary<string, Dictionary<VegetationAssetTypeTag, Queue<GameObject>>> vegitationPoolDictionaries = new Dictionary<string, Dictionary<VegetationAssetTypeTag, Queue<GameObject>>>();
    public Queue<VegetationTriggerAsset> vegetationTriggerPool = new Queue<VegetationTriggerAsset>();
    private GameObject parent;

    #region Singleton
    public static ObjectPooler Instance;

    public ObjectPooler()
    {
        Instance = this;
        CreateParentTransform();
    }
    #endregion

    //Instantiate, deactivate and queue into correct pool
    #region Instantiate pools
    /// <summary>
    /// Instantiate GameObjects with a collider that holds a tag
    /// </summary>
    /// <param name="assetPointAmount">Amount of points to instantiate</param>
    /// <param name="assetPointPrefab"></param>
    public void InstantiateVegetationTriggerPool(int assetPointAmount, GameObject assetPointPrefab)
    {
        for (int i = 0; i < assetPointAmount; i++)
        {
            GameObject obj = GameObject.Instantiate(assetPointPrefab);
            obj.transform.parent = parent.transform;
            obj.SetActive(false);
            vegetationTriggerPool.Enqueue(obj.GetComponent<VegetationTriggerAsset>());
        }
    }
    /// <summary>
    /// Instantiate VegetationAssets
    /// </summary>
    /// <param name="pools">Vegitation pools to spawn</param>
    /// <param name="roadType">The corresponding road of the vegetation</param>
    public void InstantiateVegitationPool(List<VegitationPool> pools, string roadType)
    {
        //If contains key, don't create the dictionary. we already created it for this roadType
        if (vegitationPoolDictionaries.ContainsKey(roadType))
            return;

        //Generate local dictionary to put in dictionary of pool dictionaries
        Dictionary<VegetationAssetTypeTag, Queue<GameObject>> localDictionary = new Dictionary<VegetationAssetTypeTag, Queue<GameObject>>();
        foreach (VegitationPool pool in pools)
        {
            //If object pool has no prefabs
            if (pool.prefabs.Count == 0)
                return;

            //Instantiate new random prefab for each iteration of pool.size
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                //Select random prefab from prefab-pool
                int spawnIndex = 0;
                if (pool.prefabs.Count != 1)
                    spawnIndex = Random.Range(0, pool.prefabs.Count);

                //Spawn random selected prefab
                GameObject obj = GameObject.Instantiate(pool.prefabs[spawnIndex]);
                obj.transform.parent = parent.transform;
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            localDictionary.Add(pool.tag, objectPool);
        }

        vegitationPoolDictionaries.Add(roadType, localDictionary);
    }
    #endregion

    //Dequeue object, position, activate and queue back into pool
    #region Activate from pool
    /// <summary>
    /// Activate trigger object with tag
    /// </summary>
    /// <param name="position">Position to spawn trigger on</param>
    /// <param name="tags">Tags the trigger needs to hold</param>
    /// <returns></returns>
    public VegetationTriggerAsset ActivateVegetationTriggerAsset(Vector3 position, List<VegetationAssetTypeTag> tags)
    {
        VegetationTriggerAsset objectToSpawn = vegetationTriggerPool.Dequeue();
        objectToSpawn.tags.Clear();
        foreach (VegetationAssetTypeTag at in tags)
        {
            objectToSpawn.tags.Add(at);
        }

        objectToSpawn.gameObject.SetActive(true);
        objectToSpawn.transform.position = position;
        vegetationTriggerPool.Enqueue(objectToSpawn);
        return objectToSpawn;
    }

    /// <summary>
    /// Get a asset from the vegetation pool
    /// </summary>
    /// <param name="typeTag">Tag of vegitationPools</param>
    /// <param name="roadTag">tag of road</param>
    /// <param name="position">Position to activate at</param>
    /// <param name="rotation">Rotation to activate at</param>
    /// <returns></returns>
    public GameObject ActivateVegetationAssetFromPool(VegetationAssetTypeTag typeTag, string roadTag, Vector3 position, Quaternion rotation)
    {
        if (!vegitationPoolDictionaries.ContainsKey(roadTag)) { return null; }
        if (!vegitationPoolDictionaries[roadTag].ContainsKey(typeTag)) { return null; }

        GameObject objectToSpawn = vegitationPoolDictionaries[roadTag][typeTag].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        vegitationPoolDictionaries[roadTag][typeTag].Enqueue(objectToSpawn);
        return objectToSpawn;
    }
    #endregion


    private void CreateParentTransform()
    {
        if (parent == null)
        {
            parent = new GameObject();
            parent.name = "ObjectPool";
        }
    }
}

[System.Serializable]
public class VegitationPool
{
    public VegetationAssetTypeTag tag;
    public List<GameObject> prefabs = new List<GameObject>();
    public int size;
}
public enum VegetationAssetTypeTag
{
    Grass = 0,
    Trees,
    Wheat
}


[System.Serializable]
public class DecorationPool
{
    public DecorAssetTypeTag tag;
    public List<GameObject> prefabs = new List<GameObject>();
    public int size;
}
public enum DecorAssetTypeTag
{
    Grass = 0,
    Trees,
    Wheat
}
