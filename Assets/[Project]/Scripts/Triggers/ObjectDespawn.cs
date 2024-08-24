using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDespawn : MonoBehaviour
{
    private IEnumerator coroutine;

    private void OnEnable()
    {
        if(coroutine == null)
            coroutine = DisableAfterTime(20f);
        else
            StopCoroutine(coroutine);

        StartCoroutine(coroutine);
    }

    public IEnumerator DisableAfterTime(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        this.gameObject.SetActive(false);
    }
}
