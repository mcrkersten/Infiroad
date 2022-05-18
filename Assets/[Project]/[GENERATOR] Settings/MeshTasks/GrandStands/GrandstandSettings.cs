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
        int size = meshtask.points.Count;
        currentMeshObject.name = currentMeshObject.name + " " + meshtask.points.Count;
        if (size <= 10)
            SmallGrandstand(meshtask, currentMeshObject);
        if (size > 10 && size <= 20)
            MediumGrandstand(meshtask, currentMeshObject);
        if (size > 20)
            LargeGrandstand(meshtask, currentMeshObject);

        CreateGrandstandAcrs(meshtask, currentMeshObject);
    }

    private void CreateGrandstandAcrs(MeshTask meshtask, GameObject currentMeshObject)
    {
        int i = 2;
        while(i < (meshtask.points.Count - 2))
        {
            SpawnMeshtaskObject(meshtask, currentMeshObject, i, MeshtaskPoolType.GrandstandArcs);
            i += 2;
        }
    }

    private void SmallGrandstand(MeshTask meshTask, GameObject currentMeshObject)
    {
        for (int i = 0; i < small.smokeAmount; i++)
        {
            int random = Random.Range(0, 101);
            if(random < small.smokeSpawnPercentage)
                SpawnMeshtaskObject(meshTask, currentMeshObject, Random.Range(1, meshTask.points.Count - 2), MeshtaskPoolType.SmokeBombs);
        }
    }

    private void MediumGrandstand(MeshTask meshTask, GameObject currentMeshObject)
    {
        for (int i = 0; i < medium.smokeAmount; i++)
        {
            int random = Random.Range(0, 101);
            if (random < medium.smokeSpawnPercentage)
                SpawnMeshtaskObject(meshTask, currentMeshObject, Random.Range(1, meshTask.points.Count - 2), MeshtaskPoolType.SmokeBombs);
        }
    }

    private void LargeGrandstand(MeshTask meshTask, GameObject currentMeshObject)
    {
        for (int i = 0; i < large.smokeAmount; i++)
        {
            int random = Random.Range(0, 101);
            if (random < large.smokeSpawnPercentage)
                SpawnMeshtaskObject(meshTask, currentMeshObject, Random.Range(1, meshTask.points.Count - 2), MeshtaskPoolType.SmokeBombs);
        }
    }

    [System.Serializable]
    private class GrandstandSizeSettings
    {
        public int smokeAmount;
        public float smokeSpawnPercentage;
    }
}
