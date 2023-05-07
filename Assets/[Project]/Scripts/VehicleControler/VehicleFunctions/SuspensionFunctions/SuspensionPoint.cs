using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspensionPoint : MonoBehaviour
{
   public bool broken;
   [SerializeField] private List<SuspensionPointer> suspensionPointers = new List<SuspensionPointer>();
    // Update is called once per frame
    void Update()
    {
        foreach (SuspensionPointer s in suspensionPointers)
        {
            if(!broken)
                s.UpdatePointer();
        }
    }
}
