using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolSaver : MonoBehaviour
{
    private void OnDestroy()
    {
        foreach (Transform item in this.transform)
        {
            if (item.CompareTag("PooledObject"))
            {
                item.gameObject.SetActive(false);
                item.transform.parent = null;
            }
        }
    }
}
