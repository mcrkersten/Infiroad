using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class GrandstandSettings : MeshtaskSettings
{
    [SerializeField] private GrandstandSizeSettings small;
    [SerializeField] private GrandstandSizeSettings medium;
    [SerializeField] private GrandstandSizeSettings large;
    public override void PopulateMeshtask(MeshTask meshTask, GameObject currentMeshObject)
    {
        int size = meshTask.points.Count;
        currentMeshObject.name = currentMeshObject.name + " " + meshTask.points.Count;
        if (size <= 10)
            SmallGrandstand(meshTask, currentMeshObject);
        if (size > 10 && size <= 20)
            MediumGrandstand(meshTask, currentMeshObject);
        if (size > 20)
            LargeGrandstand(meshTask, currentMeshObject);
    }

    private void SmallGrandstand(MeshTask meshTask, GameObject currentMeshObject)
    {
        for (int i = 0; i < small.smokeBombCount; i++)
        {
            int random = Random.Range(0, 101);
            if(random < small.smokeSpawnPercentage)
                SpawnSmoke(meshTask, currentMeshObject);
        }
    }

    private void MediumGrandstand(MeshTask meshTask, GameObject currentMeshObject)
    {
        for (int i = 0; i < medium.smokeBombCount; i++)
        {
            int random = Random.Range(0, 101);
            if (random < medium.smokeSpawnPercentage)
                SpawnSmoke(meshTask, currentMeshObject);
        }
    }

    private void LargeGrandstand(MeshTask meshTask, GameObject currentMeshObject)
    {
        for (int i = 0; i < large.smokeBombCount; i++)
        {
            int random = Random.Range(0, 101);
            if (random < large.smokeSpawnPercentage)
                SpawnSmoke(meshTask, currentMeshObject);
        }
    }

    private void SpawnSmoke(MeshTask meshTask, GameObject currentMeshObject)
    {
        int random = Random.Range(1, meshTask.points.Count - 1);
        MeshTask.Point p = meshTask.points[random];

        Vector2 meshDirection = meshTask.meshPosition == MeshtaskPosition.Left ? (Vector2.left) : (Vector2.right);
        float local_XOffset = meshTask.meshPosition == MeshtaskPosition.Left ? Mathf.Min(0f, p.extrusionVariables.leftExtrusion) : Mathf.Max(0f, p.extrusionVariables.rightExtrusion);
        local_XOffset = local_XOffset * meshTask.meshtaskSettings.extrusionSize;

        Vector3 noise = Vector3.zero;
        noise += meshTask.noiseChannel.generatorInstance.getNoise(meshTask.startPointIndex + random, meshTask.noiseChannel);

        CreateModelOnMesh(meshDirection, p, noise, Mathf.Abs(local_XOffset), currentMeshObject, large.smokePrefab);
    }

    [System.Serializable]
    private class GrandstandSizeSettings
    {
        public int smokeBombCount;
        public GameObject smokePrefab;
        public float smokeSpawnPercentage;
    }
}
