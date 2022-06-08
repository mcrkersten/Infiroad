using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspensionAudio : MonoBehaviour
{
    [SerializeField] private List<Suspension> suspension = new List<Suspension>();
    public GameObject right;
    public GameObject left;
    public AK.Wwise.RTPC RTCP;
    public AK.Wwise.Event Event;

    private void FixedUpdate()
    {
        float i = 0f;
        bool left = false;
        foreach (Suspension s in suspension)
        {
            if (i < s.stressSuspensionAudio)
                left = true;
            i += Mathf.Abs(s.stressSuspensionAudio);
        }
        i = (i / (suspension.Count - 1))*30f;
        if (i > .6f)
        {
            if (left)
            {
                Event.Post(this.left);
            }
            else
            {
                Event.Post(this.right);
            }
        }
        RTCP.SetGlobalValue(i);
    }
}
