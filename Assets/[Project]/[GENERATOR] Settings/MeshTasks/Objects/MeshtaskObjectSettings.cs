using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu, System.Serializable]
public class MeshtaskObjectSettings : MeshtaskSettings
{
    [SerializeField] private List<SpawnSizeSettings> sizes = new List<SpawnSizeSettings>();

    public override void PopulateMeshtask(MeshTask meshtask, GameObject parent)
    {
        int size = meshtask.positionVectors.Count;
        parent.name = parent.name + " " + meshtask.positionVectors.Count;
        foreach (SpawnSizeSettings meshtaskObject in sizes)
        {
            if(size > meshtaskObject.minimalMeshtaskPointSpawnSize)
                SpawnMeshtaskObject(meshtask, parent, meshtask.positionVectors.Count / 2, meshtaskObject.objectType);
        }
    }

    protected override void SpawnMeshtaskObject(MeshTask meshTask, GameObject parent, int meshtaskPoint, MeshtaskPoolType meshtaskPoolType)
    {
        MeshTask.Point p = meshTask.positionVectors[meshtaskPoint];

        Vector2 meshDirection = meshTask.meshPosition == MeshtaskPosition.Left ? (Vector2.left) : (Vector2.right);
        float local_XOffset = meshTask.meshPosition == MeshtaskPosition.Left ? Mathf.Min(0f, p.extrusionVariables.leftExtrusion) : Mathf.Max(0f, p.extrusionVariables.rightExtrusion);
        local_XOffset = local_XOffset * meshTask.meshtaskSettings.extrusionSize;

        Vector3 noise = Vector3.zero;
        noise += meshTask.noiseChannel.generatorInstance.GetNoise(meshTask.startPointIndex + meshtaskPoint, meshTask.noiseChannel);

        GameObject instance = ObjectPooler.Instance.GetMeshtaskObject(meshTask.meshtaskSettings.meshTaskType, meshtaskPoolType);
        PlaceModelOnMesh(meshDirection, p, noise, Mathf.Abs(local_XOffset), parent, instance);
    }

    [System.Serializable]
    private class SpawnSizeSettings
    {
        public int minimalMeshtaskPointSpawnSize;
        public MeshtaskPoolType objectType;
    }
}
