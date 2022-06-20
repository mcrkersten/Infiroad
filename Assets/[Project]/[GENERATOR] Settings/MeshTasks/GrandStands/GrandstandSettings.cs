using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu, System.Serializable]
public class GrandstandSettings : MeshtaskSettings
{
    [SerializeField] private GrandstandSizeSettings small;
    [SerializeField] private GrandstandSizeSettings medium;
    [SerializeField] private GrandstandSizeSettings large;
    public override void PopulateMeshtask(MeshTask meshtask, GameObject currentMeshObject)
    {
        int size = meshtask.positionPoints.Count;
        currentMeshObject.name = currentMeshObject.name + " " + meshtask.positionPoints.Count;
        if (size <= 5)
            SmallGrandstand(meshtask, currentMeshObject);
        if (size > 5 && size <= 10)
            MediumGrandstand(meshtask, currentMeshObject);
        if (size > 10)
            LargeGrandstand(meshtask, currentMeshObject);

        CreateGrandstandAcrs(meshtask, currentMeshObject);
    }

    private void CreateGrandstandAcrs(MeshTask meshtask, GameObject parent)
    {
        int i = 1;
        while(i < (meshtask.positionPoints.Count - 2))
        {
            SpawnMeshtaskObject(meshtask, parent, i, MeshtaskPoolType.GrandstandArcs);
            i++;
        }
    }

    private void SmallGrandstand(MeshTask meshTask, GameObject parent)
    {
        for (int i = 0; i < small.smokeAmount; i++)
        {
            int random = Random.Range(0, 101);
            if(random < small.smokeSpawnPercentage)
                SpawnMeshtaskObject(meshTask, parent, Random.Range(1, meshTask.positionPoints.Count - 2), MeshtaskPoolType.SmokeBombs);
        }
    }

    private void MediumGrandstand(MeshTask meshTask, GameObject parent)
    {
        for (int i = 0; i < medium.smokeAmount; i++)
        {
            int random = Random.Range(0, 101);
            if (random < medium.smokeSpawnPercentage)
                SpawnMeshtaskObject(meshTask, parent, Random.Range(1, meshTask.positionPoints.Count - 2), MeshtaskPoolType.SmokeBombs);
        }
    }

    private void LargeGrandstand(MeshTask meshTask, GameObject parent)
    {
        for (int i = 0; i < large.smokeAmount; i++)
        {
            int random = Random.Range(0, 101);
            if (random < large.smokeSpawnPercentage)
                SpawnMeshtaskObject(meshTask, parent, Random.Range(1, meshTask.positionPoints.Count - 2), MeshtaskPoolType.SmokeBombs);
        }
    }

    [System.Serializable]
    private class GrandstandSizeSettings
    {
        public int smokeAmount;
        public float smokeSpawnPercentage;
    }
}
