using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FencePart : MonoBehaviour
{
    public List<Transform> fenceBars = new List<Transform>();
    public void SetFenceBars(Vector3 start, Vector3 end)
    {
        float distanceBetween = Vector3.Distance(start, end);
        foreach (Transform t in fenceBars)
        {
            t.localScale = new Vector3(1, 1, distanceBetween);
        }
    }
}
