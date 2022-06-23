using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu, System.Serializable]
public class BuildingSettings : MeshtaskSettings
{
    [SerializeField] private List<BuildingSizeSettings> minimalSizes = new List<BuildingSizeSettings>();

    public override void PopulateMeshtask(MeshTask meshtask, GameObject parent)
    {
        int size = meshtask.positionPoints.Count;
        parent.name = parent.name + " " + meshtask.positionPoints.Count;
        foreach (BuildingSizeSettings building in minimalSizes)
        {
            if(size < building.minimalMeshtaskPointSize)
                SpawnMeshtaskObject(meshtask, parent, meshtask.positionPoints.Count / 2, building.buildingSize);
        }
    }

    protected override void SpawnMeshtaskObject(MeshTask meshTask, GameObject parent, int meshtaskPoint, MeshtaskPoolType meshtaskPoolType)
    {
        MeshTask.Point p = meshTask.positionPoints[meshtaskPoint];

        Vector2 meshDirection = meshTask.meshPosition == MeshtaskPosition.Left ? (Vector2.left) : (Vector2.right);
        float local_XOffset = meshTask.meshPosition == MeshtaskPosition.Left ? Mathf.Min(0f, p.extrusionVariables.leftExtrusion) : Mathf.Max(0f, p.extrusionVariables.rightExtrusion);
        local_XOffset = local_XOffset * meshTask.meshtaskSettings.extrusionSize;

        Vector3 noise = Vector3.zero;
        noise += meshTask.noiseChannel.generatorInstance.getNoise(meshTask.startPointIndex + meshtaskPoint, meshTask.noiseChannel);

        GameObject instance = ObjectPooler.Instance.GetMeshtaskObject(meshTask.meshtaskSettings.meshTaskType, meshtaskPoolType);
        PlaceModelOnMesh(meshDirection, p, noise, Mathf.Abs(local_XOffset), parent, instance);
    }

    [System.Serializable]
    private class BuildingSizeSettings
    {
        public int minimalMeshtaskPointSize;
        public MeshtaskPoolType buildingSize;
    }
}
