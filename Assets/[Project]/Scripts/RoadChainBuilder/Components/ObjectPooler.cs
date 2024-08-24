using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class ObjectPooler
{
    private Dictionary<int, Queue<GameObject>> skyDecorations = new Dictionary<int, Queue<GameObject>>();
    private Queue<AssetTrigger> assetTriggerQueue = new Queue<AssetTrigger>();
    private Dictionary<string, Dictionary<string, Queue<GameObject>>> assetQueueDictionaries = new Dictionary<string, Dictionary<string, Queue<GameObject>>>();
    private Dictionary<int, Queue<List<GameObject>>> roadDecorationQueueDictionary = new Dictionary<int, Queue<List<GameObject>>>();
    private Dictionary<string, Queue<GameObject>> meshtaskObjectQueueDictionary = new Dictionary<string, Queue<GameObject>>();
    private GameObject parent;

    #region Singleton
    public static ObjectPooler Instance;

    public ObjectPooler()
    {
        Instance = this;
        CreateParentTransform();
    }
    #endregion

    public void AddObjectDespawner(GameObject obj)
    {
        obj.AddComponent<ObjectDespawn>();
    }

    //Instantiate, deactivate and queue into correct pool
    #region Instantiate pools
    /// <summary>
    /// Instantiate GameObjects with a collider that holds a tag
    /// </summary>
    /// <param name="assetPointAmount">Amount of points to instantiate</param>
    /// <param name="assetPointPrefab"></param>
    public void InstantiateAssetTriggersPool(int assetPointAmount, GameObject assetPointPrefab, bool timer)
    {
        for (int i = 0; i < assetPointAmount; i++)
        {
            GameObject obj = GameObject.Instantiate(assetPointPrefab);
            obj.SetActive(false);
            obj.transform.parent = parent.transform;
            if(timer) obj.AddComponent<ObjectDespawn>();
            assetTriggerQueue.Enqueue(obj.GetComponent<AssetTrigger>());
        }
    }

    public void InstantiateAssetPool(List<AssetPool> pools, string roadType, bool timer)
    {
        //Returns if pool for this road already has been made.
        if (assetQueueDictionaries.ContainsKey(roadType))
            return;

        //Generate local dictionary to put in dictionary of pool dictionaries
        int poolIndex = 0;
        Dictionary<string, Queue<GameObject>> localDictionary = new Dictionary<string, Queue<GameObject>>();
        foreach (AssetPool pool in pools)
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
                obj.SetActive(false);
                obj.transform.parent = parent.transform;
                if(timer) obj.AddComponent<ObjectDespawn>();
                objectPool.Enqueue(obj);
            }
            localDictionary.Add(poolIndex++.ToString(), objectPool);
        }

        assetQueueDictionaries.Add("Zero", localDictionary);
    }
    public void OnDestroy()
    {
        assetQueueDictionaries = null;
        skyDecorations = null;
        assetTriggerQueue = null;
        roadDecorationQueueDictionary = null;
        meshtaskObjectQueueDictionary = null;

        GameObject.Destroy(parent);
        parent = null;
    }
    /// Instantiate all decorations in dedicated pools
    public void InstantiateRoadDecorationDecorationPool(RoadDecoration pool)
    {
        //Returns if pool already has been made.
        if (roadDecorationQueueDictionary.ContainsKey(pool.poolIndex))
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
        roadDecorationQueueDictionary.Add(pool.poolIndex, queueOfDecor);
    }
    /// Instantiate all decorations in dedicated pools
    public void InstantiateMeshtaskObjects(MeshtaskSettings meshtaskSettings)
    {
        //Instantiate object and add to pool.
        foreach (Decoration dec in meshtaskSettings.meshtaskPoolingObjects)
        {
            //Returns if pool already has been made.
            if (meshtaskObjectQueueDictionary.ContainsKey(meshtaskSettings.meshTaskType.ToString() + dec.meshtaskPoolType.ToString()))
                continue;

            Queue<GameObject> queueOfDecor = new Queue<GameObject>();
            for (int i = 0; i < dec.unitsInPool; i++)
            {
                GameObject decoration = GameObject.Instantiate(dec.prefab);
                decoration.transform.parent = parent.transform;
                decoration.SetActive(false);
                queueOfDecor.Enqueue(decoration);
            }
            meshtaskObjectQueueDictionary.Add(meshtaskSettings.meshTaskType.ToString() + dec.meshtaskPoolType.ToString(), queueOfDecor);
        }
    }
    /// Instantiate all decorations in dedicated pools
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
    public AssetTrigger ActivateAssetTrigger(Vector3 position, List<VegetationScannerTypeTag> tags)
    {
        AssetTrigger objectToSpawn = assetTriggerQueue.Dequeue();
        objectToSpawn.scanableByScannerType.Clear();
        foreach (VegetationScannerTypeTag at in tags)
        {
            objectToSpawn.scanableByScannerType.Add(at);
        }

        objectToSpawn.gameObject.SetActive(true);
        objectToSpawn.transform.position = position;
        assetTriggerQueue.Enqueue(objectToSpawn);
        return objectToSpawn;
    }

    /// <summary>
    /// Get a asse t from the vegetation pool
    /// </summary>
    /// <param name="assetType">tag of assetPool</param>
    /// <param name="roadTag">tag of road</param>
    /// <param name="position">Position to activate at</param>
    /// <param name="rotation">Rotation to activate at</param>
    /// <returns></returns>
    public void ActivateAssetFromAssetQueue(string assetType, string roadTag, Vector3 position, Quaternion rotation)
    {
        if (!assetQueueDictionaries.ContainsKey(roadTag)) { return; }
        if (!assetQueueDictionaries[roadTag].ContainsKey(assetType)) { return; }

        //Get object from Dictionary
        GameObject objectToSpawn = assetQueueDictionaries[roadTag][assetType].Dequeue();

        if(objectToSpawn == null)
            Debug.LogError("POOL PANIC: non excisting object was dequeued, roatTag:"+ roadTag + " assetType:" + assetType);

        //Activate and set position
        SetGameObjectPosition(objectToSpawn, position, rotation);
        objectToSpawn.SetActive(true);
        assetQueueDictionaries[roadTag][assetType].Enqueue(objectToSpawn);
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
        if (!roadDecorationQueueDictionary.ContainsKey(poolIndex)) { return null; }
        List<GameObject> decoration = roadDecorationQueueDictionary[poolIndex].Dequeue();
        roadDecorationQueueDictionary[poolIndex].Enqueue(decoration);
        return decoration;
    }

    public GameObject GetMeshtaskObject(MeshTaskType meshTaskType, MeshtaskPoolType meshtaskPoolType)
    {
        string keyS = meshTaskType.ToString() + meshtaskPoolType.ToString();
        if (!meshtaskObjectQueueDictionary.ContainsKey(keyS)) { Debug.Log(keyS); return null; }
        GameObject decoration = meshtaskObjectQueueDictionary[keyS].Dequeue();
        meshtaskObjectQueueDictionary[keyS].Enqueue(decoration);
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

    private void SetGameObjectPosition(GameObject gameObject, Vector3 position, Quaternion rotation)
    {
        Vector3 currentScale = gameObject.transform.localScale;
        gameObject.transform.localScale = Vector3.zero;
        gameObject.transform.position = position;
        gameObject.transform.rotation = rotation;
        gameObject.transform.DOScale(currentScale, 1f).SetEase(DG.Tweening.Ease.InOutQuart);
    }
}

[System.Serializable]
public class AssetPool
{
    public VegetationScannerTypeTag tag;
    public List<GameObject> prefabs = new List<GameObject>();
    public int size;
}
public enum VegetationScannerTypeTag
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
