using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu, System.Serializable]
public class MeshtaskObjectSettings : MeshtaskSettings
{
    [SerializeField] private List<SpawnSizeSettings> sizes = new List<SpawnSizeSettings>();

    public override void PopulateMeshtask(MeshTask meshtask, GameObject parent, bool reflect = false)
    {
        
		Vector2 mto_position = new Vector2(meshtask.meshtaskObject.position.x, meshtask.meshtaskObject.position.y);
		if(reflect)
			mto_position =  new Vector2(-meshtask.meshtaskObject.position.x, meshtask.meshtaskObject.position.y);

        int size = meshtask.positionVectors.Count;
        parent.name = parent.name + " " + meshtask.positionVectors.Count;
        foreach (SpawnSizeSettings meshtaskObject in sizes)
        {
            if(size > meshtaskObject.minimalMeshtaskPointSpawnSize)
                SpawnMeshtaskObject(meshtask, parent, meshtask.positionVectors.Count / 2, meshtaskObject.objectType, mto_position);
        }
    }

    protected override void SpawnMeshtaskObject(MeshTask meshTask, GameObject parent, int meshtaskPoint, MeshtaskPoolType meshtaskPoolType, Vector2 mto_position)
    {
        MeshTask.Point p = meshTask.positionVectors[meshtaskPoint];

        Vector2 meshDirection = float.IsNegative(mto_position.x) ? (Vector2.left) : (Vector2.right);
        float local_XOffset = float.IsNegative(mto_position.x) ? Mathf.Min(0f, p.extrusionVariables.leftExtrusion) : Mathf.Max(0f, p.extrusionVariables.rightExtrusion);
        local_XOffset = local_XOffset * meshTask.meshtaskObject.meshtaskSettings.extrusionSize;

        Vector3 noise = Vector3.zero;
        noise += meshTask.noiseChannel.generatorInstance.GetNoise(meshTask.startPointIndex + meshtaskPoint, meshTask.noiseChannel);

        GameObject instance = ObjectPooler.Instance.GetMeshtaskObject(meshTask.meshtaskObject.meshtaskSettings.meshTaskType, meshtaskPoolType);
        PlaceModelOnMesh(meshDirection, p, mto_position, noise, Mathf.Abs(local_XOffset), parent, instance);
    }

    [System.Serializable]
    private class SpawnSizeSettings
    {
        public int minimalMeshtaskPointSpawnSize;
        public MeshtaskPoolType objectType;
    }
}
