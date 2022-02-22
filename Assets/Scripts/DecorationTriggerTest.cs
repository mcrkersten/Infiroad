using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationTriggerTest : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("AssetPoint"))
        {
            other.gameObject.SetActive(false);
        }
    }
}
