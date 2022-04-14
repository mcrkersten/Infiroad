using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlibManager : MonoBehaviour
{
    public GameObject blibPrefab;
    private List<Blib> blibs = new List<Blib>();
    private Blib currentActiveBlib;

    public void InstantiateBlibs(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject x = Instantiate(blibPrefab, this.transform);
            blibs.Add(x.GetComponent<Blib>());
        }
    }

    public void ActivateBlib(int index)
    {
        currentActiveBlib?.Deactivate();
        blibs[index]?.Activate();
    }
}
