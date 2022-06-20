using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTriggerManager : MonoBehaviour
{
    public delegate void OnNewResetPoint(Transform transform);
    public static event OnNewResetPoint resetPoint;

    public delegate void OnRoadChainTrigger();
    public static event OnRoadChainTrigger roadChainTrigger;

    public delegate void OnSectorTrigger();
    public static event OnSectorTrigger sectorTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trigger"))
        {
            TriggerObject to = other.GetComponent<TriggerObject>();
            switch(to.triggerType)
            {
                case TriggerType.ResetPoint:
                    resetPoint?.Invoke(to.triggerPosition);
                    break;
                case TriggerType.EventPoint:
                    break;
                case TriggerType.RoadChain:
                    roadChainTrigger?.Invoke();
                    sectorTrigger?.Invoke();
                    break;
                default:
                    break;
            }
            to.gameObject.SetActive(false);
        }
    }
}

public enum TriggerType
{
    ResetPoint = 1,
    EventPoint,
    RoadChain,
    Other = default,
}
