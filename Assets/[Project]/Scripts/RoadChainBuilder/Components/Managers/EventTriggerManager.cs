using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTriggerManager : MonoBehaviour
{
    public delegate void OnNewResetPoint(Transform transform);
    public static event OnNewResetPoint resetPoint;

    public delegate void OnRoadChainTrigger(GameObject trigger);
    public static event OnRoadChainTrigger roadChainTrigger;

    public delegate void OnSectorTrigger();
    public static event OnSectorTrigger sectorTrigger;

    public delegate void OnSegmentTrigger(GameObject trigger);
    public static event OnSegmentTrigger segmentTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trigger"))
        {
            TriggerObject to = other.GetComponent<TriggerObject>();
            to.gameObject.SetActive(false);

            switch (to.triggerType)
            {
                case TriggerType.ResetPoint:
                    resetPoint?.Invoke(to.triggerPosition);
                    break;
                case TriggerType.EventPoint:
                    break;
                case TriggerType.RoadChain:
                    //sectorTrigger?.Invoke();
                    roadChainTrigger?.Invoke(to.gameObject);
                    break;
                case TriggerType.SegmentPoint:
                    segmentTrigger?.Invoke(to.gameObject);
                    break;
                default:
                    break;
            }
        }
    }
}

public enum TriggerType
{
    ResetPoint = 1,
    EventPoint,
    RoadChain,
    SegmentPoint,
    Other = default,
}
