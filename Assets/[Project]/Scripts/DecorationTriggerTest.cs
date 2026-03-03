using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationTriggerTest : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("AssetPoint"))
        {
            AssetTrigger assetTrigger = other.GetComponent<AssetTrigger>();
            if (assetTrigger != null && ObjectPooler.Instance != null)
                ObjectPooler.Instance.ReturnAssetTrigger(assetTrigger);
            else
                other.gameObject.SetActive(false);
        }
    }
}
