using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadChainTrigger : MonoBehaviour
{
    public delegate void OnRoadChainTrigger();
    public static event OnRoadChainTrigger trigger;
    private void OnTriggerEnter(Collider other)
    {
        trigger?.Invoke();
        other.gameObject.SetActive(false);
    }
}
