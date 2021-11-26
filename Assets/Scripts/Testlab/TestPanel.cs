using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPanel : MonoBehaviour
{
    public float multiply;
    public Vector3 pos;
    // Update is called once per frame
    public void Update()
    {
        this.transform.position = new Vector3(0, Mathf.Sin(Time.time * multiply), 0);
    }
}
