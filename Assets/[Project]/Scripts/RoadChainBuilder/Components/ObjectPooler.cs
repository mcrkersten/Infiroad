using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ObjectPooler
{
    private Dictionary<int, Queue<GameObject>> skyDecorations = new Dictionary<int, Queue<GameObject>>();
    private Queue<VegetationTriggerAsset> vegetationTriggerPool = new Queue<VegetationTriggerAsset>();
    private Dictionary<string, Dictionary<VegetationAssetTypeTag, Queue<GameObject>>> vegitationPoolDictionaries = new Dictionary<string, Dictionary<VegetationAssetTypeTag, Queue<GameObject>>>();
    private Dictionary<int, Queue<List<GameObject>>> roadDecorationPoolDictionary = new Dictionary<int, Queue<List<GameObject>>>();
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
        //Returns if pool for this road already has been made.
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
    /// <summary>
    /// Instantiate all decorations in dedicated pools
    /// </summary>
    /// <param name="pool"></param>
    public void InstantiateDecorationPool(RoadDecoration pool)
    {
        //Returns if pool already has been made.
        if (roadDecorationPoolDictionary.ContainsKey(pool.poolIndex))
            return;

        Queue<List<GameObject>> queueOfDecor = new Queue<List<GameObject>>();
        for (int i = 0; i < pool.wholeUnitsInPool; i++)
        {
            //Instantiate object and add to pool.
            List<GameObject> instantiated = new List<GameObject>();
            foreach (Decoration dec in pool.decor)
            {
                GameObject decoration = GameObject.Instantiate(dec.prefab);
                decoration.transform.parent = parent.transform;
                decoration.SetActive(false);
                instantiated.Add(decoration);
            }
            queueOfDecor.Enqueue(instantiated);
        }
        roadDecorationPoolDictionary.Add(pool.poolIndex, queueOfDecor);
    }

    public void InstantiateAirDecoration(SkyDecoration skyDecoration)
    {
        foreach (SkyDecor item in skyDecoration.skyDecors)
        {
            Queue<GameObject> q = new Queue<GameObject>();
            for (int i = 0; i < item.amount; i++)
            {
                GameObject decor = GameObject.Instantiate(item.prefab);
                q.Enqueue(decor);
                decor.SetActive(false);
                decor.transform.parent = parent.transform;
            }
            skyDecorations.Add(item.key, q);
        }
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
    /// Get a asse t from the vegetation pool
    /// </summary>
    /// <param name="typeTag">Tag of vegitationPools</param>
    /// <param name="roadTag">tag of road</param>
    /// <param name="position">Position to activate at</param>
    /// <param name="rotation">Rotation to activate at</param>
    /// <returns></returns>
    public void ActivateVegetationAssetFromPool(VegetationAssetTypeTag typeTag, string roadTag, Vector3 position, Quaternion rotation)
    {
        if (!vegitationPoolDictionaries.ContainsKey(roadTag)) { return; }
        if (!vegitationPoolDictionaries[roadTag].ContainsKey(typeTag)) { return; }

        //Get object from Dictionary
        GameObject objectToSpawn = vegitationPoolDictionaries[roadTag][typeTag].Dequeue();

        //Activate and set position
        objectToSpawn.SetActive(true);
        SetGameObjectPosition(objectToSpawn, position, rotation);

        //Put object back in to dictionary
        vegitationPoolDictionaries[roadTag][typeTag].Enqueue(objectToSpawn);
    }

    public void ActivateSkyDecoration(SkyDecor skyDecor, Vector3 position)
    {
        GameObject decoration = skyDecorations[skyDecor.key].Dequeue();
        decoration.transform.position = position;
        decoration.SetActive(true);
        skyDecorations[skyDecor.key].Enqueue(decoration);
    }

    public List<GameObject> GetRoadDecorationFromPool(int poolIndex)
    {
        if (!roadDecorationPoolDictionary.ContainsKey(poolIndex)) { return null; }
        List<GameObject> decoration = roadDecorationPoolDictionary[poolIndex].Dequeue();
        roadDecorationPoolDictionary[poolIndex].Enqueue(decoration);
        return decoration;
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

    private void SetGameObjectPosition(GameObject go,Vector3 position, Quaternion rotation)
    {
        Vector3 currentScale = go.transform.localScale;
        go.transform.localScale = Vector3.zero;
        go.transform.position = position;
        go.transform.rotation = rotation;
        go.transform.DOScale(currentScale, 1f).SetEase(DG.Tweening.Ease.InBack);
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
    string key;
    public int decorIndex;
    public List<RoadDecoration> prefabs = new List<RoadDecoration>();
}
