using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDespawn : MonoBehaviour
{
    private Coroutine coroutine; // Store the running coroutine

    private void OnEnable()
    {
        // Stop the coroutine if it's already running
        if (coroutine != null)
            StopCoroutine(coroutine);

        // Start a new coroutine
        coroutine = StartCoroutine(DisableAfterTime(20f));
    }

    public IEnumerator DisableAfterTime(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (TryGetComponent<AssetTrigger>(out AssetTrigger assetTrigger) && ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.ReturnAssetTrigger(assetTrigger);
            yield break;
        }

        this.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // Clean up the coroutine reference on disable
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }
}
