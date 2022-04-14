using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blib : MonoBehaviour
{
    public GameObject blibFill;
    public void Deactivate()
    {
        blibFill.SetActive(false);
    }

    public void Activate()
    {
        blibFill.SetActive(true);
    }
}
