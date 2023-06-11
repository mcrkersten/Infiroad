using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveColliders : MonoBehaviour
{
    [ContextMenu("Remove Colliders")]
    public void Remove()
    {
        foreach (var item in GetComponentsInChildren<Collider>())
        {
            DestroyImmediate(item);
        }
    }
}
